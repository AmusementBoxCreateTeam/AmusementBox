//#define USE_CACHEDTHREAD
//#define USE_SINGLETHREAD_MORPH
using UnityEngine;
using System.Collections;
using BulletXNA;
using BulletXNA.BulletCollision;
using BulletXNA.BulletDynamics;
using BulletXNA.LinearMath;
using MMD4Mecanim;

using PMXFileType				= MMD4MecanimBulletPMXCommon.PMXFileType;
using PMXRigidBodyType			= MMD4MecanimBulletPMXCommon.PMXRigidBodyType;
using PMXBone					= MMD4MecanimBulletPMXBone;
using PMXMorph					= MMD4MecanimBulletPMXMorph;
using PMXRigidBody				= MMD4MecanimBulletPMXRigidBody;
using PMXJoint					= MMD4MecanimBulletPMXJoint;
using PMXIK						= MMD4MecanimBulletPMXIK;
using PMXMesh					= MMD4MecanimBulletPMXMesh;

using IndexData					= MMD4MecanimAuxData.IndexData;
using VertexData				= MMD4MecanimAuxData.VertexData;

using MMDModelProperty			= MMD4MecanimBulletPhysics.MMDModelProperty;

using MeshFlags					= MMD4MecanimData.MeshFlags;
using UpdateFlags				= MMD4MecanimBulletPhysics.MMDModel.UpdateFlags;
using UpdateBoneFlags			= MMD4MecanimBulletPhysics.MMDModel.UpdateBoneFlags;
using UpdateRigidBodyFlags		= MMD4MecanimBulletPhysics.MMDModel.UpdateRigidBodyFlags;
using LateUpdateFlags			= MMD4MecanimBulletPhysics.MMDModel.LateUpdateFlags;
using LateUpdateBoneFlags		= MMD4MecanimBulletPhysics.MMDModel.LateUpdateBoneFlags;
using LateUpdateMeshFlags		= MMD4MecanimBulletPhysics.MMDModel.LateUpdateMeshFlags;

using CachedThreadQueue			= MMD4MecanimBulletPhysicsUtil.CachedThreadQueue;
using ThreadQueueHandle			= MMD4MecanimBulletPhysicsUtil.ThreadQueueHandle;
using CachedThread				= MMD4MecanimBulletPhysicsUtil.CachedThread;
using CachedPararellThreadQueue	= MMD4MecanimBulletPhysicsUtil.CachedPararellThreadQueue;

// Pending: Support physics reset command.

public class MMD4MecanimBulletPMXModel : MMD4MecanimBulletPhysicsEntity
{
	static CachedThreadQueue _vertexMorphThreadQueue = null;
	static CachedPararellThreadQueue _xdefPararellThreadQueue = null;

	public static CachedPararellThreadQueue GetXDEFPararellThreadQueue()
	{
		M4MDebug.Assert( _xdefPararellThreadQueue != null );
		return _xdefPararellThreadQueue;
	}

	public struct UpdateAnimData
	{
		public uint						animationHashName;
		public float					animationTime;
	}

	public class LateUpdateMeshData
	{
		public int						meshID = -1;
		public uint						lateUpdateMeshFlags = 0;
		public int						vertexLength = 0;
		public Vector3[]				vertices = null;
		public Vector3[]				normals = null;
	}

	public class UpdateData
	{
		public uint						updateFlags;
		public UpdateAnimData			updateAnimData = new UpdateAnimData();
		public float					updatePPHShoulderFixRate;
		public Matrix4x4				updateModelTransform;
		public int[]					updateBoneFlagsList;
		public Matrix4x4[]				updateBoneTransformList;
		public Vector3[]				updateBoneLocalPositionList;
		public Quaternion[]				updateBoneLocalRotationList;
		public Vector3[]				updateBoneUserPositionList;
		public Quaternion[]				updateBoneUserRotationList;
		public int[]					updateRigidBodyFlagsList;
		public float[]					updateIKWeightList;
		public float[]					updateMorphWeightList;

		public uint						lateUpdateFlags;
		public int[]					lateUpdateBoneFlagsList;
		public Vector3[]				lateUpdateBonePositionList;
		public Quaternion[]				lateUpdateBoneRotationList;
		public int[]					lateUpdateMeshFlagsList;
		public LateUpdateMeshData[]		lateUpdateMeshDataList;
	}

	PMXFileType                 _fileType;
	uint						_vertexCount;
	Vector3						_lossyScale = Vector3.one;
	float						_lossyScaleInv = 1.0f;
	bool						_isLossyScaleIdentity = false;
	bool						_isLossyScaleSquare = false;
	float						_importScaleMMDModel;
	float                       _modelToBulletScale;
	float                       _bulletToLocalScale;
	float                       _bulletToWorldScale;
	float                       _worldToBulletScale;
	float						_localToBulletScale;
	bool						_optimizeBulletXNA = true;
	float						_resetWaitTime;
	float						_resetMorphTime;
	PMXBone                     _rootBone;
	PMXBone[]                   _boneList;
	PMXBone[]					_sortedBoneList;
	PMXMorph[]					_morphList;
	PMXRigidBody[]	            _rigidBodyList;
	PMXJoint[]       			_jointList;
	PMXIK[]						_ikList;
	PMXMesh[]					_meshList;
	IndexData					_indexData;
	VertexData					_vertexData;

	PMXRigidBody[]				_simulatedRigidBodyList;

    bool                        _isJoinedWorld;
	bool						_isJoinedWorldInternal;
	bool					    _isResetWorld;
	bool					    _processResetWorld;
	float					    _processResetRatio;

	bool						_isMultiThreading = false;
	bool						_isPhysicsEnabled = true;
	bool						_isJoinedLocalWorld = false;
	bool						_isVertexMorphEnabled = false;
	bool						_isBlendShapesEnabled = false;
	bool						_isIKEnabled = false;
	bool						_isBoneInherenceEnabled = false;
	bool						_isBoneMorphEnabled = false;
	bool						_isXDEFEnabled = false;
	bool						_isXDEFNormalEnabled = false;
	bool						_isDestroyed = false;
	float						_pphShoulderFixRate = 0.0f;
	float						_pphShoulderFixRate2 = 0.0f;

	UpdateData					_sync_unusedData;
	UpdateData					_sync_updateData;
	UpdateData					_sync_updateData2; // Hotfix
	UpdateData					_sync_updatedData;
	UpdateData					_sync_lateUpdateData;
	
	UpdateData					_updateData;

	public IndexedMatrix		_modelTransform = IndexedMatrix.Identity;
	MMDModelProperty			_modelProperty;

	#if USE_CACHEDTHREAD
	CachedThread				_cachedThread = null;
	#else
	ThreadQueueHandle			_vertexMorphThreadQueueHandle = new ThreadQueueHandle();
	#endif

	public PMXMesh[] meshList { get { return _meshList; } }
	public IndexData indexData { get { return _indexData; } }
	public VertexData vertexData { get { return _vertexData; } }

    public bool isJoinedWorld { get { return _isJoinedWorld; } }
    public PMXFileType fileType { get { return _fileType; } }
	public uint vertexCount { get { return _vertexCount; } }
	public float importScaleMMDModel { get { return _importScaleMMDModel; } }
    public float modelToBulletScale { get { return _modelToBulletScale; } }
	public float bulletToLocalScale { get { return _bulletToLocalScale; } }
	public float bulletToWorldScale { get { return _bulletToWorldScale; } }
	public float worldToBulletScale { get { return _worldToBulletScale; } }
	public float localToBulletScale { get { return _localToBulletScale; } }
    public PMXBone rootBone { get { return _rootBone; } }
	public MMDModelProperty modelProperty { get { return _modelProperty; } }
	public bool optimizeBulletXNA { get { return _optimizeBulletXNA; } }
	public bool isPhysicsEnabled { get { return _isPhysicsEnabled; } }
	public bool isVertexMorphEnabled { get { return _isVertexMorphEnabled; } }
	public bool isBlendShapesEnabled { get { return _isBlendShapesEnabled; } }
	public bool isIKEnabled { get { return _isIKEnabled; } }
	public bool isBoneInherenceEnabled { get { return _isBoneInherenceEnabled; } }
	public bool isBoneMorphEnabled { get { return _isBoneMorphEnabled; } }
	public bool isXDEFEnabled  { get { return _isXDEFEnabled; } }
	public bool isXDEFNormalEnabled  { get { return _isXDEFNormalEnabled; } }
	public float pphShoulderFixRate { get { return _pphShoulderFixRate; } }
	public float pphShoulderFixRate2 { get { return _pphShoulderFixRate2; } }

