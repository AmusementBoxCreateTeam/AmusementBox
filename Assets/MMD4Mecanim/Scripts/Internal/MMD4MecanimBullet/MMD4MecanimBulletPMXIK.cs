using UnityEngine;
using System.Collections;
using BulletXNA;
using BulletXNA.BulletCollision;
using BulletXNA.BulletDynamics;
using BulletXNA.LinearMath;

using PMXFileType       	= MMD4MecanimBulletPMXCommon.PMXFileType;

using PMXModel				= MMD4MecanimBulletPMXModel;
using PMXBone				= MMD4MecanimBulletPMXBone;

using UpdateFlags			= MMD4MecanimBulletPhysics.MMDModel.UpdateFlags;
using UpdateBoneFlags		= MMD4MecanimBulletPhysics.MMDModel.UpdateBoneFlags;
using LateUpdateBoneFlags	= MMD4MecanimBulletPhysics.MMDModel.LateUpdateBoneFlags;

using BinaryReader			= MMD4MecanimCommon.BinaryReader;

public class MMD4MecanimBulletPMXIK
{
	public const float IK_SKIPDISTANCE = (1e-13f);

	struct BoneTransform
	{
		public IndexedMatrix		worldTransform;
		public IndexedVector3		localPosition;
		public IndexedQuaternion	localRotation;

		public bool _IsEqualWorld( PMXBone bone )
		{
			return this.worldTransform == bone.worldTransform;
		}

		public bool _IsEqual( PMXBone bone )
		{
			return this.worldTransform == bone.worldTransform
				&& this.localPosition == bone.localPosition
				&& this.localRotation == bone.localRotation;
		}

		public void _Set( PMXBone bone )
		{
			bone.SetTransformFromIK(
				ref this.worldTransform,
				ref this.localPosition,
				ref this.localRotation );
		}

		public void _GetWorld( PMXBone bone )
		{
			this.worldTransform = bone.worldTransform;
		}

		public void _Get( PMXBone bone )
		{
			this.worldTransform = bone.worldTransform;
			this.localPosition = bone.localPosition;
			this.localRotation = bone.localRotation;
		}
	}
	
	enum IKLinkLimitType
	{
		X,
		Y,
		Z,
		Free,
	}
	
	enum IKLinkFreeLimitType
	{
		X,
		Y,
		Z,
	}

	[System.Flags]
	public enum IKLinkFlags
	{
		HasAngleJoint = 0x01,
	}
	
	struct IKLink
	{
		public PMXBone				bone;
		public uint					ikLinkFlags;
		public IKLinkLimitType		ikLinkLimitType;
		public IKLinkFreeLimitType	ikLinkFreeLimitType;
		public IndexedVector3		lowerLimit;
		public IndexedVector3		upperLimit;
		public BoneTransform		cachedBoneTransfrom;
		public BoneTransform		solvedBoneTransfrom;

		public bool hasAngleJoint { get { return (this.ikLinkFlags & (uint)IKLinkFlags.HasAngleJoint) != 0; } }
	}

	public float ikWeight {
		get { return _ikWeight; }
		set { _ikWeight = Mathf.Clamp01( value ); }
	}

	public bool isDisabled {
		get { return _isDisabled; }
		set { _isDisabled = value; }
	}

	public PMXModel	_model = null;
	public PMXModel model { get { return _model; } }

	uint			_ikAdditionalFlags = 0;
	PMXBone			_destBone = null;
	PMXBone			_targetBone = null;
	uint			_iteration = 0;
	float			_angleConstraint = 0.0f;
	IKLink[]		_ikLinkList = null;
	float 			_ikWeight = 1.0f;
	bool			_isKnee = false;
	bool			_isCached = false;
	bool			_isDisabled = false;
	BoneTransform	_cachedDestBoneTransfrom = new BoneTransform();
	BoneTransform	_cachedTargetBoneTransfrom = new BoneTransform();
	BoneTransform	_solvedTargetBoneTransfrom = new BoneTransform();

	public uint ikAdditionalFlags { get { return _ikAdditionalFlags; } }
	public bool isTransformAfterPhysics { get { return ( _destBone != null ) ? _destBone.isTransformAfterPhysics : false; } }

	public void Destroy()
	{
		_destBone = null;
		_targetBone = null;
		_ikLinkList = null;
	}
	
