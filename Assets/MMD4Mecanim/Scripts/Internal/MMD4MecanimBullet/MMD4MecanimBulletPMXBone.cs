using UnityEngine;
using System.Collections;
using BulletXNA;
using BulletXNA.BulletCollision;
using BulletXNA.BulletDynamics;
using BulletXNA.LinearMath;

using PMXFileType       = MMD4MecanimBulletPMXCommon.PMXFileType;
using PMDBoneType       = MMD4MecanimBulletPMXCommon.PMDBoneType;
using PMXBoneFlag		= MMD4MecanimBulletPMXCommon.PMXBoneFlag;
using PMXRigidBodyType  = MMD4MecanimBulletPMXCommon.PMXRigidBodyType;
using PMXModel          = MMD4MecanimBulletPMXModel;
using PMXRigidBody      = MMD4MecanimBulletPMXRigidBody;
using PMXBone           = MMD4MecanimBulletPMXBone;

using UpdateFlags = MMD4MecanimBulletPhysics.MMDModel.UpdateFlags;
using UpdateBoneFlags = MMD4MecanimBulletPhysics.MMDModel.UpdateBoneFlags;
using UpdateRigidBodyFlags = MMD4MecanimBulletPhysics.MMDModel.UpdateRigidBodyFlags;
using LateUpdateBoneFlags = MMD4MecanimBulletPhysics.MMDModel.LateUpdateBoneFlags;

using FastVector3 = MMD4MecanimBulletPhysicsUtil.FastVector3;
using FastQuaternion = MMD4MecanimBulletPhysicsUtil.FastQuaternion;

public class MMD4MecanimBulletPMXBone
{
	public PMXModel _model;

	public PMXModel model { get { return _model; } }

	// Pending: Replace to MMD4MecanimData

	// Base Data
	public int					boneID;
	public int					sortedBoneID;
	public PMDBoneType			pmdBoneType = PMDBoneType.Unknown;
	public PMXBone              modifiedParentBone;
	public PMXBone              originalParentBone;
	public PMXBone				inherenceParentBone;
	public float				inherenceWeight;
	public IndexedVector3		baseOrigin = IndexedVector3.Zero;
	public uint					additionalFlags;
	public PMXBoneFlag			pmxBoneFlags = PMXBoneFlag.None;
	public uint					transformLayerID;
	public int					externalID;
	public string				skeletonName;

	public IndexedVector3		offset = IndexedVector3.Zero;
	public IndexedVector3		modifiedOffset = IndexedVector3.Zero;
	public float				boneLength;
	public bool					isLeft;
	public bool					isRight;
	public bool					isHip;
	public bool					isKnee;
	public bool					isFoot;
	public bool					isRootBone;

	public bool					isSetWorldTransform;
	public bool					isSetLocalPosition;
	public bool					isSetLocalRotation;
	public bool					isSetModifiedPosition;
	public bool					isSetModifiedRotation;
	public bool					isSetModifiedPosition2;
	public bool					isSetModifiedRotation2;
	public bool					isLateUpdatePositionSelf;
	public bool					isLateUpdateRotationSelf;
	public bool					isLateUpdatePosition;
	public bool					isLateUpdateRotation;
	public bool					isSetPPHBoneBasis;
	public float				pphShoulderFixRate;
	public IndexedMatrix		worldTransform = IndexedMatrix.Identity;
	public IndexedVector3		localPosition = IndexedVector3.Zero;
	public IndexedQuaternion	localRotation = IndexedQuaternion.Identity;
	public IndexedQuaternion	localRotationBeforeIK = IndexedQuaternion.Identity;
	public IndexedVector3		modifiedPosition = IndexedVector3.Zero;
	public IndexedQuaternion	modifiedRotation = IndexedQuaternion.Identity;
	public IndexedVector3		modifiedPosition2 = IndexedVector3.Zero;
	public IndexedQuaternion	modifiedRotation2 = IndexedQuaternion.Identity;
	public FastVector3			userPosition = FastVector3.Zero;
	public FastQuaternion		userRotation = FastQuaternion.Identity;
	public FastVector3			userPosition2 = FastVector3.Zero;
	public FastQuaternion		userRotation2 = FastQuaternion.Identity;
	public FastVector3			morphPosition = FastVector3.Zero;
	public FastQuaternion		morphRotation = FastQuaternion.Identity;
	public FastVector3			morphPosition2 = FastVector3.Zero;
	public FastQuaternion		morphRotation2 = FastQuaternion.Identity;
	public FastVector3			externalPosition = FastVector3.Zero;
	public FastQuaternion		externalRotation = FastQuaternion.Identity;
	public IndexedBasisMatrix	pphBoneBasis = IndexedBasisMatrix.Identity;

	bool						_isMovingOnResetWorld;
	IndexedMatrix				_moveWorldTransform;
	IndexedVector3				_moveSourcePosition;
	IndexedVector3				_moveDestPosition;
	IndexedQuaternion			_moveSourceRotation;
	IndexedQuaternion			_moveDestRotation;

	public bool					_isKinematicRigidBody;
	public PMXRigidBody			_simulatedRigidBody;
	
	public bool					isSetPrevWorldTransform;
	public bool					isSetPrevWorldTransform2;
	public IndexedMatrix		prevWorldTransform = IndexedMatrix.Identity;
	public IndexedMatrix		prevWorldTransform2 = IndexedMatrix.Identity;

	uint						_preUpdate_updateFlags;
	uint						_preUpdate_updateFlags2;
	bool						_preUpdate_isCheckUpdated;
	bool						_preUpdate_isCheckUpdated2;
	bool						_preUpdate_isWriteTransform;
	bool						_preUpdate_isWorldTransform;
	bool						_preUpdate_isPosition;
	bool						_preUpdate_isRotation;
	bool						_preUpdate_isCheckPosition;
	bool						_preUpdate_isCheckRotation;
	uint						_preUpdate_updateBoneFlags;
	uint						_preUpdate_updateBoneFlags2;
	bool						_preUpdate_isIKDepended;
	bool						_preUpdate_isIKDestination;
	bool						_preUpdate_isIKWeightEnabled;
	public bool					_preUpdate_isXDEFDepended;
	public bool					_preUpdate_isMorphPositionDepended;
	public bool					_preUpdate_isMorphRotationDepended;
	public bool					_preUpdate_isMorphPositionDepended2;
	public bool					_preUpdate_isMorphRotationDepended2;

	bool						_isIKDepended;
	bool						_isIKDestination;
	bool						_isIKWeightEnabled;
	bool						_isPrepareTransform;
	bool						_isPrepareTransform2;
	bool						_isDirtyLocalPosition;
	bool						_isDirtyLocalRotation;
	bool						_isDirtyWorldTransform;
	bool						_isDirtyLocalPosition2;
	bool						_isDirtyLocalRotation2;
	bool						_isDirtyWorldTransform2;
	bool						_isPerformWorldTransform2;
	bool						_isUpdatedWorldTransform;
	bool						_isUpdatedWorldTransformIK;
	bool						_isUpdatedWorldTransformPhysics;
	
	bool						_underIK;
	bool						_underPhysics;

	uint						_updateBoneFlags;

	public bool isRigidBodyFreezed			{ get { return ( _simulatedRigidBody != null ) && _simulatedRigidBody.isFreezed; } }
	public bool isRigidBodySimulated		{ get { return ( _simulatedRigidBody != null ) && _simulatedRigidBody.isSimulated; } }
	public PMXRigidBodyType rigidBodyType	{ get { return ( _simulatedRigidBody != null ) ? _simulatedRigidBody.rigidBodyType : PMXRigidBodyType.Kinematics; } }

	public bool isBoneTranslate				{ get { return (this.pmxBoneFlags & PMXBoneFlag.Translate) != PMXBoneFlag.None; } }
	public bool isTransformAfterPhysics		{ get { return (this.pmxBoneFlags & PMXBoneFlag.TransformAfterPhysics) != PMXBoneFlag.None; } }

	public bool preUpdate_isWorldTransform	{ get { return _preUpdate_isWorldTransform; } }
	public bool preUpdate_isPosition		{ get { return _preUpdate_isPosition; } }
	public bool preUpdate_isRotation		{ get { return _preUpdate_isRotation; } }
	public bool preUpdate_isCheckPosition	{ get { return _preUpdate_isCheckPosition; } }
	public bool preUpdate_isCheckRotation	{ get { return _preUpdate_isCheckRotation; } }

	public void Destroy()
	{
		_model = null;
		this.modifiedParentBone = null;
		this.originalParentBone = null;
		this.inherenceParentBone = null;
		_simulatedRigidBody = null;
	}

