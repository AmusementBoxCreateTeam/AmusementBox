using UnityEngine;
using System.Collections;
using BulletXNA;
using BulletXNA.BulletCollision;
using BulletXNA.BulletDynamics;
using BulletXNA.LinearMath;
using MMD4Mecanim;

using MeshFlags			= MMD4MecanimData.MeshFlags;
using UpdateFlags		= MMD4MecanimBulletPhysics.MMDModel.UpdateFlags;

using IndexData			= MMD4MecanimAuxData.IndexData;

using PMXModel          = MMD4MecanimBulletPMXModel;
using PMXBone           = MMD4MecanimBulletPMXBone;
using PMXMorph			= MMD4MecanimBulletPMXMorph;
using PMXMesh			= MMD4MecanimBulletPMXMesh;

using PMXMorphType = MMD4MecanimBulletPMXCommon.PMXMorphType;
using PMXMorphCategory = MMD4MecanimBulletPMXCommon.PMXMorphCategory;
using PMXMorphMaterialOperation = MMD4MecanimBulletPMXCommon.PMXMorphMaterialOperation;

using FastVector3		= MMD4MecanimBulletPhysicsUtil.FastVector3;
using FastQuaternion	= MMD4MecanimBulletPhysicsUtil.FastQuaternion;

public class MMD4MecanimBulletPMXMorph
{
	public const int MeshCountBitShift = IndexData.MeshCountBitShift;
	public const int VertexIndexBitMask = IndexData.VertexIndexBitMask;
	public const int HeaderSize = IndexData.HeaderSize;

	public PMXModel _model;
	
	public PMXModel model { get { return _model; } }

	public float			preUpdate_weight = 0.0f;
	public float			preUpdate_appendWeight = 0.0f;
	public float			weight = 0.0f;
	public float			appendWeight = 0.0f;
	public float			_backupWeight = 0.0f;
	int						_precheckedDependMesh = -1;

	uint					_additionalFlags = 0;
	PMXMorphCategory		_morphCategory = (PMXMorphCategory)0;
	PMXMorphType			_morphType = (PMXMorphType)0;
	uint					_indexCount = 0;
	uint[]					_indices = null;
	float[]					_weights = null;
	Vector3[]				_vertices = null;
	IndexedVector3[]		_bonePositions = null;
	IndexedQuaternion[]		_boneRotations = null;
	bool[]					_bonePositionsIsZero = null;
	bool[]					_boneRotationsIsIdentity = null;
	PMXBone[]				_bones = null;
	PMXMorph[]				_morphs = null;
	bool[]					_dependMesh = null;

	public bool isMorphBaseVertex { get { return (_additionalFlags & 0x01) != 0; } }
	public PMXMorphCategory morphCategory { get { return _morphCategory; } }