	public bool Import( BinaryReader binaryReader )
	{
		Destroy();
		
		if( !binaryReader.BeginStruct() ) {
			Debug.LogError( "BeginStruct() failed." );
			return false;
		}

		int ikLinkCount = 0;

		unchecked {
			_ikAdditionalFlags	= (uint)binaryReader.ReadStructInt();
			_destBone			= _model.GetBone( binaryReader.ReadStructInt() );
			_targetBone			= _model.GetBone( binaryReader.ReadStructInt() );
			_iteration			= (uint)binaryReader.ReadStructInt();
			_angleConstraint	= binaryReader.ReadStructFloat();
			ikLinkCount			= binaryReader.ReadStructInt();
			_ikLinkList			= new IKLink[ikLinkCount];
		}
		
		if( _targetBone == null || _destBone == null ) {
			Debug.LogError( "bone is null." );
			Destroy();
			return false;
		}
		
		for( int i = 0; i < ikLinkCount; ++i ) {
			_ikLinkList[i] = new IKLink();
			_ikLinkList[i].bone = _model.GetBone( binaryReader.ReadInt() );
			_ikLinkList[i].ikLinkFlags = (uint)binaryReader.ReadInt();
			_ikLinkList[i].ikLinkLimitType = IKLinkLimitType.Free;
			_ikLinkList[i].ikLinkFreeLimitType = IKLinkFreeLimitType.X;
			_ikLinkList[i].cachedBoneTransfrom = new BoneTransform();
			_ikLinkList[i].solvedBoneTransfrom = new BoneTransform();
			if( _ikLinkList[i].hasAngleJoint ) {
				_ikLinkList[i].lowerLimit = binaryReader.ReadVector3();
				_ikLinkList[i].upperLimit = binaryReader.ReadVector3();
				MMD4MecanimBulletPhysicsUtil.GetAngularLimitFromLeftHand(
					ref _ikLinkList[i].lowerLimit,
					ref _ikLinkList[i].upperLimit );

				IndexedVector3 lowerLimit = _ikLinkList[i].lowerLimit;
				IndexedVector3 upperLimit = _ikLinkList[i].upperLimit;

				if( MMD4MecanimCommon.FuzzyZero( lowerLimit.Y ) && MMD4MecanimCommon.FuzzyZero( upperLimit.Y ) &&
					MMD4MecanimCommon.FuzzyZero( lowerLimit.Z ) && MMD4MecanimCommon.FuzzyZero( upperLimit.Z ) ) {
					_ikLinkList[i].ikLinkLimitType = IKLinkLimitType.X;
				} else if(	MMD4MecanimCommon.FuzzyZero( lowerLimit.X ) && MMD4MecanimCommon.FuzzyZero( upperLimit.X ) &&
							MMD4MecanimCommon.FuzzyZero( lowerLimit.Z ) && MMD4MecanimCommon.FuzzyZero( upperLimit.Z ) ) {
					_ikLinkList[i].ikLinkLimitType = IKLinkLimitType.Y;
				} else if(	MMD4MecanimCommon.FuzzyZero( lowerLimit.X ) && MMD4MecanimCommon.FuzzyZero( upperLimit.X ) &&
							MMD4MecanimCommon.FuzzyZero( lowerLimit.Y ) && MMD4MecanimCommon.FuzzyZero( upperLimit.Y ) ) {
					_ikLinkList[i].ikLinkLimitType = IKLinkLimitType.Z;
				} else {
					_ikLinkList[i].ikLinkLimitType = IKLinkLimitType.Free;

					if( lowerLimit.X >= -180.0f * Mathf.Deg2Rad && upperLimit.X <= 180.0f * Mathf.Deg2Rad ) {
						_ikLinkList[i].ikLinkFreeLimitType = IKLinkFreeLimitType.X;
					} else if( lowerLimit.Y >= -180.0f * Mathf.Deg2Rad && upperLimit.Y <= 180.0f * Mathf.Deg2Rad ) {
						_ikLinkList[i].ikLinkFreeLimitType = IKLinkFreeLimitType.Y;
					} else {
						_ikLinkList[i].ikLinkFreeLimitType = IKLinkFreeLimitType.Z;
					}
				}
			} else {
				_ikLinkList[i].lowerLimit = IndexedVector3.Zero;
				_ikLinkList[i].upperLimit = IndexedVector3.Zero;
			}
			if( _ikLinkList[i].bone == null ) {
				Debug.LogError( "bone is null." );
				Destroy();
				return false;
			}

			_isKnee |= _ikLinkList[i].bone.isKnee;
		}
		
		if( !binaryReader.EndStruct() ) {
			Debug.LogError( "EndStruct() failed." );
			Destroy();
			return false;
		}
		
		return true;
	}