    ~MMD4MecanimBulletPMXModel()
    {
        Destroy();
    }

    public void Destroy()
    {
		_isDestroyed = true;
		if( _isJoinedWorld ) {
			// Nothing.( Execute on _LeaveWorld(). )
		} else {
			_DestroyImmediate();
		}
	}

    public struct ImportProperty
    {
		public bool isPhysicsEnabled;
		public bool isVertexMorphEnabled;
		public bool isBlendShapesEnabled;
		public bool isXDEFEnabled;
		public bool isXDEFNormalEnabled;
		public bool isJoinedLocalWorld;
		public bool useCustomResetTime;
		public float resetWaitTime;
		public float resetMorphTime;
		public bool optimizeBulletXNA;
		public MMD4MecanimBulletPhysics.MMDModelProperty mmdModelProperty;
    }

	static float _SafeDiv( float a, float b )
	{
		return (Mathf.Abs(b) > Mathf.Epsilon) ? (a / b) : 0.0f;
	}

    public bool Import(
		MMD4MecanimCommon.BinaryReader binaryReader,
		byte[] indexData,
		byte[] vertexData,
		int[] meshFlags,
		ref ImportProperty importProperty )
    {
        uint fourCC = (uint)binaryReader.GetFourCC();
        if( fourCC != MMD4MecanimCommon.BinaryReader.MakeFourCC("MDL1") ) {
            char cc0 = (char)fourCC;
            char cc1 = (char)((fourCC >> 8) & 0xff);
            char cc2 = (char)((fourCC >> 16) & 0xff);
            char cc3 = (char)((fourCC >> 24) & 0xff);
            Debug.LogError( "Not supported file. " + cc0 + cc1 + cc2 + cc3 );
            return false;
        }

		_isPhysicsEnabled = importProperty.isPhysicsEnabled;
		_isVertexMorphEnabled = importProperty.isVertexMorphEnabled;
		_isBlendShapesEnabled = importProperty.isBlendShapesEnabled;
		_isXDEFEnabled = importProperty.isXDEFEnabled;
		_isXDEFNormalEnabled = importProperty.isXDEFNormalEnabled;
		_isJoinedLocalWorld = importProperty.isJoinedLocalWorld;

		_modelProperty = importProperty.mmdModelProperty;
		if( _modelProperty == null ) {
			_modelProperty = new MMDModelProperty();
		} else {
			_modelProperty = _modelProperty.Clone();
		}

		// Like as 2.75
		if( _modelProperty.rigidBodyLinearDampingLossRate < 0.0f ) {
			_modelProperty.rigidBodyLinearDampingLossRate = 0.05f;
		}
		if( _modelProperty.rigidBodyAngularDampingLossRate < 0.0f ) {
			_modelProperty.rigidBodyAngularDampingLossRate = 0.05f;
		}

		if( _modelProperty.rigidBodyLinearVelocityLimit < 0.0f ) {
			_modelProperty.rigidBodyLinearVelocityLimit = 10.0f; // 10m
		}
		
		if( _modelProperty.jointRootAdditionalLimitAngle < 0.0f ) {
			_modelProperty.jointRootAdditionalLimitAngle = 135.0f;
		}

		_modelProperty.Postfix();

		_lossyScale = _modelProperty.lossyScale;
		if( Mathf.Abs(_lossyScale.x) > Mathf.Epsilon ) {
			_lossyScaleInv = 1.0f / _lossyScale.x;
		} else {
			_lossyScaleInv = 0.0f;
		}

		_isLossyScaleSquare = _isLossyScaleIdentity =
			Mathf.Abs(_lossyScale.x - 1.0f) <= Mathf.Epsilon &&
			Mathf.Abs(_lossyScale.y - 1.0f) <= Mathf.Epsilon &&
			Mathf.Abs(_lossyScale.z - 1.0f) <= Mathf.Epsilon;
		if( !_isLossyScaleSquare ) {
			_isLossyScaleSquare = 
				Mathf.Abs(_lossyScale.x - _lossyScale.y) <= Mathf.Epsilon &&
				Mathf.Abs(_lossyScale.x - _lossyScale.z) <= Mathf.Epsilon;
		}

		_optimizeBulletXNA = importProperty.optimizeBulletXNA;

		_resetMorphTime = 1.8f;
		_resetWaitTime = 1.2f;
		if( importProperty.useCustomResetTime ) {
			_resetMorphTime = importProperty.resetMorphTime;
			_resetWaitTime = importProperty.resetWaitTime;
		}

        binaryReader.BeginHeader();
	    _fileType = (PMXFileType)binaryReader.ReadHeaderInt();
	    binaryReader.ReadHeaderFloat(); // fileVersion;
	    binaryReader.ReadHeaderInt(); // fileVersion(BIN)
	    binaryReader.ReadHeaderInt(); // additionalFlags
		_vertexCount = (uint)binaryReader.ReadHeaderInt();
	    binaryReader.ReadHeaderInt(); // indexCount
	    float vertexScale = binaryReader.ReadHeaderFloat();
	    float importScale = binaryReader.ReadHeaderFloat();
	    binaryReader.EndHeader();
		_importScaleMMDModel = importScale;

		if( _modelProperty.importScale > Mathf.Epsilon ) {
			importScale = _modelProperty.importScale;
		}
		
		float unityScale = vertexScale * importScale; // Mesh > Unity(Local) Scale(Default : 8 * 0.01 = 0.08)
		
		_modelToBulletScale = 1.0f;
		_bulletToLocalScale = 1.0f;
		_bulletToWorldScale = 1.0f;
		_worldToBulletScale = 1.0f;

		float worldScale = unityScale;
		if( _modelProperty.worldScale > Mathf.Epsilon ) {
			worldScale = _modelProperty.worldScale;
		}

		if( _isJoinedLocalWorld ) {
			_modelToBulletScale = _SafeDiv(unityScale, worldScale);
		} else {
			_modelToBulletScale = _lossyScale.x * _SafeDiv(unityScale, worldScale);
		}
		
		_bulletToLocalScale = unityScale * _SafeDiv(1.0f, _modelToBulletScale);
		_localToBulletScale = _SafeDiv( 1.0f, _bulletToLocalScale );
		_worldToBulletScale = _SafeDiv( _localToBulletScale, _lossyScale.x );
		_bulletToWorldScale = _SafeDiv( 1.0f, _worldToBulletScale );

        int fourCC_Bone = MMD4MecanimCommon.BinaryReader.MakeFourCC("BONE");
        int fourCC_IK = MMD4MecanimCommon.BinaryReader.MakeFourCC("IK__");
		int fourCC_Morph = MMD4MecanimCommon.BinaryReader.MakeFourCC("MRPH");
		int fourCC_RigidBody = MMD4MecanimCommon.BinaryReader.MakeFourCC("RGBD");
        int fourCC_Joint = MMD4MecanimCommon.BinaryReader.MakeFourCC("JOIN");

	    int structListLength = binaryReader.structListLength;
	    for( int structListIndex = 0; structListIndex < structListLength; ++structListIndex ) {
            if( !binaryReader.BeginStructList() ) {
			    Debug.LogError( "BeginStructList() failed." );
                return false;
            }

            int structFourCC = binaryReader.currentStructFourCC;
            if( structFourCC == fourCC_Bone ) {
                _boneList = new PMXBone[binaryReader.currentStructLength];
				for( int structIndex = 0; structIndex < binaryReader.currentStructLength; ++structIndex ) {
					_boneList[structIndex] = new PMXBone();
					_boneList[structIndex]._model = this;
				}
			    for( int structIndex = 0; structIndex < binaryReader.currentStructLength; ++structIndex ) {
				    PMXBone pmxBone = _boneList[structIndex];
				    if( !pmxBone.Import( structIndex, binaryReader ) ) {
					    Debug.LogError( "PMXBone parse error." );
                        _boneList = null;
					    return false;
				    }
				    if( pmxBone.isRootBone ) {
					    _rootBone = pmxBone;
				    }
			    }
                if( _rootBone == null && _boneList.Length > 0 ) {
                    _rootBone = _boneList[0];
                    _rootBone.isRootBone = true;
                }
				for( int i = 0; i < _boneList.Length; ++i ) {
					_boneList[i].PostfixImport();
				}
            } else if( structFourCC == fourCC_IK ) {
				_ikList = new PMXIK[binaryReader.currentStructLength];
				for( int structIndex = 0; structIndex < binaryReader.currentStructLength; ++structIndex ) {
					PMXIK ik = new PMXIK();
					_ikList[structIndex] = ik;
					ik._model = this;
					if( !ik.Import( binaryReader ) ) {
						Debug.LogError( "PMXIK parse error." );
						_ikList = null;
						return false;
					}
				}
			} else if( structFourCC == fourCC_Morph ) {
				_morphList = new PMXMorph[binaryReader.currentStructLength];
				for( int structIndex = 0; structIndex != binaryReader.currentStructLength; ++structIndex ) {
					PMXMorph morph = new PMXMorph();
					_morphList[structIndex] = morph;
					morph._model = this;
				}

				for( int structIndex = 0; structIndex != binaryReader.currentStructLength; ++structIndex ) {
					PMXMorph morph = _morphList[structIndex];
					if( !morph.Import( binaryReader ) ) {
						Debug.LogError( "PMXIK parse error." );
						_morphList = null;
						return false;
					}
				}
			} else if( structFourCC == fourCC_RigidBody ) {
			    _rigidBodyList = new PMXRigidBody[binaryReader.currentStructLength];
			    for( int structIndex = 0; structIndex < binaryReader.currentStructLength; ++structIndex ) {
				    PMXRigidBody pmxRigidBody = new PMXRigidBody();
				    _rigidBodyList[structIndex] = pmxRigidBody;
				    pmxRigidBody._model = this;
					if( !pmxRigidBody.Import( binaryReader ) ) {
					    Debug.LogError( "PMXRigidBody parse error." );
                        _rigidBodyList = null;
					    return false;
				    }
			    }
            } else if( structFourCC == fourCC_Joint ) {
			    _jointList = new PMXJoint[binaryReader.currentStructLength];
			    for( int structIndex = 0; structIndex < binaryReader.currentStructLength; ++structIndex ) {
				    PMXJoint pmxJoint = new PMXJoint();
				    _jointList[structIndex] = pmxJoint;
				    pmxJoint._model = this;
				    if( !pmxJoint.Import( binaryReader ) ) {
					    Debug.LogError( "PMXJoint parse error." );
                        _jointList = null;
					    return false;
				    }
			    }
            }

            if( !binaryReader.EndStructList() ) {
			    Debug.LogError( "EndStructList() failed." );
			    return false;
		    }
        }

		if( _boneList != null ) {
			_sortedBoneList = new PMXBone[_boneList.Length];
			for( int i = 0; i < _boneList.Length; ++i ) {
				int sortedBoneID = _boneList[i].sortedBoneID;
				if( sortedBoneID >= 0 && sortedBoneID < _boneList.Length ) {
					_sortedBoneList[sortedBoneID] = _boneList[i];
				}
			}
			for( int i = 0; i < _boneList.Length; ++i ) {
				if( _sortedBoneList[i] == null ) {
					Debug.LogError( "SortedBoneList is invalid. boneID = " + i );
					_sortedBoneList = null;
					break;
				}
			}
		}

		if( _isPhysicsEnabled ) {
			_MakeSimulatedRigidBodyList();
			_isResetWorld = true;
		}

		// VertexMorph(Non BlendShapes) or XDEF supported.
		if( meshFlags != null && meshFlags.Length != 0 ) {
			unchecked {
				bool blendShapesAnything = false;
				for( int i = 0; i != meshFlags.Length && !blendShapesAnything; ++i ) {
					blendShapesAnything |= ( ( meshFlags[i] & (int)MeshFlags.BlendShapes ) != 0 );
				}
				_isBlendShapesEnabled = (_isVertexMorphEnabled && _isBlendShapesEnabled && blendShapesAnything);
			}
		} else {
			_isVertexMorphEnabled = false;
			_isBlendShapesEnabled = false;
			_isXDEFEnabled = false;
			_isXDEFNormalEnabled = false;
		}

		if( (_isVertexMorphEnabled && !_isBlendShapesEnabled) || _isXDEFEnabled ) {
			if( indexData != null && indexData.Length != 0 ) {
				_indexData = MMD4MecanimAuxData.BuildIndexData( indexData );
				if( _indexData != null ) {
					if( meshFlags == null || meshFlags.Length != _indexData.meshCount ) {
						_indexData = null;
					}
				}
			}
			
			if( _indexData != null && _isXDEFEnabled ) {
				if( vertexData != null && vertexData.Length != 0 ) {
					_vertexData = MMD4MecanimAuxData.BuildVertexData( vertexData );
					if( _vertexData != null ) {
						if( meshFlags == null || meshFlags.Length != _vertexData.meshCount ) {
							_vertexData = null;
						}
					}
				}
			}
			
			if( _indexData != null ) {
				if( meshFlags != null && meshFlags.Length != 0 ) {
					_meshList = new PMXMesh[meshFlags.Length];
					for( int i = 0; i != _meshList.Length; ++i ) {
						_meshList[i] = new PMXMesh();
						_meshList[i]._model = this;
						unchecked {
							_meshList[i].meshFlags = (uint)meshFlags[i];
						}
					}
					
					if( _vertexData != null && _isXDEFEnabled ) { // SDEF or QDEF
						bool isXDEFDepended = false;
						VertexData.MeshBoneInfo meshBoneInfo = new VertexData.MeshBoneInfo();
						for( int i = 0; i != _meshList.Length; ++i ) {
							unchecked {
								if( !_isBlendShapesEnabled || ( meshFlags[i] & (int)MeshFlags.BlendShapes ) == 0 ) {
									_vertexData.GetMeshBoneInfo( ref meshBoneInfo, i );
									if( meshBoneInfo.isSDEF ) { // Now supported SDEF only.
										_meshList[i].meshFlags |= (uint)MeshFlags.XDEF;
										isXDEFDepended = true;
									}
								}
							}
						}
						if( !isXDEFDepended ) {
							_isXDEFEnabled = false;
							_isXDEFNormalEnabled = false;
						}
					}
					
					if( _indexData != null && _isVertexMorphEnabled && !_isBlendShapesEnabled ) { // VertexMorph
						bool invalidateAnything = false;
						for( int m = 0; m != _morphList.Length; ++m ) {
							if( !_morphList[m].PrepareDependMesh() ) {
								invalidateAnything = true;
								break;
							}
						}
						if( invalidateAnything ) {
							_indexData = null;
							_meshList = null;
						}
					}
				}
			}
		}

		if( _indexData != null ) {
			if( meshFlags != null && _meshList != null && _meshList.Length == meshFlags.Length ) {
				unchecked {
					for( int i = 0; i != meshFlags.Length; ++i ) {
						meshFlags[i] = (int)_meshList[i].meshFlags;
					}
				}
			}
		} else {
			_meshList = null;
			if( meshFlags != null ) {
				unchecked {
					for( int i = 0; i != meshFlags.Length; ++i ) {
						meshFlags[i] &= ~(int)(MeshFlags.VertexMorph | MeshFlags.XDEF);
					}
				}
			}
			_isVertexMorphEnabled = false;
			_isBlendShapesEnabled = false;
			_isXDEFEnabled = false;
			_isXDEFNormalEnabled = false;
		}
		
		if( _isVertexMorphEnabled && !_isBlendShapesEnabled ) {
			#if USE_CACHEDTHREAD
			_cachedThread = new CachedThread();
			#else
			if( _vertexMorphThreadQueue == null ) {
				_vertexMorphThreadQueue = new CachedThreadQueue();
			}
			#endif
		}
		if( _isVertexMorphEnabled && _isXDEFEnabled ) {
			if( _xdefPararellThreadQueue == null ) {
				_xdefPararellThreadQueue = new CachedPararellThreadQueue();
			}
		}

        //Debug.Log( "MMD4MecanimBulletPMXModel::Import: Success" );
        return true;
    }

