using SourceFormatParser.Common;
using System;
using System.Collections.Generic;

namespace SourceFormatParser.BSP
{
	/// <summary>
	/// source2013: public/bspfile.h
	///		dheader_t
	///		lump_t
	/// </summary>
	public class SourceBSPStructs
	{
		public const byte HEADER_LUMPS = 64;
		public const int IDBSPHEADER = ('P' << 24) + ('S' << 16) + ('B' << 8) + 'V';

		#region enums
		[Flags]
		public enum MAP_FLAGS
		{
			/// <summary>Map was processed by vrad with -staticproplighting, no hdr data</summary>
			LVLFLAGS_BAKED_STATIC_PROP_LIGHTING_NONHDR = 0x00000001,

			/// <summary>Map was processed by vrad with -staticproplighting, in hdr</summary>
			LVLFLAGS_BAKED_STATIC_PROP_LIGHTING_HDR = 0x00000002
		}

		[Flags]
		public enum LEAF_FLAGS
		{
			// NOTE: Only 7-bits stored!!!

			/// <summary>This leaf has 3D sky in its PVS</summary>
			LEAF_FLAGS_SKY = 0x01,

			/// <summary>This leaf culled away some portals due to radial vis</summary>
			LEAF_FLAGS_RADIAL = 0x02,

			/// <summary>This leaf has 2D sky in its PVS</summary>
			LEAF_FLAGS_SKY2D = 0x04
		}

		[Flags]
		public enum WORLDLIGHT_FLAGS
		{
			/// <summary>This says that the light was put into the per-leaf ambient cubes.</summary>
			DWL_FLAGS_INAMBIENTCUBE = 0x0001,

			/// <summary>This says that the light will cast shadows from entities.</summary>
			DWL_FLAGS_CASTENTITYSHADOWS = 0x0002
		}

		[Flags]
		public enum emittype_t
		{
			// lights that were used to illuminate the world

			/// <summary>90 degree spotlight</summary>
			emit_surface,

			/// <summary>simple point light source</summary>
			emit_point,

			/// <summary>spotlight with penumbra</summary>
			emit_spotlight,

			/// <summary>directional light with no falloff (surface must trace to SKY texture)</summary>
			emit_skylight,

			/// <summary>linear falloff, non-lambertian</summary>
			emit_quakelight,

			/// <summary>spherical light source with no falloff (surface must trace to SKY texture)</summary>
			emit_skyambient,
		}
		#endregion

		#region structs
		/// <summary>Header. Size: 780 bytes</summary>
		public struct dheader_t
		{
			public int ident;
			public int version;
			public lump_t[] lumps;//[HEADER_LUMPS];
			public int mapRevision;                // the map's revision (iteration, version) number (added BSPVERSION 6)
		}

		/// <summary>Lump info. Size: 12 bytes</summary>
		public struct lump_t
		{
			public int fileofs, filelen;
			public int version;				// default to zero
											// this field was char fourCC[4] previously, but was unused, favoring the LUMP IDs above instead. It has been
											// repurposed for compression.  0 implies the lump is not compressed.
			public int uncompressedSize;	// default to zero


			//additional
			public int lumpNum;
		}

		/// <summary>Plane. Size: 20 bytes</summary>
		public struct dplane_t
		{
			// planes (x) and (x)+1 are always opposites
			public SourceVector normal;
			public float dist;
			public int type;       // PLANE_X - PLANE_ANYZ ?remove? trivial to regenerate

#if UNITY
			public Plane toPlane() => new Plane(?, ?, ?);
#endif
		}

		/// <summary>Texture data. Size: 32 bytes</summary>
		public struct dtexdata_t
		{
			public SourceVector reflectivity;
			public int nameStringTableID;              // index into g_StringTable for the texture name
			public int width, height;                  // source image
			public int view_width, view_height;        //
		}

		/// <summary>Visibility. Size: ? bytes</summary>
		public struct dvis_t
		{
			public int numclusters;
			public int[][] bitofs;// bitofs[numclusters=8][2]

			public HashSet<byte>[] pvs; //potentially visible set
			public HashSet<byte>[] pas; //potentially audible set
		}

		/// <summary>Node. Size: 30 bytes</summary>
		public struct dnode_t
		{
			public int planenum;
			public int[] children;//[2];				// negative numbers are -(leafs+1), not nodes
			public SourceVectorShort mins;//short[3];   // for frustom culling
			public SourceVectorShort maxs;//short[3];
			public ushort firstface;
			public ushort numfaces;						// counting both sides
			public short area;							// If all leaves below this node are in the same area, then
														// this is the area index. If not, this is -1.

			//additional
			//public short unused; //is always 0
		}

