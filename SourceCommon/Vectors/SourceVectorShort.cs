namespace SourceFormatParser.Common
{
    /// <summary>
    /// Vector x/y/z (short)
    /// Size: 6 bytes (2 bytes per axis)
    /// </summary>
    public struct SourceVectorShort
    {
        public short x, y, z;

        public SourceVectorShort(short x, short y, short z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override string ToString() => $"{{{x}; {y}; {z}}}";

#if (UNITY)
        public UnityEngine.Vector3 toVector3() => new UnityEngine.Vector3(x, y, z);
#else
        public System.Numerics.Vector3 toVector3() => new System.Numerics.Vector3(x, y, z);
#endif
    }
}
