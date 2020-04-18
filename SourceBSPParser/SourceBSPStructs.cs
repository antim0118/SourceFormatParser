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

		public const ushort OVERLAY_BSP_FACE_COUNT = 64;
		public const int OVERLAY_NUM_RENDER_ORDERS = (1 << OVERLAY_RENDER_ORDER_NUM_BITS);
		public const byte OVERLAY_RENDER_ORDER_NUM_BITS = 2;
		public const int OVERLAY_RENDER_ORDER_MASK = 0xC000;    // top 2 bits set

		public const ushort WATEROVERLAY_BSP_FACE_COUNT = 256;
		public const byte WATEROVERLAY_RENDER_ORDER_NUM_BITS = 2;
		public const int WATEROVERLAY_NUM_RENDER_ORDERS = (1 << WATEROVERLAY_RENDER_ORDER_NUM_BITS);
		public const int WATEROVERLAY_RENDER_ORDER_MASK = 0xC000;   // top 2 bits set

		public const int MAX_LIGHTMAPPAGE_WIDTH = 256;
		public const int MAX_LIGHTMAPPAGE_HEIGHT = 128;

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

		[Flags]
		public enum Contents
		{
			// contents flags are seperate bits
			// a given brush can contribute multiple content bits
			// multiple brushes can be in a single leaf

			/// <summary>No contents</summary>
			CONTENTS_EMPTY = 0,

			/// <summary>an eye is never valid in a solid</summary>
			CONTENTS_SOLID = 0x1,

			/// <summary>translucent, but not watery (glass)</summary>
			CONTENTS_WINDOW = 0x2,

			CONTENTS_AUX = 0x4,

			/// <summary>alpha-tested "grate" textures.  Bullets/sight pass through, but solids don't</summary>
			CONTENTS_GRATE = 0x8,

			CONTENTS_SLIME = 0x10,
			CONTENTS_WATER = 0x20,

			/// <summary>block AI line of sight</summary>
			CONTENTS_BLOCKLOS = 0x40,

			/// <summary>things that cannot be seen through (may be non-solid though)</summary>
			CONTENTS_OPAQUE = 0x80,

			CONTENTS_TESTFOGVOLUME = 0x100,
			CONTENTS_UNUSED = 0x200,
			CONTENTS_UNUSED6 = 0x400,

			/// <summary>per team contents used to differentiate collisions between players and objects on different teams</summary>
			CONTENTS_TEAM1 = 0x800,
			/// <summary>per team contents used to differentiate collisions between players and objects on different teams</summary>
			CONTENTS_TEAM2 = 0x1000,

			/// <summary>ignore CONTENTS_OPAQUE on surfaces that have SURF_NODRAW</summary>
			CONTENTS_IGNORE_NODRAW_OPAQUE = 0x2000,

			/// <summary>hits entities which are MOVETYPE_PUSH (doors, plats, etc.)</summary>
			CONTENTS_MOVEABLE = 0x4000,

			/// <summary>remaining contents are non-visible, and don't eat brushes</summary>
			CONTENTS_AREAPORTAL = 0x8000,

			CONTENTS_PLAYERCLIP = 0x10000,
			CONTENTS_MONSTERCLIP = 0x20000,

			/// <summary>can be added to any other contents, and may be mixed</summary>
			CONTENTS_CURRENT_0 = 0x40000,
			/// <summary>can be added to any other contents, and may be mixed</summary>
			CONTENTS_CURRENT_90 = 0x80000,
			/// <summary>can be added to any other contents, and may be mixed</summary>
			CONTENTS_CURRENT_180 = 0x100000,
			/// <summary>can be added to any other contents, and may be mixed</summary>
			CONTENTS_CURRENT_270 = 0x200000,
			/// <summary>can be added to any other contents, and may be mixed</summary>
			CONTENTS_CURRENT_UP = 0x400000,
			/// <summary>can be added to any other contents, and may be mixed</summary>
			CONTENTS_CURRENT_DOWN = 0x800000,

			/// <summary>removed before bsping an entity</summary>
			CONTENTS_ORIGIN = 0x1000000,

			/// <summary>should never be on a brush, only in game</summary>
			CONTENTS_MONSTER = 0x2000000,

			CONTENTS_DEBRIS = 0x4000000,

			/// <summary>brushes to be added after vis leafs</summary>
			CONTENTS_DETAIL = 0x8000000,

			/// <summary>auto set if any surface has trans</summary>
			CONTENTS_TRANSLUCENT = 0x10000000,

			CONTENTS_LADDER = 0x20000000,

			/// <summary>use accurate hitboxes on trace</summary>
			CONTENTS_HITBOX = 0x40000000
		}

		[Flags]
		public enum Surfaces
		{
			// NOTE: These are stored in a short in the engine now.  Don't use more than 16 bits

			/// <summary>value will hold the light strength</summary>
			SURF_LIGHT = 0x0001,

			/// <summary>don't draw, indicates we should skylight + draw 2d sky but not draw the 3D skybox</summary>
			SURF_SKY2D = 0x0002,

			/// <summary>don't draw, but add to skybox</summary>
			SURF_SKY = 0x0004,

			/// <summary>turbulent water warp</summary>
			SURF_WARP = 0x0008,

			SURF_TRANS = 0x0010,

			/// <summary>the surface can not have a portal placed on it</summary>
			SURF_NOPORTAL = 0x0020,

			/// <summary>FIXME: This is an xbox hack to work around elimination of trigger surfaces, which breaks occluders</summary>
			SURF_TRIGGER = 0x0040,

			/// <summary>don't bother referencing the texture</summary>
			SURF_NODRAW = 0x0080,

			/// <summary>make a primary bsp splitter</summary>
			SURF_HINT = 0x0100,

			/// <summary>completely ignore, allowing non-closed brushes</summary>
			SURF_SKIP = 0x0200,

			/// <summary>Don't calculate light</summary>
			SURF_NOLIGHT = 0x0400,

			/// <summary>calculate three lightmaps for the surface for bumpmapping</summary>
			SURF_BUMPLIGHT = 0x0800,

			/// <summary>Don't receive shadows</summary>
			SURF_NOSHADOWS = 0x1000,

			/// <summary>Don't receive decals</summary>
			SURF_NODECALS = 0x2000,

			/// <summary>Don't subdivide patches on this surface</summary>
			SURF_NOCHOP = 0x4000,

			/// <summary>surface is part of a hitbox</summary>
			SURF_HITBOX = 0x8000
		}

		[Flags]
		public enum NeighborSpan
		{
			// These denote where one dispinfo fits on another.
			// Note: tables are generated based on these indices so make sure to update
			//       them if these indices are changed.
			CORNER_TO_CORNER = 0,
			CORNER_TO_MIDPOINT = 1,
			MIDPOINT_TO_CORNER = 2
		}

		[Flags]
		public enum NeighborOrientation
		{
			// These define relative orientations of displacement neighbors.
			ORIENTATION_CCW_0 = 0,
			ORIENTATION_CCW_90 = 1,
			ORIENTATION_CCW_180 = 2,
			ORIENTATION_CCW_270 = 3
		}

		public enum GAMELUMP_CODES
		{
			GAMELUMP_DETAIL_PROPS = ('d') << 24 | ('p') << 16 | ('r') << 8 | ('p') << 0,
			GAMELUMP_DETAIL_PROP_LIGHTING = ('d') << 24 | ('p') << 16 | ('l') << 8 | ('t') << 0,
			GAMELUMP_STATIC_PROPS = ('s') << 24 | ('p') << 16 | ('r') << 8 | ('p') << 0,
			GAMELUMP_DETAIL_PROP_LIGHTING_HDR = ('d') << 24 | ('p') << 16 | ('l') << 8 | ('h') << 0,
			Unknown
		}

		[Flags]
		public enum STATICPROP_FLAGS
		{
			/// <summary>automatically computed</summary>
			STATIC_PROP_FLAG_FADES = 0x1,
			/// <summary>automatically computed</summary>
			STATIC_PROP_USE_LIGHTING_ORIGIN = 0x2,
			/// <summary>automatically computed; computed at run time based on dx level</summary>
			STATIC_PROP_NO_DRAW = 0x4,

			/// <summary>set in WC</summary>
			STATIC_PROP_IGNORE_NORMALS = 0x8,
			/// <summary>set in WC</summary>
			STATIC_PROP_NO_SHADOW = 0x10,
			/// <summary>set in WC</summary>
			STATIC_PROP_UNUSED = 0x20,

			/// <summary>in vrad, compute lighting at lighting origin, not for each vertex</summary>
			STATIC_PROP_NO_PER_VERTEX_LIGHTING = 0x40,

			/// <summary>disable self shadowing in vrad</summary>
			STATIC_PROP_NO_SELF_SHADOWING = 0x80
		}

		[Flags]
		public enum DetailPropType_t
		{
			DETAIL_PROP_TYPE_MODEL = 0,
			DETAIL_PROP_TYPE_SPRITE,
			DETAIL_PROP_TYPE_SHAPE_CROSS,
			DETAIL_PROP_TYPE_SHAPE_TRI,
		}

		[Flags]
		public enum dprimitive_type
		{
			PRIM_TRILIST = 0,
			PRIM_TRISTRIP = 1,
		}

		[Flags]
		public enum DISPTRI_TAGS
		{
			DISPTRI_TAG_SURFACE = 1 << 0,
			DISPTRI_TAG_WALKABLE = 1 << 1,
			DISPTRI_TAG_BUILDABLE = 1 << 2,
			DISPTRI_FLAG_SURFPROP1 = 1 << 3,
			DISPTRI_FLAG_SURFPROP2 = 1 << 4,
			DISPTRI_TAG_REMOVE = 1 << 5
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
			public int version;             // default to zero
											// this field was char fourCC[4] previously, but was unused, favoring the LUMP IDs above instead. It has been
											// repurposed for compression.  0 implies the lump is not compressed.
			public int uncompressedSize;    // default to zero


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
			public ushort numfaces;                     // counting both sides
			public short area;                          // If all leaves below this node are in the same area, then
														// this is the area index. If not, this is -1.

			//additional
			//public short unused; //is always 0
		}

		/// <summary>Texture info. v0. Size: 72 bytes</summary>
		public struct texinfo_t
		{
			public SourceVector4[] textureVecsTexelsPerWorldUnits;//[2];	// [s/t][xyz offset]
			public SourceVector4[] lightmapVecsLuxelsPerWorldUnits;//[2];	// [s/t][xyz offset] - length is in units of texels/area
			public int flags;                                               // miptex flags + overrides
			public int texdata;                                             // Pointer to texture name, size, etc.
		}

		/// <summary>Face. Size: 56 bytes</summary>
		public struct dface_t
		{
			public ushort planenum;
			public byte side;           // faces opposite to the node's plane direction
			public byte onNode;         // 1 of on node, 0 if in leaf

			public int firstedge;       // we must support > 64k edges
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
			public int lightofs;        // start of [numstyles*surfsize] samples
			public float area;

			// TODO: make these unsigned chars?
			public SourceVector2Int m_LightmapTextureMinsInLuxels;
			public SourceVector2Int m_LightmapTextureSizeInLuxels;

			public int origFace;        // reference the original face this face was derived from

			// non-polygon primitives (strips and lists)
			public ushort m_NumPrims;   // Top bit, if set, disables shadows on this surface (this is why there are accessors).
			public ushort firstPrimID;
			public uint smoothingGroups;


			public ushort GetNumPrims => (ushort)(m_NumPrims & 0x7FFF);
			public void SetNumPrims(ushort nPrims)
			{
				//Assert((nPrims & 0x8000) == 0);
				if ((nPrims & 0x8000) == 0) return;
				m_NumPrims = (ushort)(m_NumPrims & ~0x7FFF);
				m_NumPrims = (ushort)(m_NumPrims | (nPrims & 0x7FFF));
			}
			public bool AreDynamicShadowsEnabled => (m_NumPrims & 0x8000) == 0;
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
			public MAP_FLAGS[] getFlags => FlagsUtils.getFlags<MAP_FLAGS>(m_LevelFlags);

			public override string ToString()
			{
				string s = string.Empty;
				foreach (MAP_FLAGS f in getFlags) s += f.ToString() + " | ";
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
			public int firstpoly;               // index into doccluderpolys
			public int polycount;
			public SourceVector mins;
			public SourceVector maxs;
		}

		/// <summary>Occluder data. V2. Size: 40 bytes</summary>
		public struct doccluderdataV2_t
		{
			public int flags;
			public int firstpoly;               // index into doccluderpolys
			public int polycount;
			public SourceVector mins;
			public SourceVector maxs;
			public int area;
		}

		/// <summary>Occluder polygons. Size: 12 bytes</summary>
		public struct doccluderpolydata_t
		{
			public int firstvertexindex;        // index into doccludervertindices
			public int vertexcount;
			public int planenum;
		}

		/// <summary>Leaf. V0. Size: 56 bytes</summary>
		public struct dleafV0_t
		{
			public int contents;                        // OR of all brushes (not needed?)

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

			// Precaculated light info for entities.
			public CompressedLightCube m_AmbientLighting;

			//additional
			public LEAF_FLAGS[] getFlags => FlagsUtils.getFlags<LEAF_FLAGS>(flags);
		}

		/// <summary>Leaf. V1. Size: 32 bytes</summary>
		public struct dleafV1_t
		{
			public int contents;                        // OR of all brushes (not needed?)

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
			public LEAF_FLAGS[] getFlags => FlagsUtils.getFlags<LEAF_FLAGS>(flags);
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
			public SourceVector origin;         // for sounds or lights
			public int headnode;
			public int firstface, numfaces;     // submodels just draw faces without walking the bsp tree
		}

		/// <summary>World light. V0. Size: 88 bytes</summary>
		public struct dworldlightV0_t
		{
			public SourceVector origin;
			public SourceVector intensity;
			public SourceVector normal;         // for surfaces and spotlights
			public int cluster;
			public emittype_t type;             // is ushort?
			public int style;
			public float stopdot;               // start of penumbra for emit_spotlight
			public float stopdot2;              // end of penumbra for emit_spotlight
			public float exponent;              // 
			public float radius;                // cutoff distance
												// falloff for emit_spotlight + emit_point: 
												// 1 / (constant_attn + linear_attn * dist + quadratic_attn * dist^2)
			public float constant_attn;
			public float linear_attn;
			public float quadratic_attn;
			public int flags;                   // Uses a combination of the DWL_FLAGS_ defines.
			public int texinfo;                 // 
			public int owner;                   // entity that this light it relative to

			//additional
			public WORLDLIGHT_FLAGS[] getFlags => FlagsUtils.getFlags<WORLDLIGHT_FLAGS>(flags);
		}

		/// <summary>World light. V1. Size: 100 bytes</summary>
		public struct dworldlightV1_t
		{
			public SourceVector origin;
			public SourceVector intensity;
			public SourceVector normal;                 // for surfaces and spotlights
			public SourceVector shadow_cast_offset;     // gets added to the light origin when this light is used as a shadow caster (only if DWL_FLAGS_CASTENTITYSHADOWS flag is set)
			public int cluster;
			public emittype_t type;
			public int style;
			public float stopdot;                       // start of penumbra for emit_spotlight
			public float stopdot2;                      // end of penumbra for emit_spotlight
			public float exponent;                      // 
			public float radius;                        // cutoff distance
														// falloff for emit_spotlight + emit_point: 
														// 1 / (constant_attn + linear_attn * dist + quadratic_attn * dist^2)
			public float constant_attn;
			public float linear_attn;
			public float quadratic_attn;
			public int flags;                           // Uses a combination of the DWL_FLAGS_ defines.
			public int texinfo;                         // 
			public int owner;                           // entity that this light it relative to

			//additional
			public WORLDLIGHT_FLAGS[] getFlags => FlagsUtils.getFlags<WORLDLIGHT_FLAGS>(flags);
		}

		/// <summary>Brush. Size: 12 bytes</summary>
		public struct dbrush_t
		{
			public int firstside;
			public int numsides;
			public int contents;

			//additional
			public Contents[] getContents => FlagsUtils.getFlags<Contents>(contents);
		}

		/// <summary>Brush side. Size: 8 bytes</summary>
		public struct dbrushside_t
		{
			public ushort planenum;     // facing out of the leaf
			public short texinfo;
			public short dispinfo;      // displacement info (BSPVERSION 7)
			public short bevel;         // is the side a bevel plane? (BSPVERSION 7)
		}

		/// <summary>Area. Size: 8 bytes</summary>
		public struct darea_t
		{
			public int numareaportals, firstareaportal;
		}

		/// <summary>Areaportal. Size: 12 bytes</summary>
		public struct dareaportal_t
		{
			// each area has a list of portals that lead into other areas
			// when portals are closed, other areas may not be visible or
			// hearable even if the vis info says that it should be
			public ushort m_PortalKey;              // Entities have a key called portalnumber (and in vbsp a variable
													// called areaportalnum) which is used
													// to bind them to the area portals by comparing with this value.

			public ushort otherarea;                // The area this portal looks into.

			public ushort m_FirstClipPortalVert;    // Portal geometry.
			public ushort m_nClipPortalVerts;

			public int planenum;
		}

		/// <summary>Portal. Size: 16 bytes</summary>
		public struct dportal_t
		{
			public int firstportalvert;
			public int numportalverts;
			public int planenum;
			public ushort[] cluster;//[2];
		}

		/// <summary>Prop collision. Size: 8 bytes</summary>
		public struct dpropcollision_t
		{
			public int m_nHullCount, m_nHullStart;
		}

		/// <summary>Cluster. Size: 8 bytes</summary>
		public struct dcluster_t
		{
			public int firstportal, numportals;
		}

		/// <summary>Prop hull. Size: 16 bytes</summary>
		public struct dprophull_t
		{
			public int m_nVertCount, m_nVertStart;
			public int m_nSurfaceProp;
			public uint m_nContents;
		};

		/// <summary>Prop hull triangle. Size: 8 bytes</summary>
		public struct dprophulltris_t
		{
			public int m_nIndexStart, m_nIndexCount;
		}

		/// <summary>Displacement. Size: 176 bytes</summary>
		public struct ddispinfo_t
		{
			public SourceVector startPosition;						// start position used for orientation -- (added BSPVERSION 6)
			public int m_iDispVertStart;							// Index into LUMP_DISP_VERTS.
			public int m_iDispTriStart;								// Index into LUMP_DISP_TRIS.

			public int power;										// power - indicates size of map (2^power + 1)
			public int minTess;										// minimum tesselation allowed
			public float smoothingAngle;							// lighting smoothing angle
			public int contents;									// surface contents

			public ushort m_iMapFace;								// Which map face this displacement comes from.

			public int m_iLightmapAlphaStart;						// Index into ddisplightmapalpha.
																	// The count is m_pParent->lightmapTextureSizeInLuxels[0]*m_pParent->lightmapTextureSizeInLuxels[1].

			public int m_iLightmapSamplePositionStart;				// Index into LUMP_DISP_LIGHTMAP_SAMPLE_POSITIONS.

			//=^=46 bytes=^=

			public CDispNeighbor[] m_EdgeNeighbors;//[4];			// Indexed by NEIGHBOREDGE_ defines.
			public CDispCornerNeighbors[] m_CornerNeighbors;//[4];	// Indexed by CORNER_ defines.

			//ALLOWEDVERTS_SIZE = PAD_NUMBER(MAX_DISPVERTS, 32) / 32
			public ulong[] m_AllowedVerts;//[ALLOWEDVERTS_SIZE=10];// This is built based on the layout and sizes of our neighbors
										  // and tells us which vertices are allowed to be active.

			//additional methods
			public int NumVerts => ((1 << (power)) + 1) * ((1 << (power)) + 1);
			public int NumTris => ((1 << (power)) * (1 << (power)) * 2);
		}

		/// <summary>Displacement Subneighbor. Size: 5 bytes</summary>
		public struct CDispSubNeighbor
		{
			public ushort m_iNeighbor;     // This indexes into ddispinfos.
										   // 0xFFFF if there is no neighbor here.

			public byte m_NeighborOrientation;        // (CCW) rotation of the neighbor wrt this displacement.

			// These use the NeighborSpan type.
			public byte m_Span;                       // Where the neighbor fits onto this side of our displacement.
			public byte m_NeighborSpan;               // Where we fit onto our neighbor.


			//additional
			public ushort GetNeighborIndex => m_iNeighbor;
			public NeighborSpan GetSpan => (NeighborSpan)m_Span;
			public NeighborSpan GetNeighborSpan => (NeighborSpan)m_NeighborSpan;
			public NeighborOrientation GetNeighborOrientation => (NeighborOrientation)m_NeighborOrientation;

			public bool IsValid => m_iNeighbor != 0xFFFF;
			public void SetInvalid() { m_iNeighbor = 0xFFFF; }
		}

		/// <summary>Displacement Neighbor. Size: 10 bytes</summary>
		public struct CDispNeighbor
		{
			// Note: if there is a neighbor that fills the whole side (CORNER_TO_CORNER),
			//       then it will always be in CDispNeighbor::m_Neighbors[0]
			public CDispSubNeighbor[] m_SubNeighbors;//[2];


			//additional methods
			public void SetInvalid() { m_SubNeighbors[0].SetInvalid(); m_SubNeighbors[1].SetInvalid(); }

			// Returns false if there isn't anything touching this edge.
			public bool IsValid => m_SubNeighbors[0].IsValid || m_SubNeighbors[1].IsValid;
		}

		/// <summary>Displacement Corner Neighbors. Size: 9 bytes</summary>
		public struct CDispCornerNeighbors
		{
			public ushort[] m_Neighbors;//[MAX_DISP_CORNER_NEIGHBORS=4];  // indices of neighbors.
			public byte m_nNeighbors;

			//additional methods
			public void SetInvalid() { m_nNeighbors = 0; }
		}

		/// <summary>Displacement Physics. Size: 2 bytes</summary>
		public struct dphysdisp_t
		{
			// contains the binary blob for each displacement surface's virtual hull
			public ushort numDisplacements;
			public ushort[] dataSize;//[numDisplacements];
		}

		/// <summary>Displacement Physics. Size: 8 bytes</summary>
		public struct dphysmodel_t
		{
			public int modelIndex;
			public int dataSize;
			public int keydataSize;
			public int solidCount;

			//additional
			//public phy solid;
			public string keydata;
		}

		/// <summary>Displacement vertex. Size: 20 bytes</summary>
		public class CDispVert
		{
			public SourceVector m_vVector;		// Vector field defining displacement volume.
			public float m_flDist;				// Displacement distances.
			public float m_flAlpha;				// "per vertex" alpha values.
		}

		/// <summary>Game lump header. Size: 4 bytes</summary>
		public struct dgamelumpheader_t
		{
			public int lumpCount;

			//additional
			public dgamelump_t[] lumps;

			//static props
			public StaticPropDictLump_t[] staticPropNames;
			public StaticPropLeafLump_t[] staticPropLeafs;
			/// <summary>Usage (depending on lump version): (StaticPropLumpV?_t[])staticProps</summary>
			public object staticProps;

			//detail props
			public DetailObjectDictLump_t[] detailPropNames;
			public DetailSpriteDictLump_t[] detailPropSprites;
			public DetailObjectLump_t[] detailProps;
			public DetailPropLightstylesLump_t[] detailPropLighting, detailPropLightingHDR;
		}

		/// <summary>Game lump info. Size: 20 bytes</summary>
		public struct dgamelump_t
		{
			public int id; // This is expected to be a four-CC code ('lump')
			public ushort flags;
			public ushort version;
			public int fileofs;
			public int filelen;

			//additional
			public GAMELUMP_CODES id_code => (GAMELUMP_CODES)(id);
		}

		/// <summary>Game lump: Static prop dictionary. Size: 128 bytes</summary>
		public struct StaticPropDictLump_t
		{
			public string m_Name;//char[STATIC_PROP_NAME_LENGTH=128];       // model name
		}

		/// <summary>Game lump: Static prop leaf. Size: 2 bytes</summary>
		public struct StaticPropLeafLump_t
		{
			public ushort m_Leaf;
		}

		/// <summary>Game lump: Static prop. V4. Size: 56 bytes</summary>
		public struct StaticPropLumpV4_t
		{
			public SourceVector m_Origin;
			public SourceQAngle m_Angles;
			public ushort m_PropType;
			public ushort m_FirstLeaf;
			public ushort m_LeafCount;
			public byte m_Solid;
			public byte m_Flags;
			public int m_Skin;
			public float m_FadeMinDist;
			public float m_FadeMaxDist;
			public SourceVector m_LightingOrigin;
			//	int				m_Lighting;			// index into the GAMELUMP_STATIC_PROP_LIGHTING lump

			//additional
			public STATICPROP_FLAGS[] getFlags => FlagsUtils.getFlags<STATICPROP_FLAGS>(m_Flags);
		}

		/// <summary>Game lump: Static prop. V5. Size: 60 bytes</summary>
		public struct StaticPropLumpV5_t
		{
			public SourceVector m_Origin;
			public SourceQAngle m_Angles;
			public ushort m_PropType;
			public ushort m_FirstLeaf;
			public ushort m_LeafCount;
			public byte m_Solid;
			public byte m_Flags;
			public int m_Skin;
			public float m_FadeMinDist;
			public float m_FadeMaxDist;
			public SourceVector m_LightingOrigin;
			public float m_flForcedFadeScale;
			//	int				m_Lighting;			// index into the GAMELUMP_STATIC_PROP_LIGHTING lump

			//additional
			public STATICPROP_FLAGS[] getFlags => FlagsUtils.getFlags<STATICPROP_FLAGS>(m_Flags);
		}

		/// <summary>Game lump: Static prop. V6. Size: 64 bytes</summary>
		public struct StaticPropLumpV6_t
		{
			public SourceVector m_Origin;
			public SourceQAngle m_Angles;
			public ushort m_PropType;
			public ushort m_FirstLeaf;
			public ushort m_LeafCount;
			public byte m_Solid;
			public byte m_Flags;
			public int m_Skin;
			public float m_FadeMinDist;
			public float m_FadeMaxDist;
			public SourceVector m_LightingOrigin;
			public float m_flForcedFadeScale;
			public ushort m_nMinDXLevel;
			public ushort m_nMaxDXLevel;
			//	int				m_Lighting;			// index into the GAMELUMP_STATIC_PROP_LIGHTING lump

			//additional
			public STATICPROP_FLAGS[] getFlags => FlagsUtils.getFlags<STATICPROP_FLAGS>(m_Flags);
		}

		/// <summary>Game lump: Static prop. V7. Size: 68 bytes</summary>
		public struct StaticPropLumpV7_t
		{
			public SourceVector m_Origin;
			public SourceQAngle m_Angles;
			public ushort m_PropType;
			public ushort m_FirstLeaf;
			public ushort m_LeafCount;
			public byte m_Solid;
			public byte m_Flags;
			public int m_Skin;
			public float m_FadeMinDist;
			public float m_FadeMaxDist;
			public SourceVector m_LightingOrigin;
			public float m_flForcedFadeScale;
			public ushort m_nMinDXLevel;
			public ushort m_nMaxDXLevel;
			//	int				m_Lighting;			// index into the GAMELUMP_STATIC_PROP_LIGHTING lump
			public SourceColor32 m_DiffuseModulation;    // per instance color and alpha modulation

			//additional
			public STATICPROP_FLAGS[] getFlags => FlagsUtils.getFlags<STATICPROP_FLAGS>(m_Flags);
		}

		/// <summary>Game lump: Static prop. V8. Size: 68 bytes</summary>
		public struct StaticPropLumpV8_t
		{
			public SourceVector m_Origin;
			public SourceQAngle m_Angles;
			public ushort m_PropType;
			public ushort m_FirstLeaf;
			public ushort m_LeafCount;
			public byte m_Solid;
			public byte m_Flags;
			public int m_Skin;
			public float m_FadeMinDist;
			public float m_FadeMaxDist;
			public SourceVector m_LightingOrigin;
			public float m_flForcedFadeScale;
			public byte m_nMinCPULevel;
			public byte m_nMaxCPULevel;
			public byte m_nMinGPULevel;
			public byte m_nMaxGPULevel;
			//	int				m_Lighting;			// index into the GAMELUMP_STATIC_PROP_LIGHTING lump
			public SourceColor32 m_DiffuseModulation;    // per instance color and alpha modulation

			//additional
			public STATICPROP_FLAGS[] getFlags => FlagsUtils.getFlags<STATICPROP_FLAGS>(m_Flags);
		}

		/// <summary>Game lump: Static prop. V9. Size: 72 bytes</summary>
		public struct StaticPropLumpV9_t
		{
			public SourceVector m_Origin;
			public SourceQAngle m_Angles;
			public ushort m_PropType;
			public ushort m_FirstLeaf;
			public ushort m_LeafCount;
			public byte m_Solid;
			public byte m_Flags;
			public int m_Skin;
			public float m_FadeMinDist;
			public float m_FadeMaxDist;
			public SourceVector m_LightingOrigin;
			public float m_flForcedFadeScale;
			public byte m_nMinCPULevel;
			public byte m_nMaxCPULevel;
			public byte m_nMinGPULevel;
			public byte m_nMaxGPULevel;
			//	int				m_Lighting;			// index into the GAMELUMP_STATIC_PROP_LIGHTING lump
			public SourceColor32 m_DiffuseModulation;    // per instance color and alpha modulation
			public int m_bDisableX360;

			//additional
			public STATICPROP_FLAGS[] getFlags => FlagsUtils.getFlags<STATICPROP_FLAGS>(m_Flags);
		}

		/// <summary>Game lump: Static prop. V10. Size: 76 bytes</summary>
		public struct StaticPropLumpV10_t
		{
			public SourceVector m_Origin;
			public SourceQAngle m_Angles;
			public ushort m_PropType;
			public ushort m_FirstLeaf;
			public ushort m_LeafCount;
			public byte m_Solid;
			public byte m_Flags;
			public int m_Skin;
			public float m_FadeMinDist;
			public float m_FadeMaxDist;
			public SourceVector m_LightingOrigin;
			public float m_flForcedFadeScale;
			public byte m_nMinCPULevel;
			public byte m_nMaxCPULevel;
			public byte m_nMinGPULevel;
			public byte m_nMaxGPULevel;
			//	int				m_Lighting;			// index into the GAMELUMP_STATIC_PROP_LIGHTING lump
			public SourceColor32 m_DiffuseModulation;    // per instance color and alpha modulation
			public int m_bDisableX360;	//???
			public uint m_FlagsEx;		//???

			//additional
			public STATICPROP_FLAGS[] getFlags => FlagsUtils.getFlags<STATICPROP_FLAGS>(m_Flags);
			//public STATICPROP_FLAGSEX[] getFlagsEx => FlagsUtils.getFlags<STATICPROP_FLAGSEX>(FlagsEx);
		}

		/// <summary>Game lump: Static prop. V11. Size: 76 bytes</summary>
		public struct StaticPropLumpV11_t
		{
			public SourceVector m_Origin;
			public SourceQAngle m_Angles;
			public ushort m_PropType;
			public ushort m_FirstLeaf;
			public ushort m_LeafCount;
			public byte m_Solid;
			public byte m_Flags;
			public int m_Skin;
			public float m_FadeMinDist;
			public float m_FadeMaxDist;
			public SourceVector m_LightingOrigin;
			public float m_flForcedFadeScale;
			public byte m_nMinCPULevel;
			public byte m_nMaxCPULevel;
			public byte m_nMinGPULevel;
			public byte m_nMaxGPULevel;
			//	int				m_Lighting;			// index into the GAMELUMP_STATIC_PROP_LIGHTING lump
			public SourceColor32 m_DiffuseModulation;    // per instance color and alpha modulation
			public int m_bDisableX360;  //???
			public uint m_FlagsEx;      //???
			public float m_UniformScale;

			//additional
			public STATICPROP_FLAGS[] getFlags => FlagsUtils.getFlags<STATICPROP_FLAGS>(m_Flags);
			//public STATICPROP_FLAGSEX[] getFlagsEx => FlagsUtils.getFlags<STATICPROP_FLAGSEX>(FlagsEx);
		}

		/// <summary>Game lump: Detail prop dictionary. Size: 128 bytes</summary>
		public struct DetailObjectDictLump_t
		{
			// Model index when using studiomdls for detail props
			public string m_Name; //char[DETAIL_NAME_LENGTH=128];        // model name
		}

		/// <summary>Game lump: Detail prop sprite info. Size: 8 bytes</summary>
		public struct DetailSpriteDictLump_t
		{
			// Information about the sprite to render

			// NOTE: All detail prop sprites must lie in the material detail/detailsprites
			public SourceVector2 m_UL;      // Coordinate of upper left 
			public SourceVector2 m_LR;      // Coordinate of lower right
			public SourceVector2 m_TexUL;   // Texcoords of upper left
			public SourceVector2 m_TexLR;   // Texcoords of lower left
		}

		/// <summary>Game lump: Detail prop. Size: 52 bytes</summary>
		public struct DetailObjectLump_t
		{
			public SourceVector m_Origin;
			public SourceQAngle m_Angles;
			public ushort m_DetailModel;		// either index into DetailObjectDictLump_t or DetailPropSpriteLump_t
			public ushort m_Leaf;
			public ColorRGBExp32 m_Lighting;
			public uint m_LightStyles;
			public byte m_LightStyleCount;
			public byte m_SwayAmount;			// how much do the details sway
			public byte m_ShapeAngle;			// angle param for shaped sprites
			public byte m_ShapeSize;			// size param for shaped sprites
			public byte m_Orientation;			// See DetailPropOrientation_t
			public byte[] m_Padding2;//[3];		// FIXME: Remove when we rev the detail lump again..
			public byte m_Type;					// See DetailPropType_t
			public byte[] m_Padding3;//[3];		// FIXME: Remove when we rev the detail lump again..
			public float m_flScale;             // For sprites only currently

			//additional
			public DetailPropType_t[] getDetailPropType => FlagsUtils.getFlags<DetailPropType_t>(m_Type);
		}

		/// <summary>Game lump: Detail prop lighting info. Size: 5 bytes</summary>
		public struct DetailPropLightstylesLump_t
		{
			public ColorRGBExp32 m_Lighting;
			public byte m_Style;
		}

		/// <summary>Water leaf. Size: 10 bytes</summary>
		public struct dleafwaterdata_t
		{
			public float surfaceZ, minZ;
			public short surfaceTexInfoID;
			public short unknown;           // is always zero? 
											// mb is a part of surfaceTexInfoID (that means it can be int32)
		}

		/// <summary>Primitive. Size: 9 bytes</summary>
		public struct dprimitive_t
		{
			public byte type;
			public ushort firstIndex;
			public ushort indexCount;
			public ushort firstVert;
			public ushort vertCount;

			//additional
			public dprimitive_type[] getPrimType => FlagsUtils.getFlags<dprimitive_type>(type);
		}

		/// <summary>Cubemap. Size: 16 bytes</summary>
		public struct dcubemapsample_t
		{
			public int[] origin;//[3]          // position of light snapped to the nearest integer
								// the filename for the vtf file is derived from the position
			public byte size;             // 0 - default
										  // otherwise, 1<<(size-1)
		}

		/// <summary>Overlay. Size: 352 bytes</summary>
		public struct doverlay_t
		{
			public int nId;
			public short nTexInfo;

			public ushort m_nFaceCountAndRenderOrder;

			public int[] aFaces;//[OVERLAY_BSP_FACE_COUNT=64];
			public float[] flU;//[2];
			public float[] flV;//[2];
			public SourceVector[] vecUVPoints;//[4];
			public SourceVector vecOrigin;
			public SourceVector vecBasisNormal;

			// Accessors..
			public void SetFaceCount(ushort count)
			{
				m_nFaceCountAndRenderOrder &= OVERLAY_RENDER_ORDER_MASK;
				m_nFaceCountAndRenderOrder |= (ushort)(count & ~OVERLAY_RENDER_ORDER_MASK);
			}
			public ushort GetFaceCount() => (ushort)(m_nFaceCountAndRenderOrder & ~OVERLAY_RENDER_ORDER_MASK);

			public void SetRenderOrder(ushort order)
			{
				//m_nFaceCountAndRenderOrder &= ~OVERLAY_RENDER_ORDER_MASK;
				//m_nFaceCountAndRenderOrder |= (ushort)(order << (16 - OVERLAY_RENDER_ORDER_NUM_BITS));  // leave 2 bits for render order.
				throw new NotImplementedException();
			}
			public ushort GetRenderOrder() => (ushort)(m_nFaceCountAndRenderOrder >> (16 - OVERLAY_RENDER_ORDER_NUM_BITS));
		}

		/// <summary>Displacement triangle. Size: 2 bytes</summary>
		public struct CDispTri
		{
			public ushort m_uiTags;         // Displacement triangle tags.

			//additional
			public DISPTRI_TAGS[] getFlags => FlagsUtils.getFlags<DISPTRI_TAGS>(m_uiTags);
		}

		/// <summary>Water overlay. Size: 1120 bytes</summary>
		public struct dwateroverlay_t
		{
			//i know its similar to doverlay_t, but it has different limitations. from antim.
			public int nId;
			public short nTexInfo;

			public ushort m_nFaceCountAndRenderOrder;
			public int[] aFaces;//[WATEROVERLAY_BSP_FACE_COUNT=256];
			public float[] flU;//[2];
			public float[] flV;//[2];
			public SourceVector[] vecUVPoints;//[4];
			public SourceVector vecOrigin;
			public SourceVector vecBasisNormal;

			// Accessors..
			public void SetFaceCount(ushort count)
			{
				m_nFaceCountAndRenderOrder &= WATEROVERLAY_RENDER_ORDER_MASK;
				m_nFaceCountAndRenderOrder |= (ushort)(count & ~WATEROVERLAY_RENDER_ORDER_MASK);
			}
			public ushort GetFaceCount() => (ushort)(m_nFaceCountAndRenderOrder & ~WATEROVERLAY_RENDER_ORDER_MASK);
			public void SetRenderOrder(ushort order)
			{
				//m_nFaceCountAndRenderOrder &= ~WATEROVERLAY_RENDER_ORDER_MASK;
				//m_nFaceCountAndRenderOrder |= (order << (16 - WATEROVERLAY_RENDER_ORDER_NUM_BITS)); // leave 2 bits for render order.
				throw new NotImplementedException();
			}
			public ushort GetRenderOrder() => (ushort)(m_nFaceCountAndRenderOrder >> (16 - WATEROVERLAY_RENDER_ORDER_NUM_BITS));
		}

		/// <summary>Lightmap page. Size: 33792 bytes</summary>
		public struct dlightmappage_t
		{
			public byte[] data; //[MAX_LIGHTMAPPAGE_WIDTH * MAX_LIGHTMAPPAGE_HEIGHT]
			public ColorRGBExp32[] palette; //[256]
											//originally 'palette' was an byte array and had [256*4] len
		}

		/// <summary>Ambient lighting samples. Size: 4 bytes</summary>
		public struct dleafambientindex_t
		{
			public ushort ambientSampleCount;
			public ushort firstAmbientSample;
		}

		/// <summary>Lightmap page info. Size: 8 bytes</summary>
		public struct dlightmappageinfo_t
		{
			public byte page;					// lightmap page [0..?]
			public byte[] offset;//[2];			// offset into page (s,t)
			public byte pad;					// unused
			public ColorRGBExp32 avgColor;		// average used for runtime lighting calcs
		}

		/// <summary>Ambient lighting sample. Size: 28 bytes</summary>
		public struct dleafambientlighting_t
		{
			// each leaf contains N samples of the ambient lighting
			// each sample contains a cube of ambient light projected on to each axis
			// and a sampling position encoded as a 0.8 fraction (mins=0,maxs=255) of the leaf's bounding box
			public CompressedLightCube cube;
			public byte x;						// fixed point fraction of leaf bounds
			public byte y;						// fixed point fraction of leaf bounds
			public byte z;						// fixed point fraction of leaf bounds
			public byte pad;					// unused
		}

		/// <summary>Overlay fade. Size: 8 bytes</summary>
		public struct doverlayfade_t
		{
			public float flFadeDistMinSq, flFadeDistMaxSq;
		}
		#endregion
	}
}
