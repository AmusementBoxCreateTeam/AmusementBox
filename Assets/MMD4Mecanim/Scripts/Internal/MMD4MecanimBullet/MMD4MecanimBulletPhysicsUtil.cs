using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BulletXNA;
using BulletXNA.BulletCollision;
using BulletXNA.BulletDynamics;
using BulletXNA.LinearMath;

using PMXShapeType          = MMD4MecanimBulletPMXCommon.PMXShapeType;

public class MMD4MecanimBulletPhysicsUtil
{
	public static float NormalizeAngle( float r )
	{
		float cirHalf = 180.0f * Mathf.Deg2Rad;
		float cirFull = 360.0f * Mathf.Deg2Rad;

		if( r > cirHalf ) {
			while( r > cirHalf ) {
				r -= cirFull;
			}
		} else if( r < -cirHalf ) {
			while( r < -cirHalf ) {
				r += cirFull;
			}
		}
		
		return r;
	}
	
	public static float ClampAngle( float r, float lower, float upper )
	{
		if( r >= lower && r <= upper ) {
			return r;
		} else {
			float l = Mathf.Abs( NormalizeAngle( r - lower ) );
			float u = Mathf.Abs( NormalizeAngle( r - upper ) );
			if( l <= u ) {
				return lower;
			} else {
				return upper;
			}
		}
	}
	
	public static IndexedVector3 ClampDirection(
		IndexedVector3 prevDir,
		IndexedVector3 dir,
		float dirDot,
		float limitTheta,
		float limitTheta2 )
	{
		if( dirDot > 0.01f ) {
			float lLen = limitTheta;
			float lLen2 = limitTheta2;
			IndexedVector3 lVec = prevDir * lLen;
			float nLen = limitTheta / dirDot;
			IndexedVector3 nVec = dir * nLen;
			IndexedVector3 qVec = nVec - lVec;
			float qLen = Mathf.Sqrt(nLen * nLen - lLen2);
			return lVec + qVec * ((1.0f / qLen) * Mathf.Sqrt(1.0f - lLen2));
		} else {
			return prevDir; // Lock for safety.
		}
	}
	
	public static IndexedVector3 GetReflVector( IndexedVector3 normal, IndexedVector3 ray )
	{
		return ray - 2.0f * normal.Dot( ray ) * normal;
	}
	
	public static IndexedVector3 GetAngAccVector( IndexedVector3 prev, IndexedVector3 prev2 )
	{
		Vector3 t = GetReflVector( prev, -prev2 );
		return GetReflVector( t, -prev );
	}

	//----------------------------------------------------------------------------------------------------------------

	public static void BlendTransform( ref IndexedMatrix m, ref IndexedMatrix lhs, ref IndexedMatrix rhs, float lhsRate )
	{
		float rhsRate = 1.0f - lhsRate;
		IndexedVector3 x = lhs._basis.GetColumn(0) * lhsRate + rhs._basis.GetColumn(0) * rhsRate;
		IndexedVector3 z = lhs._basis.GetColumn(2) * lhsRate + rhs._basis.GetColumn(2) * rhsRate;
		float xLen = x.Length();
		float zLen = z.Length();
		if( xLen < 0.01f || zLen < 0.01f ) {
			m = lhs;
			return;
		}
		
		x *= 1.0f / xLen;
		z *= 1.0f / zLen;
		IndexedVector3 y = z.Cross(ref x);
		float yLen = y.Length();
		if( yLen < 0.01f ) {
			m = lhs;
			return;
		}
		
		y *= 1.0f / yLen;
		z = x.Cross(ref y);

		m._basis[0] = new IndexedVector3( x[0], y[0], z[0] );
		m._basis[1] = new IndexedVector3( x[1], y[1], z[1] );
		m._basis[2] = new IndexedVector3( x[2], y[2], z[2] );
		m._origin = lhs._origin * lhsRate + rhs._origin * rhsRate;
	}

	public static bool HitTestSphereToSphere(
		ref Vector3 translateAtoB,
		Vector3 spherePosA,
		Vector3 spherePosB,
		float lengthAtoB,
		float lengthAtoB2 )
	{
		translateAtoB = spherePosB - spherePosA;
		float len2 = translateAtoB.sqrMagnitude;
		if( len2 < lengthAtoB2 ) {
			float len = Mathf.Sqrt( len2 );
			if( len > Mathf.Epsilon ) {
				translateAtoB *= ((1.0f) / len) * (lengthAtoB - len);
				return true;
			}
		}
		
		return false;
	}

	public static bool HitTestSphereToCapsule(
		ref Vector3 translateOrig,
		ref Vector3 translateAtoB,
		Vector3 r_spherePos,
		float sphereRadius,
		float cylinderHeightH,
		float cylinderRadius,
		float lengthAtoB,
		float lengthAtoB2 )
	{
		translateOrig.Set( 0, 0, 0 );
		translateAtoB.Set( 0, 0, 0 );
		
		// XZ(Sphere)
		float xzLen2 = r_spherePos[0] * r_spherePos[0] + r_spherePos[2] * r_spherePos[2];
		if( xzLen2 < lengthAtoB2 ) {
			float absY = Mathf.Abs(r_spherePos[1]);
			// Y(Cylinder)
			if( absY < cylinderHeightH ) {
				float xzLen = Mathf.Sqrt( xzLen2 );
				if( xzLen > Mathf.Epsilon ) {
					translateAtoB.Set( -r_spherePos[0], 0.0f, -r_spherePos[2] );
					translateAtoB *= 1.0f / xzLen;
					translateAtoB *= (lengthAtoB - xzLen);
					translateOrig.Set( 0.0f, r_spherePos[1], 0.0f );
					return true;
				} else {
					return false;
				}
			}
			float xyzLen2 = xzLen2 + (absY - cylinderHeightH) * (absY - cylinderHeightH);
			// Y(Sphere)
			if( xyzLen2 < lengthAtoB2 ) {
				float xyzLen = Mathf.Sqrt( xyzLen2 );
				if( xyzLen > Mathf.Epsilon ) {
					if( r_spherePos[1] >= 0 ) {
						translateOrig.Set( 0.0f, cylinderHeightH, 0.0f );
					} else {
						translateOrig.Set( 0.0f, -cylinderHeightH, 0.0f );
					}
					translateAtoB = translateOrig - r_spherePos;
					translateAtoB *= (1.0f / xyzLen);
					translateAtoB *= (lengthAtoB - xyzLen);
					return true;
				} else {
					return false;
				}
			}
		}
		
		return false;
	}