	public bool Import( MMD4MecanimCommon.BinaryReader binaryReader )
	{
		unchecked {
			if( !binaryReader.BeginStruct() ) {
				return false;
			}

			_additionalFlags = (uint)binaryReader.ReadStructInt();
			binaryReader.ReadStructInt(); // nameJp
			binaryReader.ReadStructInt(); // nameEn
			_morphCategory = (PMXMorphCategory)binaryReader.ReadStructInt();
			_morphType = (PMXMorphType)binaryReader.ReadStructInt();
			_indexCount = (uint)binaryReader.ReadStructInt();
			
			switch( _morphType ) {
			case PMXMorphType.Vertex:
				_indices = new uint[_indexCount];
				_vertices = new Vector3[_indexCount];
				for( uint i = 0; i != _indexCount; ++i ) {
					_indices[i] = (uint)binaryReader.ReadInt();
					_vertices[i] = binaryReader.ReadVector3();
					if( _indices[i] >= _model.vertexCount ) {
						return false;
					}
				}
				if( Mathf.Abs( _model.importScaleMMDModel - _model.modelProperty.importScale ) > Mathf.Epsilon ) {
					if( _model.importScaleMMDModel > Mathf.Epsilon ) {
						float postfixScale = _model.modelProperty.importScale / _model.importScaleMMDModel;
						for( int i = 0; i != _indexCount; ++i ) {
							_vertices[i] *= postfixScale;
						}
					}
				}
				break;
			case PMXMorphType.Group:
				_morphs = new PMXMorph[ _indexCount ];
				_weights = new float[ _indexCount ];
				for( uint i = 0; i != _indexCount; ++i ) {
					int morphID = binaryReader.ReadInt();
					_weights[i] = binaryReader.ReadFloat();
					_morphs[i] = _model.GetMorph( morphID );
				}
				break;
			case PMXMorphType.Bone:
				_bones = new PMXBone[ _indexCount ];
				_bonePositions = new IndexedVector3[ _indexCount ];
				_boneRotations = new IndexedQuaternion[ _indexCount ];
				_bonePositionsIsZero = new bool[ _indexCount ];
				_boneRotationsIsIdentity = new bool[ _indexCount ];
				for( uint i = 0; i != _indexCount; ++i ) {
					int boneID = binaryReader.ReadInt();
					_bonePositions[i] = binaryReader.ReadVector3();
					_boneRotations[i] = binaryReader.ReadQuaternion();
					_bonePositionsIsZero[i] = (_bonePositions[i] == IndexedVector3.Zero);
					_boneRotationsIsIdentity[i] = (_boneRotations[i] == IndexedQuaternion.Identity);
					
					_bonePositions[i].Z = -_bonePositions[i].Z; // LeftHand to RightHand
					_boneRotations[i].X = -_boneRotations[i].X; // LeftHand to RightHand
					_boneRotations[i].Y = -_boneRotations[i].Y; // LeftHand to RightHand

					_bonePositions[i] *= _model.modelToBulletScale;

					_bones[i] = _model.GetBone( boneID );
				}
				break;
			case PMXMorphType.Material:
				break;
			}
			
			if( !binaryReader.EndStruct() ) {
				return false;
			}
		}
		
		return true;
	}

	public bool PrepareDependMesh()
	{
		unchecked {
			if( _morphType == PMXMorphType.Vertex ) {
				PMXMesh[] meshList = _model.meshList;
				IndexData indexData = _model.indexData;
				if( indexData != null ) {
					if( meshList.Length == indexData.meshCount ) {
						uint meshLength = (uint)meshList.Length;
						_dependMesh = new bool[meshLength];
						int[] indexValues = indexData.indexValues;
						int indexValuesLength = indexValues.Length;
						int indexLength = _indices.Length;
						for( int i = 0; i != indexLength; ++i ) {
							// GetVertexOffset() performance efficient.
							uint index = _indices[i];
							if( (HeaderSize + (uint)index + 1u) < (uint)indexValuesLength ) {
								uint vertexOffset = (uint)indexValues[HeaderSize + (uint)index + 0u];
								uint vertexOffsetEnd = (uint)indexValues[HeaderSize + (uint)index + 1u];
								if( vertexOffset < (uint)indexValuesLength && vertexOffsetEnd < (uint)indexValuesLength ) {
									for( ; vertexOffset != vertexOffsetEnd; ++vertexOffset ) {
										uint meshIndex = (uint)indexValues[vertexOffset] >> MeshCountBitShift;
										if( meshIndex < meshLength && meshList[meshIndex] != null ) {
											_dependMesh[meshIndex] = true;
											meshList[meshIndex].meshFlags |= (uint)MeshFlags.VertexMorph;
										} else {
											M4MDebug.LogError( "MeshIndex out of range." );
											_dependMesh = null;
											return false;
										}
									}
								}
							} else {
								M4MDebug.LogError( "Index out of range." );
								_dependMesh = null;
								return false;
							}
						}
					}
				}
			}

			return true;
		}
	}

