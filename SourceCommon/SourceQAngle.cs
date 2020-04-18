using System;

namespace SourceFormatParser.Common
{
	/// <summary>
	/// Vector x/y/z (float)
	/// Size: 12 bytes (4 bytes per axis)
	/// </summary>
	public struct SourceQAngle
	{
		// Members
		public float x, y, z;

		// Initialization
		public SourceQAngle(float ix = 0.0f, float iy = 0.0f, float iz = 0.0f)
		{
			this.x = ix;
			this.y = iy;
			this.z = iz;
		}
		public static SourceQAngle Random(float minVal, float maxVal)
		{
			Random rand = new Random();
			return new SourceQAngle(
				ix: minVal + ((float)rand.NextDouble()) * (float)(maxVal - minVal),
				iy: minVal + ((float)rand.NextDouble()) * (float)(maxVal - minVal),
				iz: minVal + ((float)rand.NextDouble()) * (float)(maxVal - minVal)
			);
		}

		// equality
		public static bool operator ==(SourceQAngle v1, SourceQAngle v2) => v1.x == v2.x && v1.y == v2.y && v1.z == v2.z;
		public static bool operator !=(SourceQAngle v1, SourceQAngle v2) => !(v1 == v2);

		public override bool Equals(object obj) => this == (SourceQAngle)obj;
		public override int GetHashCode() => base.GetHashCode();

		// arithmetic operations
		public static SourceQAngle operator +(SourceQAngle v1, SourceQAngle v2) => new SourceQAngle(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
		public static SourceQAngle operator -(SourceQAngle v1, SourceQAngle v2) => new SourceQAngle(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
		public static SourceQAngle operator *(SourceQAngle v, float s) => new SourceQAngle(v.x * s, v.y * s, v.z * s);
		public static SourceQAngle operator /(SourceQAngle v, float fl)
		{
			if (fl == 0.0f)
				return new SourceQAngle();
			float oofl = 1.0f / fl;
			return new SourceQAngle(v.x * oofl, v.y * oofl, v.z * oofl);
		}

		// Get the vector's magnitude.
		public float Length() => (float)Math.Sqrt(LengthSqr());
		public float LengthSqr() => x * x + y * y + z * z;

		public override string ToString() => $"{{{x}; {y}; {z}}}";
	}
}
