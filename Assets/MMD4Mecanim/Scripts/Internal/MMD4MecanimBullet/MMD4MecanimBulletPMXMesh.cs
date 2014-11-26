//#define USE_SINGLETHREAD
using UnityEngine;
using System.Collections;
using BulletXNA;
using BulletXNA.BulletCollision;
using BulletXNA.BulletDynamics;
using BulletXNA.LinearMath;
using MMD4Mecanim;

using PMXModel					= MMD4MecanimBulletPMXModel;
using PMXBone					= MMD4MecanimBulletPMXBone;
using MeshFlags					= MMD4MecanimData.MeshFlags;
using VertexData				= MMD4MecanimAuxData.VertexData;
using CachedPararellThreadQueue	= MMD4MecanimBulletPhysicsUtil.CachedPararellThreadQueue;
using ThreadQueueHandle			= MMD4MecanimBulletPhysicsUtil.ThreadQueueHandle;

public class MMD4MecanimBulletPMXMesh
{
	public PMXModel	_model = null;
	public PMXModel model { get { return _model; } }

	public int				meshID = -1;
	public uint				meshFlags = 0;
	public bool				isMorphChanged = false;
	public bool				isXDEFChanged = false;
	public bool				isChanged { get { return isMorphChanged || isXDEFChanged; } }

	public Vector3[]		vertices;
	public Vector3[]		backupVertices;
	public Vector3[]		normals;
	public Vector3[]		backupNormals;
	public BoneWeight[]		boneWeights;
	public IndexedMatrix[]	bindposes;

	int _xdefSDEFVertexIndex = 0;
	int _xdefSDEFVertexCount = 0;
	IndexedMatrix[] _xdefBoneTransformCache;
	IndexedMatrix[] _xdefBoneTransformInvCache;
	IndexedQuaternion[] _xdefBoneRotationCache;

	public void Destroy()
	{
		this.vertices = null;
		this.backupVertices = null;
		this.normals = null;
		this.backupNormals = null;
		this.boneWeights = null;
		this.bindposes = null;
		_xdefBoneTransformCache = null;
		_xdefBoneTransformInvCache = null;
		_xdefBoneRotationCache = null;
	}

	public bool UploadMesh( int meshID, Vector3[] vertices, Vector3[] normals, BoneWeight[] boneWeights, Matrix4x4[] bindposes )
	{
		this.meshID = meshID;
		this.vertices = null;
		this.backupVertices = vertices;
		this.normals = null;
		this.backupNormals = normals;
		this.boneWeights = boneWeights;

		if( bindposes != null ) {
			this.bindposes = new IndexedMatrix[bindposes.Length];
			for( int i = 0; i != bindposes.Length; ++i ) {
				this.bindposes[i] = MMD4MecanimBulletPhysicsUtil.MakeIndexedMatrix( ref bindposes[i] );
			}
		}
		if( !_PrepareAndValidationCheck() ) {
			Destroy();
			return false;
		}

		return true;
	}

	public void PrepareUpdate()
	{
		this.isMorphChanged = false;
	}

	public void PrepareMorph()
	{
		// No clear isMorphChanged
		this.isXDEFChanged = false;
		if( this.backupVertices == null ) {
			M4MDebug.LogError("");
			return;
		}

		if( this.vertices == null || this.vertices.Length != this.backupVertices.Length ) {
			this.vertices = (Vector3[])this.backupVertices.Clone();
		} else {
			System.Array.Copy( this.backupVertices, this.vertices, this.backupVertices.Length );
		}
	}

	public bool PrepareXDEF()
	{
		if( _model.isVertexMorphEnabled && _model.isXDEFEnabled ) {
			// Nothing.
		} else {
			return false;
		}

		unchecked {
			if( ( this.meshFlags & (uint)MeshFlags.XDEF ) == 0 ) {
				return false;
			}
			if( _model.isBlendShapesEnabled ) {
				if( ( this.meshFlags & (uint)MeshFlags.BlendShapes ) != 0 ) {
					return false;
				}
			}
		}

		if( _xdefBoneTransformCache == null ) {
			return false;
		}
	
		// todo: _isXDEFDepended & matrix change check.
		this.isXDEFChanged = true;
		return true;
	}