	public void PreUpdate_ApplyGroupMorph()
	{
		if( _morphType == PMXMorphType.Group ) {
			if( _morphs == null || _weights == null ) {
				return;
			}

			if( Mathf.Abs( this.preUpdate_weight - 1.0f ) <= Mathf.Epsilon ) {
				for( int i = 0; i != _morphs.Length; ++i ) {
					if( _morphs[i] != null ) {
						_morphs[i].preUpdate_appendWeight += _weights[i];
					}
				}
			} else {
				for( int i = 0; i != _morphs.Length; ++i ) {
					if( _morphs[i] != null ) {
						_morphs[i].preUpdate_appendWeight += _weights[i] * this.preUpdate_weight;
					}
				}
			}
		}
	}

	public void PreUpdate_ApplyMorph( uint updateFlags )
	{
		if( this.isMorphBaseVertex ) {
			return;
		}

		// for Not Base
		float weight = this.preUpdate_weight + this.preUpdate_appendWeight;
		
		switch( _morphType ) {
		case PMXMorphType.Vertex:
			// Nothing yet.
			break;
		case PMXMorphType.Bone:
			unchecked {
				if( (updateFlags & (uint)UpdateFlags.BoneMorphEnabled) == 0
				   || _bones == null
				   || _bonePositionsIsZero == null
				   || _boneRotationsIsIdentity == null ) {
					return;
				}
			}
			
			if( Mathf.Abs( weight ) <= Mathf.Epsilon ) {
				for( int i = 0; i != _bones.Length; ++i ) {
					PMXBone bone = _bones[i];
					if( bone != null ) {
						bone._preUpdate_isMorphPositionDepended = false; // Memo: Reset to zero
						bone._preUpdate_isMorphRotationDepended = false; // Memo: Reset to identity
					}
				}
			} else {
				for( int i = 0; i != _bones.Length; ++i ) {
					PMXBone bone = _bones[i];
					if( bone != null ) {
						if( _bonePositionsIsZero[i] ) {
							bone._preUpdate_isMorphPositionDepended = false; // Memo: Reset to zero
						} else {
							bone._preUpdate_isMorphPositionDepended = true;
						}
						if( _boneRotationsIsIdentity[i] ) {
							bone._preUpdate_isMorphRotationDepended = false; // Memo: Reset to identity
						} else {
							bone._preUpdate_isMorphRotationDepended = true;
						}
					}
				}
			}
			break;
		default:
			break;
		}
	}

	public void PrepareUpdate()
	{
		_backupWeight = this.weight + this.appendWeight;
	}

	public void ApplyGroupMorph()
	{
		if( _morphType == PMXMorphType.Group ) {
			if( _morphs == null || _weights == null ) {
				return;
			}
			
			if( Mathf.Abs( this.weight - 1.0f ) <= Mathf.Epsilon ) {
				for( int i = 0; i != _morphs.Length; ++i ) {
					if( _morphs[i] != null ) {
						_morphs[i].appendWeight += _weights[i];
					}
				}
			} else {
				for( int i = 0; i != _morphs.Length; ++i ) {
					if( _morphs[i] != null ) {
						_morphs[i].appendWeight += _weights[i] * this.weight;
					}
				}
			}
		}
	}