	public void PreUpdate_MarkIKDepended( uint updateFlags, float ikWeight )
	{
		if( ( updateFlags & (uint)UpdateFlags.IKEnabled ) == 0 || _destBone == null || ikWeight <= 0.0f ) {
			return;
		}

		if( ikWeight > 1.0f ) {
			ikWeight = 1.0f;
		}

		bool ikWeightEnabled = (ikWeight != 1.0f);
		_destBone.PreUpdate_MarkIKDepended( true, ikWeightEnabled );
		_targetBone.PreUpdate_MarkIKDepended( false, ikWeightEnabled );
		for( int i = 0; i < _ikLinkList.Length; ++i ) {
			_ikLinkList[i].bone.PreUpdate_MarkIKDepended( false, ikWeightEnabled );
		}
	}

	public void MarkIKDepended( uint updateFlags, float ikWeight )
	{
		ikWeight = Mathf.Clamp01( ikWeight );
		_ikWeight = ikWeight;

		if( ( updateFlags & (uint)UpdateFlags.IKEnabled ) == 0 || _destBone == null || ikWeight == 0.0f ) {
			return;
		}

		bool ikWeightEnabled = (ikWeight != 1.0f);
		_destBone.MarkIKDepended( true, ikWeightEnabled );
		_targetBone.MarkIKDepended( false, ikWeightEnabled );
		for( int i = 0; i < _ikLinkList.Length; ++i ) {
			_ikLinkList[i].bone.MarkIKDepended( false, ikWeightEnabled );
		}
	}

	public void Solve()
	{
		// Cache solved transform results.
		if( !_model.isIKEnabled || _destBone == null || _isDisabled || _ikWeight == 0.0f ) {
			return;
		}
		
		bool isKeepIKTargetBone = _model.modelProperty.keepIKTargetBoneFlag != 0;
		
		IndexedQuaternion origTargetLocalRotation = IndexedQuaternion.Identity;
		if( isKeepIKTargetBone ) {
			origTargetLocalRotation = _targetBone.localRotation;
		}
		
		_PrepareIKTransform();

		if( !_ProcessCachedBoneTransform() ) {
			_PrefixCachedBoneTransform();

			if( _model.fileType == PMXFileType.PMD ) {
				_SolvePMD();
			} else if( _model.fileType == PMXFileType.PMX ) {
				_SolvePMX();
			}
			
			if( isKeepIKTargetBone ) {
				_targetBone.SetLocalRotationFromIK( ref origTargetLocalRotation );
				_targetBone.PerformTransformFromIK();
			}

			_PostfixCachedBoneTransform();
		}
		
		_PostfixIKTransform();
	}

	bool _ProcessCachedBoneTransform()
	{
		if( !_isCached ) {
			return false;
		}

		if( !_cachedDestBoneTransfrom._IsEqualWorld( _destBone ) ) {
			return false;
		}

		if( !_cachedTargetBoneTransfrom._IsEqual( _targetBone ) ) {
			return false;
		}

		for( int i = 0; i < _ikLinkList.Length; ++i ) {
			if( !_ikLinkList[i].cachedBoneTransfrom._IsEqual( _ikLinkList[i].bone ) ) {
				return false;
			}
		}

		_solvedTargetBoneTransfrom._Set( _targetBone );

		for( int i = 0; i < _ikLinkList.Length; ++i ) {
			_ikLinkList[i].solvedBoneTransfrom._Set( _ikLinkList[i].bone );
		}
		
		return true;
	}
	
	void _PrefixCachedBoneTransform()
	{
		_cachedDestBoneTransfrom._GetWorld( _destBone );
		_cachedTargetBoneTransfrom._Get( _targetBone );
		for( int i = 0; i < _ikLinkList.Length; ++i ) {
			_ikLinkList[i].cachedBoneTransfrom._Get( _ikLinkList[i].bone );
		}
	}
	