		/// <summary>Texture info. v0. Size: 72 bytes</summary>
		public struct texinfo_t
		{
			public SourceVector4[] textureVecsTexelsPerWorldUnits;//[2];	// [s/t][xyz offset]
			public SourceVector4[] lightmapVecsLuxelsPerWorldUnits;//[2];	// [s/t][xyz offset] - length is in units of texels/area
			public int flags;												// miptex flags + overrides
			public int texdata;												// Pointer to texture name, size, etc.
		}

		/// <summary>Face. Size: 56 bytes</summary>
		public struct dface_t
		{
			public ushort planenum;
			public byte side;			// faces opposite to the node's plane direction
			public byte onNode;			// 1 of on node, 0 if in leaf

			public int firstedge;		// we must support > 64k edges
			public short numedges;
			public short texinfo;
			// This is a union under the assumption that a fog volume boundary (ie. water surface)
			// isn't a displacement map.
			// FIXME: These should be made a union with a flags or type field for which one it is
			// if we can add more to this.
			//	union
			//	{
			public short dispinfo;
			// This is only for surfaces that are the boundaries of fog volumes
			// (ie. water surfaces)
			// All of the rest of the surfaces can look at their leaf to find out
			// what fog volume they are in.
			public short surfaceFogVolumeID;
			//	};

			// lighting info
			public byte[] styles;//[MAXLIGHTMAPS=4];
			public int lightofs;		// start of [numstyles*surfsize] samples
			public float area;

			// TODO: make these unsigned chars?
			public SourceVector2Int m_LightmapTextureMinsInLuxels;
			public SourceVector2Int m_LightmapTextureSizeInLuxels;

			public int origFace;		// reference the original face this face was derived from

			// non-polygon primitives (strips and lists)
			public ushort m_NumPrims;	// Top bit, if set, disables shadows on this surface (this is why there are accessors).
			public ushort firstPrimID;
			public uint smoothingGroups;


			public ushort GetNumPrims() => (ushort)(m_NumPrims & 0x7FFF);
			public void SetNumPrims(ushort nPrims)
			{
				//Assert((nPrims & 0x8000) == 0);
				if ((nPrims & 0x8000) == 0) return;
				m_NumPrims = (ushort)(m_NumPrims & ~0x7FFF);
				m_NumPrims = (ushort)(m_NumPrims | (nPrims & 0x7FFF));
			}
			public bool AreDynamicShadowsEnabled() => (m_NumPrims & 0x8000) == 0;
			public void SetDynamicShadowsEnabled(bool bEnabled)
			{
				if (bEnabled)
					m_NumPrims = (ushort)(m_NumPrims & ~0x8000);
				else
					m_NumPrims |= 0x8000;
			}
		}

		/// <summary>Map flags. Size: 4 bytes</summary>
		public struct dflagslump_t
		{
			/// <summary>
			/// use enum MAP_FLAGS
			/// </summary>
			public uint m_LevelFlags;                        // LVLFLAGS_xxx

			//additional
			public MAP_FLAGS[] getFlags() => FlagsUtils.getFlags<MAP_FLAGS>(m_LevelFlags);

			public override string ToString()
			{
				string s = string.Empty;
				foreach (MAP_FLAGS f in getFlags()) s += f.ToString() + " | ";
				if (s.Length > 3)
					s = s.Substring(0, s.Length - 3);
				return s;
			}
		}

		/// <summary>Occluder. Size: ? bytes</summary>
		public struct doccluder_t
		{
			//struct from valve dev wiki, can't find in source sdk 2013
			public int count;
			public object data; //[count];				//"doccluderdataV1_t[]" or "doccluderdataV2_t[]"
			public int polyDataCount;
			public doccluderpolydata_t[] polyData;//[polyDataCount];
			public int vertexIndexCount;
			public int[] vertexIndices;//[vertexIndexCount];
		}

		/// <summary>Occluder data. V1. Size: 36 bytes</summary>
		public struct doccluderdataV1_t
		{
			public int flags;
			public int firstpoly;				// index into doccluderpolys
			public int polycount;
			public SourceVector mins;
			public SourceVector maxs;
		}

		/// <summary>Occluder data. V2. Size: 40 bytes</summary>
		public struct doccluderdataV2_t
		{
			public int flags;
			public int firstpoly;				// index into doccluderpolys
			public int polycount;
			public SourceVector mins;
			public SourceVector maxs;
			public int area;
		}

		/// <summary>Occluder polygons. Size: 12 bytes</summary>
		public struct doccluderpolydata_t
		{
			public int firstvertexindex;		// index into doccludervertindices
			public int vertexcount;
			public int planenum;
		}

		/// <summary>Leaf. V0. Size: 56 bytes</summary>
		public struct dleafV0_t
		{
			public int contents;						// OR of all brushes (not needed?)

			public short cluster;

