using System;
using System.IO;
using System.Text;

namespace SourceFormatParser.BigEndian
{
    public class BigBinaryReader : BinaryReader
    {
        public BigBinaryReader(Stream stream, Encoding encoding) : base(stream, encoding) { }

        public override byte[] ReadBytes(int count)
        {
            var data = base.ReadBytes(count);
            Array.Reverse(data);
            return data;
        }

        public override short ReadInt16() => BitConverter.ToInt16(ReadBytes(2), 0);
        public override ushort ReadUInt16() => BitConverter.ToUInt16(ReadBytes(2), 0);

        public override int ReadInt32() => BitConverter.ToInt32(ReadBytes(4), 0);
        public override uint ReadUInt32() => BitConverter.ToUInt32(ReadBytes(4), 0);
        
        public override long ReadInt64() => BitConverter.ToInt64(ReadBytes(8), 0);
        public override ulong ReadUInt64() => BitConverter.ToUInt64(ReadBytes(8), 0);

        public override float ReadSingle() => BitConverter.ToSingle(ReadBytes(4), 0);
    }
}