	void _PostfixCachedBoneTransform()
	{
		_solvedTargetBoneTransfrom._Get( _targetBone );
		for( int i = 0; i < _ikLinkList.Length; ++i ) {
			_ikLinkList[i].solvedBoneTransfrom._Get( _ikLinkList[i].bone );
		}
		
		_isCached = true;
	}

	void _PrepareIKTransform()
	{
		for( int i = _ikLinkList.Length - 1; i >= 0; --i ) {
			_ikLinkList[i].bone.PrepareIKTransform( _ikWeight );
		}
		
		_targetBone.PrepareIKTransform( _ikWeight );
	}

	void _PostfixIKTransform()
	{
		for( int i = _ikLinkList.Length - 1; i >= 0; --i ) {
			_ikLinkList[i].bone.PostfixIKTransform( _ikWeight );
		}
		
		_targetBone.PostfixIKTransform( _ikWeight );
	}

	static void _InnerLockR( ref float lowerAngle, ref float upperAngle, float innerLockScale )
	{
		float lm = Mathf.Max( (upperAngle - lowerAngle) * innerLockScale, 0.0f );
		float l = lowerAngle + lm; // Anti Gimbal Lock for 1st phase.(Inner lcok)
		float u = upperAngle - lm; // Anti Gimbal Lock for 1st phase.(Inner lcok)
		lowerAngle = l;
		upperAngle = u;
	}
	
	static void _InnerLockR( ref IndexedVector3 lowerAngle, ref IndexedVector3 upperAngle, float innerLockScale )
	{
		_InnerLockR( ref lowerAngle.X, ref upperAngle.X, innerLockScale );
		_InnerLockR( ref lowerAngle.Y, ref upperAngle.Y, innerLockScale );
		_InnerLockR( ref lowerAngle.Z, ref upperAngle.Z, innerLockScale );
	}
	
	static float _ClampEuler( float r, float lower, float upper, bool inverse )
	{
		if( r < lower ) {
			if( inverse ) {
				float inv = lower * 2.0f - r;
				if( inv <= upper ) {
					return inv;
				}
			}
			return lower;
		} else if( r > upper ) {
			if( inverse ) {
				float inv = upper * 2.0f - r;
				if( inv >= lower ) {
					return inv;
				}
			}
			
			return upper;
		}
		
		return r;
	}

	static bool _Normalize( ref IndexedVector3 v )
	{
		float len = v.Length();
		if( len > Mathf.Epsilon ) {
			v *= 1.0f / len;
			return true;
		}
		return false;
	}

	static IndexedVector3 _ComputeEulerZYX( ref IndexedBasisMatrix m )
	{
		float rx = 0, ry = 0, rz = 0;
		MMD4MecanimBulletPhysicsUtil.ComputeEulerZYX( ref m, ref rz, ref ry, ref rx );
		return new IndexedVector3( rx, ry, rz );
	}
	
	static IndexedQuaternion _GetRotationEulerZYX( ref IndexedVector3 r )
	{
		IndexedBasisMatrix m = MMD4MecanimBulletPhysicsUtil.EulerZYX( r.X, r.Y, r.Z );
		return m.GetRotation();
	}
	