	public static bool HitTestSphereToBox(
		ref Vector3 translateOrig,
		ref Vector3 translateAtoB,
		Vector3 r_spherePos,
		float sphereRadius,
		float sphereRadius2,
		Vector3 boxSizeH )
	{
		translateOrig.Set( 0, 0, 0 );
		translateAtoB.Set( 0, 0, 0 );
		
		float absX = Mathf.Abs(r_spherePos[0]);
		float absY = Mathf.Abs(r_spherePos[1]);
		float absZ = Mathf.Abs(r_spherePos[2]);
		
		{
			int dim = -1;
			bool innerX = (absX <= boxSizeH[0]);
			bool innerY = (absY <= boxSizeH[1]);
			bool innerZ = (absZ <= boxSizeH[2]);
			
			if( innerX && innerY && innerZ ) {
				if( boxSizeH[0] <= Mathf.Epsilon || boxSizeH[1] <= Mathf.Epsilon || boxSizeH[2] <= Mathf.Epsilon ) {
					return false;
				}
				
				bool zeroX = (absX <= Mathf.Epsilon);
				bool zeroY = (absY <= Mathf.Epsilon);
				bool zeroZ = (absZ <= Mathf.Epsilon);
				
				float boxYZ = boxSizeH[1] / boxSizeH[2];
				float boxZX = boxSizeH[2] / boxSizeH[0];
				float boxYX = boxSizeH[1] / boxSizeH[0];
				
				if( zeroX && zeroY && zeroZ ) {
					// Nothing.
				} else if( zeroX && zeroY ) {
					dim = 2;
				} else if( zeroX && zeroZ ) {
					dim = 1;
				} else if( zeroY && zeroZ ) {
					dim = 0;
				} else if( zeroX ) {
					float rYZ = absY / absZ;
					dim = ( rYZ > boxYZ ) ? 1 : 2;
				} else if( zeroY ) {
					float rZX = absZ / absX;
					dim = ( rZX > boxZX ) ? 2 : 0;
				} else if( zeroZ ) {
					float rYX = absY / absX;
					dim = ( rYX > boxYX ) ? 1 : 0;
				} else {
					float rYX = absY / absX;
					if( rYX > boxYX ) {
						float rYZ = absY / absZ;
						dim = ( rYZ > boxYZ ) ? 1 : 2;
					} else {
						float rZX = absZ / absX;
						dim = ( rZX > boxZX ) ? 2 : 0;
					}
				}
			} else if( absX < boxSizeH[0] + sphereRadius && innerY && innerZ ) {
				dim = 0;
			} else if( innerX && absY < boxSizeH[1] + sphereRadius && innerZ ) {
				dim = 1;
			} else if( innerX && innerY && absZ < boxSizeH[2] + sphereRadius ) {
				dim = 2;
			}
			
			switch( dim ) {
			case 0: // X
			{
				float lenX = (boxSizeH[0] - absX) + sphereRadius;
				translateOrig.Set( 0.0f, r_spherePos[1], r_spherePos[2] );
				translateAtoB.Set( (r_spherePos[0] >= 0) ? -lenX : lenX, 0.0f, 0.0f );
			}
				return true;
			case 1: // Y
			{
				float lenY = (boxSizeH[1] - absY) + sphereRadius;
				translateOrig.Set( r_spherePos[0], 0.0f, r_spherePos[2] );
				translateAtoB.Set( 0.0f, (r_spherePos[1] >= 0) ? -lenY : lenY, 0.0f );
			}
				return true;
			case 2: // Z
			{
				float lenZ = (boxSizeH[2] - absZ) + sphereRadius;
				translateOrig.Set( r_spherePos[0], r_spherePos[1], 0.0f );
				translateAtoB.Set( 0.0f, 0.0f, (r_spherePos[2] >= 0) ? -lenZ : lenZ );
			}
				return true;
			}
		}

		{
			translateOrig.x = ( ( r_spherePos[0] >= 0 ) ? boxSizeH[0] : -boxSizeH[0] );
			translateOrig.y = ( ( r_spherePos[1] >= 0 ) ? boxSizeH[1] : -boxSizeH[1] );
			translateOrig.z = ( ( r_spherePos[2] >= 0 ) ? boxSizeH[2] : -boxSizeH[2] );
			float xyzLen2 = (r_spherePos - translateOrig).sqrMagnitude;
			if( xyzLen2 < sphereRadius2 ) {
				float xyzLen = Mathf.Sqrt( xyzLen2 );
				if( xyzLen > Mathf.Epsilon ) {
					translateAtoB = translateOrig - r_spherePos;
					translateAtoB *= (1.0f / xyzLen) * (sphereRadius - xyzLen);
					Vector3 movedSpherePos = r_spherePos - translateAtoB;
					float movedAbsX = Mathf.Abs(movedSpherePos.x);
					float movedAbsY = Mathf.Abs(movedSpherePos.y);
					float movedAbsZ = Mathf.Abs(movedSpherePos.z);
					bool movedInnerX = (movedAbsX <= boxSizeH.x);
					bool movedInnerY = (movedAbsY <= boxSizeH.y);
					bool movedInnerZ = (movedAbsZ <= boxSizeH.z);
					if( movedInnerX && movedInnerY && movedInnerZ ) {
						// Nothing.
					} else {
						return true;
					}
				}
			}
		}

		return false;
	}

	//----------------------------------------------------------------------------------------------------------------

	static void _FeedbackImpulse( MMD4MecanimBulletPMXCollider colliderA, Vector3 translateAtoB, Vector3 translateOrig )
	{
		colliderA.isCollision = true;
		colliderA.transform._origin -= translateAtoB;
	}
	
	static bool _FastCollideStoK( MMD4MecanimBulletPMXCollider colliderA, MMD4MecanimBulletPMXCollider colliderB )
	{
		IndexedMatrix transformB = colliderB.transform;
		IndexedMatrix transformBInv = colliderB.transform.Inverse();
		
		IndexedMatrix transformA = colliderA.transform;
		MMD4MecanimBulletPMXColliderCircles circlesA = colliderA.circles;
		Vector3[] vertices = colliderA.circles.GetTransformVertices();
		int vertexCount = vertices.Length;

		Vector3 translateOrig = Vector3.zero;
		Vector3 translateAtoB = Vector3.zero;

		switch( colliderB.shape ) {
		case (int)PMXShapeType.Sphere:
		{
			float lengthAtoB = circlesA.GetRadius() + colliderB.size[0];
			float lengthAtoB2 = lengthAtoB * lengthAtoB;
			Vector3 spherePos = colliderB.transform._origin;
			Vector3 colliderTranslate = Vector3.zero;
			{
				circlesA.Transform( transformA );
				for( int i = 0; i != vertexCount; ++i ) {
					if( HitTestSphereToSphere( ref translateAtoB, vertices[i], spherePos, lengthAtoB, lengthAtoB2 ) ) {
						translateOrig = colliderA.transform._origin;
						colliderTranslate -= translateAtoB;
						_FeedbackImpulse( colliderA, translateAtoB, translateOrig );
					}
				}
				return colliderA.isCollision;
			}
		}
		case (int)PMXShapeType.Box:
		{
			float radiusA = circlesA.GetRadius();
			float radiusA2 = circlesA.GetRadius2();
			Vector3 boxSizeH = colliderB.size;
			Vector3 colliderTranslate = Vector3.zero;
			{
				circlesA.Transform( transformBInv * transformA );
				for( int i = 0; i != vertexCount; ++i ) {
					if( HitTestSphereToBox( ref translateOrig, ref translateAtoB, vertices[i] + colliderTranslate, radiusA, radiusA2, boxSizeH ) ) {
						colliderTranslate -= translateAtoB;
						translateAtoB = transformB._basis * translateAtoB;
						_FeedbackImpulse( colliderA, translateAtoB, translateOrig );
					}
				}
				return colliderA.isCollision;
			}
		}
		case (int)PMXShapeType.Capsule:
		{
			float radiusA = circlesA.GetRadius();
			float lengthAtoB = circlesA.GetRadius() + colliderB.size[0];
			float lengthAtoB2 = lengthAtoB * lengthAtoB;
			float cylinderHeightH = Mathf.Max( colliderB.size[1] * 0.5f, 0.0f );
			float cylinderRadius = colliderB.size[0];
			Vector3 colliderTranslate = Vector3.zero;
			{
				circlesA.Transform( transformBInv * transformA );
				for( int i = 0; i != vertexCount; ++i ) {
					if( HitTestSphereToCapsule( ref translateOrig, ref translateAtoB, vertices[i] + colliderTranslate, radiusA, cylinderHeightH, cylinderRadius, lengthAtoB, lengthAtoB2 ) ) {
						colliderTranslate -= translateAtoB;
						translateAtoB = transformB._basis * translateAtoB;
						_FeedbackImpulse( colliderA, translateAtoB, translateOrig );
					}
				}
				return colliderA.isCollision;
			}
		}
		}
		
		return false;
	}
	
