﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using MMD4Mecanim;

public static class MMD4MecanimAuxData
{
	//------------------------------------------------------------------------------------------------
	
	public class IndexData
	{
		public const int VertexIndexBitMask	= 0xffffff;
		public const int MeshCountBitShift	= 24;
		public const int HeaderSize			= 2;

		public int[]						indexValues;
		
		public int vertexCount {
			get {
				if( this.indexValues != null && 0 < this.indexValues.Length ) {
					return (this.indexValues[0] & VertexIndexBitMask);
				}
				
				return 0;
			}
		}
		
		public int meshCount {
			get {
				if( this.indexValues != null && 1 < this.indexValues.Length ) {
					unchecked {
						return (int)((uint)this.indexValues[1] >> MeshCountBitShift);
					}
				}
				
				return 0;
			}
		}
		
		public int meshVertexCount {
			get {
				if( this.indexValues != null && 1 < this.indexValues.Length ) {
					return (this.indexValues[1] & VertexIndexBitMask);
				}
				
				return 0;
			}
		}
	}

	public static IndexData BuildIndexData( TextAsset indexFile )
	{
		if( indexFile == null ) {
			Debug.LogError( "BuildIndexData: indexFile is norhing." );
			return null;
		}

		return BuildIndexData( indexFile.bytes );
	}

	public static IndexData BuildIndexData( byte[] indexBytes )
	{
		if( indexBytes == null || indexBytes.Length == 0 ) {
			Debug.LogError( "BuildIndexData: Nothing indexBytes." );
			return null;
		}
		int valueLength = indexBytes.Length / 4;
		if( valueLength < 2 ) {
			Debug.LogError( "BuildIndexData:modelFile is unsupported fomart." );
			return null;
		}
		
		int[] indexValues = new int[valueLength];
		#if UNITY_WEBPLAYER
		System.Buffer.BlockCopy( indexBytes, 0, indexValues, 0, valueLength * 4 );
		#else
		GCHandle gch = GCHandle.Alloc( indexBytes, GCHandleType.Pinned );
		Marshal.Copy( gch.AddrOfPinnedObject(), indexValues, 0, valueLength );
		gch.Free();
		#endif
		
		IndexData indexData = new IndexData();
		indexData.indexValues = indexValues;
		return indexData;
	}
	
	public static bool ValidateIndexData( IndexData indexData, SkinnedMeshRenderer[] skinnedMeshRenderers )
	{
		if( indexData == null || skinnedMeshRenderers == null ) {
			return false;
		}
		
		if( indexData.meshCount != skinnedMeshRenderers.Length ) {
			Debug.LogError( "ValidateIndexData: FBX reimported. Disabled morph, please recreate index file." );
			return false;
		} else {
			int meshVertexCount = 0;
			foreach( SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers ) {
				if( skinnedMeshRenderer.sharedMesh != null ) {
					meshVertexCount += skinnedMeshRenderer.sharedMesh.vertexCount;
				}
			}
			if( indexData.meshVertexCount != meshVertexCount ) {
				Debug.LogError( "ValidateIndexData: FBX reimported. Disabled morph, please recreate index file." );
				return false;
			}
		}
		
		return true;
	}

	//------------------------------------------------------------------------------------------------------------
	
	public class VertexData
	{
		public const int VertexIndexBitMask			= 0xffffff;
		public const int MeshCountBitShift			= 24;
		public const int MeshBoneOffsetBitMask		= 0xffffff;
		public const int MeshVertexOffsetBitMask	= 0xffffff;

		public const int HeaderSize					= 5;
		public const int FloatHeaderSize			= 2;

		public struct MeshBoneInfo
		{
			public bool isSDEF;
			public bool isQDEF;
			public bool isXDEF; // SDEF/QDEF
			public int index;
			public int count;
		}
		
		public struct MeshVertexInfo
		{
			public int index;
			public int count;
		}
		
		[System.Flags]
		public enum MeshFlags
		{
			None				= 0,
			SDEF				= unchecked((int)0x80000000),
			QDEF				= unchecked((int)0x40000000),
			XDEF				= unchecked((int)0xc0000000),
		}
		