	public bool Import( int boneID, MMD4MecanimCommon.BinaryReader binaryReader )
	{
		if( !binaryReader.BeginStruct() ) {
			Debug.LogError("BeginStruct() failed.");
			return false;
		}

		unchecked {
			this.boneID					= boneID;
			this.additionalFlags		= (uint)binaryReader.ReadStructInt();
			binaryReader.ReadStructInt(); // nameJp
			binaryReader.ReadStructInt(); // nameEn
			int skeletonName			= binaryReader.ReadStructInt(); // skeletonName
			int modifiedParentBoneID	= binaryReader.ReadStructInt();
			this.sortedBoneID			= binaryReader.ReadStructInt(); // sortedBoneID
			binaryReader.ReadStructInt(); // orderedBoneID
			int originalParentBoneID	= binaryReader.ReadStructInt(); // originalPanretBoneID
			binaryReader.ReadStructInt(); // originalSortedBoneID
			this.baseOrigin				= binaryReader.ReadStructVector3();

			this.skeletonName			= binaryReader.GetName( skeletonName );

			if( this.skeletonName != null ) {
				this.isLeft				= this.skeletonName.Contains("Left");
				this.isRight			= this.skeletonName.Contains("Right");
				this.isHip				= this.skeletonName.Contains("Hip");
				this.isFoot				= this.skeletonName.Contains("Foot");
			}

			this.originalParentBone		= _model.GetBone( originalParentBoneID );
			this.modifiedParentBone		= _model.GetBone( modifiedParentBoneID );

			if( _model.fileType == PMXFileType.PMD ) {
				this.pmdBoneType				= (PMDBoneType)binaryReader.ReadStructInt();
				int childBoneID					= binaryReader.ReadStructInt();
				int targetBoneID				= binaryReader.ReadStructInt(); // targetBoneID / rotateCoef(pmdBoneType == FollowRotate)
				this.inherenceWeight			= binaryReader.ReadStructFloat();

				if( this.pmdBoneType == PMDBoneType.UnderRotate ) {
					this.inherenceParentBone	= _model.GetBone( targetBoneID );
				} else if( this.pmdBoneType == PMDBoneType.FollowRotate ) {
					this.inherenceParentBone	= _model.GetBone( childBoneID );
				}

				if( this.pmdBoneType == PMDBoneType.RotateAndMove || this.pmdBoneType == PMDBoneType.IKDestination ) {
					this.pmxBoneFlags			|= PMXBoneFlag.Translate; // isBoneTranslate
				}
				if( this.pmdBoneType == PMDBoneType.UnderRotate || this.pmdBoneType == PMDBoneType.FollowRotate ) {
					this.pmxBoneFlags			|= PMXBoneFlag.InherenceRotate;
				}
			} else if( _model.fileType == PMXFileType.PMX ) {
				this.transformLayerID			= (uint)binaryReader.ReadStructInt();
				this.pmxBoneFlags				= (PMXBoneFlag)binaryReader.ReadStructInt();
				int inherenceParentBoneID		= binaryReader.ReadStructInt();
				this.inherenceWeight			= binaryReader.ReadStructFloat();
				this.externalID					= binaryReader.ReadStructInt();
				
				if( (this.pmxBoneFlags & (PMXBoneFlag.InherenceRotate | PMXBoneFlag.InherenceTranslate)) != PMXBoneFlag.None ) {
					this.inherenceParentBone	= _model.GetBone( inherenceParentBoneID );
				}
			}
		}

		if( !binaryReader.EndStruct() ) {
			Debug.LogError("EndStruct() failed.");
			return false;
		}
		
		this.isKnee			= (this.additionalFlags & 0x01u) != 0;
		//this.isRigidBody	= (this.additionalFlags & 0x02u) != 0;
		//this.isKinematic	= (this.additionalFlags & 0x04u) != 0;
		this.isRootBone		= ((this.additionalFlags & 0xff000000u) == 0x80000000u);
		//_isDummyCharBone	= ((this.additionalFlags & 0xff000000u) == 0xc0000000u);
		
		this.baseOrigin *= _model.modelToBulletScale;
		this.baseOrigin.Z = -this.baseOrigin.Z; // LH to RH
		this.worldTransform._origin = this.baseOrigin;
		return true;
	}
	
	public void PostfixImport()
	{
		if( this.originalParentBone != null ) {
			this.offset = this.baseOrigin - this.originalParentBone.baseOrigin;
		} else {
			this.offset = this.baseOrigin;
		}
		
		this.boneLength = this.offset.Length();
		
		if( this.originalParentBone == this.modifiedParentBone ) {
			this.modifiedOffset = this.offset;
			this.modifiedOffset.X = -this.modifiedOffset.X;
			this.modifiedOffset *= _model.bulletToLocalScale;
		} else {
			if( this.modifiedParentBone != null ) {
				this.modifiedOffset = (this.baseOrigin - this.modifiedParentBone.baseOrigin) * _model.bulletToLocalScale;
				this.modifiedOffset.X = -this.modifiedOffset.X;
			} else {
				this.modifiedOffset = this.baseOrigin * _model.bulletToLocalScale;
				this.modifiedOffset.X = -this.modifiedOffset.X;
			}
		}
		
		this.localPosition = this.offset;
		this.modifiedPosition = this.modifiedOffset;
	}

	public void CleanupMoveWorldTransform()
	{
		_isMovingOnResetWorld = false;
	}
	
	public void FeedbackMoveWorldTransform( bool isMovingOnResetWorld )
	{
		_isMovingOnResetWorld = isMovingOnResetWorld;
		if( this.isSetWorldTransform ) {
			_moveWorldTransform = this.worldTransform;
			this.isSetWorldTransform = false;
		}
	}
	
	public void PrepareMoveWorldTransform()
	{
		if( this.isSetWorldTransform ) {
			return;
		}
		
		IndexedVector3 moveSourcePosition = IndexedVector3.Zero;
		if( this.isRootBone ) {
			moveSourcePosition = _moveWorldTransform._origin;
		} else {
			if( _model.rootBone != null ) {
				moveSourcePosition = this.baseOrigin + _model.rootBone._moveWorldTransform._origin;
			} else {
				moveSourcePosition = this.baseOrigin;
			}
		}
		
		if( _isMovingOnResetWorld ) {
			if( this.originalParentBone != null && this.originalParentBone._isMovingOnResetWorld ) {
				this.originalParentBone.PrepareMoveWorldTransform();
				IndexedMatrix destTransform = this.originalParentBone._moveWorldTransform.Inverse() * _moveWorldTransform;
				_moveSourcePosition = this.offset;
				_moveDestPosition = IndexedVector3.Zero;
				_moveSourceRotation = IndexedQuaternion.Identity;
				_moveDestRotation = destTransform.GetRotation();
			} else {
				_moveSourcePosition = moveSourcePosition;
				_moveDestPosition = _moveWorldTransform._origin;
				_moveSourceRotation = IndexedQuaternion.Identity;
				_moveDestRotation = _moveWorldTransform.GetRotation();
			}
		}
		
		this.worldTransform._basis = IndexedBasisMatrix.Identity;
		this.worldTransform._origin = moveSourcePosition;
		this.isSetWorldTransform = true;
	}
	
	public void ResetWorldTransformOnMoving()
	{
		this.isSetWorldTransform = false;
	}

	public void PerformMoveWorldTransform( float r )
	{
		if( this.isSetWorldTransform ) {
			return;
		}
		
		if( _isMovingOnResetWorld ) {
			if( r == 1.0f ) {
				this.worldTransform = _moveWorldTransform;
			} else {
				if( this.originalParentBone != null && this.originalParentBone._isMovingOnResetWorld ) {
					this.originalParentBone.PerformMoveWorldTransform( r );
					IndexedMatrix transform = new IndexedMatrix(
						new IndexedBasisMatrix( MMD4MecanimBulletPhysicsUtil.Slerp( ref _moveSourceRotation, ref _moveDestRotation, r ) ),
						MMD4MecanimBulletPhysicsUtil.Lerp( ref _moveSourcePosition, ref _moveDestPosition, r ) );
					this.worldTransform = this.originalParentBone.worldTransform * transform;
				} else {
					this.worldTransform._basis.SetRotation( MMD4MecanimBulletPhysicsUtil.Slerp( ref _moveSourceRotation, ref _moveDestRotation, r ) );
					this.worldTransform._origin = MMD4MecanimBulletPhysicsUtil.Lerp( ref _moveSourcePosition, ref _moveDestPosition, r );
				}
			}
		}
		
		this.isSetWorldTransform = true;
	}
	
	//------------------------------------------------------------------------------------------------------------

	public void PreUpdate_Prepare( uint updateFlags, uint updateBoneFlags )
	{
		_preUpdate_updateFlags2				= _preUpdate_updateFlags;
		_preUpdate_updateFlags				= updateFlags;
		_preUpdate_isCheckUpdated			= false;
		_preUpdate_isCheckUpdated2			= false;
		_preUpdate_isWriteTransform			= false;
		_preUpdate_isWorldTransform			= false;
		_preUpdate_isPosition				= false;
		_preUpdate_isRotation				= false;
		_preUpdate_isCheckPosition			= false;
		_preUpdate_isCheckRotation			= false;
		_preUpdate_updateBoneFlags2			= _preUpdate_updateBoneFlags;
		_preUpdate_updateBoneFlags			= updateBoneFlags;
		_preUpdate_isIKDepended				= false;
		_preUpdate_isIKDestination			= false;
		_preUpdate_isIKWeightEnabled		= false;
		_preUpdate_isXDEFDepended			= false;
		_preUpdate_isMorphPositionDepended2	= _preUpdate_isMorphPositionDepended;
		_preUpdate_isMorphPositionDepended	= false;
		_preUpdate_isMorphRotationDepended2	= _preUpdate_isMorphRotationDepended;
		_preUpdate_isMorphRotationDepended	= false;
	}
	