	public static bool FastCollide( MMD4MecanimBulletPMXCollider colliderA, MMD4MecanimBulletPMXCollider colliderB )
	{
		colliderA.Prepare();
		colliderB.Prepare();
		if( colliderA.isKinematic && colliderB.isKinematic ) {
			return false; // Not process.
		}
		if( colliderA.isKinematic ) {
			return _FastCollideStoK( colliderB, colliderA );
		} else if( colliderB.isKinematic ) {
			return _FastCollideStoK( colliderA, colliderB );
		} else {
			return false;
		}
	}

	//----------------------------------------------------------------------------------------------------------------

    public class SimpleMotionState : IMotionState
    {
        public SimpleMotionState()
            : this(IndexedMatrix.Identity)
        {
        }

        public SimpleMotionState(IndexedMatrix startTrans)
        {
            m_graphicsWorldTrans = startTrans;
        }

        public SimpleMotionState(ref IndexedMatrix startTrans)
        {
            m_graphicsWorldTrans = startTrans;
        }
		
        public virtual void GetWorldTransform(out IndexedMatrix centerOfMassWorldTrans)
        {
            centerOfMassWorldTrans = m_graphicsWorldTrans;
        }

        public virtual void SetWorldTransform(IndexedMatrix centerOfMassWorldTrans)
        {
            SetWorldTransform(ref centerOfMassWorldTrans);
        }

        public virtual void SetWorldTransform(ref IndexedMatrix centerOfMassWorldTrans)
        {
            m_graphicsWorldTrans = centerOfMassWorldTrans;
        }

        public virtual void Rotate(IndexedQuaternion iq)
        {
            IndexedMatrix im = IndexedMatrix.CreateFromQuaternion(iq);
            im._origin = m_graphicsWorldTrans._origin;
            SetWorldTransform(ref im);
        }

        public virtual void Translate(IndexedVector3 v)
        {
            m_graphicsWorldTrans._origin += v;
        }

        public IndexedMatrix m_graphicsWorldTrans;
    }
	
    public class KinematicMotionState : IMotionState
    {
        public KinematicMotionState()
        {
			m_graphicsWorldTrans = IndexedMatrix.Identity;
        }

        public KinematicMotionState(ref IndexedMatrix startTrans)
        {
            m_graphicsWorldTrans = startTrans;
        }

        public virtual void GetWorldTransform(out IndexedMatrix centerOfMassWorldTrans)
        {
            centerOfMassWorldTrans = m_graphicsWorldTrans;
        }

        public virtual void SetWorldTransform(IndexedMatrix centerOfMassWorldTrans)
        {
			// Nothing.
        }

        public virtual void SetWorldTransform(ref IndexedMatrix centerOfMassWorldTrans)
        {
			// Nothing.
        }

        public virtual void Rotate(IndexedQuaternion iq)
        {
			// Nothing.
        }

        public virtual void Translate(IndexedVector3 v)
        {
			// Nothing.
        }

        public IndexedMatrix m_graphicsWorldTrans;
    }

	public static IndexedMatrix MakeIndexedMatrix( ref Vector3 position, ref Quaternion rotation )
	{
		IndexedQuaternion indexedQuaternion = new IndexedQuaternion(ref rotation);
		IndexedBasisMatrix indexedBasisMatrix = new IndexedBasisMatrix(indexedQuaternion);
		IndexedVector3 origin = new IndexedVector3(ref position);
		return new IndexedMatrix(indexedBasisMatrix, origin);
	}

	public static void MakeIndexedMatrix( ref IndexedMatrix matrix, ref Vector3 position, ref Quaternion rotation )
	{
		matrix.SetRotation( new IndexedQuaternion(ref rotation) );
		matrix._origin = position;
	}

	public static IndexedMatrix MakeIndexedMatrix( ref Matrix4x4 m )
	{
		return new IndexedMatrix(
			m.m00, m.m01, m.m02,
			m.m10, m.m11, m.m12,
			m.m20, m.m21, m.m22,
			m.m03, m.m13, m.m23 );
	}

    public static void CopyBasis(ref IndexedBasisMatrix m0, ref Matrix4x4 m1)
    {
        m0._el0.X = m1.m00;
        m0._el1.X = m1.m10;
        m0._el2.X = m1.m20;

        m0._el0.Y = m1.m01;
        m0._el1.Y = m1.m11;
        m0._el2.Y = m1.m21;

        m0._el0.Z = m1.m02;
        m0._el1.Z = m1.m12;
        m0._el2.Z = m1.m22;
    }

    public static void CopyBasis(ref Matrix4x4 m0, ref IndexedBasisMatrix m1)
    {
        m0.m00 = m1._el0.X;
        m0.m10 = m1._el1.X;
        m0.m20 = m1._el2.X;

        m0.m01 = m1._el0.Y;
        m0.m11 = m1._el1.Y;
        m0.m21 = m1._el2.Y;

        m0.m02 = m1._el0.Z;
        m0.m12 = m1._el1.Z;
        m0.m22 = m1._el2.Z;
    }

    public static void CopyBasis(ref Matrix4x4 m0, ref Matrix4x4 m1)
    {
        m0.m00 = m1.m00;
        m0.m10 = m1.m10;
        m0.m20 = m1.m20;

        m0.m01 = m1.m01;
        m0.m11 = m1.m11;
        m0.m21 = m1.m21;

        m0.m02 = m1.m02;
        m0.m12 = m1.m12;
        m0.m22 = m1.m22;
    }

    public static void SetRotation(ref Matrix4x4 m, ref Quaternion q)
    {
        float d = q.x * q.x + q.y + q.y + q.z * q.z + q.w * q.w;
        float s = (d > Mathf.Epsilon) ? (2.0f / d) : 0.0f;
        float xs = q.x * s, ys = q.y * s, zs = q.z * s;
        float wx = q.w * xs, wy = q.w * ys, wz = q.w * zs;
        float xx = q.x * xs, xy = q.x * ys, xz = q.x * zs;
        float yy = q.y * ys, yz = q.y * zs, zz = q.z * zs;
        // el0
        m.m00 = 1.0f - (yy + zz);
        m.m01 = xy - wz;
        m.m02 = xz + wy;
        // el1
        m.m10 = xy + wz;
        m.m11 = 1.0f - (xx + zz);
        m.m12 = yz - wx;
        // el2
        m.m20 = xz - wy;
        m.m21 = yz + wx;
        m.m22 = 1.0f - (xx + yy);
    }

