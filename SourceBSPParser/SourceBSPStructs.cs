using SourceFormatParser.Common;
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
			public int version;        // default to zero
									   // this field was char fourCC[4] previously, but was unused, favoring the LUMP IDs above instead. It has been
									   // repurposed for compression.  0 implies the lump is not compressed.
			public int uncompressedSize; // default to zero


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

		public struct dvis_t
		{
			public int numclusters;
			public int[][] bitofs;// bitofs[numclusters=8][2]

			public HashSet<byte>[] pvs; //potentially visible set
			public HashSet<byte>[] pas; //potentially audible set
		}
	}
}