	public void PreUpdate_MarkIKDepended( bool isIKDestination, bool ikWeightEnabled )
	{
		_preUpdate_isIKDepended				= true;
		_preUpdate_isIKDestination			|= isIKDestination;
		_preUpdate_isIKWeightEnabled		|= ikWeightEnabled;
	}
	
	public void PreUpdate_MarkXDEFDepended()
	{
		_preUpdate_isXDEFDepended			= true;
	}
	
	public void PreUpdate_CheckUpdated()
	{
		unchecked {
			if( _preUpdate_isCheckUpdated ) {
				return;
			}

			_preUpdate_isCheckUpdated = true;

			uint updateFlags			= _preUpdate_updateFlags;

			bool isPhysicsEnabled		= _model.isPhysicsEnabled;
			bool isIKEnabled			= (updateFlags & (uint)UpdateFlags.IKEnabled) != 0;
			bool isBoneInherenceEnabled	= (updateFlags & (uint)UpdateFlags.BoneInherenceEnabled) != 0;
			bool isBoneMorphEnabled		= (updateFlags & (uint)UpdateFlags.BoneMorphEnabled) != 0;
			bool isXDEFEnabled			= _model.isXDEFEnabled;
			bool isPPHShoulderEnabled	= (updateFlags & (uint)UpdateFlags.PPHShoulderEnabled) != 0;

			if( isBoneInherenceEnabled && this.inherenceParentBone != null ) {
				bool inherencePosition = ((this.pmxBoneFlags & PMXBoneFlag.InherenceTranslate) != PMXBoneFlag.None);
				bool inherenceRotation = ((this.pmxBoneFlags & PMXBoneFlag.InherenceRotate) != PMXBoneFlag.None);
				_preUpdate_isWriteTransform = true;
				if( this.originalParentBone == this.modifiedParentBone ) {
					_preUpdate_isCheckPosition |= inherencePosition;
					_preUpdate_isCheckRotation |= inherenceRotation;
					_preUpdate_isPosition = !inherencePosition;
					_preUpdate_isRotation = !inherenceRotation;
				} else {
					_preUpdate_isWorldTransform = true;
				}
				
				if( (this.pmxBoneFlags & PMXBoneFlag.InherenceLocal) == PMXBoneFlag.None ) {
					if( this.inherenceParentBone.originalParentBone == this.inherenceParentBone.modifiedParentBone ) {
						this.inherenceParentBone._preUpdate_isCheckPosition |= inherencePosition;
						this.inherenceParentBone._preUpdate_isCheckRotation |= inherenceRotation;
						this.inherenceParentBone._preUpdate_isPosition |= inherencePosition;
						this.inherenceParentBone._preUpdate_isRotation |= inherenceRotation;
					} else {
						this.inherenceParentBone._preUpdate_isWorldTransform = true;
						if( this.inherenceParentBone.originalParentBone != null ) {
							this.inherenceParentBone.originalParentBone._preUpdate_isWorldTransform = true;
						}
					}
				} else {
					this.inherenceParentBone._preUpdate_isWorldTransform = true;
				}
			}
			
			if( isIKEnabled && _preUpdate_isIKDepended ) {
				if( _preUpdate_isIKDestination ) {
					_preUpdate_isWorldTransform = true;
				} else {
					_preUpdate_isWriteTransform = true;
					if( _preUpdate_isIKWeightEnabled ) {
						if( this.originalParentBone == this.modifiedParentBone ) {
							_preUpdate_isRotation = true;
						} else {
							_preUpdate_isWorldTransform = true;
							if( this.originalParentBone != null ) {
								this.originalParentBone._preUpdate_isWorldTransform = true;
							}
						}
					}
				}
			}
			
			if( isPhysicsEnabled ) {
				if( _isKinematicRigidBody || ( _simulatedRigidBody != null &&
					(_simulatedRigidBody.preUpdate_updateRigidBodyFlags & (uint)UpdateRigidBodyFlags.Freezed) != 0) ) {
					_preUpdate_isWorldTransform = true;
				} else if( _simulatedRigidBody != null ) {
					_preUpdate_isWriteTransform = true;
					if( this.originalParentBone != null ) {
						this.originalParentBone._preUpdate_isWorldTransform = true;
					}
				}
			}
			
			bool isOverwritePosition = (isBoneMorphEnabled && _preUpdate_isMorphPositionDepended) || _preUpdate_isMorphPositionDepended2;
			bool isOverwriteRotation = (isBoneMorphEnabled && _preUpdate_isMorphRotationDepended) || _preUpdate_isMorphRotationDepended2;
			isOverwritePosition |= ( ( (_preUpdate_updateBoneFlags | _preUpdate_updateBoneFlags2) & (uint)UpdateBoneFlags.UserPosition ) != 0 );
			isOverwriteRotation |= ( ( (_preUpdate_updateBoneFlags | _preUpdate_updateBoneFlags2) & (uint)UpdateBoneFlags.UserRotation ) != 0 );
			if( isOverwritePosition || isOverwriteRotation ) {
				_preUpdate_isWriteTransform = true;
				_preUpdate_isCheckPosition |= isOverwritePosition;
				_preUpdate_isCheckRotation |= isOverwriteRotation;
				if( this.originalParentBone == this.modifiedParentBone ) {
					_preUpdate_isPosition = true;
					_preUpdate_isRotation = true;
				} else {
					_preUpdate_isWorldTransform = true;
				}
			}
			
			if( isXDEFEnabled && _preUpdate_isXDEFDepended ) {
				_preUpdate_isWorldTransform = true;
			}

			// PPHBone
			if( isPPHShoulderEnabled || (_preUpdate_updateFlags2 | (uint)UpdateFlags.PPHShoulderEnabled) != 0 ) {
				uint shoulderType = (_preUpdate_updateBoneFlags & (uint)UpdateBoneFlags.SkeletonMask);
				
				if( shoulderType == (uint)UpdateBoneFlags.SkeletonLeftShoulder ||
				    shoulderType == (uint)UpdateBoneFlags.SkeletonRightShoulder ) {
					_preUpdate_isWriteTransform = true;
					_preUpdate_isCheckRotation = true;
					_preUpdate_isRotation = true;
					if( this.originalParentBone == this.modifiedParentBone ) {
						_preUpdate_isPosition = true;
					} else {
						_preUpdate_isWorldTransform = true;
						if( this.originalParentBone != null ) {
							this.originalParentBone._preUpdate_isWorldTransform = true;
						}
					}
				}
				
				if( shoulderType == (uint)UpdateBoneFlags.SkeletonLeftUpperArm ||
				    shoulderType == (uint)UpdateBoneFlags.SkeletonRightUpperArm ) {
					_preUpdate_isWriteTransform = true;
					_preUpdate_isCheckRotation = true;
					_preUpdate_isRotation = true;
					if( this.originalParentBone == this.modifiedParentBone ) {
						_preUpdate_isWorldTransform = true;
						_preUpdate_isPosition = true;
					} else {
						_preUpdate_isWorldTransform = true;
						if( this.originalParentBone != null ) {
							this.originalParentBone._preUpdate_isWorldTransform = true;
						}
					}
				}
			}

			if( _preUpdate_isWriteTransform ) {
				if( this.originalParentBone != null ) {
					this.originalParentBone._preUpdate_isWorldTransform = true;
				}
				if( this.originalParentBone != this.modifiedParentBone ) {
					if( this.modifiedParentBone != null ) {
						this.modifiedParentBone._preUpdate_isWorldTransform = true;
					}
				}
			}
		}
	}
	