    public static void SetPosition(ref Matrix4x4 m, Vector3 v)
    {
        m.m03 = v.x;
        m.m13 = v.y;
        m.m23 = v.z;
    }

    public static void SetPosition(ref Matrix4x4 m, ref Vector3 v)
    {
        m.m03 = v.x;
        m.m13 = v.y;
        m.m23 = v.z;
    }

    public static Vector3 GetPosition(ref Matrix4x4 m)
    {
        return new Vector3(m.m03, m.m13, m.m23);
    }

	/* Unused.
    public static Quaternion GetRotation(ref Matrix4x4 m)
    {
        Quaternion q = new Quaternion();
        q.w = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] + m[1, 1] + m[2, 2])) / 2;
        q.x = Mathf.Sqrt(Mathf.Max(0, 1 + m[0, 0] - m[1, 1] - m[2, 2])) / 2;
        q.y = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] + m[1, 1] - m[2, 2])) / 2;
        q.z = Mathf.Sqrt(Mathf.Max(0, 1 - m[0, 0] - m[1, 1] + m[2, 2])) / 2;
        q.x *= Mathf.Sign(q.x * (m[2, 1] - m[1, 2]));
        q.y *= Mathf.Sign(q.y * (m[0, 2] - m[2, 0]));
        q.z *= Mathf.Sign(q.z * (m[1, 0] - m[0, 1]));
        return q;
    }
    */

	// from: Bullet Physics 2.79(C/C++)
	public static void ComputeEulerZYX( ref IndexedBasisMatrix m, ref float yaw, ref float pitch, ref float roll )
	{
		if( Mathf.Abs( m._el2.X ) >= 1.0f ) {
			yaw = 0.0f;
			// From difference of angles formula
			float delta = Mathf.Atan2( m._el0.X, m._el0.Z );
			if( Mathf.Abs( m._el2.X ) > 0 ) { //gimbal locked up
				pitch = Mathf.PI * 0.5f;
				roll = pitch + delta;
			} else { // gimbal locked down
				pitch = -Mathf.PI * 0.5f;
				roll = -pitch + delta;
			}
		} else {
			pitch = -Mathf.Asin( m._el2.X );
			float cos_pitch = Mathf.Cos( pitch );
			if( Mathf.Abs(cos_pitch) > Mathf.Epsilon ) {
				float r_cos_pitch = 1.0f / cos_pitch;
				roll = Mathf.Atan2( m._el2.Y * r_cos_pitch, m._el2.Z * r_cos_pitch);
				yaw = Mathf.Atan2( m._el1.X * r_cos_pitch, m._el0.X * r_cos_pitch);
			} else {
				roll = yaw = 0.0f;
			}
		}
	}

	public static IndexedBasisMatrix EulerX( float eulerX )
	{
		float ci = Mathf.Cos(eulerX);
		float si = Mathf.Sin(eulerX);
		return new IndexedBasisMatrix(
			1.0f, 0.0f, 0.0f,
			0.0f,   ci,  -si,
			0.0f,   si,   ci );
	}

	public static IndexedBasisMatrix EulerY( float eulerY )
	{
		float cj = Mathf.Cos(eulerY);
		float sj = Mathf.Sin(eulerY);
		return new IndexedBasisMatrix(
			  cj, 0.0f,   sj,
			0.0f, 1.0f, 0.0f, 
			 -sj, 0.0f,   cj );
	}

	public static IndexedBasisMatrix EulerZ( float eulerZ )
	{
		float ch = Mathf.Cos(eulerZ);
		float sh = Mathf.Sin(eulerZ);
		return new IndexedBasisMatrix(
			  ch,  -sh, 0.0f,
			  sh,   ch, 0.0f,
			0.0f, 0.0f, 1.0f );
	}

	// from: Bullet Physics 2.79(C/C++)
	public static IndexedBasisMatrix EulerZYX( float eulerX, float eulerY, float eulerZ )
	{
		float ci = Mathf.Cos(eulerX);
		float cj = Mathf.Cos(eulerY);
		float ch = Mathf.Cos(eulerZ);
		float si = Mathf.Sin(eulerX);
		float sj = Mathf.Sin(eulerY);
		float sh = Mathf.Sin(eulerZ);
		float cc = ci * ch; 
		float cs = ci * sh; 
		float sc = si * ch; 
		float ss = si * sh;
		return new IndexedBasisMatrix(
				cj * ch, sj * sc - cs, sj * cc + ss,
		        cj * sh, sj * ss + cc, sj * cs - sc, 
		        -sj,     cj * si,      cj * ci);
	}

	public static IndexedBasisMatrix BasisRotationYXZ(ref Vector3 rotation)
    {
		IndexedBasisMatrix rx = EulerX( rotation.x );
		IndexedBasisMatrix ry = EulerY( rotation.y );
		IndexedBasisMatrix rz = EulerZ( rotation.z );
        return ry * rx * rz; // Yaw-Pitch-Roll
    }

	public static IndexedQuaternion QuaternionX( float x )
	{
		float halfAngle = x * 0.5f;
		float s = Mathf.Sin(halfAngle);
		return new IndexedQuaternion( s, 0.0f, 0.0f, Mathf.Cos(halfAngle) );
	}

	public static IndexedQuaternion QuaternionY( float y )
	{
		float halfAngle = y * 0.5f;
		float s = Mathf.Sin(halfAngle);
		return new IndexedQuaternion( 0.0f, s, 0.0f, Mathf.Cos(halfAngle) );
	}

	public static IndexedQuaternion QuaternionZ( float z )
	{
		float halfAngle = z * 0.5f;
		float s = Mathf.Sin(halfAngle);
		return new IndexedQuaternion( 0.0f, 0.0f, s, Mathf.Cos(halfAngle) );
	}

	public static float ComputeEulerX( ref IndexedBasisMatrix m )
	{
		float rx = 0.0f;
		if( Mathf.Abs(m._el1.Z) <= Mathf.Epsilon && Mathf.Abs(m._el2.Z) <= Mathf.Epsilon ) {
			rx = Mathf.Atan2(m._el1.Y, -m._el2.Y) - 90.0f * Mathf.Deg2Rad;
		} else {
			rx = Mathf.Atan2(m._el1.Z, -m._el2.Z) - 180.0f * Mathf.Deg2Rad;
		}
		
		return NormalizeAngle( rx );
	}
	
	public static float ComputeEulerY( ref IndexedBasisMatrix m )
	{
		float ry = 0.0f;
		if( Mathf.Abs(m._el0.X) <= Mathf.Epsilon && Mathf.Abs(m._el2.X) <= Mathf.Epsilon ) {
			ry = Mathf.Atan2(-m._el2.Z, m._el0.Z) - 270.0f * Mathf.Deg2Rad;
		} else {
			ry = Mathf.Atan2(-m._el2.X, m._el0.X);
		}
		
		return NormalizeAngle( ry );
	}
	