	public int UploadMesh( int meshID, Vector3[] vertices, Vector3[] normals, BoneWeight[] boneWeights, Matrix4x4[] bindposes )
	{
		if( _meshList == null || meshID < 0 || meshID >= _meshList.Length ) {
			return 0;
		}
		
		return _meshList[meshID].UploadMesh( meshID, vertices, normals, boneWeights, bindposes ) ? 1 : 0;
	}

	private static bool _IsParentBone( PMXBone targetBone, PMXBone bone )
	{
		for( ; bone != null; bone = bone.originalParentBone ) {
			if( bone == targetBone ) {
				return true;
			}
		}
		return false;
	}

	private static void _Swap( ref PMXRigidBody lhs, ref PMXRigidBody rhs )
	{
		PMXRigidBody tmp = lhs;
		lhs = rhs;
		rhs = tmp;
	}

	public int PreUpdate(
		uint updateFlags,
		int[] iBoneValues,
		int[] iRigidBodyValues,
		float[] ikWeights,
		float[] morphWeights )
	{
		if( iBoneValues == null || _boneList == null ) {
			return 0;
		}

		unchecked {
			if( _boneList.Length == iBoneValues.Length ) {
				for( int i = 0; i != _boneList.Length; ++i ) {
					_boneList[i].PreUpdate_Prepare( updateFlags, (uint)iBoneValues[i] );
				}
			}

			if( _rigidBodyList != null && iRigidBodyValues != null && _rigidBodyList.Length == iRigidBodyValues.Length ) {
				for( int i = 0; i != _rigidBodyList.Length; ++i ) {
					_rigidBodyList[i].preUpdate_updateRigidBodyFlags = (uint)iRigidBodyValues[i];
				}
			}

			if( (updateFlags & (int)UpdateFlags.IKEnabled) != 0 &&
			    ikWeights != null && _ikList != null && ikWeights.Length == _ikList.Length ) {
				for( int i = 0; i != _ikList.Length; ++i ) {
					_ikList[i].PreUpdate_MarkIKDepended( updateFlags, ikWeights[i] );
				}
			}
			
			if( _isVertexMorphEnabled && _isXDEFEnabled ) {
				if( _vertexData != null && _meshList != null && _vertexData.meshCount == _meshList.Length ) {
					int meshCount = _vertexData.meshCount;
					VertexData.MeshBoneInfo meshBoneInfo = new VertexData.MeshBoneInfo();
					for( int i = 0; i != meshCount; ++i ) {
						if( !_isBlendShapesEnabled || (_meshList[i].meshFlags & (uint)MeshFlags.BlendShapes) == 0 ) { // Expect BlendShapes meshes.
							_vertexData.GetMeshBoneInfo( ref meshBoneInfo, i );
							if( meshBoneInfo.isSDEF ) { // Now supported SDEF only.
								for( int boneIndex = 0; boneIndex != meshBoneInfo.count; ++boneIndex ) {
									VertexData.BoneFlags boneFlags = _vertexData.GetBoneFlags( ref meshBoneInfo, boneIndex );
									if( ( boneFlags & VertexData.BoneFlags.SDEF ) != VertexData.BoneFlags.None ) { // SDEF only.
										int boneID = _vertexData.GetBoneID( ref meshBoneInfo, boneIndex );
										if( (uint)boneID < (uint)_boneList.Length ) {
											_boneList[i]._preUpdate_isXDEFDepended = true;
										}
									}
								}
							}
						}
					}
				}
			}

			if( morphWeights != null && _morphList != null && morphWeights.Length == _morphList.Length ) {
				for( int i = 0; i != _morphList.Length; ++i ) {
					_morphList[i].preUpdate_weight = morphWeights[i];
					_morphList[i].preUpdate_appendWeight = 0.0f;
				}
				for( int i = 0; i != _morphList.Length; ++i ) {
					_morphList[i].PreUpdate_ApplyGroupMorph();
				}
				for( int i = 0; i != _morphList.Length; ++i ) {
					_morphList[i].PreUpdate_ApplyMorph( updateFlags );
				}
			}

			for( int i = 0; i != _boneList.Length; ++i ) {
				_boneList[i].PreUpdate_CheckUpdated();
			}
			for( int i = 0; i != _boneList.Length; ++i ) {
				_boneList[i].PreUpdate_CheckUpdated2();
			}

			for( int i = 0; i != _boneList.Length; ++i ) {
				if( !_isMultiThreading ) {
					if( _boneList[i].preUpdate_isWorldTransform ) {
						iBoneValues[i] |= (int)UpdateBoneFlags.WorldTransform;
					}
				}
				if( _boneList[i].preUpdate_isPosition ) {
					iBoneValues[i] |= (int)UpdateBoneFlags.Position;
				}
				if( _boneList[i].preUpdate_isRotation ) {
					iBoneValues[i] |= (int)UpdateBoneFlags.Rotation;
				}
				if( _boneList[i].preUpdate_isCheckPosition ) {
					iBoneValues[i] |= (int)UpdateBoneFlags.CheckPosition;
				}
				if( _boneList[i].preUpdate_isCheckRotation ) {
					iBoneValues[i] |= (int)UpdateBoneFlags.CheckRotation;;
				}
			}
			
			if( _isMultiThreading ) {
				for( int i = 0; i != _boneList.Length; ++i ) {
					iBoneValues[i] |= (int)UpdateBoneFlags.Position;
					iBoneValues[i] |= (int)UpdateBoneFlags.Rotation;
					iBoneValues[i] |= (int)UpdateBoneFlags.CheckPosition;
					iBoneValues[i] |= (int)UpdateBoneFlags.CheckRotation;
				}
			}

			return 1;
		}
	}