	public void ProcessXDEF( ref IndexedMatrix xdefRootTransformInv, ref IndexedQuaternion xdefRootRotationInv )
	{
		if( !this.isXDEFChanged ) {
			return;
		}

		if( this.meshID != 1 ) {
			return;
		}

		if( !this.isMorphChanged ) {
			if( this.vertices == null || this.vertices.Length != this.backupVertices.Length ) {
				this.vertices = (Vector3[])this.backupVertices.Clone();
			} else {
				System.Array.Copy( this.backupVertices, this.vertices, this.backupVertices.Length );
			}
		}
		bool isXDEFNormalEnabled = _model.isXDEFNormalEnabled;
		if( isXDEFNormalEnabled ) {
			if( this.normals == null || this.normals.Length != this.backupNormals.Length ) {
				this.normals = (Vector3[])this.backupNormals.Clone();
			} else {
				System.Array.Copy( this.backupNormals, this.normals, this.backupNormals.Length );
			}
		}

		if( _xdefBoneTransformCache == null ||
		    _xdefBoneTransformInvCache == null ||
		    _xdefBoneRotationCache == null ) {
			return;
		}
		
		if( this.boneWeights == null || this.vertices == null ) {
			return;
		}
		
		VertexData vertexData = _model.vertexData;
		if( vertexData == null ) {
			return;
		}

		VertexData.MeshBoneInfo meshBoneInfo = new VertexData.MeshBoneInfo();
		vertexData.GetMeshBoneInfo( ref meshBoneInfo, this.meshID );
		
		// Prepare to SDEF
		for( int boneIndex = 0; boneIndex != meshBoneInfo.count; ++boneIndex ) {
			VertexData.BoneFlags boneFlags = vertexData.GetBoneFlags( ref meshBoneInfo, boneIndex );
			if( ( boneFlags & VertexData.BoneFlags.SDEF ) != VertexData.BoneFlags.None ) {
				int boneID = vertexData.GetBoneID( ref meshBoneInfo, boneIndex );
				PMXBone bone = _model.GetBone( boneID );
				M4MDebug.Assert( bone != null );
				
				_xdefBoneTransformCache[boneIndex] = xdefRootTransformInv * bone.worldTransform;
				_BulletToUnityTransform( ref _xdefBoneTransformCache[boneIndex] );
				if( ( boneFlags & VertexData.BoneFlags.OptimizeBindPoses ) != VertexData.BoneFlags.None ) { // Translate only.
					_xdefBoneTransformCache[boneIndex]._origin = _xdefBoneTransformCache[boneIndex] * this.bindposes[boneIndex]._origin;
				} else {
					_xdefBoneTransformCache[boneIndex] *= this.bindposes[boneIndex];
				}
				_xdefBoneTransformInvCache[boneIndex] = _xdefBoneTransformCache[boneIndex].Inverse();
				_xdefBoneRotationCache[boneIndex] = xdefRootRotationInv * bone.worldTransform.GetRotation();
				_xdefBoneRotationCache[boneIndex].Y = -_xdefBoneRotationCache[boneIndex].Y;
				_xdefBoneRotationCache[boneIndex].Z = -_xdefBoneRotationCache[boneIndex].Z;
			}
		}

		#if USE_SINGLETHREAD
		_PararellProcessXDEF( 0, this.vertices.Length );
		#else
		if( this.vertices.Length <= 32 ) {
			_PararellProcessXDEF( 0, this.vertices.Length );
		} else {
			CachedPararellThreadQueue threadQueue = MMD4MecanimBulletPMXModel.GetXDEFPararellThreadQueue();
			M4MDebug.Assert( threadQueue != null );
			if( threadQueue != null ) {
				ThreadQueueHandle handle = threadQueue.Invoke( _PararellProcessXDEF, this.vertices.Length );
				threadQueue.WaitEnd( ref handle );
			}
		}
		#endif
	}

