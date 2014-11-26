using UnityEngine;
using System.Collections;
using BulletXNA;
using BulletXNA.BulletCollision;
using BulletXNA.BulletDynamics;
using BulletXNA.LinearMath;

public class MMD4MecanimBulletRigidBody : MMD4MecanimBulletPhysicsEntity
{
	// Pending: Replace to MMD4MecanimData

	bool						_isKinematic = false;
	int							_group = -1;
	int							_mask = -1;
	float						_worldToBulletScale = 0.0f;
	float						_bulletToWorldScale = 0.0f;
	CollisionShape				_shape;
	IMotionState				_motionState;
	RigidBody					_body;
	DiscreteDynamicsWorld		_world;

	bool						_isFreezed;
	bool						_isJoinedWorld;
	bool						_isDestroyed;
	bool						_isMultiThreading;

	UpdateData					_sync_unusedData;
	UpdateData					_sync_updateData;
	UpdateData					_sync_updatedData;
	UpdateData					_sync_lateUpdateData;
	
	UpdateData					_updateData;

	public struct CreateProperty
	{
		public bool			isKinematic;
		public bool			isAdditionalDamping;
		public int			group;
		public int			mask;
		public int			shapeType;
		public Vector3		shapeSize;
		public Vector3		position;
		public Quaternion	rotation;
		public float		mass;
		public float		linearDamping;
		public float		angularDamping;
		public float		restitution;
		public float		friction;
		public float		unityScale;
	}
	
	public class UpdateData
	{
		public int			updateFlags;
		public bool			updateTransform;
		public bool			lateUpdateTransform;
		public Vector3		updatePosition;
		public Quaternion	updateRotation;
		public Vector3		lateUpdatePosition;
		public Quaternion	lateUpdateRotation;
	}

	public MMD4MecanimBulletRigidBody()
	{
	}

	~MMD4MecanimBulletRigidBody()
	{
		_DestroyImmediate();
	}

	public bool Create( ref CreateProperty createProperty )
	{
		_isKinematic = createProperty.isKinematic;
		_group = createProperty.group;
		_mask = createProperty.mask;
		
		_bulletToWorldScale = 1.0f;
		_worldToBulletScale = 1.0f;
		if( createProperty.unityScale > Mathf.Epsilon ) {
			_bulletToWorldScale = createProperty.unityScale;
			_worldToBulletScale = 1.0f / _bulletToWorldScale;
		}
		
		Vector3 shapeSize = createProperty.shapeSize * _worldToBulletScale;

		switch( createProperty.shapeType ) {
		case 0:
			_shape = new SphereShape( shapeSize.x );
			break;
		case 1:
			_shape = new BoxShape( new IndexedVector3( shapeSize.x, shapeSize.y, shapeSize.z ) );
			break;
		case 2:
			_shape = new CapsuleShape( shapeSize.x, shapeSize.y );
			break;
		default:
			return false;
		}

		Vector3 position = createProperty.position;

		position.x = -position.x; // Unity to Bullet Position
		position *= _worldToBulletScale; // Unity to Bullet Position

		Quaternion rotation = createProperty.rotation;
		rotation.y = -rotation.y;
		rotation.z = -rotation.z;

		IndexedMatrix bodyTransform = MMD4MecanimBulletPhysicsUtil.MakeIndexedMatrix( ref position, ref rotation );
		
		if( _isKinematic ) {
			_motionState = new MMD4MecanimBulletPhysicsUtil.KinematicMotionState( ref bodyTransform );
		} else {
			_motionState = new MMD4MecanimBulletPhysicsUtil.SimpleMotionState( ref bodyTransform );
		}
		
		float mass = _isKinematic ? 0.0f : createProperty.mass;
        bool isDynamic = mass != 0.0f;
        IndexedVector3 localInertia = IndexedVector3.Zero;
        if( isDynamic ) {
	        _shape.CalculateLocalInertia(mass, out localInertia);
        }
		
        RigidBodyConstructionInfo rbInfo = new RigidBodyConstructionInfo(mass, _motionState, _shape, localInertia);
		rbInfo.m_additionalDamping = createProperty.isAdditionalDamping;
        _body = new RigidBody(rbInfo);

		if( _isKinematic ) {
			_body.SetCollisionFlags( _body.GetCollisionFlags() | BulletXNA.BulletCollision.CollisionFlags.CF_KINEMATIC_OBJECT );
			_body.SetActivationState(ActivationState.DISABLE_DEACTIVATION);
		}
		
		return true;
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

	UpdateData _PrepareUpdate()
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

		return updateData;
	}

	void _PostUpdate( UpdateData updateData )
	{
		if( _isMultiThreading ) {
			lock(this) {
				_sync_updateData = updateData;
			}
		} else {
			_sync_updateData = updateData;
		}
	}

	public void Update( int updateFlags )
	{
		UpdateData updateData = _PrepareUpdate();
		updateData.updateFlags = updateFlags;
		updateData.updateTransform = false;
		_PostUpdate( updateData );
	}