	public static void ArrayCopy<Type>( Type[] src, Type[] dst )
	{
		if( src != null && dst != null && dst.Length == src.Length ) {
			System.Array.Copy( src, dst, src.Length );
		}
	}

	public static void PrimArrayClone<Type>( Type[] src, ref Type[] dst )
	{
		if( src != null ) {
			if( dst == null || dst.Length != src.Length ) {
				dst = new Type[src.Length];
			}
			System.Buffer.BlockCopy( src, 0, dst, 0, System.Buffer.ByteLength( src ) );
		} else {
			dst = null;
		}
	}

	public static void ArrayClone<Type>( Type[] src, ref Type[] dst )
	{
		if( src != null ) {
			if( dst == null || dst.Length != src.Length ) {
				dst = new Type[src.Length];
			}
			System.Array.Copy( src, dst, src.Length );
		} else {
			dst = null;
		}
	}

	public void Update( uint updateFlags, int[] iValues, float[] fValues, ref Matrix4x4 modelTransform,
		int[] iBoneValues, Matrix4x4[] boneTransforms, Vector3[] bonePositions, Quaternion[] boneRotations,
	    Vector3[] boneUserPositions, Quaternion[] boneUserRotations,
	    int[] iRigidBodyValues,
	    float[] ikWights,
	    float[] morphWeights )
    {
		UpdateData updateData = null;
		if( _isMultiThreading ) {
			lock(this) {
				updateData = _sync_unusedData; // Optimize: Recycle _sync_unusedData
				_sync_unusedData = null;
			}
		} else {
			updateData = _sync_unusedData; // Optimize: Recycle _sync_unusedData
			_sync_unusedData = null;
		}

		if( updateData == null ) {
			updateData = new UpdateData();
		}

		updateData.updateFlags = updateFlags;

		unchecked {
			if( iValues != null && iValues.Length >= 1 ) {
				updateData.updateAnimData.animationHashName	= (uint)iValues[0];
			} else {
				updateData.updateAnimData.animationHashName	= 0;
			}
			if( fValues != null && fValues.Length >= 2 ) {
				updateData.updateAnimData.animationTime		= fValues[0];
				updateData.updatePPHShoulderFixRate			= fValues[1];
			} else {
				updateData.updateAnimData.animationTime		= 0;
				updateData.updatePPHShoulderFixRate			= 0.0f;
			}
		}

		updateData.updateModelTransform = modelTransform;

		if( _isMultiThreading ) {
			PrimArrayClone( iBoneValues, ref updateData.updateBoneFlagsList );
			ArrayClone( boneTransforms, ref updateData.updateBoneTransformList );
			ArrayClone( bonePositions, ref updateData.updateBoneLocalPositionList );
			ArrayClone( boneRotations, ref updateData.updateBoneLocalRotationList );
			ArrayClone( boneUserPositions, ref updateData.updateBoneUserPositionList );
			ArrayClone( boneUserRotations, ref updateData.updateBoneUserRotationList );
			PrimArrayClone( iRigidBodyValues, ref updateData.updateRigidBodyFlagsList );
			PrimArrayClone( ikWights, ref updateData.updateIKWeightList );
			PrimArrayClone( morphWeights, ref updateData.updateMorphWeightList );
		} else {
			// Optimized for SingleThread.
			updateData.updateBoneFlagsList = iBoneValues;
			updateData.updateBoneTransformList = boneTransforms;
			updateData.updateBoneLocalPositionList = bonePositions;
			updateData.updateBoneLocalRotationList = boneRotations;
			updateData.updateBoneUserPositionList = boneUserPositions;
			updateData.updateBoneUserRotationList = boneUserRotations;
			updateData.updateRigidBodyFlagsList = iRigidBodyValues;
			updateData.updateIKWeightList = ikWights;
			updateData.updateMorphWeightList = morphWeights;
		}

		if( _isMultiThreading ) {
			lock(this) {
				if( _sync_updateData != null ) {
					_sync_updateData2 = updateData; // Hotfix: If PhysicsWorld::_InternalUpdate is very slowly, don't _LockUpdateData() yet.
				} else {
					_sync_updateData = updateData;
				}
			}
		} else {
			if( _sync_updateData != null ) {
				_sync_updateData2 = updateData; // Hotfix: If PhysicsWorld::_InternalUpdate is very slowly, don't _LockUpdateData() yet.
			} else {
				_sync_updateData = updateData;
			}
		}
	}