	void _PararellProcessXDEF( int vertexIndex, int vertexCount )
	{
		M4MDebug.Assert( _model != null );
		VertexData vertexData = _model.vertexData;
		M4MDebug.Assert( vertexData != null );
		
		bool isXDEFNormalEnabled = _model.isXDEFNormalEnabled;
		
		VertexData.MeshVertexInfo meshVertexInfo = new VertexData.MeshVertexInfo();
		vertexData.GetMeshVertexInfo( ref meshVertexInfo, this.meshID );
		
		int xdefSDEFVertexIndex = _xdefSDEFVertexIndex;
		
		for( int i = 0; i != vertexIndex; ++i ) {
			VertexData.VertexFlags vertexFlags = vertexData.GetVertexFlags( ref meshVertexInfo, i );
			if( ( vertexFlags & VertexData.VertexFlags.SDEF ) != VertexData.VertexFlags.None ) {
				++xdefSDEFVertexIndex;
			}
		}
		
		M4MDebug.Assert( vertexIndex < this.vertices.Length );
		M4MDebug.Assert( vertexIndex + vertexCount <= this.vertices.Length );
		
		VertexData.SDEFParams sdefParams = new VertexData.SDEFParams();
		int xdefSDEFOffset = vertexData.SDEFIndexToSDEFOffset( xdefSDEFVertexIndex );
		int vertexIndexEnd = vertexIndex + vertexCount;
		for( ; vertexIndex != vertexIndexEnd; ++vertexIndex ) {
			VertexData.VertexFlags vertexFlags = vertexData.GetVertexFlags( ref meshVertexInfo, vertexIndex );
			if( ( vertexFlags & VertexData.VertexFlags.SDEF ) != VertexData.VertexFlags.None ) {
				int boneIndex0 = this.boneWeights[vertexIndex].boneIndex0;
				int boneIndex1 = this.boneWeights[vertexIndex].boneIndex1;
				float boneWeight0 = this.boneWeights[vertexIndex].weight0;
				float boneWeight1 = this.boneWeights[vertexIndex].weight1;
				
				vertexData.GetSDEFParams( ref sdefParams, ref xdefSDEFOffset );
				
				IndexedVector3 sdefR0 = _xdefBoneTransformCache[boneIndex0] * sdefParams.r0 * boneWeight0;
				IndexedVector3 sdefC0 = (_xdefBoneTransformCache[boneIndex0] * sdefParams.c - sdefParams.c) * boneWeight0;
				IndexedQuaternion rotation0 = _xdefBoneRotationCache[boneIndex0];
				
				IndexedVector3 sdefR1 = _xdefBoneTransformCache[boneIndex1] * sdefParams.r1 * boneWeight1;
				IndexedVector3 sdefC1 = (_xdefBoneTransformCache[boneIndex1] * sdefParams.c - sdefParams.c) * boneWeight1;
				IndexedQuaternion rotation1 = _xdefBoneRotationCache[boneIndex1];
				
				IndexedVector3 pos = ((sdefR0 + sdefR1) + (sdefC0 + sdefC1 + sdefParams.c)) * 0.5f;
				IndexedQuaternion rot = MMD4MecanimBulletPhysicsUtil.Slerp( ref rotation0, ref rotation1, boneWeight1 );
				IndexedBasisMatrix basis = new IndexedBasisMatrix( ref rot );
				
				IndexedVector3 vertex = this.vertices[vertexIndex];
				IndexedVector3 vertex2 = basis * (vertex - sdefParams.c) + pos;
				vertex = _xdefBoneTransformInvCache[boneIndex0] * vertex2 * boneWeight0;
				vertex += _xdefBoneTransformInvCache[boneIndex1] * vertex2 * boneWeight1;
				this.vertices[vertexIndex] = vertex;
				
				if( isXDEFNormalEnabled ) {
					IndexedVector3 normal = this.normals[vertexIndex];
					IndexedVector3 normal2 = basis * normal;
					normal = _xdefBoneTransformInvCache[boneIndex0]._basis * normal2 * boneWeight0;
					normal += _xdefBoneTransformInvCache[boneIndex1]._basis * normal2 * boneWeight1;
					float len = normal.Length();
					if( Mathf.Abs(len) > Mathf.Epsilon ) {
						this.normals[vertexIndex] = normal * (1.0f / len);
					}
				}
			}
		}
	}