	public void PreUpdate_CheckUpdated2()
	{
		if( _preUpdate_isCheckUpdated2 ) {
			return;
		}

		_preUpdate_isCheckUpdated2 = true;

		if( this.originalParentBone == this.modifiedParentBone ) {
			if( this.originalParentBone != null ) {
				this.originalParentBone.PreUpdate_CheckUpdated2();
				if( this.originalParentBone._preUpdate_isWriteTransform ) {
					if( this.originalParentBone.sortedBoneID < sortedBoneID ) {
						_preUpdate_isPosition = true;
						_preUpdate_isRotation = true;
					} else {
						_preUpdate_isWorldTransform = true;
					}
					_preUpdate_isWriteTransform = true;
					this.originalParentBone._preUpdate_isWorldTransform = true;
				}
			}
		} else {
			if( this.originalParentBone != null ) {
				this.originalParentBone.PreUpdate_CheckUpdated2();
				if( this.originalParentBone._preUpdate_isWriteTransform ) {
					_preUpdate_isWriteTransform = true;
					_preUpdate_isWorldTransform = true;
					this.originalParentBone._preUpdate_isWorldTransform = true;
					if( this.modifiedParentBone != null ) {
						this.modifiedParentBone._preUpdate_isWorldTransform = true;
					}
				}
			}
			if( this.modifiedParentBone != null ) {
				this.modifiedParentBone.PreUpdate_CheckUpdated2();
				if( this.modifiedParentBone._preUpdate_isWriteTransform ) {
					_preUpdate_isWriteTransform = true;
					_preUpdate_isWorldTransform = true;
					this.modifiedParentBone._preUpdate_isWorldTransform = true;
					if( this.originalParentBone != null ) {
						this.originalParentBone._preUpdate_isWorldTransform = true;
					}
				}
			}
		}
	}

	//------------------------------------------------------------------------------------------------------------

	public void PrepareUpdate( uint updateBoneFlags )
	{
		this.isSetWorldTransform			= false;
		//this.worldTransform				= IndexedMatrix.Identity;
		this.isSetLocalPosition				= false;
		this.isSetLocalRotation				= false;
		//this.localPosition				= this.offset;
		//this.localRotation				= IndexedQuaternion.Identity;
		this.isSetModifiedPosition2			= this.isSetModifiedPosition;
		this.isSetModifiedRotation2			= this.isSetModifiedRotation;
		this.isSetModifiedPosition			= false;
		this.isSetModifiedRotation			= false;
		this.modifiedPosition2				= this.modifiedPosition;
		this.modifiedRotation2				= this.modifiedRotation;
		//this.modifiedPosition				= this.modifiedOffset;
		//this.modifiedRotation				= IndexedQuaternion.Identity;
		this.isLateUpdatePositionSelf		= false;
		this.isLateUpdateRotationSelf		= false;
		this.isLateUpdatePosition			= false;
		this.isLateUpdateRotation			= false;
		this.userPosition2					= this.userPosition;
		this.userRotation2					= this.userRotation;
		//this.userPosition					= IndexedVector3.Zero;
		//this.userRotation					= IndexedQuaternion.Identity;
		this.morphPosition2					= this.morphPosition;
		this.morphRotation2					= this.morphRotation;
		//this.morphPosition				= IndexedVector3.Zero;
		//this.morphRotation				= IndexedQuaternion.Identity;

		_isIKDepended						= false;
		_isIKDestination					= false;
		_isIKWeightEnabled					= false;
		_isPrepareTransform					= false;
		_isPrepareTransform2				= false;
		_isDirtyLocalPosition				= false;
		_isDirtyLocalRotation				= false;
		_isDirtyWorldTransform				= false;
		_isDirtyLocalPosition2				= false;
		_isDirtyLocalRotation2				= false;
		_isDirtyWorldTransform2				= false;
		_isPerformWorldTransform2			= false;
		_isUpdatedWorldTransform			= false;
		_isUpdatedWorldTransformIK			= false;
		_isUpdatedWorldTransformPhysics		= false;
		this.isSetPPHBoneBasis				= false;
		//this.pphBoneBasis					= IndexedBasisMatrix.Identity;

		_underIK							= false;
		_underPhysics						= false;
		
		_updateBoneFlags					= updateBoneFlags;
	}

	public void SetWorldTransform( ref IndexedMatrix worldTransform )
	{
		this.isSetWorldTransform			= true;
		this.worldTransform					= worldTransform;
	}

	public void SetModifiedTransform( ref IndexedVector3 modifiedPosition, ref IndexedQuaternion modifiedRotation )
	{
		this.isSetModifiedPosition			= true;
		this.isSetModifiedRotation			= true;
		this.modifiedPosition				= modifiedPosition;
		this.modifiedRotation				= modifiedRotation;
	}
	
	public void SetModifiedTransform( ref Vector3 modifiedPosition, ref Quaternion modifiedRotation )
	{
		this.isSetModifiedPosition			= true;
		this.isSetModifiedRotation			= true;
		this.modifiedPosition				= modifiedPosition;
		this.modifiedRotation				= modifiedRotation;
	}
	
	public void SetUserTransform( ref IndexedVector3 userPosition, ref IndexedQuaternion userRotation )
	{
		this.userPosition					= userPosition;
		this.userRotation					= userRotation;
	}

	public void SetUserTransform( ref Vector3 userPosition, ref Quaternion userRotation )
	{
		this.userPosition					= userPosition;
		this.userRotation					= userRotation;
	}

	public void MarkIKDepended( bool isIKDestination, bool ikWeightEnabled )
	{
		_isIKDepended						= true;
		_isIKDestination					|= isIKDestination;
		_isIKWeightEnabled					|= ikWeightEnabled;
	}

	// Multi threading only.
	public void PerformWorldTransform()
	{
		if( this.isSetWorldTransform ) {
			return;
		}
		
		this.isSetWorldTransform = true;
		
		if( this.modifiedParentBone != null ) {
			this.modifiedParentBone.PerformWorldTransform();
		}

		if( !this.isSetModifiedPosition ) {
			this.isSetModifiedPosition = true;
			this.modifiedPosition = this.modifiedPosition2;
		}
		
		if( !this.isSetModifiedRotation ) {
			this.isSetModifiedRotation = true;
			this.modifiedRotation = this.modifiedRotation2;
		}
		
		IndexedQuaternion localRotation = this.modifiedRotation;
		localRotation.Y = -localRotation.Y; // UnityToBulletRotation
		localRotation.Z = -localRotation.Z; // UnityToBulletRotation
		this.worldTransform.SetRotation( ref localRotation );
		if( this.modifiedParentBone != null ) {
			IndexedVector3 localPosition = this.modifiedPosition * _model.localToBulletScale;
			localPosition.X = -localPosition.X; // UnityToBulletPosition
			this.worldTransform._origin = localPosition;
			this.worldTransform = this.modifiedParentBone.worldTransform * this.worldTransform;
		} else {
			IndexedVector3 localPosition = this.modifiedPosition * _model.localToBulletScale;
			localPosition.X = -localPosition.X; // UnityToBulletPosition
			this.worldTransform._origin = localPosition;
			this.worldTransform = _model._modelTransform * this.worldTransform;
		}
	}