	public UpdateData LateUpdate()
    {
		UpdateData updateData = null;
		if( _isMultiThreading ) {
			lock(this) {
				updateData = _sync_lateUpdateData;
			}
		} else {
			updateData = _sync_lateUpdateData;
		}

		M4MDebug.Assert( updateData != null );
		return updateData; // Useable updateData until next PhysicsWorld::Update() / PMXModel::PrepareLateUpdate()
	}

	public void _ResetWorldTransformOnMoving()
    {
		if( _boneList != null ) {
			for( int i = 0; i < _boneList.Length; ++i ) {
				_boneList[i].ResetWorldTransformOnMoving();
			}
		}
	}

    public PMXBone GetBone( int boneID )
    {
		unchecked {
	        if( _boneList != null && (uint)boneID < (uint)_boneList.Length ) {
	            return _boneList[boneID];
	        }
		}

        return null;
    }

	public PMXMorph GetMorph( int morphID )
	{
		unchecked {
			if( _morphList != null && (uint)morphID < (uint)_morphList.Length ) {
				return _morphList[morphID];
			}
		}
		
		return null;
	}

    public PMXRigidBody GetRigidBody( int rigidBodyID )
    {
        if( _rigidBodyList != null && (uint)rigidBodyID < (uint)_rigidBodyList.Length ) {
            return _rigidBodyList[rigidBodyID];
        }

        return null;
    }

	// from MMD4MecanimBulletPhysicsWorld
	public override bool _JoinWorld()
	{
		_isJoinedWorld = true;
		_isMultiThreading = (this.physicsWorld != null && this.physicsWorld.isMultiThreading);
		return true;
	}
	
	// from MMD4MecanimBulletPhysicsWorld
	public override void _LeaveWorld()
	{
		if( _isPhysicsEnabled ) {
			if( _jointList != null ) {
				for( int i = 0; i < _jointList.Length; ++i ) {
					_jointList[i].LeaveWorld();
				}
			}
			if( _rigidBodyList != null ) {
				for( int i = 0; i < _rigidBodyList.Length; ++i ) {
					_rigidBodyList[i].LeaveWorld();
				}
			}
		}

		_isJoinedWorld = false;
		_isJoinedWorldInternal = false;
		_isMultiThreading = false;
		
		if( _isDestroyed ) {
			_DestroyImmediate();
		}
	}

	// from MMD4MecanimBulletPhysicsWorld
	public override float _GetResetWorldTime()
	{
		if( !_isPhysicsEnabled || !_isResetWorld ) {
			return 0.0f;
		}

		return _resetMorphTime + _resetWaitTime;
	}
	
	// from MMD4MecanimBulletPhysicsWorld
	public override void _PreResetWorld()
	{
		if( !_isPhysicsEnabled || !_isResetWorld ) {
			return;
		}

		if( _isMultiThreading ) {
			lock(this) {
				_updateData = _sync_updateData;
			}
		} else {
			_updateData = _sync_updateData;
		}

		_FeedbackUpdateData();
		_PerformTransform();
		_PerformTransformAfterPhysics();
		_PrepareMoveWorldTransform();
		
		_ProcessJoinWorld();
		
		_processResetWorld = true;
		_processResetRatio = 0.0f;
		_isResetWorld = false;
	}
	
	// from MMD4MecanimBulletPhysicsWorld
	public override void _StepResetWorld( float elapsedTime )
	{
		if( !_isPhysicsEnabled || !_processResetWorld ) {
			return;
		}
		
		if( elapsedTime < _resetMorphTime ) {
			if( elapsedTime > 0.0f && _resetMorphTime > 0.0f ) {
				_processResetRatio = elapsedTime / _resetMorphTime;
				_ResetWorldTransformOnMoving();
				_PerformMoveWorldTransform( _processResetRatio );
			}
		} else {
			if( _processResetRatio != 1.0f ) {
				_processResetRatio = 1.0f;
				_ResetWorldTransformOnMoving();
				_PerformMoveWorldTransform( 1.0f );
			}
		}
	}
	
	// from MMD4MecanimBulletPhysicsWorld
	public override void _PostResetWorld()
	{
		if( !_isPhysicsEnabled || !_processResetWorld ) {
			return;
		}

		if( _boneList != null ) {
			for( int i = 0; i < _boneList.Length; ++i ) {
				_boneList[i].CleanupMoveWorldTransform();
			}
		}
		
		_processResetWorld = false;
		_processResetRatio = 0.0f;
	}

	// from MMD4MecanimBulletPhysicsWorld
	public override void _PreUpdate()
	{
		if( !_processResetWorld ) {
			_LockUpdateData();
			_FeedbackUpdateData();
			_PerformTransform();
		}
	}

	// from MMD4MecanimBulletPhysicsWorld
	public override void _PostUpdate()
	{
		if( !_processResetWorld ) {
			_PerformTransformAfterPhysics();
			_ProcessXDEF();
			_FeedbackLateUpdateData();
			_UnlockUpdateData();
		}
		
		_isResetWorld = false;
	}
	
	// from MMD4MecanimBulletPhysicsWorld
	public override void _PreUpdateWorld( float deltaTime )
	{
		if( _isPhysicsEnabled ) {
			_ProcessJoinWorld();

			if( _rigidBodyList != null ) {
				for( int i = 0; i < _rigidBodyList.Length; ++i ) {
					_rigidBodyList[i].FeedbackBoneToBodyTransform();
				}
			}
			
			if( _simulatedRigidBodyList != null ) {
				for( int i = 0; i < _simulatedRigidBodyList.Length; ++i ) {
					_simulatedRigidBodyList[i].ProcessPreBoneAlignment();
				}
			}
		}
	}

	// from MMD4MecanimBulletPhysicsWorld
	public override void _PostUpdateWorld( float deltaTime )
	{
		if( _isPhysicsEnabled && deltaTime > 0.0f ) {
			if( _rigidBodyList != null ) {
				for( int i = 0; i < _rigidBodyList.Length; ++i ) {
					_rigidBodyList[i].PrepareTransform();
				}
			}
			if( _simulatedRigidBodyList != null ) {
				for( int i = 0; i < _simulatedRigidBodyList.Length; ++i ) {
					_simulatedRigidBodyList[i].ApplyTransformToBone( deltaTime );
				}
			}

			if( !_optimizeBulletXNA ) {
				if( _modelProperty != null && _modelProperty.rigidBodyIsAdditionalCollider ) {
					DiscreteDynamicsWorld bulletWorld = this.bulletWorld;
					if( bulletWorld != null && bulletWorld.GetDispatcher() != null ) {
						IDispatcher dispatcher = bulletWorld.GetDispatcher();
						int manifolds = dispatcher.GetNumManifolds();
						for( int i = 0; i < manifolds; ++i ) {
							var manifold = dispatcher.GetManifoldByIndexInternal( i );
							RigidBody rigidBody0 = manifold.GetBody0() as RigidBody;
							RigidBody rigidBody1 = manifold.GetBody1() as RigidBody;
							if( rigidBody0 != null && rigidBody1 != null ) {
								MMD4MecanimBulletPMXRigidBody pmxRigidBody0 = rigidBody0.GetUserPointer() as MMD4MecanimBulletPMXRigidBody;
								MMD4MecanimBulletPMXRigidBody pmxRigidBody1 = rigidBody1.GetUserPointer() as MMD4MecanimBulletPMXRigidBody;
								if( pmxRigidBody0 != null && pmxRigidBody1 != null ) {
									pmxRigidBody0.ProcessCollider( pmxRigidBody1 );
								}
							}
						}
					}
				}

				if( _simulatedRigidBodyList != null ) {
					for( int i = 0; i < _simulatedRigidBodyList.Length; ++i ) {
						_simulatedRigidBodyList[i].FeedbackTransform();
					}
					for( int i = 0; i < _simulatedRigidBodyList.Length; ++i ) {
						_simulatedRigidBodyList[i].AntiJitterTransform();
					}
				}
			}
		}
	}