	static bool _GetIKMuscle( PMXModel model, PMXBone ikBone, ref IndexedVector3 xyzMin, ref IndexedVector3 xyzMax )
	{
		if( model.modelProperty.enableIKMuscleFootFlag != 0 && ikBone.isFoot ) {
			float upperXAngle = model.modelProperty.muscleFootUpperXAngle; // 45.0
			float lowerXAngle = model.modelProperty.muscleFootLowerXAngle; // 90.0
			float innerYAngle = model.modelProperty.muscleFootInnerYAngle; // 25.0
			float outerYAngle = model.modelProperty.muscleFootOuterYAngle; // 25.0
			float innerZAngle = model.modelProperty.muscleFootInnerZAngle; // 0.0( Legacy 12.5 )
			float outerZAngle = model.modelProperty.muscleFootOuterZAngle; // 0.0
			if( ikBone.isLeft ) {
				xyzMin.X = -upperXAngle; xyzMax.X = lowerXAngle;
				xyzMin.Y = -innerYAngle; xyzMax.Y = outerYAngle;
				xyzMin.Z = -innerZAngle; xyzMax.Z = outerZAngle;
			} else {
				xyzMin.X = -upperXAngle; xyzMax.X = lowerXAngle;
				xyzMin.Y = -outerYAngle; xyzMax.Y = innerYAngle;
				xyzMin.Z = -outerZAngle; xyzMax.Z = innerZAngle;
			}
			return true;
		}
		
		if( model.modelProperty.enableIKMuscleHipFlag != 0 && ikBone.isHip ) {
			float upperXAngle = model.modelProperty.muscleHipUpperXAngle; // 176.0
			float lowerXAngle = model.modelProperty.muscleHipLowerXAngle; // 86.0
			float innerYAngle = model.modelProperty.muscleHipInnerYAngle; // 45.0
			float outerYAngle = model.modelProperty.muscleHipOuterYAngle; // 90.0
			float innerZAngle = model.modelProperty.muscleHipInnerZAngle; // 30.0
			float outerZAngle = model.modelProperty.muscleHipOuterZAngle; // 90.0
			if( ikBone.isLeft ) {
				xyzMin.X = -upperXAngle; xyzMax.X = lowerXAngle;
				xyzMin.Y = -innerYAngle; xyzMax.Y = outerYAngle;
				xyzMin.Z = -innerZAngle; xyzMax.Z = outerZAngle;
			} else {
				xyzMin.X = -upperXAngle; xyzMax.X = lowerXAngle;
				xyzMin.Y = -outerYAngle; xyzMax.Y = innerYAngle;
				xyzMin.Z = -outerZAngle; xyzMax.Z = innerZAngle;
			}
			return true;
		}
		
		return false;
	}
	
	static void _ClampIKMuscle( ref IndexedVector3 r, ref IndexedVector3 xyzMin, ref IndexedVector3 xyzMax )
	{
		r.X = Mathf.Clamp( r.X, xyzMin.X, xyzMax.X );
		r.Y = Mathf.Clamp( r.Y, xyzMin.Y, xyzMax.Y );
		r.Z = Mathf.Clamp( r.Z, xyzMin.Z, xyzMax.Z );
	}
	
	static void _IKMuscle( PMXModel model, PMXBone ikBone )
	{
		IndexedVector3 xyzMin = IndexedVector3.Zero;
		IndexedVector3 xyzMax = IndexedVector3.Zero;
		if( _GetIKMuscle( model, ikBone, ref xyzMin, ref xyzMax ) ) {
			IndexedBasisMatrix m = new IndexedBasisMatrix( ikBone.localRotation );
			IndexedQuaternion q = ikBone.localRotation.Inverse();

			IndexedVector3 r = _ComputeEulerZYX( ref m );
			_ClampIKMuscle( ref r, ref xyzMin, ref xyzMax );
			q *= _GetRotationEulerZYX( ref r );
			
			ikBone.ApplyLocalRotationFromIK( ref q );
		}
	}