		[System.Flags]
		public enum BoneFlags
		{
			None				= 0,
			SDEF				= 0x01,
			QDEF				= 0x02,
			XDEF				= 0x03,
			OptimizeBindPoses	= 0x04,	// Translate only.
		}
		
		[System.Flags]
		public enum VertexFlags
		{
			None				= 0,
			SDEF				= 0x01,
			QDEF				= 0x02,
			XDEF				= 0x03,
			SDEFSwapIndex		= 0x80,
		}

		public struct SDEFParams
		{
			public Vector3 c;
			public Vector3 r0;
			public Vector3 r1;
		}
		
		public int[]						intValues;
		public float[]						floatValues;
		public byte[]						byteValues;
		int									_meshCount;
		int									_meshBoneIDOffset;

		/*
			int[0] >> 24					... (Reserved, 0)
			int[0] & 0xffffff				... vertexCount( for Validation Check. )
			int[1] >> 24					... meshCount
			int[1] & 0xffffff				... meshVertexCount
			int[2]							... intValuesCount
			int[3]							... floatValuesCount
			int[4]							... byteValuesCount

			int[meshCount + 1] & 0xffffff	... meshBoneOffset( 0x80000000 != 0, Contains meshSDEFFlag, 0x40000000 != 0, QDEF )
			int[meshCount + 1] & 0xffffff	... meshVertexOffset

			int[] x meshCount x boneCount	... meshBoneID

			byte[boneOffset]				... boneXDEFFlags & boneOptimizeBindPosesFlags(BindPose as Translate only)
			x meshCount x boneCount(meshXDEFFlag != 0 only)
			byte[vertexOffset]				... vertexValues
			x meshCount x vertexCount(meshXDEFFlag != 0 only)
		*/

		public static int GetIntValueCount( byte[] bytes )
		{
			// Memo: for Version 0
			return HeaderSize + MMD4MecanimCommon.ReadInt( bytes, 2 );
		}
		
		public static int GetFloatValueCount( byte[] bytes )
		{
			// Memo: for Version 0
			return MMD4MecanimCommon.ReadInt( bytes, 3 );
		}
		
		public static int GetByteValueCount( byte[] bytes )
		{
			// Memo: for Version 0
			return MMD4MecanimCommon.ReadInt( bytes, 4 );
		}
		
		public void PostfixBuild()
		{
			if( this.intValues != null && 1 < this.intValues.Length ) {
				unchecked {
					_meshCount = (int)((uint)this.intValues[1] >> MeshCountBitShift);
				}
			} else {
				Debug.LogError("");
			}
			
			_meshBoneIDOffset = HeaderSize + ((_meshCount + 1) << 1);
		}

		public int vertexCount {
			get {
				if( this.intValues != null && 0 < this.intValues.Length ) {
					return (this.intValues[0] & VertexIndexBitMask);
				} else {
					Debug.LogError("");
				}
				
				return 0;
			}
		}
		
		public int meshCount {
			get {
				return _meshCount;
			}
		}
		
		public int meshVertexCount {
			get {
				if( this.intValues != null && 1 < this.intValues.Length ) {
					return (this.intValues[1] & VertexIndexBitMask);
				} else {
					Debug.LogError("");
				}
				
				return 0;
			}
		}

		public void GetMeshBoneInfo( ref MeshBoneInfo r, int meshIndex )
		{
			unchecked {
				int meshCount = this.meshCount;
				if( (uint)meshIndex < (uint)meshCount ) {
					int ofst = HeaderSize + meshIndex;
					if( this.intValues != null && ofst + 1 < this.intValues.Length ) {
						r.isXDEF = (this.intValues[ofst] & (int)MeshFlags.XDEF) != 0;
						r.isSDEF = (this.intValues[ofst] & (int)MeshFlags.SDEF) != 0;
						r.isQDEF = (this.intValues[ofst] & (int)MeshFlags.QDEF) != 0;
						r.index = (this.intValues[ofst + 0] & MeshBoneOffsetBitMask);
						r.count = (this.intValues[ofst + 1] & MeshBoneOffsetBitMask) - r.index;
					}
				} else {
					Debug.LogError("");
				}
			}
		}
		
