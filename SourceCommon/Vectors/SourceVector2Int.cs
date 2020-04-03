namespace SourceFormatParser.Common
{
    /// <summary>
    /// Vector x/y (int)
    /// Size: 8 bytes (4 bytes per axis)
    /// </summary>
    public struct SourceVector2Int
    {
        public int x, y;

        public SourceVector2Int(int x, int y)
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