	public static float ComputeEulerZ( ref IndexedBasisMatrix m )
	{
		float rz = 0.0f;
		if( Mathf.Abs(m._el0.X) <= Mathf.Epsilon && Mathf.Abs(m._el1.X) <= Mathf.Epsilon ) {
			rz = Mathf.Atan2(m._el1.Y, m._el0.Y) - 90.0f * Mathf.Deg2Rad;
		} else {
			rz = Mathf.Atan2(m._el1.X, m._el0.X);
		}
		
		return NormalizeAngle( rz );
	}

	// LeftHand to RightHand
	public static void GetLinearLimitFromLeftHand( ref IndexedVector3 limitPosFrom, ref IndexedVector3 limitPosTo )
	{
		IndexedVector3 tempPosFrom = limitPosFrom;
		IndexedVector3 tempPosTo = limitPosTo;
		limitPosFrom	= new IndexedVector3( tempPosFrom[0], tempPosFrom[1], -tempPosTo[2] );
		limitPosTo		= new IndexedVector3( tempPosTo[0],   tempPosTo[1],   -tempPosFrom[2] );
	}
	
	// LeftHand to RightHand
	public static void GetAngularLimitFromLeftHand( ref IndexedVector3 limitRotFrom, ref IndexedVector3 limitRotTo )
	{
		IndexedVector3 tempRotFrom = limitRotFrom;
		IndexedVector3 tempRotTo = limitRotTo;
		limitRotFrom	= new IndexedVector3( -tempRotTo[0],   -tempRotTo[1],   tempRotFrom[2] );
		limitRotTo		= new IndexedVector3( -tempRotFrom[0], -tempRotFrom[1], tempRotTo[2] );
	}

	/*
	public static void UnityToBulletPosition( ref IndexedVector3 v, float scale )
	{
		v *= scale;
		v.X = -v.X;
	}
	
	public static void BulletToUnityPosition( ref IndexedVector3 v, float scale )
	{
		v *= scale;
		v.X = -v.X;
	}
	
	public static void UnityToBulletRotation( ref IndexedQuaternion r )
	{
		r.Y = -r.Y;
		r.Z = -r.Z;
	}
	
	public static void BulletToUnityRotation( ref IndexedQuaternion r )
	{
		r.Y = -r.Y;
		r.Z = -r.Z;
	}
	*/

	public static IndexedVector3 Lerp( ref IndexedVector3 lhs, ref IndexedVector3 rhs, float t )
	{
		return (rhs - lhs) * t + lhs;
	}

	public static float Dot( ref IndexedQuaternion lhs, ref IndexedQuaternion rhs )
	{
		return lhs.X * rhs.X + lhs.Y * rhs.Y + lhs.Z * rhs.Z + lhs.W * rhs.W;
	}

	public static float Length2( ref IndexedQuaternion q )
	{
		return q.X * q.X + q.Y * q.Y + q.Z * q.Z + q.W * q.W;
	}

	public static float Angle( ref IndexedQuaternion lhs, ref IndexedQuaternion rhs )
	{
		float s = Mathf.Sqrt( Length2(ref lhs) * Length2(ref rhs) );
		return ( Mathf.Abs(s) > Mathf.Epsilon ) ? Mathf.Acos( Dot(ref lhs, ref rhs) / s ) : 0.0f;
	}

	public static IndexedVector3 GetAxis( ref IndexedQuaternion q )
	{
		float s_squared = 1.0f - q.W * q.W;
		if( s_squared < 10.0f * Mathf.Epsilon ) {
			return new IndexedVector3(1.0f, 0.0f, 0.0f);
		}
		float s = 1.0f / Mathf.Sqrt( s_squared );
		return new IndexedVector3(q.X * s, q.Y * s, q.Z * s);
	}
	 
	public static float GetAngle( ref IndexedQuaternion q )
	{
		return 2.0f * Mathf.Acos( q.W );
	}

	public static IndexedQuaternion SlerpFromIdentity( ref IndexedQuaternion rhs, float t )
	{
		float s = Mathf.Sqrt( Length2( ref rhs ) );
		float theta = (Mathf.Abs(s) > Mathf.Epsilon) ? Mathf.Acos( rhs.W / s ) : 0.0f;
		if( theta != 0.0f ) {
			float d = 1.0f / Mathf.Sin( theta );
			float s0 = Mathf.Sin((1.0f - t) * theta);
			float s1 = Mathf.Sin(t * theta);
			if( rhs.W < 0.0f ) {
				return new IndexedQuaternion(	(-rhs.X * s1) * d,
												(-rhs.Y * s1) * d,
												(-rhs.Z * s1) * d,
												(s0 + -rhs.W * s1) * d);
			} else {
				return new IndexedQuaternion(	(rhs.X * s1) * d,
												(rhs.Y * s1) * d,
				                             	(rhs.Z * s1) * d,
				                             	(s0 + rhs.W * s1) * d);
			}
		} else {
			return IndexedQuaternion.Identity;
		}
	}

	public static IndexedQuaternion Slerp( ref IndexedQuaternion lhs, ref IndexedQuaternion rhs, float t )
	{
		float theta = Angle(ref lhs, ref rhs);
		if( theta != 0.0f ) {
			float d = 1.0f / Mathf.Sin( theta );
			float s0 = Mathf.Sin((1.0f - t) * theta);
			float s1 = Mathf.Sin(t * theta);
			if( Dot( ref lhs, ref rhs ) < 0.0f ) {
				return new IndexedQuaternion(	(lhs.X * s0 + -rhs.X * s1) * d,
				                             	(lhs.Y * s0 + -rhs.Y * s1) * d,
												(lhs.Z * s0 + -rhs.Z * s1) * d,
												(lhs.W * s0 + -rhs.W * s1) * d);
			} else {
				return new IndexedQuaternion(	(lhs.X * s0 + rhs.X * s1) * d,
				                             	(lhs.Y * s0 + rhs.Y * s1) * d,
												(lhs.Z * s0 + rhs.Z * s1) * d,
												(lhs.W * s0 + rhs.W * s1) * d);
			}
		} else {
			return lhs;
		}
	}

	public static bool IsPositionFuzzyZero( ref IndexedVector3 v )
	{
		return Mathf.Abs(v.X) < 0.00001f
			&& Mathf.Abs(v.Y) < 0.00001f
			&& Mathf.Abs(v.Z) < 0.00001f;
	}
	
	public static bool IsRotationFuzzyIdentity( ref IndexedQuaternion r )
	{
		return Mathf.Abs(r.X) < 0.0000001f
			&& Mathf.Abs(r.Y) < 0.0000001f
			&& Mathf.Abs(r.Z) < 0.0000001f
			&& Mathf.Abs(1.0f - r.W) < 0.0000001f;
	}

	public struct FastVector3
	{
		public IndexedVector3 v;
		public bool isZero;

		public static FastVector3 Zero
		{
			get {
				FastVector3 fv = new FastVector3();
				fv.v = IndexedVector3.Zero;
				fv.isZero = true;
				return fv;
			}
		}

		public static implicit operator IndexedVector3(FastVector3 fv)
		{
			return fv.v;
		}
		
		public static implicit operator FastVector3(IndexedVector3 v)
		{
			FastVector3 fv = new FastVector3();
			fv.v = v;
			fv.isZero = (v == IndexedVector3.Zero);
			return fv;
		}

