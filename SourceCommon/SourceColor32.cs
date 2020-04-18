namespace SourceFormatParser.Common
{
	/// <summary>
	/// compressed color rgba format. 
	/// Size: 4 bytes
	/// </summary>
	public struct SourceColor32
	{
		public byte r, g, b, a;

		public SourceColor32(byte r, byte g, byte b, byte a)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			this.a = a;
		}
		public SourceColor32(float r, float g, float b, float a)
		{
			this.r = (byte)(r * 255);
			this.g = (byte)(g * 255);
			this.b = (byte)(b * 255);
			this.a = (byte)(a * 255);
		}

		public SourceVector toSVector() => new SourceVector(r * (1.0f / 255.0f), g * (1.0f / 255.0f), b * (1.0f / 255.0f));

		public override string ToString() => $"{{{r}; {g}; {b}; {a}}}";
	}
}
