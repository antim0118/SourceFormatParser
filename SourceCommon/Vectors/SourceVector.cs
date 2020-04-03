namespace SourceFormatParser.Common
{
    /// <summary>
    /// Vector x/y/z (float)
    /// Size: 12 bytes (4 bytes per axis)
    /// </summary>
    public struct SourceVector
    {
        public float x, y, z;

        public SourceVector(float x, float y, float z)
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