	void _SolvePMD()
	{
		float innerLockKneeAngleR		= 0.0f;
		bool isEnableIKInnerLockKnee	= (_model.modelProperty.enableIKInnerLockKneeFlag != 0);
		if( isEnableIKInnerLockKnee && _isKnee ) {
			float innerLockKneeClamp	= _model.modelProperty.innerLockKneeClamp;
			float innerLockKneeRatioL	= _model.modelProperty.innerLockKneeRatioL;
			float innerLockKneeRatioU	= _model.modelProperty.innerLockKneeRatioU;
			float innerLockKneeScale	= _model.modelProperty.innerLockKneeScale;
			// _GetInnerAngleR = 0.0 ～ 1.0
			// innerLockScale = 0.25( 180 * 0.25 = 45 )
			innerLockKneeAngleR = _GetInnerLockKneeAngleR( innerLockKneeRatioL, innerLockKneeRatioU, innerLockKneeScale ) * innerLockKneeClamp;
		}
		
		IndexedVector3 destPos = _destBone.worldTransform._origin;
		IndexedVector3 muscleMin = IndexedVector3.Zero;
		IndexedVector3 muscleMax = IndexedVector3.Zero;

		int ikLinkListLength = _ikLinkList.Length;
		for( uint ite = 0; ite < _iteration; ++ite ) {
			bool processingAnything = false;
			for( int j = 0; j < ikLinkListLength; ++j ) {
				IndexedVector3 targetPos = _targetBone.worldTransform._origin;
				PMXBone childBone = _ikLinkList[j].bone;

				IndexedVector3 localDestVec = destPos;
				IndexedVector3 localTargetVec = targetPos;
				
				{
					IndexedMatrix inverseTransform = childBone.worldTransform.Inverse();
					localDestVec = inverseTransform * localDestVec;
					localTargetVec = inverseTransform * localTargetVec;
					if( !_Normalize( ref localDestVec ) || !_Normalize( ref localTargetVec ) ) {
						continue;
					}
					IndexedVector3 tempVec = localDestVec - localTargetVec;
					if( tempVec.Dot( ref tempVec ) < IK_SKIPDISTANCE ) {
						continue;
					}
				}
				
				//bool inverseAngle = limitFlag; // MMD
				bool inverseAngle = (ite == 0);

				processingAnything = true;
				IndexedVector3 axis = localTargetVec.Cross( ref localDestVec );
				
				if( childBone.isKnee /* && limitFlag */ ) {
					if( axis.X >= 0.0f ) {
						axis = new IndexedVector3( 1.0f, 0.0f, 0.0f );
					} else {
						axis = new IndexedVector3( -1.0f, 0.0f, 0.0f );
					}
				} else {
					if( !_Normalize( ref axis ) ) {
						continue;
					}
				}
				
				float dot = localTargetVec.Dot( ref localDestVec );
				dot = Mathf.Clamp( dot, -1.0f, 1.0f );
				
				float rx = Mathf.Acos(dot) * 0.5f;
				rx = Mathf.Min( rx, _angleConstraint * (float)((j + 1) * 2) );
				
				float rs = Mathf.Sin( rx );

				IndexedQuaternion q = new IndexedQuaternion( axis.X * rs, axis.Y * rs, axis.Z * rs, Mathf.Cos( rx ) );

				if( childBone.isKnee /* && limitFlag */ ) {
					float lowerLimit = 0.5f * Mathf.Deg2Rad;
					float upperLimit = 180.0f * Mathf.Deg2Rad;
					
					childBone.ApplyLocalRotationFromIK( ref q );
					IndexedQuaternion q2 = childBone.localRotation.Inverse();

					IndexedBasisMatrix m = new IndexedBasisMatrix( ref childBone.localRotation );
					rx = MMD4MecanimBulletPhysicsUtil.ComputeEulerX( ref m );
					if( ite == 0 && isEnableIKInnerLockKnee ) {
						_InnerLockR( ref lowerLimit, ref upperLimit, innerLockKneeAngleR );
					}
					
					rx = _ClampEuler( rx, lowerLimit, upperLimit, inverseAngle );
					
					if( _GetIKMuscle( _model, childBone, ref muscleMin, ref muscleMax ) ) {
						rx = Mathf.Clamp( rx, muscleMin.X, muscleMax.X );
					}

					q2 *= MMD4MecanimBulletPhysicsUtil.QuaternionX( rx );
					childBone.ApplyLocalRotationFromIK( ref q2 );
				} else {
					childBone.ApplyLocalRotationFromIK( ref q );
					_IKMuscle( _model, childBone );
				}
				
				for( int i = j; i >= 0; --i ) {
					_ikLinkList[i].bone.PerformTransformFromIK();
				}
				
				_targetBone.PerformTransformFromIK();
			}
			if( !processingAnything ) {
				break;
			}
		}
	}

