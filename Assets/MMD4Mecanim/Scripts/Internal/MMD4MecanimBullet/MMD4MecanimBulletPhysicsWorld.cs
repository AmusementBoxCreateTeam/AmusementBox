﻿//#define USE_CACHEDTHREAD
using UnityEngine;
using System.Collections;
using BulletXNA;
using BulletXNA.BulletCollision;
using BulletXNA.BulletDynamics;
using BulletXNA.LinearMath;

using WorldProperty			= MMD4MecanimBulletPhysics.WorldProperty;
using WorldUpdateProperty	= MMD4MecanimBulletPhysics.WorldUpdateProperty;

using CachedThreadQueue		= MMD4MecanimBulletPhysicsUtil.CachedThreadQueue;
using ThreadQueueHandle		= MMD4MecanimBulletPhysicsUtil.ThreadQueueHandle;
using CachedThread			= MMD4MecanimBulletPhysicsUtil.CachedThread;

public class MMD4MecanimBulletPhysicsWorld
{
	#if USE_CACHEDTHREAD
	#else
	static CachedThreadQueue _cachedThreadQueue = null;
	#endif

	public const int DefaultFramePerSecond = 60;
	public const int DefaultResetFrameRate = 10;
	public const int DefaultLimitDeltaFrames = 2;

	DefaultCollisionConfiguration		_collisionConfig;
	CollisionDispatcher					_dispatcher;
	IBroadphaseInterface				_broadphase;
	SequentialImpulseConstraintSolver	_solver;
	DiscreteDynamicsWorld				_world;

	WorldProperty						_worldProperty;
	int									_maxSubSteps;
	float								_subStep;
	float								_elapsedTime;
	bool								_multiThreading;
	bool								_isRunningThread;

	#if USE_CACHEDTHREAD
	CachedThread						_cachedThread = null;
	#else
	ThreadQueueHandle					_threadQueueHandle = new ThreadQueueHandle();
	#endif

	public bool isMultiThreading { get { return _multiThreading; } }

	ArrayList 							_physicsEntityList = new ArrayList();
	
	public DiscreteDynamicsWorld bulletWorld {
		get {
			return _world;
		}
	}

	public bool Create( WorldProperty worldProperty )
	{
		Destroy();

		_worldProperty = worldProperty;
		if( _worldProperty == null ) {
			_worldProperty = new WorldProperty();
		}

		if( _worldProperty.optimizeBulletXNA ) {
			_worldProperty.accurateStep = false;
		}

		if( _worldProperty.framePerSecond <= 0 ) {
			_worldProperty.framePerSecond = DefaultFramePerSecond;
		}
		if( _worldProperty.resetFrameRate <= 0 ) {
			_worldProperty.resetFrameRate = DefaultResetFrameRate;
		}
		if( _worldProperty.limitDeltaFrames <= 0 ) {
			_worldProperty.limitDeltaFrames = DefaultLimitDeltaFrames;
		}

		if( _worldProperty.optimizeBulletXNA ) {
			if( _worldProperty.framePerSecond > DefaultFramePerSecond ) {
				_worldProperty.framePerSecond = DefaultFramePerSecond;
			}
			if( _worldProperty.axisSweepDistance <= 0.0f ) {
				_worldProperty.axisSweepDistance = 1000.0f;
			}
		}

		_maxSubSteps = _worldProperty.framePerSecond; // = 1.0 second.
		_subStep = 1.0f / (float)_worldProperty.framePerSecond;

		_collisionConfig = new DefaultCollisionConfiguration();
		_dispatcher = new CollisionDispatcher(_collisionConfig);
	
		if( _worldProperty.axisSweepDistance > 0.0f ) {
			float dist = _worldProperty.axisSweepDistance;
			IndexedVector3 worldMin = new IndexedVector3(-dist, -dist, -dist);
			IndexedVector3 worldMax = -worldMin;
			_broadphase = new AxisSweep3Internal(ref worldMin, ref worldMax, 0xfffe, 0xffff, 16384, null, false);
		} else {
			_broadphase = new DbvtBroadphase();
		}

        _solver = new SequentialImpulseConstraintSolver();
        _world = new DiscreteDynamicsWorld(_dispatcher, _broadphase, _solver, _collisionConfig);

		IndexedVector3 worldGravity = new IndexedVector3(0.0f, -9.8f * _worldProperty.gravityScale, 0.0f);
		if( _worldProperty.gravityDirection != new Vector3( 0.0f, -1.0f, 0.0f ) ) {
			Vector3 v = _worldProperty.gravityDirection;
			v.z = -v.z;
			float len = v.magnitude;
			if( len > Mathf.Epsilon ) {
				worldGravity = v * (1.0f / len) * (9.8f * _worldProperty.gravityScale);
			} else {
				worldGravity = IndexedVector3.Zero;
			}
		}
		_world.SetGravity(ref worldGravity);

		if( _world.GetSolverInfo() != null ) {
			if( _worldProperty.worldSolverInfoNumIterations <= 0 ) {
				_world.GetSolverInfo().m_numIterations = (int)(10 * 60 / _worldProperty.framePerSecond);
				if( _worldProperty.optimizeBulletXNA ) {
					_world.GetSolverInfo().m_numIterations /= 2;
				}
			} else {
				_world.GetSolverInfo().m_numIterations = _worldProperty.worldSolverInfoNumIterations;
			}
		}

		if( !_worldProperty.optimizeBulletXNA ) {
			_world.GetSolverInfo().m_splitImpulse = worldProperty.worldSolverInfoSplitImpulse;
		}

		_multiThreading = worldProperty.multiThreading;
		if( _multiThreading ) {
			#if USE_CACHEDTHREAD
			_cachedThread = new CachedThread();
			#else
			if( _cachedThreadQueue == null ) {
				_cachedThreadQueue = new CachedThreadQueue( 1 ); // Bullet XNA use static variables
			}
			#endif
		}
		return true;
	}

