using System;

namespace SourceFormatParser.Common
{
	/// <summary>
	/// compressed color format. 
	/// Size: 4 bytes
	/// </summary>
	public struct ColorRGBExp32
	{
		public byte r, g, b, exponent;

		public ColorRGBExp32(byte r, byte g, byte b, byte exponent)
		{
			this.r = r;
			this.g = g;
			this.b = b;
			this.exponent = exponent;
		}

		/// <summary>
		/// Not implemented. mathlib/color_conversion.cpp - "VectorToColorRGBExp32" method if you want to.
		/// </summary>
		public static ColorRGBExp32 FromVector(SourceVector vin) => throw new NotImplementedException();
		public SourceVector ToVector()
		{
			// FIXME: Why is there a factor of 255 built into this?
			return new SourceVector(
				255.0f * ColorConversion.TexLightToLinear(r, exponent),
				255.0f * ColorConversion.TexLightToLinear(g, exponent),
				255.0f * ColorConversion.TexLightToLinear(b, exponent)
				);
		}

		public override string ToString() => $"{{{r}; {g}; {b}; exp={exponent}}}";
	}
}