		public static implicit operator FastVector3(Vector3 v)
		{
			FastVector3 fv = new FastVector3();
			fv.v = v;
			fv.isZero = (v == Vector3.zero);
			return fv;
		}

		public static FastVector3 operator+(IndexedVector3 lhs, FastVector3 rhs)
		{
			if( rhs.isZero ) {
				FastVector3 fv = new FastVector3();
				fv.v = lhs;
				fv.isZero = false;
				return fv;
			} else {
				FastVector3 fv = new FastVector3();
				fv.v = lhs + rhs.v;
				fv.isZero = false;
				return fv;
			}
		}
		
		public static FastVector3 operator+(FastVector3 lhs, IndexedVector3 rhs)
		{
			if( lhs.isZero ) {
				FastVector3 fv = new FastVector3();
				fv.v = rhs;
				fv.isZero = false;
				return fv;
			} else {
				FastVector3 fv = new FastVector3();
				fv.v = lhs.v + rhs;
				fv.isZero = false;
				return fv;
			}
		}
		
		public static FastVector3 operator+(FastVector3 lhs, FastVector3 rhs)
		{
			if( lhs.isZero ) {
				return rhs;
			} else if( rhs.isZero ) {
				return lhs;
			} else {
				FastVector3 fv = new FastVector3();
				fv.v = lhs.v + rhs.v;
				fv.isZero = false;
				return fv;
			}
		}

		public override bool Equals(System.Object obj)
		{
			if( obj is FastVector3 ) {
				FastVector3 fv = (FastVector3)obj;
				return this.isZero == fv.isZero && this.v == fv.v;
			} else {
				return false;
			}
		}
		
		public override int GetHashCode()
		{
			return this.v.GetHashCode();
		}
		
		public static bool operator==(FastVector3 a, FastVector3 b)
		{
			return a.isZero == b.isZero && a.v == b.v;
		}
		
		public static bool operator!=(FastVector3 a, FastVector3 b)
		{
			if( a.isZero ) {
				return !b.isZero;
			} else if( b.isZero ) {
				return true;
			} else {
				return a.v != b.v;
			}
		}
	}

	public struct FastQuaternion
	{
		public IndexedQuaternion q;
		public bool isIdentity;

		public static FastQuaternion Identity
		{
			get {
				FastQuaternion fq = new FastQuaternion();
				fq.q = IndexedQuaternion.Identity;
				fq.isIdentity = true;
				return fq;
			}
		}

		public static implicit operator IndexedQuaternion(FastQuaternion fq)
		{
			return fq.q;
		}

		public static implicit operator FastQuaternion(IndexedQuaternion q)
		{
			FastQuaternion fq = new FastQuaternion();
			fq.q = q;
			fq.isIdentity = (q == IndexedQuaternion.Identity);
			return fq;
		}

		public static implicit operator FastQuaternion(Quaternion q)
		{
			FastQuaternion fq = new FastQuaternion();
			fq.q = q;
			fq.isIdentity = (q == Quaternion.identity);
			return fq;
		}

		public static FastQuaternion operator*(IndexedQuaternion lhs, FastQuaternion rhs)
		{
			if( rhs.isIdentity ) {
				FastQuaternion fq = new FastQuaternion();
				fq.q = lhs;
				fq.isIdentity = false;
				return fq;
			} else {
				FastQuaternion fq = new FastQuaternion();
				fq.q = lhs * rhs.q;
				fq.isIdentity = false;
				return fq;
			}
		}

		public static FastQuaternion operator*(FastQuaternion lhs, IndexedQuaternion rhs)
		{
			if( lhs.isIdentity ) {
				FastQuaternion fq = new FastQuaternion();
				fq.q = rhs;
				fq.isIdentity = false;
				return fq;
			} else {
				FastQuaternion fq = new FastQuaternion();
				fq.q = lhs.q * rhs;
				fq.isIdentity = false;
				return fq;
			}
		}

		public static FastQuaternion operator*(FastQuaternion lhs, FastQuaternion rhs)
		{
			if( lhs.isIdentity ) {
				return rhs;
			} else if( rhs.isIdentity ) {
				return lhs;
			} else {
				FastQuaternion fq = new FastQuaternion();
				fq.q = lhs.q * rhs.q;
				fq.isIdentity = false;
				return fq;
			}
		}
		
		public override bool Equals(System.Object obj)
		{
			if( obj is FastQuaternion ) {
				FastQuaternion fq = (FastQuaternion)obj;
				return this.isIdentity == fq.isIdentity && this.q == fq.q;
			} else {
				return false;
			}
		}

		public override int GetHashCode()
		{
			return this.q.GetHashCode();
		}

		public static bool operator==(FastQuaternion a, FastQuaternion b)
		{
			return a.isIdentity == b.isIdentity && a.q == b.q;
		}
		
		public static bool operator!=(FastQuaternion a, FastQuaternion b)
		{
			if( a.isIdentity ) {
				return !b.isIdentity;
			} else if( b.isIdentity ) {
				return true;
			} else {
				return a.q != b.q;
			}
		}
	}

	public static void InstantSleep()
	{
		// Memo: Don't use 1 ms. Unstable sleep time in Windows.
		System.Threading.Thread.Sleep( 0 );
	}

	public static void ShortlySleep()
	{
		System.Threading.Thread.Sleep( 1 );
	}

	public static int GetProcessCount()
	{
		return 4;
	}

	// Memo: Non-threadsafe every functions.( _mutex is invoker, this is supported single thread only. )
	// Memo: Don't call every functions from invoked function.
	public class CachedThread
	{
		System.Threading.Mutex	_mutex;				// Call & Running thread.
		System.Threading.Thread	_thread;			// Call thread only.
		System.Action			_function;			// Call & Running thread.
		bool					_isLocking;			// Call thread only.
		bool					_isProcessing;		// Call & Running thread.
		bool					_isFinalized;		// Call & Running thread.

		public CachedThread()
		{
			_mutex = new System.Threading.Mutex();
		}

		~CachedThread()
		{
			_Finalize();
		}
		
		public void _Finalize()
		{
			if( _thread != null ) {
				WaitEnd();
				
				_isLocking = false;
				_isFinalized = true;
				_mutex.ReleaseMutex();

				_thread.Join();
				_thread = null;
			}
		}

		public void Invoke( System.Action function )
		{
			WaitEnd();
			
			_function		= function;
			_isProcessing	= true;
			
			if( _thread == null ) {
				_thread = new System.Threading.Thread( new System.Threading.ThreadStart( _Run ) );
				_thread.Start();
			} else {
				_isLocking = false;
				_mutex.ReleaseMutex();
			}
		}

		public void WaitEnd()
		{
			if( _thread != null && !_isLocking ) {
				for(;;) {
					_mutex.WaitOne();
					bool isProcessing = _isProcessing;
					if( !isProcessing ) {
						_isLocking = true;
						break;
					} else {
						_isLocking = false;
						_mutex.ReleaseMutex();
						System.Threading.Thread.Sleep(0);
					}
				}
			}
		}