	public void _PrepareTransform()
	{
		if( _isPrepareTransform ) {
			return;
		}
		
		_isPrepareTransform = true;

		unchecked {
			bool inherencePosition = ( (this.pmxBoneFlags & PMXBoneFlag.InherenceTranslate) != PMXBoneFlag.None );
			bool inherenceRotation = ( (this.pmxBoneFlags & PMXBoneFlag.InherenceRotate) != PMXBoneFlag.None );
			bool inherenceLocal = ( (this.pmxBoneFlags & PMXBoneFlag.InherenceLocal) != PMXBoneFlag.None );

			bool isRemoveExternalPosition = false;
			bool isRemoveExternalRotation = false;
			if( _simulatedRigidBody != null && !_simulatedRigidBody.isFreezed ) {
				isRemoveExternalPosition = true;
				isRemoveExternalRotation = true;
			}
			if( _isIKDepended && !_isIKDestination && !_isIKWeightEnabled ) {
				isRemoveExternalPosition = true;
				isRemoveExternalRotation = true;
			}
			if( _model.isBoneInherenceEnabled && this.inherenceParentBone != null ) {
				if( inherenceLocal ) {
					isRemoveExternalPosition = true;
					isRemoveExternalRotation = true;
				} else {
					isRemoveExternalPosition |= inherencePosition;
					isRemoveExternalRotation |= inherenceRotation;
				}
			}

			if( isRemoveExternalPosition ) {
				this.externalPosition = FastVector3.Zero;
			} else {
				if( (_updateBoneFlags & (uint)UpdateBoneFlags.ChangedPosition) != 0 ) {
					ComputeLocalPosition();
					this.externalPosition = this.localPosition - this.offset;
					_isDirtyLocalPosition |= (!this.userPosition.isZero || !this.morphPosition.isZero);
					if( MMD4MecanimBulletPhysicsUtil.IsPositionFuzzyZero( ref this.externalPosition.v ) ) {
						this.externalPosition = FastVector3.Zero;
					}
				}
			}
			
			if( isRemoveExternalRotation ) {
				this.externalRotation = FastQuaternion.Identity;
			} else {
				// PPHBone(Shoulder)
				uint shoulderType = (_updateBoneFlags & (uint)UpdateBoneFlags.SkeletonMask);
				if( shoulderType == (uint)UpdateBoneFlags.SkeletonLeftShoulder ||
				    shoulderType == (uint)UpdateBoneFlags.SkeletonRightShoulder ) {
					if( _model.pphShoulderFixRate != _model.pphShoulderFixRate2 ) {
						this.pphShoulderFixRate = _model.pphShoulderFixRate;
						_isDirtyLocalRotation = true;
					}
				}
				// PPHBone(UpperArm)
				if( shoulderType == (uint)UpdateBoneFlags.SkeletonLeftUpperArm ||
				    shoulderType == (uint)UpdateBoneFlags.SkeletonRightUpperArm ) {
					if( _model.pphShoulderFixRate != 0.0f ) {
						this.pphBoneBasis = this.worldTransform._basis;
						this.isSetPPHBoneBasis = true;
					}
					if( _model.pphShoulderFixRate != _model.pphShoulderFixRate2 ) {
						_isDirtyLocalRotation = true;
					}
				}

				if( (_updateBoneFlags & (uint)UpdateBoneFlags.ChangedRotation) != 0 ) {
					ComputeLocalRotation();
					this.externalRotation = this.localRotation;
					_isDirtyLocalRotation |= (!this.userRotation.isIdentity || !this.morphRotation.isIdentity || this.pphShoulderFixRate != 0.0f || this.isSetPPHBoneBasis);
					if( MMD4MecanimBulletPhysicsUtil.IsRotationFuzzyIdentity( ref this.externalRotation.q ) ) {
						this.externalRotation = FastQuaternion.Identity;
					}
				}
			}
			
			if( this.originalParentBone != null && this.originalParentBone.sortedBoneID < this.sortedBoneID ) {
				this.originalParentBone._PrepareTransform();
				_isDirtyWorldTransform |= this.originalParentBone._isDirtyWorldTransform;
				_isPerformWorldTransform2 |= this.originalParentBone._isPerformWorldTransform2; // Will be changed 2nd pass or later.
			}

			if( _model.isBoneInherenceEnabled && this.inherenceParentBone != null ) {
				if( !inherenceLocal && this.originalParentBone == this.modifiedParentBone &&
				    this.inherenceParentBone.originalParentBone == this.inherenceParentBone.modifiedParentBone ) {
					if( inherencePosition ) {
						_isDirtyLocalPosition |= ((_updateBoneFlags & (uint)UpdateBoneFlags.ChangedPosition) != 0);
						_isDirtyLocalPosition |= ((this.inherenceParentBone._updateBoneFlags & (uint)UpdateBoneFlags.ChangedPosition) != 0);
					}
					if( inherenceRotation ) {
						_isDirtyLocalRotation |= ((_updateBoneFlags & (uint)UpdateBoneFlags.ChangedRotation) != 0);
						_isDirtyLocalRotation |= ((this.inherenceParentBone._updateBoneFlags & (uint)UpdateBoneFlags.ChangedRotation) != 0);
					}
				} else {
					_isDirtyLocalPosition |= inherencePosition;
					_isDirtyLocalRotation |= inherenceRotation;
				}
			}

			_isDirtyLocalPosition |= (this.userPosition != this.userPosition2) || (this.morphPosition != this.morphPosition2);
			_isDirtyLocalRotation |= (this.userRotation != this.userRotation2) || (this.morphRotation != this.morphRotation2);
			
			_isDirtyWorldTransform |= (_isDirtyLocalPosition | _isDirtyLocalRotation);

			bool computePosition = false;
			bool computeRotation = false;
			
			// for _PerformWorldTransform
			if( _isDirtyWorldTransform ) {
				computePosition |= !_isDirtyLocalPosition;
				computeRotation |= !_isDirtyLocalRotation;
			}

			// for UnderIK/UnderPhysics( 2nd pass or later )
			if( this.originalParentBone != null ) {
				if( this.originalParentBone._isIKDepended && !this.originalParentBone._isIKDestination ) { // UnderIK
					if( this.originalParentBone.sortedBoneID < this.sortedBoneID ) {
						computePosition |= !_isDirtyLocalPosition;
						computeRotation |= !_isDirtyLocalRotation;
						_isPerformWorldTransform2 = true; // Will be changed 2nd pass or later.
					}
				}
				if( this.originalParentBone.isRigidBodySimulated && !this.originalParentBone.isRigidBodyFreezed ) { // UnderPhysics
					if( this.isTransformAfterPhysics ) {
						computePosition |= !_isDirtyLocalPosition;
						computeRotation |= !_isDirtyLocalRotation;
						_isPerformWorldTransform2 = true; // Will be changed 2nd pass or later.
					}
				}
			}

			// for _PerformWorldTransform
			if( _isPerformWorldTransform2 ) {
				computePosition |= !_isDirtyLocalPosition;
				computeRotation |= !_isDirtyLocalRotation;
			}

			// for IK
			if( _isIKDepended && !_isIKDestination && _isIKWeightEnabled ) {
				computeRotation |= !_isDirtyLocalRotation;
				_isPerformWorldTransform2 = true; // Will be changed 2nd pass or later.
			}
			
			// for BoneInherence( 2nd pass or later )
			if( _model.isBoneInherenceEnabled && this.inherenceParentBone != null ) {
				if( inherencePosition ) {
					computeRotation |= !_isDirtyLocalRotation;
					_isPerformWorldTransform2 = true; // Will be changed 2nd pass or later.
				}
				if( inherenceRotation ) {
					computePosition |= !_isDirtyLocalPosition;
					_isPerformWorldTransform2 = true; // Will be changed 2nd pass or later.
				}
			}

			if( computePosition ) {
				ComputeLocalPosition();
			}
			if( computeRotation ) {
				ComputeLocalRotation();
			}
		}
	}

	public void _PerformTransform()
	{
		if( _underIK || _underPhysics ) {
			return;
		}
		
		if( this.inherenceParentBone != null ) {
			this.inherenceParentBone._PerformLocalTransform();
			if( this.inherenceParentBone._isDirtyWorldTransform ) {
				if( (this.pmxBoneFlags & PMXBoneFlag.InherenceLocal) != PMXBoneFlag.None ) {
					if( (this.pmxBoneFlags & PMXBoneFlag.InherenceTranslate) != PMXBoneFlag.None ) {
						_isDirtyWorldTransform2 = _isDirtyLocalPosition2 = true;
					}
					if( (this.pmxBoneFlags & PMXBoneFlag.InherenceRotate) != PMXBoneFlag.None ) {
						_isDirtyWorldTransform2 = _isDirtyLocalRotation2 = true;
					}
				}
			} else {
				if( this.inherenceParentBone._isDirtyLocalPosition2 ) {
					if( (this.pmxBoneFlags & PMXBoneFlag.InherenceTranslate) != PMXBoneFlag.None ) {
						_isDirtyWorldTransform2 = _isDirtyLocalPosition2 = true;
					}
				}
				if( this.inherenceParentBone._isDirtyLocalRotation2 ) {
					if( (this.pmxBoneFlags & PMXBoneFlag.InherenceRotate) != PMXBoneFlag.None ) {
						_isDirtyWorldTransform2 = _isDirtyLocalRotation2 = true;
					}
				}
			}
		}
		
		_PerformLocalTransform();
		_PerformWorldTransform();
	}
	
	public void _PerformTransform2()
	{
		if( _underIK || _underPhysics ) {
			return;
		}
		
		_isDirtyLocalPosition |= _isDirtyLocalPosition2;
		_isDirtyLocalRotation |= _isDirtyLocalRotation2;
		_isDirtyWorldTransform |= _isDirtyWorldTransform2;
		_isDirtyLocalPosition2 = false;
		_isDirtyLocalRotation2 = false;
		_isDirtyWorldTransform2 = false;
		_isPerformWorldTransform2 = false;

		if( this.inherenceParentBone != null ) {
			this.inherenceParentBone._PerformLocalTransform();
		}
		
		_PerformLocalTransform();
		_PerformWorldTransform();
	}
	
	void _PerformWorldTransform()
	{
		if( _isDirtyWorldTransform ) {
			_isDirtyWorldTransform = false;
			this.worldTransform._origin = this.localPosition;
			this.worldTransform._basis.SetRotation( ref this.localRotation );
			if( this.originalParentBone != null ) {
				this.worldTransform = this.originalParentBone.worldTransform * this.worldTransform;
			}
			this.isSetWorldTransform = true;
		}
	}
	