	bool _PrepareAndValidationCheck()
	{
		if( this.meshID < 0 ) {
			return false;
		}

		unchecked {
			int meshID = this.meshID;

			if( _model.isVertexMorphEnabled && _model.isXDEFEnabled ) {
				VertexData vertexData = _model.vertexData;
				if( vertexData != null && this.backupVertices != null && this.bindposes != null && this.boneWeights != null
				   && (this.meshFlags & (uint)MeshFlags.XDEF) != 0 ) {
					int vertexLength = this.backupVertices.Length;
					int boneLength = this.bindposes.Length;

					bool invalidateAnything = false;

					VertexData.MeshBoneInfo meshBoneInfo = new VertexData.MeshBoneInfo();
					VertexData.MeshVertexInfo meshVertexInfo = new VertexData.MeshVertexInfo();
					vertexData.GetMeshBoneInfo( ref meshBoneInfo, meshID );
					vertexData.GetMeshVertexInfo( ref meshVertexInfo, meshID );

					if( !vertexData.PrecheckMeshBoneInfo( ref meshBoneInfo ) ) {
						M4MDebug.Log( "PrecheckMeshBoneInfo() failed. meshID:" + meshID );
						invalidateAnything = true;
					}
					if( !vertexData.PrecheckMeshVertexInfo( ref meshVertexInfo ) ) {
						M4MDebug.Log( "PrecheckMeshVertexInfo() failed. meshID:" + meshID );
						invalidateAnything = true;
					}
					if( meshVertexInfo.count != vertexLength ) {
						M4MDebug.Log( "meshVertexInfo.count is invalid. meshID:" + meshID
						             + " meshVertexInfo.count:" + meshVertexInfo.count
						             + " vertexLength:" + vertexLength );
						invalidateAnything = true;
					}
					if( meshVertexInfo.count != this.boneWeights.Length ) {
						M4MDebug.Log( "meshVertexInfo.count is invalid. meshID:" + meshID
						             + " meshVertexInfo.count:" + meshVertexInfo.count
						             + " this.boneWeights.Length:" + vertexLength );
						invalidateAnything = true;
					}
					if( meshBoneInfo.count != boneLength ) {
						M4MDebug.Log( "meshBoneInfo.count is invalid. meshID:" + meshID
						             + " meshVertexInfo.count:" + meshVertexInfo.count
						             + " vertexLength:" + vertexLength );
						invalidateAnything = true;
					}

					// Check boneID and Swap boneIndex & weight for SDEF
					if( !invalidateAnything ) {
						M4MDebug.Assert( vertexLength == meshVertexInfo.count );
						M4MDebug.Assert( vertexLength == this.boneWeights.Length );
						for( int vertexIndex = 0; vertexIndex != meshVertexInfo.count; ++vertexIndex ) {
							VertexData.VertexFlags vertexFlags = vertexData.GetVertexFlags( ref meshVertexInfo, vertexIndex );
							if( ( vertexFlags & VertexData.VertexFlags.SDEF ) != VertexData.VertexFlags.None ) {
								int boneIndex0 = this.boneWeights[vertexIndex].boneIndex0;
								int boneIndex1 = this.boneWeights[vertexIndex].boneIndex1;
								int boneID0 = vertexData.GetBoneID( ref meshBoneInfo, boneIndex0 );
								int boneID1 = vertexData.GetBoneID( ref meshBoneInfo, boneIndex1 );
								if( _model.GetBone( boneID0 ) != null && _model.GetBone( boneID1 ) != null ) {
									if( ( vertexFlags & VertexData.VertexFlags.SDEFSwapIndex ) != VertexData.VertexFlags.None ) {
										this.boneWeights[vertexIndex].boneIndex0 = boneIndex1;
										this.boneWeights[vertexIndex].boneIndex1 = boneIndex0;
										float weight = this.boneWeights[vertexIndex].weight0;
										this.boneWeights[vertexIndex].weight0 = this.boneWeights[vertexIndex].weight1;
										this.boneWeights[vertexIndex].weight1 = weight;
									}
								} else {
									M4MDebug.Log( "boneID is Nothing. meshID:" + meshID + 
									             " vertexIndex:" + vertexIndex +
									             " boneIndex0:" + boneIndex0 +
									             " boneIndex1:" + boneIndex1 +
									             " boneID0:" + boneID0 +
									             " boneID1:" + boneID1 );
									invalidateAnything = true;
									break;
								}
							}
						}
					}
					
					if( !invalidateAnything ) {
						// Precheck boneID
						M4MDebug.Assert( this.bindposes.Length == meshBoneInfo.count );
						for( int boneIndex = 0; boneIndex != meshBoneInfo.count; ++boneIndex ) {
							int boneID = vertexData.GetBoneID( ref meshBoneInfo, boneIndex );
							M4MDebug.Assert( _model.GetBone( boneID ) != null );
							if( _model.GetBone( boneID ) != null ) {
								// Nothing.
							} else {
								M4MDebug.Log( "invalidate boneID. meshID:" + meshID + " boneID:" + boneID );
								invalidateAnything = true;
								break;
							}
						}
					}
					
					if( !invalidateAnything ) {
						// Prepare SDEFVertexIndex / SDEFVertexCount per mesh.
						_xdefSDEFVertexIndex = 0;
						_xdefSDEFVertexCount = 0;
						
						for( int vertexIndex = 0; vertexIndex != meshVertexInfo.count; ++vertexIndex ) {
							VertexData.VertexFlags vertexFlags = vertexData.GetVertexFlags( ref meshVertexInfo, vertexIndex );
							if( ( vertexFlags & VertexData.VertexFlags.SDEF ) != VertexData.VertexFlags.None ) {
								++_xdefSDEFVertexCount;
							}
						}
						
						for( int m = 0; m != this.meshID; ++m ) {
							if( _model.meshList[m] != null ) {
								_xdefSDEFVertexIndex += _model.meshList[m]._xdefSDEFVertexCount;
							}
						}
						
						if( !vertexData.PrecheckGetSDEFParams( _xdefSDEFVertexIndex, _xdefSDEFVertexCount ) ) {
							M4MDebug.Log( "invalidate SDEFVertexIndex/Count. meshID:" + meshID +
							             " _xdefSDEFVertexIndex:" + _xdefSDEFVertexIndex +
							             " _xdefSDEFVertexCount:" + _xdefSDEFVertexCount );
							invalidateAnything = true;
						}
					}
					
					if( invalidateAnything ) {
						return false;
					}
					
					if( !invalidateAnything ) {
						// Allocate
						if( _xdefSDEFVertexCount > 3 ) { // Optimized. SDEF wait proseccing is missing, I think.
							if( !invalidateAnything && meshBoneInfo.count != 0 ) {
								_xdefBoneTransformCache = new IndexedMatrix[meshBoneInfo.count];
								_xdefBoneTransformInvCache = new IndexedMatrix[meshBoneInfo.count];
								_xdefBoneRotationCache = new IndexedQuaternion[meshBoneInfo.count];
							}
						}
					}
				}
			}
		}
		
		return true;
	}

	void _BulletToUnityTransform( ref IndexedMatrix transform )
	{
		// RightHand to LeftHand
		transform._basis._el0.Y = -transform._basis._el0.Y;
		transform._basis._el0.Z = -transform._basis._el0.Z;
		transform._basis._el1.X = -transform._basis._el1.X;
		transform._basis._el2.X = -transform._basis._el2.X;
		transform._origin.X = -transform._origin.X;
		transform._origin *= _model.bulletToWorldScale;
	}
}
