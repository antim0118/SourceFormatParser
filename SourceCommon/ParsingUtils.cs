using System.IO;

namespace SourceFormatParser.Common
{
    public static class ParsingUtils
    {
        public static SourceVector ReadVector(this BinaryReader BR) => new SourceVector(BR.ReadSingle(), BR.ReadSingle(), BR.ReadSingle());
        public static SourceVectorShort ReadVectorShort(this BinaryReader BR) => new SourceVectorShort(BR.ReadInt16(), BR.ReadInt16(), BR.ReadInt16());
        public static SourceVector2Short ReadVector2Short(this BinaryReader BR) => new SourceVector2Short(BR.ReadInt16(), BR.ReadInt16());
        public static SourceVector2Int ReadVector2Int(this BinaryReader BR) => new SourceVector2Int(BR.ReadInt32(), BR.ReadInt32());
        public static SourceVector4 ReadVector4(this BinaryReader BR) => new SourceVector4(BR.ReadSingle(), BR.ReadSingle(), BR.ReadSingle(), BR.ReadSingle());

        public static ColorRGBExp32 ReadColorRGBExp32(this BinaryReader BR) => new ColorRGBExp32(BR.ReadByte(), BR.ReadByte(), BR.ReadByte(), BR.ReadByte());

        public static CompressedLightCube ReadCompressedLightCube(this BinaryReader BR) => new CompressedLightCube { 
            m_Color = new ColorRGBExp32[6] { 
                ReadColorRGBExp32(BR), ReadColorRGBExp32(BR), ReadColorRGBExp32(BR), ReadColorRGBExp32(BR), ReadColorRGBExp32(BR), ReadColorRGBExp32(BR) 
            } 
        };

        public static int[] ReadInt32Array(this BinaryReader BR, int count)
        {
            int[] ret = new int[count];
            for (int c = 0; c < count; c++) ret[c] = BR.ReadInt32();
            return ret;
        }
    }
}