		void _Run()
		{
			for(;;) {
				_mutex.WaitOne(); // Wait Invoke()
				bool isProcessing = _isProcessing;
				bool isFinalized = _isFinalized;
				if( isProcessing ) {
					_isProcessing = false;
					if( _function != null ) {
						_function();
						_function = null;
					}
				}
				_mutex.ReleaseMutex();
				if( isFinalized ) {
					break;
				}
				if( !isProcessing ) {
					System.Threading.Thread.Sleep(0); // Wait WaitEnd()
				}
			}
		}
	}

	public struct ThreadQueueHandle
	{
		public object queuePtr;
		public uint queueID;
		public uint uniqueID;

		public ThreadQueueHandle( object queuePtr, uint queueID, uint uniqueID )
		{
			this.queuePtr = queuePtr;
			this.queueID = queueID;
			this.uniqueID = uniqueID;
		}
		
		public void Reset()
		{
			queuePtr = null;
			queueID = 0;
			uniqueID = 0;
		}

		public static implicit operator bool(ThreadQueueHandle rhs)
		{
			return rhs.queuePtr != null;
		}
	}

	// Memo: Threadsafe every functions.
	public class CachedThreadQueue
	{
		System.Threading.ManualResetEvent	_invokeEvent = new System.Threading.ManualResetEvent(false);
		ArrayList							_threads = new ArrayList();
		int									_maxThreads = 0;
		bool								_isFinalized = false;
		uint								_uniqueID = 0;
		
		class Queue
		{
			public System.Action function;
			public uint queueID;
			public uint uniqueID;
			public bool processingWaitEnd;
			public System.Threading.ManualResetEvent processedEvent = new System.Threading.ManualResetEvent(true);

			public Queue( System.Action function )
			{
				this.function = function;
				this.queueID = 0;
				this.uniqueID = 0;
			}
			
			public void Unuse()
			{
				this.function = null;
				unchecked {
					++this.queueID;
				}
			}
			
			public void Reuse( System.Action function )
			{
				this.function = function;
			}
		}

		ArrayList _processingQueues = new ArrayList();
		ArrayList _reservedQueues = new ArrayList();
		ArrayList _unusedQueues = new ArrayList();

		static Queue _FindQueue( ArrayList queues, ref ThreadQueueHandle queueHandle )
		{
			if( queues != null ) {
				for( int i = 0; i != queues.Count; ++i ) {
					Queue queue = (Queue)queues[i];
					if( queue == queueHandle.queuePtr && queue.queueID == queueHandle.queueID ) {
						return queue;
					}
				}
			}

			return null;
		}

		public CachedThreadQueue()
		{
			_maxThreads = GetProcessCount();
		}

		public CachedThreadQueue( int maxThreads )
		{
			_maxThreads = maxThreads;
			if( _maxThreads <= 0 ) {
				_maxThreads = Mathf.Max( GetProcessCount(), 1 );
			}
		}
		
		~CachedThreadQueue()
		{
			if( _threads.Count != 0 ) {
				_Finalize();
			}
		}
		
		public void _Finalize()
		{
			bool isFinalized = false;
			lock(this) {
				isFinalized = _isFinalized;
				_isFinalized = true;
				if( !isFinalized ) {
					_invokeEvent.Set();
				}
			}
			
			if( isFinalized ) {
				return; // If finalizing, return function.
			}
			
			for( int i = 0; i != _threads.Count; ++i ) {
				((System.Threading.Thread)_threads[i]).Join();
			}
			
			_threads.Clear();
			
			lock(this) {
				_isFinalized = false;
			}
		}
		
		public ThreadQueueHandle Invoke( System.Action function )
		{
			ThreadQueueHandle r = new ThreadQueueHandle();
			if( function == null ) {
				return r;
			}
			bool isFinalized = false;
			lock(this) {
				isFinalized = _isFinalized;
				if( !isFinalized ) {
					// Extends thread pool automatically.
					int processingSize = _processingQueues.Count;
					if( processingSize == _threads.Count && _threads.Count < _maxThreads ) {
						System.Threading.Thread thread = new System.Threading.Thread( new System.Threading.ThreadStart( _Run ) );
						_threads.Add( thread );
						thread.Start();
					}
					
					Queue queue = null;
					for( int i = _unusedQueues.Count - 1; i >= 0; --i ) {
						queue = (Queue)_unusedQueues[ i ];
						if( !queue.processingWaitEnd ) {
							_unusedQueues.RemoveAt( i );
							queue.Reuse( function );
							break;
						} else {
							queue = null;
						}
					}
					if( queue == null ) {
						queue = new Queue( function );
					}
					queue.uniqueID = _uniqueID;
					unchecked {
						++_uniqueID;
					}
					_reservedQueues.Add( queue );
					r.queuePtr = queue;
					r.queueID = queue.queueID;
					r.uniqueID = queue.uniqueID;
					queue = null;
					_invokeEvent.Set();
				}
			}
			if( isFinalized ) {
				function(); // If finalizing, invoke directly.
			}
			return r;
		}
		
		public void WaitEnd( ref ThreadQueueHandle queueHandle )
		{
			if( queueHandle.queuePtr == null ) {
				return;
			}

			Queue queue = null;
			lock(this) {
				queue = _FindQueue( _processingQueues, ref queueHandle );
				if( queue == null ) {
					queue = _FindQueue( _reservedQueues, ref queueHandle );
				}
				if( queue != null ) {
					queue.processingWaitEnd = true; // Denied recycle.
				}
			}

			if( queue == null ) {
				queueHandle.Reset();
				return;
			}
			
			for(;;) {
				InstantSleep();

				queue.processedEvent.WaitOne();

				lock(this) {
					if( queue.queueID != queueHandle.queueID ) {
						queue.processingWaitEnd = false; // Accept recycle.
						queue = null;
					}
				}
				if( queue == null ) {
					queueHandle.Reset();
					break;
				}
			}
		}

		void _Run()
		{
			for(;;) {
				Queue queue = null;
				bool isProcessing = false;
				bool isFinalized = false;
				bool isEmpty = false;
				_invokeEvent.WaitOne();
				lock(this) {
					if( _reservedQueues.Count != 0 ) {
						queue = (Queue)_reservedQueues[0];
						_reservedQueues.RemoveAt( 0 );
						_processingQueues.Add( queue );
					}
					isProcessing = (queue != null);
					isFinalized = _isFinalized;
					isEmpty = _processingQueues.Count == 0 && _reservedQueues.Count == 0;
				}

				if( queue != null ) {
					if( queue.function != null ) {
						queue.function();
					}

					lock(this) {
						queue.Unuse();
						_processingQueues.Remove( queue );
						_unusedQueues.Add( queue );
						queue.processedEvent.Set();
						queue = null;
						isFinalized = _isFinalized;
						isEmpty = _processingQueues.Count == 0 && _reservedQueues.Count == 0;
						if( isEmpty ) {
							_invokeEvent.Reset();
						}
					}
				}
				
				if( isEmpty && isFinalized ) {
					break;
				}
				if( !isProcessing ) {
					InstantSleep();
				}
			}
		}
	}

	// Memo: Threadsafe every functions.
	public class CachedPararellThreadQueue
	{
		public delegate void Function(int index, int count);

		System.Threading.ManualResetEvent	_invokeEvent = new System.Threading.ManualResetEvent(false);
		System.Threading.Thread[]			_threads = null;
		int									_maxThreads = 0;
		bool								_isFinalized = false;
		uint								_uniqueID = 0;
		
