using SourceFormatParser.Common;
using System;

namespace SourceFormatParser.VHV
{
	public static class SourceVHVStructs
	{
		public const byte VHV_VERSION = 2;

		[Flags]
		public enum VertexFlags
		{
			VERTEX_POSITION = 0x0001,
			VERTEX_NORMAL = 0x0002,
			VERTEX_COLOR = 0x0004,
			VERTEX_SPECULAR = 0x0008
		}

		/// <summary>Mesh Header. Size: 28 bytes</summary>
		public struct MeshHeader_t
		{
			// this mesh is part of this lod
			public uint m_nLod;

			// this mesh has this many vertexes
			public uint m_nVertexes;

			// starting at this offset
			public uint m_nOffset;

			//public uint[] m_nUnused;//[4];

			public ColorRGBExp32[] m_VertexColors;
		}

		/// <summary>Header. Size: 40 bytes</summary>
		public struct FileHeader_t
		{
			// file version as defined by VHV_VERSION
			public int m_nVersion;

			// must match checkSum in the .mdl header
			public uint m_nChecksum;

			// a vertex consists of these components
			public VertexFlags m_nVertexFlags; //is uint

			// the byte size of a single vertex
			// this won't be adequate, need some concept of byte format i.e. rgbexp32 vs rgba8888
			public uint m_nVertexSize;

			// total number of vertexes
			public uint m_nVertexes;

			public int m_nMeshes;

			//public uint m_nUnused[4];

			//additional
			public MeshHeader_t[] pMesh;
			//void pVertexBase(int nMesh) => (void*)((byte*)this + pMesh(nMesh)->m_nOffset);
		}

	}
}
