using System.IO;
using System.Text;

namespace SourceFormatParser.Common
{
    public static class ParsingUtils
    {
        public static SourceVector ReadVector(this BinaryReader BR) => new SourceVector(BR.ReadSingle(), BR.ReadSingle(), BR.ReadSingle());
        public static SourceVectorShort ReadVectorShort(this BinaryReader BR) => new SourceVectorShort(BR.ReadInt16(), BR.ReadInt16(), BR.ReadInt16());
        public static SourceVector2 ReadVector2(this BinaryReader BR) => new SourceVector2(BR.ReadSingle(), BR.ReadSingle());
        public static SourceVector2Short ReadVector2Short(this BinaryReader BR) => new SourceVector2Short(BR.ReadInt16(), BR.ReadInt16());
        public static SourceVector2Int ReadVector2Int(this BinaryReader BR) => new SourceVector2Int(BR.ReadInt32(), BR.ReadInt32());
        public static SourceVector4 ReadVector4(this BinaryReader BR) => new SourceVector4(BR.ReadSingle(), BR.ReadSingle(), BR.ReadSingle(), BR.ReadSingle());

        public static SourceQAngle ReadQAngle(this BinaryReader BR) => new SourceQAngle(BR.ReadSingle(), BR.ReadSingle(), BR.ReadSingle());
        public static SourceQuaternion ReadQuaternion(this BinaryReader BR) => new SourceQuaternion(BR.ReadSingle(), BR.ReadSingle(), BR.ReadSingle(), BR.ReadSingle());
        public static SourceRadianEuler ReadRadianEuler(this BinaryReader BR) => new SourceRadianEuler(BR.ReadSingle(), BR.ReadSingle(), BR.ReadSingle());

        public static SourceMatrix3x4 ReadMatrix3x4(this BinaryReader BR) => new SourceMatrix3x4(BR.ReadVector(), BR.ReadVector(), BR.ReadVector(), BR.ReadVector());

        public static SourceColor32 ReadColor32(this BinaryReader BR) => new SourceColor32(BR.ReadByte(), BR.ReadByte(), BR.ReadByte(), BR.ReadByte());
        public static ColorRGBExp32 ReadColorRGBExp32(this BinaryReader BR) => new ColorRGBExp32(BR.ReadByte(), BR.ReadByte(), BR.ReadByte(), BR.ReadByte());

        public static CompressedLightCube ReadCompressedLightCube(this BinaryReader BR) => new CompressedLightCube { 
            m_Color = new ColorRGBExp32[6] { 
                ReadColorRGBExp32(BR), ReadColorRGBExp32(BR), ReadColorRGBExp32(BR), ReadColorRGBExp32(BR), ReadColorRGBExp32(BR), ReadColorRGBExp32(BR) 
            } 
        };

        #region Read Array
        public static ushort[] ReadUInt16Array(this BinaryReader BR, int count)
        {
            ushort[] ret = new ushort[count];
            for (int c = 0; c < count; c++) ret[c] = BR.ReadUInt16();
            return ret;
        }
        public static short[] ReadInt16Array(this BinaryReader BR, int count)
        {
            short[] ret = new short[count];
            for (int c = 0; c < count; c++) ret[c] = BR.ReadInt16();
            return ret;
        }
        public static uint[] ReadUInt32Array(this BinaryReader BR, int count)
        {
            uint[] ret = new uint[count];
            for (int c = 0; c < count; c++) ret[c] = BR.ReadUInt32();
            return ret;
        }
        public static int[] ReadInt32Array(this BinaryReader BR, int count)
        {
            int[] ret = new int[count];
            for (int c = 0; c < count; c++) ret[c] = BR.ReadInt32();
            return ret;
        }

        public static float[] ReadSingleArray(this BinaryReader BR, int count)
        {
            float[] ret = new float[count];
            for (int c = 0; c < count; c++) ret[c] = BR.ReadSingle();
            return ret;
        }
        #endregion

        public static string ReadString(this BinaryReader BR, int count) => Encoding.ASCII.GetString(BR.ReadBytes(64)).Replace("\0", "");
    }
}