	void _PerformLocalTransform()
	{
		bool isBoneInherenceEnabled = _model.isBoneInherenceEnabled;
		bool isBoneMorphEnabled = _model.isBoneMorphEnabled;
		
		this.isSetLocalPosition |= _isDirtyLocalPosition;
		this.isSetLocalRotation |= _isDirtyLocalRotation;
		
		this.isLateUpdatePositionSelf |= _isDirtyLocalPosition;
		this.isLateUpdateRotationSelf |= _isDirtyLocalRotation;
		
		if( _model.fileType == PMXFileType.PMD ) {
			if( _isDirtyLocalPosition ) {
				_isDirtyLocalPosition = false;
				if( (this.pmxBoneFlags & PMXBoneFlag.Translate) != PMXBoneFlag.None ) {
					this.localPosition = (this.externalPosition + this.userPosition).v + this.offset;
				} else {
					this.localPosition = this.offset;
				}
			}
			
			if( _isDirtyLocalRotation ) {
				_isDirtyLocalRotation = false;
				FastQuaternion localRotation = FastQuaternion.Identity;
				if( this.isSetPPHBoneBasis ) {
					IndexedQuaternion q = IndexedQuaternion.Identity;
					if( this.originalParentBone != null ) {
						IndexedBasisMatrix basis = this.originalParentBone.worldTransform._basis.Transpose() * this.pphBoneBasis;
						q = basis.GetRotation();
					} else {
						q = this.pphBoneBasis.GetRotation();
					}
					localRotation = q;
				} else {
					if( !this.externalRotation.isIdentity ) {
						if( this.pphShoulderFixRate != 0.0f ) {
							if( this.pphShoulderFixRate != 1.0f ) {
								localRotation = MMD4MecanimBulletPhysicsUtil.SlerpFromIdentity( ref this.externalRotation.q, 1.0f - this.pphShoulderFixRate );
							}
						} else {
							localRotation = this.externalRotation;
						}
					}
				}
				localRotation *= this.userRotation;

				if( isBoneInherenceEnabled && this.pmdBoneType == PMDBoneType.UnderRotate ) {
					if( this.inherenceParentBone != null ) {
						this.inherenceParentBone.ComputeLocalRotation();
						localRotation *= this.inherenceParentBone.localRotation;
					}
				} else if( isBoneInherenceEnabled && this.pmdBoneType == PMDBoneType.FollowRotate ) {
					if( this.inherenceParentBone != null ) {
						this.inherenceParentBone.ComputeLocalRotation();
						IndexedQuaternion r = MMD4MecanimBulletPhysicsUtil.SlerpFromIdentity( ref this.inherenceParentBone.localRotation, this.inherenceWeight );
						localRotation *= r;
					}
				}
				this.localRotation = localRotation.q;
				this.localRotation.Normalize();
			}
		} else if( _model.fileType == PMXFileType.PMX ) {
			if( _isDirtyLocalPosition ) {
				_isDirtyLocalPosition = false;
				if( (this.pmxBoneFlags & PMXBoneFlag.Translate) != PMXBoneFlag.None ) {
					FastVector3 localPosition = this.externalPosition + this.userPosition;
					if( isBoneMorphEnabled ) {
						localPosition += this.morphPosition;
					}
					
					if( isBoneInherenceEnabled && (this.pmxBoneFlags & PMXBoneFlag.InherenceTranslate) != PMXBoneFlag.None ) {
						if( this.inherenceParentBone != null ) {
							IndexedVector3 parentInherencePosition = IndexedVector3.Zero;
							if( (this.pmxBoneFlags & PMXBoneFlag.InherenceLocal) != PMXBoneFlag.None ) {
								parentInherencePosition = this.inherenceParentBone.worldTransform._origin - this.inherenceParentBone.baseOrigin;
							} else {
								this.inherenceParentBone.ComputeLocalPosition();
								parentInherencePosition = this.inherenceParentBone.localPosition - this.inherenceParentBone.offset;
							}
							if( !MMD4MecanimCommon.FuzzyZero( this.inherenceWeight - 1.0f ) ) {
								parentInherencePosition *= this.inherenceWeight;
							}
							localPosition += parentInherencePosition;
						}
					}
					this.localPosition = localPosition.v + this.offset;
				} else {
					this.localPosition = this.offset;
				}
			}

			if( _isDirtyLocalRotation ) {
				_isDirtyLocalRotation = false;
				FastQuaternion localRotation = FastQuaternion.Identity;
				if( this.isSetPPHBoneBasis ) {
					IndexedQuaternion q = IndexedQuaternion.Identity;
					if( this.originalParentBone != null ) {
						IndexedBasisMatrix basis = this.originalParentBone.worldTransform._basis.Transpose() * this.pphBoneBasis;
						q = basis.GetRotation();
					} else {
						q = this.pphBoneBasis.GetRotation();
					}
					localRotation = q;
				} else {
					if( !this.externalRotation.isIdentity ) {
						if( this.pphShoulderFixRate != 0.0f ) {
							if( this.pphShoulderFixRate != 1.0f ) {
								localRotation = MMD4MecanimBulletPhysicsUtil.SlerpFromIdentity( ref this.externalRotation.q, 1.0f - this.pphShoulderFixRate );
							}
						} else {
							localRotation = this.externalRotation;
						}
					}
				}
				localRotation *= this.userRotation;

				if( isBoneMorphEnabled ) {
					localRotation *= this.morphRotation;
				}
				
				if( isBoneInherenceEnabled && (this.pmxBoneFlags & PMXBoneFlag.InherenceRotate) != PMXBoneFlag.None ) {
					if( this.inherenceParentBone != null ) {
						IndexedQuaternion parentInherenceRotation = IndexedQuaternion.Identity;
						if( (this.pmxBoneFlags & PMXBoneFlag.InherenceLocal) != PMXBoneFlag.None ) {
							parentInherenceRotation = this.inherenceParentBone.worldTransform.GetRotation();
						} else {
							this.inherenceParentBone.ComputeLocalRotation();
							parentInherenceRotation = this.inherenceParentBone.localRotation;
						}
						if( !MMD4MecanimCommon.FuzzyZero( this.inherenceWeight - 1.0f ) ) {
							float inherenceWeightAbs = Mathf.Abs( this.inherenceWeight );
							if( inherenceWeightAbs < 1.0f ) {
								parentInherenceRotation = MMD4MecanimBulletPhysicsUtil.SlerpFromIdentity( ref parentInherenceRotation, inherenceWeightAbs );
							} else if( inherenceWeightAbs > 1.0f ) {
								parentInherenceRotation = new IndexedQuaternion(
									MMD4MecanimBulletPhysicsUtil.GetAxis( ref parentInherenceRotation ),
									MMD4MecanimBulletPhysicsUtil.GetAngle( ref parentInherenceRotation ) * inherenceWeightAbs );
							}
							if( this.inherenceWeight < 0.0f ) {
								parentInherenceRotation.X = -parentInherenceRotation.X;
								parentInherenceRotation.Y = -parentInherenceRotation.Y;
								parentInherenceRotation.Z = -parentInherenceRotation.Z;
							}
						}
						localRotation *= parentInherenceRotation;
					}
				}

				this.localRotation = localRotation.q;
				this.localRotation.Normalize();
			}
		}
	}
	
	/*------------------------------------------------------------------------------------------------------------*/
	
	public void PrepareUpdate2()
	{
		_isPrepareTransform2				= false;
		_isDirtyLocalPosition				= false;
		_isDirtyLocalRotation				= false;
		_isDirtyWorldTransform				= false;
		_isDirtyLocalPosition2				= false;
		_isDirtyLocalRotation2				= false;
		_isDirtyWorldTransform2				= false;
		_isUpdatedWorldTransform			= false;
		_isUpdatedWorldTransformIK			= false;
		_isUpdatedWorldTransformPhysics		= false;
	}
	
