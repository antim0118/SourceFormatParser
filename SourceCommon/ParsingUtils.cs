using System.IO;

namespace SourceFormatParser.Common
{
    public static class ParsingUtils
    {
        public static SourceVector ReadVector(this BinaryReader BR) => new SourceVector(BR.ReadSingle(), BR.ReadSingle(), BR.ReadSingle());

        public static int[] ReadInt32Array(this BinaryReader BR, int count)
        {
            int[] ret = new int[count];
            for (int c = 0; c < count; c++) ret[c] = BR.ReadInt32();
            return ret;
        }
    }
}