	void _SolvePMX()
	{
		float innerLockKneeAngleR		= 0.0f;
		bool isEnableIKInnerLockKnee	= (_model.modelProperty.enableIKInnerLockKneeFlag != 0);
		if( isEnableIKInnerLockKnee && _isKnee ) {
			float innerLockKneeClamp	= _model.modelProperty.innerLockKneeClamp;
			float innerLockKneeRatioL	= _model.modelProperty.innerLockKneeRatioL;
			float innerLockKneeRatioU	= _model.modelProperty.innerLockKneeRatioU;
			float innerLockKneeScale	= _model.modelProperty.innerLockKneeScale;
			// _GetInnerAngleR = 0.0 ～ 1.0
			// innerLockScale = 0.25( 180 * 0.25 = 45 )
			innerLockKneeAngleR = _GetInnerLockKneeAngleR( innerLockKneeRatioL, innerLockKneeRatioU, innerLockKneeScale ) * innerLockKneeClamp;
		}

		IndexedVector3 destPos = _destBone.worldTransform._origin;
		IndexedVector3 muscleMin = IndexedVector3.Zero;
		IndexedVector3 muscleMax = IndexedVector3.Zero;

		int ikLinkListLength = _ikLinkList.Length;
		for( int ite = 0; ite < _iteration; ++ite ) {
			bool processingAnything = false;
			for( int j = 0; j < ikLinkListLength; ++j ) {
				IndexedVector3 targetPos = _targetBone.worldTransform._origin;
				PMXBone childBone = _ikLinkList[j].bone;
				
				IndexedVector3 localDestVec = destPos;
				IndexedVector3 localTargetVec = targetPos;
				
				{
					IndexedMatrix inverseTransform = childBone.worldTransform.Inverse();
					localDestVec = inverseTransform * localDestVec;
					localTargetVec = inverseTransform * localTargetVec;
					if( !_Normalize( ref localDestVec ) || !_Normalize( ref localTargetVec ) ) {
						continue;
					}
					IndexedVector3 tempVec = localDestVec - localTargetVec;
					if( tempVec.Dot( ref tempVec ) < IK_SKIPDISTANCE ) {
						continue;
					}
				}
				
				bool hasAngleJoint = _ikLinkList[j].hasAngleJoint;
				IKLinkLimitType linkLimitType = _ikLinkList[j].ikLinkLimitType;

				processingAnything = true;
				IndexedVector3 axis = localTargetVec.Cross( ref localDestVec );

				//if( hasAngleJoint /*&& limitFlag*/ ) {
				if( hasAngleJoint /*&& limitFlag*/ ) {
					if( linkLimitType == IKLinkLimitType.X ) {
						if( axis.X >= 0.0f ) {
							axis = new IndexedVector3( 1.0f, 0.0f, 0.0f );
						} else {
							axis = new IndexedVector3( -1.0f, 0.0f, 0.0f );
						}
					} else if( linkLimitType == IKLinkLimitType.Y ) {
						/* Y Limit */
						if( axis.Y >= 0.0f ) {
							axis = new IndexedVector3( 0.0f, 1.0f, 0.0f );
						} else {
							axis = new IndexedVector3( 0.0f, -1.0f, 0.0f );
						}
					} else if( linkLimitType == IKLinkLimitType.Z ) {
						/* Z Limit */
						if( axis.Z >= 0.0f ) {
							axis = new IndexedVector3( 0.0f, 0.0f, 1.0f );
						} else {
							axis = new IndexedVector3( 0.0f, 0.0f, -1.0f );
						}
					} else {
						if( !_Normalize( ref axis ) ) {
							continue;
						}
					}
				} else {
					if( !_Normalize( ref axis ) ) {
						continue;
					}
				}
				
				float dot = localTargetVec.Dot( ref localDestVec );
				dot = Mathf.Clamp( dot, -1.0f, 1.0f );
				
				float rx = Mathf.Acos(dot) * 0.5f;
				rx = Mathf.Min( rx, _angleConstraint * (float)((j + 1) * 2) );
				
				float rs = Mathf.Sin( rx );
				
				IndexedQuaternion q = new IndexedQuaternion( axis.X * rs, axis.Y * rs, axis.Z * rs, Mathf.Cos( rx ) );

				//const bool inverseAngle = limitFlag; // MMD
				bool inverseAngle = (ite == 0);
				
				if( hasAngleJoint /*&& limitFlag*/ ) {
					IndexedQuaternion q2 = childBone.localRotation * q;
					IndexedBasisMatrix m = new IndexedBasisMatrix( ref q2 );

					IndexedVector3 lowerLimit = _ikLinkList[j].lowerLimit;
					IndexedVector3 upperLimit = _ikLinkList[j].upperLimit;
					if( isEnableIKInnerLockKnee && ite == 0 && childBone.isKnee ) {
						_InnerLockR( ref lowerLimit, ref upperLimit, innerLockKneeAngleR );
					}
					IKLinkFreeLimitType linkFreeLimitType = _ikLinkList[j].ikLinkFreeLimitType;

					bool isMuscle = _GetIKMuscle( _model, childBone, ref muscleMin, ref muscleMax );
					
					if( linkLimitType == IKLinkLimitType.X ) {
						// X Limit
						rx = MMD4MecanimBulletPhysicsUtil.ComputeEulerX( ref m );
						rx = _ClampEuler( rx, lowerLimit.X, upperLimit.X, inverseAngle );
						if( isMuscle ) {
							rx = Mathf.Clamp( rx, muscleMin.X, muscleMax.X );
						}
						q2 = MMD4MecanimBulletPhysicsUtil.QuaternionX( rx );
					} else if( linkLimitType == IKLinkLimitType.Y ) {
						// Y Limit
						float ry = MMD4MecanimBulletPhysicsUtil.ComputeEulerY( ref m );
						// Anti Gimbal Lock
						ry = _ClampEuler( ry, lowerLimit.Y, upperLimit.Y, inverseAngle );
						if( isMuscle ) {
							ry = Mathf.Clamp( ry, muscleMin.Y, muscleMax.Y );
						}
						q2 = MMD4MecanimBulletPhysicsUtil.QuaternionY( ry );
					} else if( linkLimitType == IKLinkLimitType.Z ) {
						// Z Limit
						float rz = MMD4MecanimBulletPhysicsUtil.ComputeEulerZ( ref m );
						rz = _ClampEuler( rz, lowerLimit.Z, upperLimit.Z, inverseAngle );
						if( isMuscle ) {
							rz = Mathf.Clamp( rz, muscleMin.Z, muscleMax.Z );
						}
						q2 = MMD4MecanimBulletPhysicsUtil.QuaternionZ( rz );
					} else {
						IndexedVector3 r = _ComputeEulerZYX( ref m );
						// Anti Gimbal Lock
						if( linkFreeLimitType == IKLinkFreeLimitType.X ) {
							r.X = Mathf.Clamp( r.X, -176.0f * Mathf.Deg2Rad, 176.0f * Mathf.Deg2Rad );
						} else if( linkFreeLimitType == IKLinkFreeLimitType.Y ) {
							r.Y = Mathf.Clamp( r.Y, -176.0f * Mathf.Deg2Rad, 176.0f * Mathf.Deg2Rad );
						} else {
							r.Z = Mathf.Clamp( r.Z, -176.0f * Mathf.Deg2Rad, 176.0f * Mathf.Deg2Rad );
						}
						r.X = _ClampEuler( r.X, lowerLimit.X, upperLimit.X, inverseAngle );
						r.Y = _ClampEuler( r.Y, lowerLimit.Y, upperLimit.Y, inverseAngle );
						r.Z = _ClampEuler( r.Z, lowerLimit.Z, upperLimit.Z, inverseAngle );
						if( isMuscle ) {
							_ClampIKMuscle( ref r, ref muscleMin, ref muscleMax );
						}
						q2 = _GetRotationEulerZYX( ref r );
					}
					
					IndexedQuaternion q3 = childBone.localRotation.Inverse() * q2;
					childBone.ApplyLocalRotationFromIK( ref q3 );
				} else {
					childBone.ApplyLocalRotationFromIK( ref q );
					_IKMuscle( _model, childBone );
				}
				
				for( int i = j; i >= 0; --i ) {
					_ikLinkList[i].bone.PerformTransformFromIK();
				}
				
				_targetBone.PerformTransformFromIK();
			}
			if( !processingAnything ) {
				break;
			}
		}
	}