	public void _PrepareTransform2( bool isAfterPhysics )
	{
		if( _isPrepareTransform2 ) {
			return;
		}
		
		if( _underIK || _underPhysics ) {
			return;
		}
		
		_isPrepareTransform2 = true;
		
		if( _model.fileType == PMXFileType.PMX ) {
			if( this.originalParentBone != null ) {
				this.originalParentBone._PrepareTransform2( isAfterPhysics );
				if( this.originalParentBone._isUpdatedWorldTransformPhysics ) {
					if( this.isTransformAfterPhysics ) {
						_isUpdatedWorldTransform = true;
						_isDirtyWorldTransform = true;
					}
				} else if( this.originalParentBone._isUpdatedWorldTransformIK ) {
					if( this.originalParentBone.sortedBoneID < this.sortedBoneID ) {
						if( !isAfterPhysics || isAfterPhysics == this.isTransformAfterPhysics ) {
							_isUpdatedWorldTransform = true;
							_isDirtyWorldTransform = true;
						}
					}
				} else if( this.originalParentBone._isUpdatedWorldTransform ) {
					if( this.originalParentBone.sortedBoneID < this.sortedBoneID ) {
						if( !isAfterPhysics || isAfterPhysics == this.isTransformAfterPhysics ) {
							_isUpdatedWorldTransform = true;
							_isDirtyWorldTransform = true;
						}
					}
				}
			}
		}
		
		if( _model.isBoneInherenceEnabled ) {
			if( !isAfterPhysics || isAfterPhysics == this.isTransformAfterPhysics ) {
				if( this.inherenceParentBone != null ) {
					this.inherenceParentBone._PrepareTransform2( isAfterPhysics );
					if( (this.pmxBoneFlags & PMXBoneFlag.InherenceTranslate) != PMXBoneFlag.None ) {
						if( (this.pmxBoneFlags & PMXBoneFlag.InherenceLocal) != PMXBoneFlag.None ) {
							_isDirtyLocalPosition |=
								this.inherenceParentBone._isDirtyWorldTransform |
									this.inherenceParentBone._isUpdatedWorldTransform |
									this.inherenceParentBone._isUpdatedWorldTransformIK |
									this.inherenceParentBone._isUpdatedWorldTransformPhysics;
						} else {
							_isDirtyLocalPosition |=
								this.inherenceParentBone._isDirtyLocalPosition |
									this.inherenceParentBone._isUpdatedWorldTransform |
									this.inherenceParentBone._isUpdatedWorldTransformIK |
									this.inherenceParentBone._isUpdatedWorldTransformPhysics;
						}
						if( _isDirtyLocalPosition ) {
							this.externalPosition = FastVector3.Zero;
						}
					}
					if( (this.pmxBoneFlags & PMXBoneFlag.InherenceRotate) != PMXBoneFlag.None ) {
						if( (this.pmxBoneFlags & PMXBoneFlag.InherenceLocal) != PMXBoneFlag.None ) {
							_isDirtyLocalRotation |=
								this.inherenceParentBone._isDirtyWorldTransform |
									this.inherenceParentBone._isUpdatedWorldTransform |
									this.inherenceParentBone._isUpdatedWorldTransformIK |
									this.inherenceParentBone._isUpdatedWorldTransformPhysics;
						} else {
							_isDirtyLocalRotation |=
								this.inherenceParentBone._isDirtyLocalRotation |
									this.inherenceParentBone._isUpdatedWorldTransform |
									this.inherenceParentBone._isUpdatedWorldTransformIK |
									this.inherenceParentBone._isUpdatedWorldTransformPhysics;
						}
						if( _isDirtyLocalRotation ) {
							this.externalRotation = FastQuaternion.Identity;
							this.isSetPPHBoneBasis = false;
						}
					}
				}
			}
		}
		
		_isDirtyWorldTransform |= (_isDirtyLocalPosition | _isDirtyLocalRotation);

		if( _isDirtyWorldTransform ) {
			// Failsafe.(Normally, already compute position / rotation)
			bool computePosition = false;
			bool computeRotation = false;
			
			// for _PerformWorldTransform
			if( _isDirtyWorldTransform ) {
				computePosition |= !_isDirtyLocalPosition;
				computeRotation |= !_isDirtyLocalRotation;
			}
			
			if( computePosition ) {
				ComputeLocalPosition();
			}
			if( computeRotation ) {
				ComputeLocalRotation();
			}
		}
	}
	
	/*------------------------------------------------------------------------------------------------------------*/
	
	public void PrepareLateUpdate()
	{
		if( this.isLateUpdatePositionSelf ) {
			this.isSetModifiedPosition = false;
		}
		if( this.isLateUpdateRotationSelf ) {
			this.isSetModifiedRotation = false;
		}
		
		if( this.modifiedParentBone != null && this.originalParentBone != null && this.modifiedParentBone != this.originalParentBone ) {
			bool isUpdated	= this.modifiedParentBone.isLateUpdatePositionSelf | this.modifiedParentBone.isLateUpdateRotationSelf
							| this.originalParentBone.isLateUpdatePositionSelf | this.originalParentBone.isLateUpdateRotationSelf;
			if( isUpdated ) {
				this.isLateUpdatePosition = this.isLateUpdateRotation = true;
				this.isSetModifiedPosition = false;
				this.isSetModifiedRotation = false;
			}
		}
	}
	
	/*------------------------------------------------------------------------------------------------------------*/
	
	public void SetWorldTransformFromBody( ref IndexedMatrix worldTransform )
	{
		this.worldTransform = worldTransform;
		this.isSetWorldTransform = true;
		this.isSetLocalPosition = false;
		this.isSetLocalRotation = false;
		this.isSetModifiedPosition = false; // Fix for ComputeLocalPosition
		this.isSetModifiedRotation = false; // Fix for ComputeLocalRotation
		_underPhysics = true;
		_isUpdatedWorldTransformPhysics = true;
		this.isLateUpdatePositionSelf = true;
		this.isLateUpdateRotationSelf = true;
		this.externalPosition = FastVector3.Zero;
		this.externalRotation = FastQuaternion.Identity;
		this.isSetPPHBoneBasis = false;
	}
	
	public void SetWorldTransformBoneAligned( ref IndexedMatrix worldTransform )
	{
		if( this.originalParentBone != null ) {
			this.worldTransform._origin = this.originalParentBone.worldTransform * this.offset;
			this.worldTransform._basis = worldTransform._basis;
		} else {
			this.worldTransform._basis = worldTransform._basis;
		}
		this.isSetWorldTransform = true;
		this.isSetLocalPosition = false;
		this.isSetLocalRotation = false;
		this.isSetModifiedPosition = false; // Fix for ComputeLocalPosition
		this.isSetModifiedRotation = false; // Fix for ComputeLocalRotation
		_underPhysics = true;
		_isUpdatedWorldTransformPhysics = true;
		this.isLateUpdatePositionSelf = true;
		this.isLateUpdateRotationSelf = true;
		this.externalPosition = FastVector3.Zero;
		this.externalRotation = FastQuaternion.Identity;
		this.isSetPPHBoneBasis = false;
	}

	public void AntiJitterWorldTransform( bool isTouchKinematic )
	{
		// for Simulated/SimulatedAligned rigidBody only.
		if( this.modifiedParentBone != null ) {
			if( !this.isSetPrevWorldTransform ) {
				this.isSetPrevWorldTransform = true;
				this.prevWorldTransform = this.worldTransform;
				return;
			}
			
			this.prevWorldTransform2 = this.prevWorldTransform;
			this.isSetPrevWorldTransform2 = true;
			
			float antiJitterRate = isTouchKinematic
				? _model.modelProperty.rigidBodyAntiJitterRateOnKinematic
				: _model.modelProperty.rigidBodyAntiJitterRate;
			
			if( antiJitterRate > 0.0f ) {
				float lhsRate = Mathf.Max( 1.0f - antiJitterRate * 0.5f, 0.5f );
				IndexedMatrix blendedTransform = IndexedMatrix.Identity;
				MMD4MecanimBulletPhysicsUtil.BlendTransform( ref blendedTransform, ref this.worldTransform, ref this.prevWorldTransform, lhsRate );
				this.prevWorldTransform = this.worldTransform;
				this.worldTransform = blendedTransform;
			} else {
				this.prevWorldTransform = this.worldTransform;
			}
		}
	}
	
	public void AntiJitterWorldTransformOnDisabled()
	{
		if( this.modifiedParentBone != null ) {
			if( !this.isSetPrevWorldTransform ) {
				this.isSetPrevWorldTransform = true;
				this.prevWorldTransform = this.worldTransform;
				return;
			}
			
			this.prevWorldTransform2 = this.prevWorldTransform;
			this.isSetPrevWorldTransform2 = true;
			
			this.prevWorldTransform = this.worldTransform;
		}
	}

	public void PrepareIKTransform( float ikWeight )
	{
		if( ikWeight != 1.0f ) {
			ComputeLocalRotation();
			this.localRotationBeforeIK = this.localRotation;
		}
		
		this.localPosition = this.offset;
		this.localRotation = IndexedQuaternion.Identity;
		this.worldTransform._basis = IndexedBasisMatrix.Identity;
		if( this.originalParentBone != null ) {
			this.worldTransform._origin = this.localPosition; // Reset localPosition under IK
			this.worldTransform = this.originalParentBone.worldTransform * this.worldTransform;
		}
		
		this.isSetLocalPosition = true;
		this.isSetLocalRotation = true;
		this.isSetWorldTransform = true;
		// for _PrepareTransform2()
		_isUpdatedWorldTransformIK = true;
		// for _PerformTransform()
		_underIK = true;
		// for LateUpdate()
		this.isLateUpdateRotationSelf = true;
		// Reset externalTransform.
		this.externalPosition = FastVector3.Zero;
		this.externalRotation = FastQuaternion.Identity;
		this.isSetPPHBoneBasis = false;
	}
	
	public void ApplyLocalRotationFromIK( ref IndexedQuaternion rotation )
	{
		this.localRotation *= rotation;
		this.localRotation.Normalize();
	}
	
	public void SetLocalRotationFromIK( ref IndexedQuaternion rotation )
	{
		this.localRotation = rotation;
	}
	
	public void SetTransformFromIK( ref IndexedMatrix worldTransform, ref IndexedVector3 localPosition, ref IndexedQuaternion localRotation )
	{
		this.worldTransform = worldTransform;
		this.localPosition = localPosition;
		this.localRotation = localRotation;
	}
	
	public void PerformTransformFromIK()
	{
		if( this.originalParentBone != null ) { // Memo: Use originalParentBone
			this.worldTransform._origin = this.localPosition;
			this.worldTransform._basis.SetRotation( ref this.localRotation );
			this.worldTransform = this.originalParentBone.worldTransform * this.worldTransform;
		} else {
			this.worldTransform._basis.SetRotation( ref this.localRotation );
		}
	}
	
