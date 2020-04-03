namespace SourceFormatParser.Common
{
    /// <summary>
    /// Vector x/y (short)
    /// Size: 4 bytes (2 bytes per axis)
    /// </summary>
    public struct SourceVector2Short
    {
        public short x, y;

        public SourceVector2Short(short x, short y)
        {
            this.x = x;
            this.y = y;
        }

        public override string ToString() => $"{{{x}; {y}}}";

#if (UNITY)
        public UnityEngine.Vector2 toVector2() => new UnityEngine.Vector2(x, y);
#else
        public System.Numerics.Vector2 toVector2() => new System.Numerics.Vector2(x, y);
#endif
    }
}