	public void Destroy()
	{
		WaitEndThreading();

		while( _physicsEntityList.Count > 0 ) {
			int index = _physicsEntityList.Count - 1;
			MMD4MecanimBulletPhysicsEntity physicsEntity = (MMD4MecanimBulletPhysicsEntity)(_physicsEntityList[index]);
			_physicsEntityList.RemoveAt( index );
			physicsEntity.LeaveWorld();
		}
		
		if( _world != null ) {
			_world.Cleanup();
			_world = null;
		}
		if( _solver != null ) {
			_solver.Cleanup();
			_solver = null;
		}
		if( _broadphase != null ) {
			_broadphase.Cleanup();
			_broadphase = null;
		}
		if( _dispatcher != null ) {
			_dispatcher.Cleanup();
			_dispatcher = null;
		}
		if( _collisionConfig != null ) {
			_collisionConfig.Cleanup();
			_collisionConfig = null;
		}
	}

	public void SetGravity( float gravityScale, float gravityNoise, Vector3 gravityDirection )
	{
		if( _world == null ) {
			return;
		}

		IndexedVector3 worldGravity = new IndexedVector3( 0.0f, -9.8f * gravityScale, 0.0f );
		if( gravityDirection != new Vector3( 0.0f, -1.0f, 0.0f ) ) {
			Vector3 v = gravityDirection;
			v.z = -v.z;
			float len = v.magnitude;
			if( len > Mathf.Epsilon ) {
				worldGravity = v * (1.0f / len) * (9.8f * gravityScale);
			} else {
				worldGravity = IndexedVector3.Zero;
			}
		}

		_world.SetGravity( ref worldGravity );
	}

	public void Update( float deltaTime, WorldUpdateProperty worldUpdateProperty )
	{
		WaitEndThreading();

		if( worldUpdateProperty != null ) {
			SetGravity( worldUpdateProperty.gravityScale, worldUpdateProperty.gravityNoise, worldUpdateProperty.gravityDirection );
		}

		float resetWorldTime = 0.0f;
		bool isSyncUpdate = false;
		for( int i = 0; i < _physicsEntityList.Count; ++i ) {
			resetWorldTime = Mathf.Max( ((MMD4MecanimBulletPhysicsEntity)_physicsEntityList[i])._GetResetWorldTime(), resetWorldTime );
			if( _multiThreading ) {
				isSyncUpdate |= !((MMD4MecanimBulletPhysicsEntity)_physicsEntityList[i])._isUpdateAtLeastOnce;
			}
		}
	
		if( resetWorldTime > 0.0f ) {
			for( int i = 0; i < _physicsEntityList.Count; ++i ) {
				((MMD4MecanimBulletPhysicsEntity)_physicsEntityList[i])._PreResetWorld();
			}

			for( int i = 0; i < _physicsEntityList.Count; ++i ) {
				((MMD4MecanimBulletPhysicsEntity)_physicsEntityList[i])._StepResetWorld( 0.0f );
			}
			
			float elapsedTime = 0.0f;
			bool finalStep = false;
			float resetWorldFPSInv = 1.0f / (float)_worldProperty.resetFrameRate;
			while( elapsedTime < resetWorldTime && !finalStep ) {
				_Entity_Update( resetWorldFPSInv );
				elapsedTime += resetWorldFPSInv;
				if( elapsedTime > resetWorldTime ) {
					elapsedTime = resetWorldTime;
					finalStep = true;
				}
				for( int i = 0; i < _physicsEntityList.Count; ++i ) {
					((MMD4MecanimBulletPhysicsEntity)_physicsEntityList[i])._StepResetWorld( elapsedTime );
				}
			}
			
			for( int i = 0; i < _physicsEntityList.Count; ++i ) {
				((MMD4MecanimBulletPhysicsEntity)_physicsEntityList[i])._PostResetWorld();
			}
		}

		if( !_multiThreading || isSyncUpdate ) { // Todo: _isUpdateAtLeastOnce per entity
			_InternalUpdate( deltaTime );
			_Entity_PrepareLateUpdate();
			if( _multiThreading ) {
				for( int i = 0; i != _physicsEntityList.Count; ++i ) {
					((MMD4MecanimBulletPhysicsEntity)_physicsEntityList[i])._isUpdateAtLeastOnce = true;
				}
			}
		} else {
			_Invoke( deltaTime );
		}
	}