	// from MMD4MecanimBulletPhysicsWorld
	public override void _NoUpdateWorld()
	{
		if( _isPhysicsEnabled ) {
			_ProcessJoinWorld();

			if( _rigidBodyList != null ) {
				for( int i = 0; i < _rigidBodyList.Length; ++i ) {
					_rigidBodyList[i].FeedbackBoneToBodyTransform();
				}
			}

			if( _simulatedRigidBodyList != null ) {
				for( int i = 0; i < _simulatedRigidBodyList.Length; ++i ) {
					_simulatedRigidBodyList[i].ProcessPostBoneAlignment();
				}
			}
		}
	}

	// from MMD4MecanimBulletPhysicsWorld
	public override void _PrepareLateUpdate()
	{
		if( _isMultiThreading ) {
			lock(this) {
				_sync_unusedData = _sync_lateUpdateData; // Recycled.
				_sync_lateUpdateData = _sync_updatedData;
				_sync_updatedData = null;
			}
		} else {
			_sync_unusedData = _sync_lateUpdateData; // Recycled.
			_sync_lateUpdateData = _sync_updatedData;
			_sync_updatedData = null;
		}
	}

	void _ProcessJoinWorld()
	{
		if( !_isPhysicsEnabled ) {
			return;
		}

		if( _isJoinedWorldInternal == false ) {
			_isJoinedWorldInternal = true;
			if( _rigidBodyList != null ) {
				for( int i = 0; i < _rigidBodyList.Length; ++i ) {
					_rigidBodyList[i].JoinWorld();
				}
			}
			if( _jointList != null ) {
				for( int i = 0; i < _jointList.Length; ++i ) {
					_jointList[i].JoinWorld();
				}
			}
		}
	}

	//--------------------------------------------------------------------------------------------------------------------

	void _MakeSimulatedRigidBodyList()
	{
		if( _rigidBodyList != null ) {
			int simulatedRigidBodyLength = 0;
			for( int i = 0; i < _rigidBodyList.Length; ++i ) {
				if( _rigidBodyList[i].bone != null && _rigidBodyList[i].isSimulated ) {
					++simulatedRigidBodyLength;
				}
			}
			
			_simulatedRigidBodyList = new PMXRigidBody[simulatedRigidBodyLength];
			for( int i = 0, j = 0; i < _rigidBodyList.Length; ++i ) {
				if( _rigidBodyList[i].bone != null && _rigidBodyList[i].isSimulated ) {
					_simulatedRigidBodyList[j] = _rigidBodyList[i];
					++j;
				}
			}
			
			for( int i = 0; i + 1 < _simulatedRigidBodyList.Length; ++i ) {
				if( _simulatedRigidBodyList[i].originalParentBoneID < 0 ) {
					continue;
				}
				for( int j = i + 1; j < _simulatedRigidBodyList.Length; ++j ) {
					if( _simulatedRigidBodyList[j].originalParentBoneID < 0 ) {
						_Swap( ref _simulatedRigidBodyList[i], ref _simulatedRigidBodyList[j] );
						break;
					} else {
						if( _IsParentBone( _simulatedRigidBodyList[j].bone, _simulatedRigidBodyList[i].bone ) ) {
							_Swap( ref _simulatedRigidBodyList[i], ref _simulatedRigidBodyList[j] );
						}
					}
				}
			}
		}
	}

	void _PrepareMoveWorldTransform()
	{
		if( !_isPhysicsEnabled ) {
			return;
		}
		if( _boneList != null ) {
			for( int i = 0; i < _boneList.Length; ++i ) {
				bool isMovingOnResetWorld = _boneList[i]._isKinematicRigidBody || _boneList[i].isRigidBodyFreezed;
				_boneList[i].FeedbackMoveWorldTransform( isMovingOnResetWorld );
			}
			for( int i = 0; i < _boneList.Length; ++i ) {
				_boneList[i].PrepareMoveWorldTransform();
			}
		}
	}

	void _PerformMoveWorldTransform( float r )
	{
		if( !_isPhysicsEnabled ) {
			return;
		}

		if( _boneList != null ) {
			for( int i = 0; i < _boneList.Length; ++i ) {
				_boneList[i].PerformMoveWorldTransform( r );
			}
		}
	}

	void _PerformTransform()
	{
		if( _sortedBoneList != null ) {
			for( int i = 0; i < _sortedBoneList.Length; ++i ) {
				_sortedBoneList[i]._PrepareTransform();
			}
			for( int i = 0; i < _sortedBoneList.Length; ++i ) {
				_sortedBoneList[i]._PerformTransform();
			}
			for( int i = 0; i < _sortedBoneList.Length; ++i ) {
				_sortedBoneList[i]._PerformTransform2();
			}
		}

		_PerformTransform_SolveIK( false );
		
		// Prefix Physics
		if( _isPhysicsEnabled ) {
			for( int i = 0; i < _sortedBoneList.Length; ++i ) {
				_sortedBoneList[i].PrepareUpdate2();
			}
		}
	}

	void _PerformTransformAfterPhysics()
	{
		// Postfix Physics
		if( _isPhysicsEnabled ) {
			if( _sortedBoneList != null ) {
				for( int i = 0; i < _sortedBoneList.Length; ++i ) {
					_sortedBoneList[i]._PrepareTransform2( true );
				}
				for( int i = 0; i < _sortedBoneList.Length; ++i ) {
					_sortedBoneList[i]._PerformTransform();
				}
				for( int i = 0; i < _sortedBoneList.Length; ++i ) {
					_sortedBoneList[i]._PerformTransform2();
				}
			}
		}
		
		_PerformTransform_SolveIK( true );
	}

	void _PerformTransform_SolveIK( bool isAfterPhysics )
	{
		if( _sortedBoneList == null || _ikList == null ) {
			return;
		}

		if( _isIKEnabled ) {
			if( _fileType == PMXFileType.PMD ) {
				if( isAfterPhysics ) {
					return;
				}

				for( int i = 0; i < _sortedBoneList.Length; ++i ) {
					_sortedBoneList[i].PrepareUpdate2();
				}
				for( int i = 0; i < _ikList.Length; ++i ) {
					_ikList[i].Solve();
				}
				for( int i = 0; i < _sortedBoneList.Length; ++i ) {
					_sortedBoneList[i]._PrepareTransform2( isAfterPhysics );
				}
				for( int i = 0; i < _sortedBoneList.Length; ++i ) {
					_sortedBoneList[i]._PerformTransform();
				}
				for( int i = 0; i < _sortedBoneList.Length; ++i ) {
					_sortedBoneList[i]._PerformTransform2();
				}
			} else if( _fileType == PMXFileType.PMX ) {
				for( int n = 0; n < _ikList.Length; ++n ) {
					if( !_ikList[n].isDisabled && _ikList[n].ikWeight != 0.0f ) {
						if( _ikList[n].isTransformAfterPhysics == isAfterPhysics ) {
							for( int i = 0; i < _sortedBoneList.Length; ++i ) {
								_sortedBoneList[i].PrepareUpdate2();
							}
							
							_ikList[n].Solve();
							
							for( int i = 0; i < _sortedBoneList.Length; ++i ) {
								_sortedBoneList[i]._PrepareTransform2( isAfterPhysics );
							}
							for( int i = 0; i < _sortedBoneList.Length; ++i ) {
								_sortedBoneList[i]._PerformTransform();
							}
							for( int i = 0; i < _sortedBoneList.Length; ++i ) {
								_sortedBoneList[i]._PerformTransform2();
							}
						}
					}
				}
			}
		}
	}

	void _ProcessXDEF()
	{
		if( _isXDEFEnabled && _meshList != null ) {
			bool xdefAnything = false;
			for( int i = 0; i != _meshList.Length; ++i ) {
				xdefAnything |= _meshList[i].PrepareXDEF();
			}
			if( xdefAnything ) {
				if( _isVertexMorphEnabled && !_isBlendShapesEnabled ) {
					_WaitVertexMorph();
				}

				IndexedMatrix xdefRootTransformInv = _modelTransform.Inverse();
				IndexedQuaternion xdefRootRotationInv = _modelTransform.GetRotation().Inverse();
				for( int i = 0; i != _meshList.Length; ++i ) {
					_meshList[i].ProcessXDEF( ref xdefRootTransformInv, ref xdefRootRotationInv );
				}
			}
		}
	}