			//BEGIN_BITFIELD(bf );
			public short area;//:9;
			public short flags;//:7;					// Per leaf flags.
							   //END_BITFIELD();

			public SourceVectorShort mins;//short[3];	// for frustum culling
			public SourceVectorShort maxs;//short[3];

			public ushort firstleafface;
			public ushort numleaffaces;

			public ushort firstleafbrush;
			public ushort numleafbrushes;
			public short leafWaterDataID;				// -1 for not in water

			// Precaculated light info for entities.
			public CompressedLightCube m_AmbientLighting;

			//additional
			public LEAF_FLAGS[] getFlags() => FlagsUtils.getFlags<LEAF_FLAGS>(flags);
		}

		/// <summary>Leaf. V1. Size: 32 bytes</summary>
		public struct dleafV1_t
		{
			public int contents;						// OR of all brushes (not needed?)

			public short cluster;

			//BEGIN_BITFIELD(bf );
			public short area;//:9;
			public short flags;//:7;					// Per leaf flags.
										   //END_BITFIELD();

			public SourceVectorShort mins;//short[3];	// for frustum culling
			public SourceVectorShort maxs;//short[3];

			public ushort firstleafface;
			public ushort numleaffaces;

			public ushort firstleafbrush;
			public ushort numleafbrushes;
			public short leafWaterDataID;               // -1 for not in water

			// NOTE: removed this for version 1 and moved into separate lump "LUMP_LEAF_AMBIENT_LIGHTING" or "LUMP_LEAF_AMBIENT_LIGHTING_HDR"
			// Precaculated light info for entities.
			//	CompressedLightCube m_AmbientLighting;

			//additional
			public LEAF_FLAGS[] getFlags() => FlagsUtils.getFlags<LEAF_FLAGS>(flags);
		}

		/// <summary>Face ID. Size: 2 bytes</summary>
		public struct dfaceid_t
		{
			public ushort hammerfaceid;
		}

		/// <summary>Edge. Size: 4 bytes</summary>
		public struct dedge_t
		{
			// note that edge 0 is never used, because negative edge nums are used for
			// counterclockwise use of the edge in a face
			public SourceVector2Short v;//short[2];        // vertex numbers
		}

		/// <summary>Model. Size: 48 bytes</summary>
		public struct dmodel_t
		{
			public SourceVector mins, maxs;
			public SourceVector origin;			// for sounds or lights
			public int headnode;
			public int firstface, numfaces;     // submodels just draw faces without walking the bsp tree
		}

		/// <summary>World light. V0. Size: 88 bytes</summary>
		public struct dworldlightV0_t
		{
			public SourceVector origin;
			public SourceVector intensity;
			public SourceVector normal;			// for surfaces and spotlights
			public int cluster;
			public emittype_t type;				// is ushort?
			public int style;
			public float stopdot;				// start of penumbra for emit_spotlight
			public float stopdot2;				// end of penumbra for emit_spotlight
			public float exponent;				// 
			public float radius;                // cutoff distance
												// falloff for emit_spotlight + emit_point: 
												// 1 / (constant_attn + linear_attn * dist + quadratic_attn * dist^2)
			public float constant_attn;
			public float linear_attn;
			public float quadratic_attn;
			public int flags;					// Uses a combination of the DWL_FLAGS_ defines.
			public int texinfo;					// 
			public int owner;                   // entity that this light it relative to

			//additional
			public WORLDLIGHT_FLAGS[] getFlags() => FlagsUtils.getFlags<WORLDLIGHT_FLAGS>(flags);
		}

		/// <summary>World light. V1. Size: 100 bytes</summary>
		public struct dworldlightV1_t
		{
			public SourceVector origin;
			public SourceVector intensity;
			public SourceVector normal;					// for surfaces and spotlights
			public SourceVector shadow_cast_offset;		// gets added to the light origin when this light is used as a shadow caster (only if DWL_FLAGS_CASTENTITYSHADOWS flag is set)
			public int cluster;
			public emittype_t type;
			public int style;
			public float stopdot;						// start of penumbra for emit_spotlight
			public float stopdot2;						// end of penumbra for emit_spotlight
			public float exponent;						// 
			public float radius;                        // cutoff distance
														// falloff for emit_spotlight + emit_point: 
														// 1 / (constant_attn + linear_attn * dist + quadratic_attn * dist^2)
			public float constant_attn;
			public float linear_attn;
			public float quadratic_attn;
			public int flags;							// Uses a combination of the DWL_FLAGS_ defines.
			public int texinfo;							// 
			public int owner;                           // entity that this light it relative to

			//additional
			public WORLDLIGHT_FLAGS[] getFlags() => FlagsUtils.getFlags<WORLDLIGHT_FLAGS>(flags);
		}
		#endregion
	}
}