		public void GetMeshVertexInfo( ref MeshVertexInfo r, int meshIndex )
		{
			unchecked {
				int meshCount = this.meshCount;
				if( (uint)meshIndex < (uint)meshCount ) {
					int ofst = HeaderSize + meshCount + 1 + meshIndex;
					if( this.intValues != null && ofst + 1 < this.intValues.Length ) {
						r.index = (this.intValues[ofst + 0] & MeshVertexOffsetBitMask);
						r.count = (this.intValues[ofst + 1] & MeshVertexOffsetBitMask) - r.index;
					}
				} else {
					Debug.LogError("");
				}
			}
		}
		
		public bool PrecheckMeshBoneInfo( ref MeshBoneInfo meshBoneInfo )
		{
			unchecked {
				int ofst = _meshBoneIDOffset;
				return this.byteValues != null && (uint)(meshBoneInfo.index + meshBoneInfo.count) <= this.byteValues.Length
					&& this.intValues != null && (uint)(ofst + meshBoneInfo.index + meshBoneInfo.count) <= this.intValues.Length;
			}
		}
		
		public BoneFlags GetBoneFlags( ref MeshBoneInfo meshBoneInfo, int boneIndex )
		{
			// Memo: Require PrecheckMeshBoneInfo()
			unchecked {
				return (BoneFlags)this.byteValues[meshBoneInfo.index + boneIndex];
			}
		}

		public int GetBoneID( ref MeshBoneInfo meshBoneInfo, int boneIndex )
		{
			// Memo: Require PrecheckMeshBoneInfo()
			unchecked {
				int ofst = _meshBoneIDOffset;
				return this.intValues[ofst + meshBoneInfo.index + boneIndex];
			}
		}
		
		public bool PrecheckMeshVertexInfo( ref MeshVertexInfo meshVertexInfo )
		{
			unchecked {
				return this.byteValues != null && (uint)(meshVertexInfo.index + meshVertexInfo.count) <= this.byteValues.Length;
			}
		}
		
		public VertexFlags GetVertexFlags( ref MeshVertexInfo meshVertexInfo, int vertexIndex )
		{
			// Memo: Require PrecheckMeshVertexInfo()
			unchecked {
				return (VertexFlags)this.byteValues[meshVertexInfo.index + vertexIndex];
			}
		}
		
		public float vertexScale {
			get {
				if( this.floatValues != null && 0 < this.floatValues.Length ) {
					return this.floatValues[0];
				} else {
					Debug.LogError("");
				}
				return 0.0f;
			}
		}
		
		public float importScale {
			get {
				if( this.floatValues != null && 1 < this.floatValues.Length ) {
					return this.floatValues[1];
				} else {
					Debug.LogError("");
				}
				return 0.0f;
			}
		}

		public bool PrecheckGetSDEFParams( int sdefIndex, int sdefCount )
		{
			unchecked {
				uint ofst = (uint)FloatHeaderSize + (uint)(sdefIndex + sdefCount) * 9u;
				return ofst <= (uint)this.floatValues.Length;
			}
		}
		
		// 1st SDEF index only
		public int SDEFIndexToSDEFOffset( int sdefIndex )
		{
			return FloatHeaderSize + sdefIndex * 9;
		}

		// Increment every XDEFVertexFlags.SDEF
		public void GetSDEFParams( ref SDEFParams r, ref int ofst )
		{
			unchecked {
				M4MDebug.Assert( (uint)ofst + 9u <= (uint)this.floatValues.Length );
				r.c = new Vector3( this.floatValues[ofst + 0], this.floatValues[ofst + 1], this.floatValues[ofst + 2] );
				r.r0 = new Vector3( this.floatValues[ofst + 3], this.floatValues[ofst + 4], this.floatValues[ofst + 5] );
				r.r1 = new Vector3( this.floatValues[ofst + 6], this.floatValues[ofst + 7], this.floatValues[ofst + 8] );
				ofst += 9;
			}
		}
	}

	//------------------------------------------------------------------------------------------------