	void _DestroyImmediate()
	{
		if( _isVertexMorphEnabled && !_isBlendShapesEnabled ) {
			_WaitVertexMorph();
		}

		if( _ikList != null ) {
			for( int i = 0; i < _ikList.Length; ++i ) {
				_ikList[i].Destroy();
			}
		}
		if( _jointList != null ) {
			for( int i = 0; i < _jointList.Length; ++i ) {
				_jointList[i].Destroy();
			}
		}
		if( _rigidBodyList != null ) {
			for( int i = 0; i < _rigidBodyList.Length; ++i ) {
				_rigidBodyList[i].Destroy();
			}
		}
		if( _boneList != null ) {
			for( int i = 0; i < _boneList.Length; ++i ) {
				_boneList[i].Destroy();
			}
		}
		
		_simulatedRigidBodyList = null;
		_sortedBoneList = null;
		
		_ikList = null;
		_jointList = null;
		_rigidBodyList = null;
		_boneList = null;
		_rootBone = null;
	}

	void _LockUpdateData()
	{
		if( _isMultiThreading ) {
			lock(this) {
				_updateData = _sync_updateData;
				_sync_updateData = _sync_updateData2; // Hotfix: If PhysicsWorld::_InternalUpdate() is very slowly, PMXModel::Update() called two times.
				_sync_updateData2 = null;
			}
		} else {
			_updateData = _sync_updateData;
			_sync_updateData = _sync_updateData2; // Hotfix: If PhysicsWorld::_InternalUpdate() is very slowly, PMXModel::Update() called two times.
			_sync_updateData2 = null;
		}
	}

	void _UnlockUpdateData()
	{
		if( _isMultiThreading ) {
			lock(this) {
				_sync_updatedData = _updateData;
			}
		} else {
			_sync_updatedData = _updateData;
		}

		_updateData = null;
	}

	void _GetWorldTransformToBone( ref IndexedMatrix transform, ref Matrix4x4 t )
	{
		if( _isLossyScaleIdentity ) {
			transform._basis.SetValue(
				t.m00, -t.m01, -t.m02,
				-t.m10,  t.m11,  t.m12,
				-t.m20,  t.m21,  t.m22 );
		} else if( _isLossyScaleSquare ) {
			transform._basis.SetValue(
				t.m00 * _lossyScaleInv, -t.m01 * _lossyScaleInv, -t.m02 * _lossyScaleInv,
				-t.m10 * _lossyScaleInv,  t.m11 * _lossyScaleInv,  t.m12 * _lossyScaleInv,
				-t.m20 * _lossyScaleInv,  t.m21 * _lossyScaleInv,  t.m22 * _lossyScaleInv );
		} else {
			float sX = Mathf.Sqrt( t.m00 * t.m00 + t.m01 * t.m01 + t.m02 * t.m02 );
			float sY = Mathf.Sqrt( t.m10 * t.m10 + t.m11 * t.m11 + t.m12 * t.m12 );
			float sZ = Mathf.Sqrt( t.m20 * t.m20 + t.m21 * t.m21 + t.m22 * t.m22 );
			sX = ( Mathf.Abs( sX ) > Mathf.Epsilon ) ? ( 1.0f / sX ) : 0.0f;
			sY = ( Mathf.Abs( sY ) > Mathf.Epsilon ) ? ( 1.0f / sY ) : 0.0f;
			sZ = ( Mathf.Abs( sZ ) > Mathf.Epsilon ) ? ( 1.0f / sZ ) : 0.0f;
			transform._basis.SetValue(
				t.m00 * sX, -t.m01 * sX, -t.m02 * sX,
				-t.m10 * sY,  t.m11 * sY,  t.m12 * sY,
				-t.m20 * sZ,  t.m21 * sZ,  t.m22 * sZ );
		}
		
		transform._origin = new IndexedVector3( -t.m03, t.m13, t.m23 ) * _worldToBulletScale;
	}

	void _FeedbackUpdateData()
	{
		unchecked {
			if( _updateData == null || _boneList == null ) {
				return;
			}

			bool isMultiThreading = _isMultiThreading;

			int boneListLength = _boneList.Length;

			_isIKEnabled			= (_updateData.updateFlags & (uint)UpdateFlags.IKEnabled) != 0;
			_isBoneInherenceEnabled	= (_updateData.updateFlags & (uint)UpdateFlags.BoneInherenceEnabled) != 0;
			_isBoneMorphEnabled		= (_updateData.updateFlags & (uint)UpdateFlags.BoneMorphEnabled) != 0;

			_pphShoulderFixRate2 = _pphShoulderFixRate;
			_pphShoulderFixRate = _updateData.updatePPHShoulderFixRate;

			_GetWorldTransformToBone( ref _modelTransform, ref _updateData.updateModelTransform );

			if( _updateData.updateBoneFlagsList			!= null && _updateData.updateBoneFlagsList.Length			== boneListLength &&
				_updateData.updateBoneTransformList		!= null && _updateData.updateBoneTransformList.Length		== boneListLength &&
				_updateData.updateBoneLocalPositionList	!= null && _updateData.updateBoneLocalPositionList.Length	== boneListLength &&
				_updateData.updateBoneLocalRotationList	!= null && _updateData.updateBoneLocalRotationList.Length	== boneListLength ) {

				for( int i = 0; i != boneListLength; ++i ) {
					uint updateBoneFlags = (uint)_updateData.updateBoneFlagsList[i];
					_boneList[i].PrepareUpdate( updateBoneFlags );

					if( !isMultiThreading ) {
						if( (updateBoneFlags & (uint)UpdateBoneFlags.WorldTransform) != 0 ) {
							_GetWorldTransformToBone( ref _boneList[i].worldTransform, ref _updateData.updateBoneTransformList[i] );
							_boneList[i].isSetWorldTransform = true;
						}
						if( (updateBoneFlags & (uint)UpdateBoneFlags.Position) != 0 ) {
							_boneList[i].modifiedPosition = _updateData.updateBoneLocalPositionList[i];
							_boneList[i].isSetModifiedPosition = true;
						}
						if( (updateBoneFlags & (uint)UpdateBoneFlags.Rotation) != 0 ) {
							_boneList[i].modifiedRotation = _updateData.updateBoneLocalRotationList[i];
							_boneList[i].isSetModifiedRotation = true;
						}
					} else {
						if( (updateBoneFlags & (uint)UpdateBoneFlags.Position) != 0 && (updateBoneFlags & (uint)UpdateBoneFlags.ChangedPosition) != 0 ) {
							_boneList[i].modifiedPosition = _updateData.updateBoneLocalPositionList[i];
							_boneList[i].isSetModifiedPosition = true;
						}
						if( (updateBoneFlags & (uint)UpdateBoneFlags.Rotation) != 0 && (updateBoneFlags & (uint)UpdateBoneFlags.ChangedRotation) != 0 ) {
							_boneList[i].modifiedRotation = _updateData.updateBoneLocalRotationList[i];
							_boneList[i].isSetModifiedRotation = true;
						}
					}
				}
			}

			if( isMultiThreading ) {
				for( int i = 0; i != boneListLength; ++i ) {
					_boneList[i].PerformWorldTransform();
				}
			}

			if( _updateData.updateBoneUserPositionList != null && _updateData.updateBoneUserPositionList.Length == boneListLength &&
				_updateData.updateBoneUserRotationList != null && _updateData.updateBoneUserRotationList.Length == boneListLength ) {
				for( int i = 0; i < boneListLength; ++i ) {
					// Convert transform LeftHand to RightHand
					float convertScale = ((_boneList[i].modifiedParentBone != null) ? _localToBulletScale : _worldToBulletScale);
					_boneList[i].userPosition = _updateData.updateBoneUserPositionList[i] * convertScale;
					_boneList[i].userRotation = _updateData.updateBoneUserRotationList[i];
					_boneList[i].userPosition.v.X = -_boneList[i].userPosition.v.X;
					_boneList[i].userRotation.q.Y = -_boneList[i].userRotation.q.Y;
					_boneList[i].userRotation.q.Z = -_boneList[i].userRotation.q.Z;
				}
			}

			if( _updateData.updateRigidBodyFlagsList != null && _rigidBodyList != null &&
			    _updateData.updateRigidBodyFlagsList.Length == _rigidBodyList.Length ) {
				for( int i = 0; i < _rigidBodyList.Length; ++i ) {
					bool isFreezed = (_updateData.updateRigidBodyFlagsList[i] & (uint)UpdateRigidBodyFlags.Freezed) != 0;
					_rigidBodyList[i].SetFreezed( isFreezed );
				}
			}

			if( _updateData.updateIKWeightList != null && _ikList != null &&
			    _updateData.updateIKWeightList.Length == _ikList.Length ) {
				for( int i = 0; i < _ikList.Length; ++i ) {
					_ikList[i].MarkIKDepended( _updateData.updateFlags, _updateData.updateIKWeightList[i] );
				}
			}

			if( _isVertexMorphEnabled && !_isBlendShapesEnabled ) {
				_WaitVertexMorph();
			}
			
			if( _meshList != null ) {
				for( int i = 0; i != _meshList.Length; ++i ) {
					_meshList[i].PrepareUpdate();
				}
			}

			if( _updateData.updateMorphWeightList != null && _morphList != null &&
			    _updateData.updateMorphWeightList.Length == _morphList.Length ) {
				for( int i = 0; i != _morphList.Length; ++i ) {
					_morphList[i]._backupWeight = _morphList[i].weight + _morphList[i].appendWeight; // Optimized.
					_morphList[i].weight = _updateData.updateMorphWeightList[i];
					_morphList[i].appendWeight = 0.0f;
				}
				for( int i = 0; i != _morphList.Length; ++i ) {
					_morphList[i].ApplyGroupMorph();
				}
				for( int i = 0; i != _morphList.Length; ++i ) {
					_morphList[i].ApplyMorph();
				}
			}

			if( _meshList != null && _isVertexMorphEnabled && !_isBlendShapesEnabled ) {
				bool vertexMorphProcessing = false;
				for( int i = 0; i != _meshList.Length && !vertexMorphProcessing; ++i ) {
					vertexMorphProcessing |= _meshList[i].isMorphChanged;
				}
				if( vertexMorphProcessing ) {
					// Recycle cache.
					if( _updateData.lateUpdateMeshDataList != null && _updateData.lateUpdateMeshDataList.Length == _meshList.Length ) {
						for( int i = 0; i != _meshList.Length; ++i ) {
							if( _updateData.lateUpdateMeshDataList[i] != null ) {
								_meshList[i].vertices = _updateData.lateUpdateMeshDataList[i].vertices;
							}
						}
					}

					_RunVertexMorph();
				}
			}
		}
	}