	public void PostfixIKTransform( float ikWeight )
	{
		if( ikWeight != 1.0f ) {
			this.localRotation = MMD4MecanimBulletPhysicsUtil.Slerp( ref this.localRotationBeforeIK, ref this.localRotation, ikWeight );
			if( this.originalParentBone != null ) { // Memo: Use originalParentBone
				this.worldTransform._origin = this.localPosition;
				this.worldTransform._basis.SetRotation( ref this.localRotation );
				this.worldTransform = this.originalParentBone.worldTransform * this.worldTransform;
			} else {
				this.worldTransform._basis.SetRotation( ref this.localRotation );
			}
		}
	}
	
	//------------------------------------------------------------------------------------------------------------------------------------
	
	void ComputeLocalPosition()
	{
		if( this.isSetLocalPosition ) {
			return;
		}
		
		bool rigidBodyIsForceTranslate = _model.modelProperty.rigidBodyIsForceTranslate;
		
		this.isSetLocalPosition = true;
		
		if( this.isSetModifiedPosition && this.modifiedParentBone == this.originalParentBone ) { // Failsafe.
			if( this.modifiedParentBone != null ) {
				if( !rigidBodyIsForceTranslate && !isBoneTranslate ) {
					this.localPosition = this.offset;
				} else {
					this.localPosition = this.modifiedPosition * _model.localToBulletScale; // UnityToBullet
					this.localPosition.X = -this.localPosition.X; // UnityToBullet
				}
			} else {
				this.localPosition = this.modifiedPosition * _model.localToBulletScale; // UnityToBullet
				this.localPosition.X = -this.localPosition.X; // UnityToBullet
			}
			return;
		}
		
		if( !this.isSetWorldTransform ) { // Failsafe.
			this.localPosition = this.offset;
			return;
		}
		
		if( this.originalParentBone != null ) {
			if( !rigidBodyIsForceTranslate && !isBoneTranslate ) {
				this.localPosition = this.offset;
			} else {
				IndexedBasisMatrix parentWorldInverseBasis = this.originalParentBone.worldTransform._basis.Transpose();
				IndexedVector3 position = this.worldTransform._origin;
				IndexedVector3 parentPosition = this.originalParentBone.worldTransform._origin;
				IndexedVector3 translate = position - parentPosition;
				this.localPosition = parentWorldInverseBasis * translate;
			}
		} else {
			IndexedBasisMatrix parentWorldInverseBasis = _model._modelTransform._basis.Transpose();
			IndexedVector3 position = this.worldTransform._origin;
			IndexedVector3 parentPosition = _model._modelTransform._origin;
			IndexedVector3 translate = position - parentPosition;
			this.localPosition = parentWorldInverseBasis * translate;
		}
	}
	
	void ComputeLocalRotation()
	{
		if( this.isSetLocalRotation ) {
			return;
		}

		this.isSetLocalRotation = true;
		
		if( this.isSetModifiedRotation && this.modifiedParentBone == this.originalParentBone ) { // Failsafe.
			this.localRotation.X = this.modifiedRotation.X; // UnityToBullet
			this.localRotation.Y = -this.modifiedRotation.Y; // UnityToBullet
			this.localRotation.Z = -this.modifiedRotation.Z; // UnityToBullet
			this.localRotation.W = this.modifiedRotation.W; // UnityToBullet
			return;
		}
		
		if( !this.isSetWorldTransform ) { // Failsafe.
			this.localRotation = IndexedQuaternion.Identity;
			return;
		}
		
		if( this.originalParentBone != null ) {
			IndexedBasisMatrix parentWorldInverseBasis = this.originalParentBone.worldTransform._basis.Transpose();
			IndexedBasisMatrix worldBasis = parentWorldInverseBasis * this.worldTransform._basis;
			this.localRotation = worldBasis.GetRotation();
		} else {
			IndexedBasisMatrix parentWorldInverseBasis = _model._modelTransform._basis.Transpose();
			IndexedBasisMatrix worldBasis = parentWorldInverseBasis * this.worldTransform._basis;
			this.localRotation = worldBasis.GetRotation();
		}
	}
	
	//------------------------------------------------------------------------------------------------------------------------------------
	
	public void ComputeModifiedPosition()
	{
		if( this.isSetModifiedPosition ) {
			return;
		}
		
		this.isSetModifiedPosition = true;
		
		bool rigidBodyIsForceTranslate = _model.modelProperty.rigidBodyIsForceTranslate;
		
		bool isSetModifiedPosition = false;
		
		if( this.modifiedParentBone == this.originalParentBone ) {
			if( this.isSetLocalPosition && (this.originalParentBone == null || this.originalParentBone.sortedBoneID < this.sortedBoneID) ) {
				isSetModifiedPosition = true;
				if( this.modifiedParentBone != null ) {
					if( !rigidBodyIsForceTranslate && !isBoneTranslate ) {
						this.modifiedPosition = this.modifiedOffset;
					} else {
						this.modifiedPosition = this.localPosition * _model.bulletToLocalScale; // BulletToUnity
						this.modifiedPosition.X = -this.modifiedPosition.X; // BulletToUnity
					}
				} else {
					this.modifiedPosition = this.localPosition * _model.bulletToLocalScale; // BulletToUnity
					this.modifiedPosition.X = -this.modifiedPosition.X; // BulletToUnity
				}
			}
		}
		
		if( !this.isSetWorldTransform ) { // Failsafe.
			if( !isSetModifiedPosition ) {
				this.modifiedPosition = this.modifiedOffset;
			}
			return;
		}
		
		if( this.modifiedParentBone != null ) {
			if( !isSetModifiedPosition ) {
				if( !rigidBodyIsForceTranslate && !isBoneTranslate ) {
					this.modifiedPosition = this.modifiedOffset;
				} else {
					IndexedBasisMatrix parentWorldInverseBasis = this.modifiedParentBone.worldTransform._basis.Transpose();
					IndexedVector3 position = this.worldTransform._origin;
					IndexedVector3 parentPosition = this.modifiedParentBone.worldTransform._origin;
					IndexedVector3 translate = position - parentPosition;
					this.modifiedPosition = (parentWorldInverseBasis * translate) * _model.bulletToLocalScale; // BulletToUnity
					this.modifiedPosition.X = -this.modifiedPosition.X; // BulletToUnity
				}
			}
		} else {
			if( !isSetModifiedPosition ) {
				IndexedBasisMatrix parentWorldInverseBasis = _model._modelTransform._basis.Transpose();
				IndexedVector3 position = this.worldTransform._origin;
				IndexedVector3 parentPosition = _model._modelTransform._origin;
				IndexedVector3 translate = position - parentPosition;
				this.modifiedPosition = (parentWorldInverseBasis * translate) * _model.bulletToLocalScale; // BulletToUnity
				this.modifiedPosition.X = -this.modifiedPosition.X; // BulletToUnity
			}
		}
	}
	
	public void ComputeModifiedRotation()
	{
		if( this.isSetModifiedRotation ) {
			return;
		}
		
		this.isSetModifiedRotation = true;

		bool isSetModifiedRotation = false;
		
		if( this.modifiedParentBone == this.originalParentBone ) {
			if( this.isSetLocalRotation && (this.originalParentBone == null || this.originalParentBone.sortedBoneID < this.sortedBoneID) ) {
				isSetModifiedRotation = true;
				this.modifiedRotation.X = this.localRotation.X; // BulletToUnity
				this.modifiedRotation.Y = -this.localRotation.Y; // BulletToUnity
				this.modifiedRotation.Z = -this.localRotation.Z; // BulletToUnity
				this.modifiedRotation.W = this.localRotation.W; // BulletToUnity
			}
		}
		
		if( !this.isSetWorldTransform ) { // Failsafe.
			if( !isSetModifiedRotation ) {
				this.modifiedRotation = IndexedQuaternion.Identity;
			}
			return;
		}
		
		if( this.modifiedParentBone != null ) {
			if( !isSetModifiedRotation ) {
				IndexedBasisMatrix parentWorldInverseBasis = this.modifiedParentBone.worldTransform._basis.Transpose();
				IndexedBasisMatrix worldBasis = parentWorldInverseBasis * this.worldTransform._basis;
				this.modifiedRotation = worldBasis.GetRotation();
				this.modifiedRotation.Y = -this.modifiedRotation.Y; // BulletToUnity
				this.modifiedRotation.Z = -this.modifiedRotation.Z; // BulletToUnity
			}
		} else {
			if( !isSetModifiedRotation ) {
				IndexedBasisMatrix parentWorldInverseBasis = _model._modelTransform._basis.Transpose();
				IndexedBasisMatrix worldBasis = parentWorldInverseBasis * this.worldTransform._basis;
				this.modifiedRotation = worldBasis.GetRotation();
				this.modifiedRotation.Y = -this.modifiedRotation.Y; // BulletToUnity
				this.modifiedRotation.Z = -this.modifiedRotation.Z; // BulletToUnity
			}
		}
	}
}