	public void Update( int updateFlags, ref Vector3 position, ref Quaternion rotation )
	{
		UpdateData updateData = _PrepareUpdate();
		updateData.updateFlags = updateFlags;
		updateData.updateTransform = true;
		updateData.updatePosition = position;
		updateData.updateRotation = rotation;
		_PostUpdate( updateData );
	}

	public int LateUpdate( ref Vector3 position, ref Quaternion rotation )
	{
		UpdateData updateData = null;
		if( _isMultiThreading ) {
			lock(this) {
				updateData = _sync_lateUpdateData;
			}
		} else {
			updateData = _sync_lateUpdateData;
		}

		if( updateData == null ) {
			return 0;
		}
		
		// Simulated only.
		if( updateData.lateUpdateTransform ) {
			position = updateData.lateUpdatePosition;
			rotation = updateData.lateUpdateRotation;
			return 1;
		} else {
			return 0;
		}
	}

	void _DestroyImmediate()
	{
		if( _body != null ) {
			_body.Cleanup();
			_body = null;
		}
		_motionState = null;
		_shape = null;
	}

	//--------------------------------------------------------------------------------------------------------------------

	void _LockUpdateData()
	{
		if( _isMultiThreading ) {
			lock(this) {
				_updateData = _sync_updateData;
				_sync_updateData = null;
			}
		} else {
			_updateData = _sync_updateData;
			_sync_updateData = null;
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
	
	void _FeedbackUpdateData()
	{
		if( _updateData == null ) {
			return;
		}
		
		_isFreezed = ((_updateData.updateFlags & 0x01) != 0);

		if( _updateData.updateTransform ) {
			bool isKinematic = _isKinematic;
			if( isKinematic || _isFreezed ) {
				Vector3 position = _updateData.updatePosition;
				Quaternion rotation = _updateData.updateRotation;

				position.x = -position.x; // Unity to Bullet Position
				position *= _worldToBulletScale; // Unity to Bullet Position

				rotation.y = -rotation.y; // Unity to Bullet Rotation
				rotation.z = -rotation.z; // Unity to Bullet Rotation

				if( _isKinematic ) {
					if( _motionState != null ) {
						MMD4MecanimBulletPhysicsUtil.MakeIndexedMatrix(
							ref ((MMD4MecanimBulletPhysicsUtil.KinematicMotionState)_motionState).m_graphicsWorldTrans,
							ref position, ref rotation );
					}
				} else if( _isFreezed ) {
					if( _body != null ) {
						IndexedMatrix m = IndexedMatrix.Identity;
						MMD4MecanimBulletPhysicsUtil.MakeIndexedMatrix( ref m, ref position, ref rotation );
						_body.SetCenterOfMassTransform( ref m );
					}
				}
			}
		}
	}
	
	void _FeedbackLateUpdateData()
	{
		if( _updateData == null ) {
			return;
		}

		_updateData.lateUpdateTransform = false;

		bool isKinematic = _isKinematic;
		if( !isKinematic && !_isFreezed ) {
			if( _motionState != null ) {
				_updateData.lateUpdateTransform = true;

				IndexedMatrix m;
				_motionState.GetWorldTransform( out m );

				_updateData.lateUpdatePosition = m._origin;
				_updateData.lateUpdateRotation = m.GetRotation();

				_updateData.lateUpdatePosition.x = -_updateData.lateUpdatePosition.x; // Bullet to Unity Position
				_updateData.lateUpdatePosition *= _bulletToWorldScale; // Bullet to Unity Position

				_updateData.lateUpdateRotation.y = -_updateData.lateUpdateRotation.y; // Bullet to Unity Rotation
				_updateData.lateUpdateRotation.z = -_updateData.lateUpdateRotation.z; // Bullet to Unity Rotation
			}
		}
	}
	
	//--------------------------------------------------------------------------------------------------------------------

	public override bool _JoinWorld()
	{
		if( _body == null ) {
			return false;
		}
		if( this.physicsWorld == null || this.bulletWorld == null ) {
			return false;
		}

		_isJoinedWorld = true;
		_isMultiThreading = this.physicsWorld.isMultiThreading;

		_world = this.bulletWorld;
		if( _group < 0 && _mask < 0 ) {
			_world.AddRigidBody( _body );
		} else {
			unchecked {
				_world.AddRigidBody( _body, (CollisionFilterGroups)_group, (CollisionFilterGroups)_mask );
			}
		}

		return true;
	}

	public override void _LeaveWorld()
	{
		if( _body != null ) {
            if( _world != null ) {
    			_world.RemoveRigidBody( _body );
            }
		}
		
		_world = null;

		_isJoinedWorld = false;
		_isMultiThreading = false;
		
		if( _isDestroyed ) {
			_DestroyImmediate();
		}
	}

	public override void _PreUpdate()
	{
		_LockUpdateData();
		_FeedbackUpdateData();
	}
	
	public override void _PostUpdate()
	{
		_FeedbackLateUpdateData();
		_UnlockUpdateData();
	}

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
}