	void _FeedbackLateUpdateData()
	{
		unchecked {
			if( _updateData == null || _boneList == null || _sortedBoneList == null ) {
				return;
			}
			
			for( int i = 0; i != _sortedBoneList.Length; ++i ) {
				_sortedBoneList[i].PrepareLateUpdate();
			}
			
			bool isMultiThreading = _isMultiThreading;

			_updateData.lateUpdateFlags = (int)LateUpdateFlags.Bone;

			int boneListLength = _boneList.Length;
			if( _updateData.lateUpdateBoneFlagsList == null || _updateData.lateUpdateBoneFlagsList.Length != boneListLength ) {
				_updateData.lateUpdateBoneFlagsList = new int[boneListLength];
			} else {
				for( int i = 0; i != boneListLength; ++i ) {
					_updateData.lateUpdateBoneFlagsList[i] = 0;
				}
			}
			if( _updateData.lateUpdateBonePositionList == null || _updateData.lateUpdateBonePositionList.Length != boneListLength ) {
				_updateData.lateUpdateBonePositionList = new Vector3[boneListLength];
			}
			if( _updateData.lateUpdateBoneRotationList == null || _updateData.lateUpdateBoneRotationList.Length != boneListLength ) {
				_updateData.lateUpdateBoneRotationList = new Quaternion[boneListLength];
			}

			for( int i = 0; i != boneListLength; ++i ) {
				PMXBone bone = _boneList[i];
				if( isMultiThreading
				   || bone.isLateUpdatePositionSelf || bone.isLateUpdatePosition
				   || bone.isLateUpdateRotationSelf || bone.isLateUpdateRotation ) {
					_updateData.lateUpdateBoneFlagsList[i] = (int)LateUpdateBoneFlags.LateUpdated;

					if( isMultiThreading || bone.isLateUpdatePositionSelf || bone.isLateUpdatePosition ) {
						_updateData.lateUpdateBoneFlagsList[i] |= (int)LateUpdateBoneFlags.Position;
					}
					if( isMultiThreading || bone.isLateUpdateRotationSelf || bone.isLateUpdateRotation ) {
						_updateData.lateUpdateBoneFlagsList[i] |= (int)LateUpdateBoneFlags.Rotation;
					}

					if( (_updateData.lateUpdateBoneFlagsList[i] & (int)LateUpdateBoneFlags.Position) != 0 ) {
						bone.ComputeModifiedPosition();
						_updateData.lateUpdateBonePositionList[i] = bone.modifiedPosition;
					}
					if( (_updateData.lateUpdateBoneFlagsList[i] & (int)LateUpdateBoneFlags.Rotation) != 0 ) {
						bone.ComputeModifiedRotation();
						_updateData.lateUpdateBoneRotationList[i] = bone.modifiedRotation;
					}
				}
			}

			if( _isVertexMorphEnabled && !_isBlendShapesEnabled ) {
				_WaitVertexMorph();
			}
			{
				bool updatedMesh = false;
				if( _meshList != null ) {
					if( _updateData.lateUpdateMeshFlagsList == null || _updateData.lateUpdateMeshFlagsList.Length != _meshList.Length ) {
						_updateData.lateUpdateMeshFlagsList = new int[_meshList.Length];
					}

					for( int i = 0; i != _meshList.Length; ++i ) {
						_updateData.lateUpdateMeshFlagsList[i] = 0;
					}

					for( int i = 0; i != _meshList.Length; ++i ) {
						if( _meshList[i].isChanged ) {
							_updateData.lateUpdateMeshFlagsList[i] = (int)LateUpdateMeshFlags.Vertices;
							updatedMesh = true;
						}
					}
				} else {
					_updateData.lateUpdateMeshFlagsList = null;
				}

				if( updatedMesh ) { // todo: Support Normal for XDEF
					_updateData.lateUpdateFlags |= (uint)LateUpdateFlags.Mesh;
					if( _updateData.lateUpdateMeshDataList == null || _updateData.lateUpdateMeshDataList.Length != _meshList.Length ) {
						_updateData.lateUpdateMeshDataList = new LateUpdateMeshData[_meshList.Length];
					}
					LateUpdateMeshData[] lateUpdateMeshDataList = _updateData.lateUpdateMeshDataList;

					for( int i = 0; i != _meshList.Length; ++i ) {
						if( lateUpdateMeshDataList[i] == null ) {
							lateUpdateMeshDataList[i] = new LateUpdateMeshData();
						}
						LateUpdateMeshData lateUpdateMeshData = lateUpdateMeshDataList[i];
						if( _meshList[i].isChanged ) {
							lateUpdateMeshData.lateUpdateMeshFlags = (uint)LateUpdateMeshFlags.Vertices;
							lateUpdateMeshData.vertices = _meshList[i].vertices;
							_meshList[i].vertices = null;
						} else {
							lateUpdateMeshData.lateUpdateMeshFlags = 0;
						}
					}
				}
			}
		}
	}

	void _RunVertexMorph()
	{
		#if USE_SINGLETHREAD_MORPH
		_VertexMorph();
		#else
		if( _isVertexMorphEnabled && !_isBlendShapesEnabled ) {
			_WaitVertexMorph();
			#if USE_CACHEDTHREAD
			_cachedThread.Invoke( _VertexMorph );
			#else
			M4MDebug.Assert( _vertexMorphThreadQueue != null );
			if( _vertexMorphThreadQueue != null ) {
				_vertexMorphThreadQueueHandle = _vertexMorphThreadQueue.Invoke( _VertexMorph );
			}
			#endif
		}
		#endif
	}
	
	void _WaitVertexMorph()
	{
		#if USE_SINGLETHREAD_MORPH
		#else
		if( _isVertexMorphEnabled && !_isBlendShapesEnabled ) {
			#if USE_CACHEDTHREAD
			_cachedThread.WaitEnd();
			#else
			M4MDebug.Assert( _vertexMorphThreadQueue != null );
			if( _vertexMorphThreadQueue != null ) {
				_vertexMorphThreadQueue.WaitEnd( ref _vertexMorphThreadQueueHandle );
			}
			#endif
		}
		#endif
	}
	
	public void _VertexMorph()
	{
		if( _meshList != null ) {
			for( int i = 0; i != _meshList.Length; ++i ) {
				if( _meshList[i].isMorphChanged ) {
					_meshList[i].PrepareMorph();
				}
			}
			for( int i = 0; i != _morphList.Length; ++i ) {
				_morphList[i].ProcessVertexMorph();
			}
		}
	}

}
