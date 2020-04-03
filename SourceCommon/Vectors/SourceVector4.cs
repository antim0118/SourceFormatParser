namespace SourceFormatParser.Common
{
    /// <summary>
    /// Vector x/y/z/w (float)
    /// Size: 16 bytes (4 bytes per axis)
    /// </summary>
    public struct SourceVector4
    {
        public float x, y, z, w;

        public SourceVector4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public override string ToString() => $"{{{x}; {y}; {z}; {w}}}";

#if (UNITY)
        public UnityEngine.Vector4 toVector4() => new UnityEngine.Vector4(x, y, z, w);
#else
        public System.Numerics.Vector4 toVector4() => new System.Numerics.Vector4(x, y, z, w);
#endif
    }
}