	public void ApplyMorph()
	{
		if( this.isMorphBaseVertex ) {
			return;
		}

		// for Not Base
		float weight = this.weight + this.appendWeight;
		
		switch( _morphType ) {
		case PMXMorphType.Vertex:
			if( weight != _backupWeight ) {
				_MarkChangedDependMesh();
			}
			break;
		case PMXMorphType.Bone:
			if( !_model.isBoneMorphEnabled ||
			    _bones == null ||
			    _bonePositions == null ||
			    _boneRotations == null || 
			    _bonePositionsIsZero == null ||
			    _boneRotationsIsIdentity == null ) {
				return;
			}
			
			if( Mathf.Abs( weight ) <= Mathf.Epsilon ) {
				for( int i = 0; i != _bones.Length; ++i ) {
					PMXBone bone = _bones[i];
					if( bone != null ) {
						bone.morphPosition = FastVector3.Zero; // Memo: Reset to zero
						bone.morphRotation = FastQuaternion.Identity; // Memo: Reset to identity
					}
				}
			} else if( Mathf.Abs( weight - 1.0f ) <= Mathf.Epsilon  ) {
				for( int i = 0; i != _bones.Length; ++i ) {
					PMXBone bone = _bones[i];
					if( bone != null ) {
						if( _bonePositionsIsZero[i] ) {
							bone.morphPosition = FastVector3.Zero; // Memo: Reset to zero
						} else {
							bone.morphPosition = _bonePositions[i];
						}
						if( _boneRotationsIsIdentity[i] ) {
							bone.morphRotation = FastQuaternion.Identity; // Memo: Reset to identity
						} else {
							bone.morphRotation =_boneRotations[i];
						}
					}
				}
			} else {
				for( int i = 0; i != _bones.Length; ++i ) {
					PMXBone bone = _bones[i];
					if( bone != null ) {
						if( _bonePositionsIsZero[i] ) {
							bone.morphPosition = FastVector3.Zero; // Memo: Reset to zero
						} else {
							bone.morphPosition = _bonePositions[i] * weight;
						}
						if( _boneRotationsIsIdentity[i] ) {
							bone.morphRotation = FastQuaternion.Identity; // Memo: Reset to identity
						} else {
							bone.morphRotation = MMD4MecanimBulletPhysicsUtil.SlerpFromIdentity( ref _boneRotations[i], weight );
						}
					}
				}
			}
			break;
		default:
			break;
		}
	}

	public void ProcessVertexMorph()
	{
		if( _morphType == PMXMorphType.Vertex ) {
			if( this.isMorphBaseVertex ) {
				_ProcessVertexBaseMorph();
			} else {
				_ProcessVertexMorph();
			}
		}
	}

	void _MarkChangedDependMesh()
	{
		M4MDebug.Assert( _model != null );
		PMXMesh[] meshList = _model.meshList;
		if( meshList != null && _dependMesh != null && meshList.Length == _dependMesh.Length ) {
			for( int i = 0; i != _dependMesh.Length; ++i ) {
				if( _dependMesh[i] ) {
					meshList[i].isMorphChanged = true;
				}
			}
		}
	}

	bool _PrecheckDependMesh()
	{
		unchecked {
			if( _precheckedDependMesh != -1 ) {
				return _precheckedDependMesh != 0;
			}
			
			_precheckedDependMesh = 0;
			
			M4MDebug.Assert( _model != null );
			PMXMesh[] meshList = _model.meshList;
			IndexData indexData = _model.indexData;

			if( meshList != null && indexData != null && _dependMesh != null ) {
				int meshLength = meshList.Length;
				if( _indices != null && _vertices != null && _indices.Length == _vertices.Length ) {
					int[] indexValues = indexData.indexValues;
					if( indexValues != null ) {
						int indexValuesLength = indexValues.Length;
						int vertexCount = _vertices.Length;
						for( int i = 0; i != vertexCount; ++i ) {
							// GetVertexOffset() performance efficient.
							uint index = _indices[i];
							if( (HeaderSize + (uint)index + 1u) < (uint)indexValuesLength ) {
								uint vertexOffset = (uint)indexValues[HeaderSize + (uint)index + 0u];
								uint vertexOffsetEnd = (uint)indexValues[HeaderSize + (uint)index + 1u];
								if( vertexOffset < (uint)indexValuesLength && vertexOffsetEnd < (uint)indexValuesLength ) {
									for( ; vertexOffset != vertexOffsetEnd; ++vertexOffset ) {
										uint meshIndex = (uint)indexValues[vertexOffset] >> MeshCountBitShift;
										uint realIndex = (uint)indexValues[vertexOffset] & (uint)VertexIndexBitMask;
										if( meshIndex < (uint)meshLength ) {
											if( meshList[meshIndex] != null &&
											    meshList[meshIndex].backupVertices != null &&
											    realIndex < meshList[meshIndex].backupVertices.Length ) {
												// Nothing.
											} else {
												return false;
											}
										} else {
											M4MDebug.LogError( "MeshIndex out of range." );
											return false;
										}
									}
								} else {
									M4MDebug.LogError( "Vertex offset out of range." );
									return false;
								}
							} else {
								M4MDebug.LogError( "Index out of range." );
								return false;
							}
						}
						_precheckedDependMesh = 1;
						return true;
					}
				}
			}

			return false;
		}
	}

