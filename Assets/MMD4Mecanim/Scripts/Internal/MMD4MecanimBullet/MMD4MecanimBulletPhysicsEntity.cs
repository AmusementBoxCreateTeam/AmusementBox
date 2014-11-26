using UnityEngine;
using System.Collections;
using BulletXNA;
using BulletXNA.BulletCollision;
using BulletXNA.BulletDynamics;
using BulletXNA.LinearMath;

public class MMD4MecanimBulletPhysicsEntity
{
	// from MMD4MecanimBulletPhysicsWorld
	public MMD4MecanimBulletPhysicsWorld _physicsWorld;
	public bool _isUpdateAtLeastOnce;

	public MMD4MecanimBulletPhysicsWorld physicsWorld {
		get {
			return _physicsWorld;
		}
	}
	
	public DiscreteDynamicsWorld bulletWorld {
		get {
			if( _physicsWorld != null ) {
				return _physicsWorld.bulletWorld;
			}

			return null;
		}
	}
	
	public void LeaveWorld()
	{
		_LeaveWorld();
		if( _physicsWorld != null ) {
			_physicsWorld.WaitEndThreading();
			_physicsWorld._RemoveEntity( this );
			_physicsWorld = null;
			_isUpdateAtLeastOnce = false;
		}
	}

	// from MMD4MecanimBulletPhysicsWorld
	public virtual bool _JoinWorld()
	{
		return false;
	}

	// from MMD4MecanimBulletPhysicsWorld
	public virtual void _LeaveWorld()
	{
	}

	// from MMD4MecanimBulletPhysicsWorld
	public virtual void _PreUpdate()
	{
	}

	// from MMD4MecanimBulletPhysicsWorld
	public virtual void _PreUpdateWorld( float deltaTime )
	{
	}

	// from MMD4MecanimBulletPhysicsWorld
	public virtual void _PostUpdateWorld( float deltaTime )
	{
	}

	// from MMD4MecanimBulletPhysicsWorld
	public virtual void _NoUpdateWorld()
	{
	}

	// from MMD4MecanimBulletPhysicsWorld
	public virtual void _PostUpdate()
	{
	}

	// from MMD4MecanimBulletPhysicsWorld
	public virtual void _PrepareLateUpdate()
	{
	}

	// from MMD4MecanimBulletPhysicsWorld
	public virtual float _GetResetWorldTime()
	{
		return 0.0f;
	}

	// from MMD4MecanimBulletPhysicsWorld
	public virtual void _PreResetWorld()
	{
	}

	// from MMD4MecanimBulletPhysicsWorld
	public virtual void _StepResetWorld( float elapsedTime )
	{
	}

	// from MMD4MecanimBulletPhysicsWorld
	public virtual void _PostResetWorld()
	{
	}
}