	float _GetInnerLockKneeAngleR( float innerLockRatioL, float innerLockRatioU, float innerLockScale )
	{
		int ikLinkListLength = _ikLinkList.Length;
		if( ikLinkListLength == 0 ) {
			return 0.0f;
		}

		IndexedVector3 destPos = _destBone.worldTransform._origin;
		IndexedVector3 targetPos = _targetBone.worldTransform._origin;
		IndexedVector3 rootPos = _ikLinkList[ikLinkListLength - 1].bone.worldTransform._origin;
		float length0 = (destPos - rootPos).LengthSquared();
		float length1 = (targetPos - rootPos).LengthSquared();
		
		float innerAngleR = 0.0f;
		if( length1 > Mathf.Epsilon ) {
			if( length0 < length1 ) { // Inner
				float r = length0 / length1;
				if( r > 1.0f - innerLockRatioL ) {
					innerAngleR = (r - (1.0f - innerLockRatioL)) / innerLockRatioL;
				}
			} else if( length0 - length1 < length1 * innerLockRatioU ) { // Outer
				innerAngleR = 1.0f - (length0 - length1) / (length1 * innerLockRatioU);
			}
			innerAngleR = Mathf.Clamp( innerAngleR * innerLockScale, 0.0f, 1.0f );
		}
		
		return innerAngleR;
	}
}