	public void JoinWorld( MMD4MecanimBulletPhysicsEntity physicsEntity )
	{
		if( physicsEntity == null ) {
			Debug.LogError("");
			return;
		}
		if( physicsEntity.physicsWorld != null ) {
			Debug.LogError("");
			return;
		}

		WaitEndThreading();

		physicsEntity._physicsWorld = this;
		if( !physicsEntity._JoinWorld() ) {
			Debug.LogError("");
			physicsEntity._physicsWorld = null;
		} else {
			_physicsEntityList.Add( physicsEntity );
		}
	}

	void _Invoke( float deltaTime )
	{
		_isRunningThread = true;
		#if USE_CACHEDTHREAD
		_cachedThread.Invoke( () => { this._InternalUpdate(deltaTime); } );
		#else
		_threadQueueHandle = _cachedThreadQueue.Invoke( () => { this._InternalUpdate(deltaTime); } );
		#endif
	}
	
	void _InternalUpdate( float deltaTime )
	{
		{
			float deltaTimeLimitRate = (float)_worldProperty.limitDeltaFrames;
			float limitDeltaTime = deltaTimeLimitRate / (float)_worldProperty.framePerSecond;
			if( deltaTime >= limitDeltaTime ) {
				deltaTime = limitDeltaTime;
			}
		}

		_Entity_PreUpdate();

		bool updatedAnything = false;
		_elapsedTime += deltaTime;
		if( _subStep > 0.0f ) {
			if( _worldProperty.accurateStep ) {
				while( _elapsedTime >= _subStep ) {
					updatedAnything = true;
					_Entity_Update( _subStep );
					_elapsedTime -= _subStep;
				}
			} else {
				float subStepAll = 0.0f;
				while( _elapsedTime >= _subStep ) {
					subStepAll += _subStep;
					_elapsedTime -= _subStep;
				}
				if( subStepAll != 0.0f ) {
					updatedAnything = true;
					_Entity_Update( subStepAll );
				}
			}
		}
		if( !updatedAnything ) {
			_Entity_NoUpdate();
		}
		
		_Entity_PostUpdate();
	}

	void _Entity_PreUpdate()
	{
		for( int i = 0; i != _physicsEntityList.Count; ++i ) {
			((MMD4MecanimBulletPhysicsEntity)_physicsEntityList[i])._PreUpdate();
		}
	}

	void _Entity_Update( float deltaTime )
	{
		if( _world != null ) {
			for( int i = 0; i != _physicsEntityList.Count; ++i ) {
				((MMD4MecanimBulletPhysicsEntity)_physicsEntityList[i])._PreUpdateWorld( deltaTime );
			}

			int numSubSteps = _world.StepSimulation( deltaTime, _maxSubSteps, _subStep );
			float elapsedTime = (numSubSteps > 0) ? ((float)numSubSteps * _subStep) : 0;

			for( int i = 0; i != _physicsEntityList.Count; ++i ) {
				((MMD4MecanimBulletPhysicsEntity)_physicsEntityList[i])._PostUpdateWorld( elapsedTime );
			}
		}
	}

	void _Entity_NoUpdate()
	{
		for( int i = 0; i != _physicsEntityList.Count; ++i ) {
			((MMD4MecanimBulletPhysicsEntity)_physicsEntityList[i])._NoUpdateWorld();
		}
	}

	void _Entity_PostUpdate()
	{
		for( int i = 0; i != _physicsEntityList.Count; ++i ) {
			((MMD4MecanimBulletPhysicsEntity)_physicsEntityList[i])._PostUpdate();
		}
	}

	void _Entity_PrepareLateUpdate()
	{
		for( int i = 0; i != _physicsEntityList.Count; ++i ) {
			((MMD4MecanimBulletPhysicsEntity)_physicsEntityList[i])._PrepareLateUpdate();
		}
	}

	public void WaitEndThreading()
	{
		if( _multiThreading ) {
			if( _isRunningThread ) {
				_isRunningThread = false;
				#if USE_CACHEDTHREAD
				_cachedThread.WaitEnd();
				#else
				_cachedThreadQueue.WaitEnd( ref _threadQueueHandle );
				#endif
				_Entity_PrepareLateUpdate();
			}
		}
	}

	// from MMD4MecanimBulletPhysicsEntity
	public void _RemoveEntity( MMD4MecanimBulletPhysicsEntity physicsEntity )
	{
		for( int i = 0; i != _physicsEntityList.Count; ++i ) {
			if( (MMD4MecanimBulletPhysicsEntity)_physicsEntityList[i] == physicsEntity ) {
				_physicsEntityList.RemoveAt( i );
				physicsEntity._physicsWorld = null;
				break;
			}
		}
	}
}