	void _ProcessVertexBaseMorph()
	{
		unchecked {
			if( !_PrecheckDependMesh() ) {
				return;
			}

			M4MDebug.Assert( _model != null );
			M4MDebug.Assert( this.isMorphBaseVertex );
			PMXMesh[] meshList = _model.meshList;
			IndexData indexData = _model.indexData;

			if( meshList != null && indexData != null ) {
				M4MDebug.Assert( _indices != null && _vertices != null && _indices.Length == _vertices.Length ); // Precheccked.
				int[] indexValues = indexData.indexValues;
				int vertexCount = _vertices.Length;
				for( int i = 0; i != vertexCount; ++i ) {
					uint index = (uint)_indices[i];

					uint vertexOffset = (uint)indexValues[HeaderSize + (uint)index + 0u];
					uint vertexOffsetEnd = (uint)indexValues[HeaderSize + (uint)index + 1u];
					for( ; vertexOffset != vertexOffsetEnd; ++vertexOffset ) {
						uint meshIndex = (uint)indexValues[vertexOffset] >> MeshCountBitShift;
						uint realIndex = (uint)indexValues[vertexOffset] & (uint)VertexIndexBitMask;
						if( meshList[meshIndex].isMorphChanged ) {
							meshList[meshIndex].vertices[realIndex] += _vertices[i];
						}
					}
				}
			}
		}
	}

	void _ProcessVertexMorph()
	{
		unchecked {
			if( !_PrecheckDependMesh() ) {
				return;
			}
			
			M4MDebug.Assert( _model != null );
			M4MDebug.Assert( !this.isMorphBaseVertex );
			float weight = this.weight + this.appendWeight;
			PMXMesh[] meshList = _model.meshList;
			IndexData indexData = _model.indexData;

			if( meshList != null && indexData != null ) {
				M4MDebug.Assert( _indices != null && _vertices != null && _indices.Length == _vertices.Length ); // Precheccked.
				int[] indexValues = indexData.indexValues;
				if( weight > Mathf.Epsilon ) {
					int vertexCount = _vertices.Length;
					if( Mathf.Abs( 1.0f - weight ) <= Mathf.Epsilon ) {
						for( int i = 0; i != vertexCount; ++i ) {
							uint index = (uint)_indices[i];
							uint vertexOffset = (uint)indexValues[HeaderSize + (uint)index + 0u];
							uint vertexOffsetEnd = (uint)indexValues[HeaderSize + (uint)index + 1u];
							for( ; vertexOffset != vertexOffsetEnd; ++vertexOffset ) {
								uint meshIndex = (uint)indexValues[vertexOffset] >> MeshCountBitShift;
								uint realIndex = (uint)indexValues[vertexOffset] & (uint)VertexIndexBitMask;
								meshList[meshIndex].vertices[realIndex] += _vertices[i];
							}
						}
					} else {
						for( int i = 0; i != vertexCount; ++i ) {
							uint index = (uint)_indices[i];
							uint vertexOffset = (uint)indexValues[HeaderSize + (uint)index + 0u];
							uint vertexOffsetEnd = (uint)indexValues[HeaderSize + (uint)index + 1u];
							for( ; vertexOffset != vertexOffsetEnd; ++vertexOffset ) {
								uint meshIndex = (uint)indexValues[vertexOffset] >> MeshCountBitShift;
								uint realIndex = (uint)indexValues[vertexOffset] & (uint)VertexIndexBitMask;
								meshList[meshIndex].vertices[realIndex] += _vertices[i] * weight;
							}
						}
					}
				}
			}
		}
	}
}
