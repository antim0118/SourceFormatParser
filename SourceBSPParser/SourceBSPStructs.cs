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

		public struct dheader_t
		{
			public int ident;
			public int version;
			public lump_t[] lumps;//[HEADER_LUMPS];
			public int mapRevision;                // the map's revision (iteration, version) number (added BSPVERSION 6)
		}

		public struct lump_t
		{
			public int fileofs, filelen;
			public int version;        // default to zero
									   // this field was char fourCC[4] previously, but was unused, favoring the LUMP IDs above instead. It has been
									   // repurposed for compression.  0 implies the lump is not compressed.
			public int uncompressedSize; // default to zero
		};
	}
}