		class Queue
		{
			public Function function;
			public int length;
			public int processingThreads;
			public int processedThreads;
			public uint queueID;
			public uint uniqueID;
			public bool processingWaitEnd;
			public System.Threading.ManualResetEvent processedEvent = new System.Threading.ManualResetEvent(true);
			
			public Queue( Function function, int length )
			{
				this.function = function;
				this.length = length;
				this.processingThreads = 0;
				this.processedThreads = 0;
				this.queueID = 0;
				this.uniqueID = 0;
			}
			
			public void Unuse()
			{
				this.function = null;
				this.length = 0;
				this.processingThreads = 0;
				this.processedThreads = 0;
				unchecked {
					++this.queueID;
				}
			}
			
			public void Reuse( Function function, int length )
			{
				this.function = function;
				this.length = length;
			}
		}

		ArrayList _processedQueues = new ArrayList();
		Queue _processingQueue;
		ArrayList _reservedQueues = new ArrayList();
		ArrayList _unusedQueues = new ArrayList();

		static bool _IsEqualQueue( Queue queue, ref ThreadQueueHandle queueHandle )
		{
			if( queue != null ) {
				if( queue == queueHandle.queuePtr && queue.queueID == queueHandle.queueID ) {
					return true;
				}
			}
			
			return false;
		}

		static Queue _FindQueue( ArrayList queues, ref ThreadQueueHandle queueHandle )
		{
			if( queues != null ) {
				for( int i = 0; i != queues.Count; ++i ) {
					Queue queue = (Queue)queues[i];
					if( queue == queueHandle.queuePtr && queue.queueID == queueHandle.queueID ) {
						return queue;
					}
				}
			}
			
			return null;
		}

		void _AwakeThread()
		{
			if( _threads == null ) {
				_threads = new System.Threading.Thread[_maxThreads];
				for( int i = 0; i != _maxThreads; ++i ) {
					System.Threading.Thread thread = new System.Threading.Thread( new System.Threading.ThreadStart( _Run ) );
					_threads[i] = thread;
					thread.Start();
				}
			}
		}

		public CachedPararellThreadQueue()
		{
			_maxThreads = GetProcessCount();
		}
		
		public CachedPararellThreadQueue( int maxThreads )
		{
			_maxThreads = maxThreads;
			if( _maxThreads <= 0 ) {
				_maxThreads = Mathf.Max( GetProcessCount(), 1 );
			}
		}
		
		~CachedPararellThreadQueue()
		{
			if( _threads != null ) {
				_Finalize();
			}
		}
		
		public void _Finalize()
		{
			bool isFinalized = false;
			lock(this) {
				isFinalized = _isFinalized;
				_isFinalized = true;
				if( !isFinalized ) {
					_invokeEvent.Set();
				}
			}
			
			if( isFinalized ) {
				return; // If finalizing, return function.
			}

			if( _threads != null ) {
				for( int i = 0; i != _threads.Length; ++i ) {
					_threads[i].Join();
				}
				
				_threads = null;
			}
			
			lock(this) {
				_isFinalized = false;
			}
		}
		
		public ThreadQueueHandle Invoke( Function function, int length )
		{
			ThreadQueueHandle r = new ThreadQueueHandle();
			if( function == null ) {
				return r;
			}
			bool isFinalized = false;
			lock(this) {
				isFinalized = _isFinalized;
				if( !isFinalized ) {
					_AwakeThread();

					Queue queue = null;
					for( int i = _unusedQueues.Count - 1; i >= 0; --i ) {
						queue = (Queue)_unusedQueues[ i ];
						if( !queue.processingWaitEnd ) {
							_unusedQueues.RemoveAt( i );
							queue.Reuse( function, length );
							break;
						} else {
							queue = null;
						}
					}
					if( queue == null ) {
						queue = new Queue( function, length );
					}
					queue.uniqueID = _uniqueID;
					unchecked {
						++_uniqueID;
					}

					_reservedQueues.Add( queue );
					r.queuePtr = queue;
					r.queueID = queue.queueID;
					r.uniqueID = queue.uniqueID;
					queue = null;
					_invokeEvent.Set();
				}
			}
			if( isFinalized ) {
				function( 0, length ); // If finalizing, invoke directly.
			}
			return r;
		}
		
		public void WaitEnd( ref ThreadQueueHandle queueHandle )
		{
			if( queueHandle.queuePtr == null ) {
				return;
			}
			
			Queue queue = null;
			lock(this) {
				queue = _FindQueue( _processedQueues, ref queueHandle );
				if( queue == null ) {
					if( _IsEqualQueue( _processingQueue, ref queueHandle ) ) {
						queue = _processingQueue;
					} else {
						queue = _FindQueue( _reservedQueues, ref queueHandle );
					}
				}
				if( queue != null ) {
					queue.processingWaitEnd = true; // Denied recycle.
				}
			}
			
			if( queue == null ) {
				queueHandle.Reset();
				return;
			}
			
			for(;;) {
				InstantSleep();
				
				queue.processedEvent.WaitOne();
				
				lock(this) {
					if( queue.queueID != queueHandle.queueID ) {
						queue.processingWaitEnd = false; // Accept recycle.
						queue = null;
					}
				}
				if( queue == null ) {
					queueHandle.Reset();
					break;
				}
			}
		}
		
		void _Run()
		{
			for(;;) {
				Queue queue = null;
				int threadIndex = 0;
				bool isProcessing = false;
				bool isFinalized = false;
				bool isEmpty = false;
				_invokeEvent.WaitOne();
				lock(this) {
					if( _processingQueue != null ) {
						queue = _processingQueue;
					} else if( _reservedQueues.Count != 0 ) {
						queue = (Queue)_reservedQueues[0];
						_reservedQueues.RemoveAt( 0 );
						_processingQueue = queue;
					}
					if( queue != null ) {
						threadIndex = queue.processingThreads;
						++(queue.processingThreads);
						if( queue.processingThreads == _maxThreads ) {
							_processingQueue = null;
							_processedQueues.Add( queue );
						}
					}
					isProcessing = (queue != null);
					isFinalized = _isFinalized;
					isEmpty = _processingQueue == null && _reservedQueues.Count == 0;
				}
				
				if( queue != null ) {
					int length = (queue.length + _maxThreads - 1) / _maxThreads;
					int index = threadIndex * length;
					if( index < queue.length ) {
						if( index + length > queue.length ) {
							length = queue.length - index;
						}
						if( queue.function != null ) {
							queue.function( index, length );
						}
					}

					lock(this) {
						if( ++(queue.processedThreads) == _maxThreads ) {
							queue.Unuse();
							_processedQueues.Remove( queue );
							_unusedQueues.Add( queue );
							queue.processedEvent.Set();
							queue = null;
							isFinalized = _isFinalized;
							isEmpty = _processingQueue == null && _reservedQueues.Count == 0;
							if( isEmpty ) {
								_invokeEvent.Reset();
							}
						}
					}
				}
				
				if( isEmpty && isFinalized ) {
					break;
				}
				if( !isProcessing ) {
					InstantSleep();
				}
			}
		}
	}
}