	public static VertexData BuildVertexData( TextAsset vertexFile )
	{
		if( vertexFile == null ) {
			Debug.LogError( "BuildVertexData: xdefFile is norhing." );
			return null;
		}

		return BuildVertexData( vertexFile.bytes );
	}

	public static VertexData BuildVertexData( byte[] vertexBytes )
	{
		if( vertexBytes == null ) {
			Debug.LogError( "BuildVertexData: vertexBytes is null." );
			return null;
		}
		if( vertexBytes.Length == 0 ) {
			Debug.LogError( "BuildVertexData: vertexBytes.Length == 0" );
			return null;
		}
		
		unchecked {
			int intValueCount	= VertexData.GetIntValueCount( vertexBytes );
			int floatValueCount	= VertexData.GetFloatValueCount( vertexBytes );
			int byteValueCount	= VertexData.GetByteValueCount( vertexBytes );
			if( intValueCount <= 0 || floatValueCount < 0 || byteValueCount < 0 ) {
				Debug.LogError( "BuildVertexData: Unknown format." );
				return null;
			}
			if( vertexBytes.Length != intValueCount * 4 + floatValueCount * 4 + byteValueCount ) {
				Debug.LogError( "BuildVertexData: Invalid format(Not enough file length)." );
				return null;
			}
			
			VertexData vertexData = new VertexData();
			#if UNITY_WEBPLAYER
			int filePos = 0;
			
			vertexData.intValues = new int[intValueCount];
			System.Buffer.BlockCopy( vertexBytes, filePos, vertexData.intValues, 0, intValueCount * 4 );
			filePos += intValueCount * 4;
			
			if( floatValueCount > 0 ) {
				vertexData.floatValues = new float[floatValueCount];
				System.Buffer.BlockCopy( vertexBytes, filePos, vertexData.floatValues, 0, floatValueCount * 4 );
				filePos += floatValueCount * 4;
			}
			if( byteValueCount > 0 ) {
				vertexData.byteValues = new byte[byteValueCount];
				System.Buffer.BlockCopy( vertexBytes, filePos, vertexData.byteValues, 0, byteValueCount );
			}
			#else
			int filePos = 0;
			GCHandle gch = GCHandle.Alloc( vertexBytes, GCHandleType.Pinned );
			IntPtr addr = gch.AddrOfPinnedObject();
			vertexData.intValues = new int[intValueCount];
			Marshal.Copy( new IntPtr( addr.ToInt64() + (long)filePos ), vertexData.intValues, 0, intValueCount );
			filePos += intValueCount * 4;
			if( floatValueCount > 0 ) {
				vertexData.floatValues = new float[floatValueCount];
				Marshal.Copy( new IntPtr( addr.ToInt64() + (long)filePos ), vertexData.floatValues, 0, floatValueCount );
				filePos += floatValueCount * 4;
			}
			if( byteValueCount > 0 ) {
				vertexData.byteValues = new byte[byteValueCount];
				Marshal.Copy( new IntPtr( addr.ToInt64() + (long)filePos ), vertexData.byteValues, 0, byteValueCount );
				filePos += byteValueCount * 4;
			}
			gch.Free();
			#endif
			vertexData.PostfixBuild();
			return vertexData;
		}
	}
	
	public static bool ValidateVertexData( VertexData vertexData, SkinnedMeshRenderer[] skinnedMeshRenderers )
	{
		if( vertexData == null || skinnedMeshRenderers == null ) {
			return false;
		}
		
		if( vertexData.meshCount != skinnedMeshRenderers.Length ) {
			Debug.LogError( "ValidateXDEFData: FBX reimported. Disabled morph, please recreate xdef file." );
			return false;
		} else {
			int meshVertexCount = 0;
			foreach( SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers ) {
				if( skinnedMeshRenderer.sharedMesh != null ) {
					meshVertexCount += skinnedMeshRenderer.sharedMesh.vertexCount;
				}
			}
			if( vertexData.meshVertexCount != meshVertexCount ) {
				Debug.LogError( "ValidateXDEFData: FBX reimported. Disabled morph, please recreate xdef file." );
				return false;
			}
		}
		
		return true;
	}

}
