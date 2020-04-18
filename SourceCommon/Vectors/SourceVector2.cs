using System;

namespace SourceFormatParser.Common
{
    /// <summary>
    /// Vector x/y (float)
    /// Size: 8 bytes (4 bytes per axis). 
	/// Is Vector2D in Source Engine.
    /// </summary>
    public struct SourceVector2
    {
        public float x, y;

        public SourceVector2(float ix = 0.0f, float iy = 0.0f)
        {
            this.x = ix;
            this.y = iy;
        }

		// Initialization methods
		public static SourceVector2 Random(float minVal, float maxVal)
		{
			Random rand = new Random();
			return new SourceVector2(
				ix: minVal + ((float)rand.NextDouble()) * (float)(maxVal - minVal),
				iy: minVal + ((float)rand.NextDouble()) * (float)(maxVal - minVal)
			);
		}

		// equality
		public static bool operator ==(SourceVector2 v1, SourceVector2 v2) => v1.x == v2.x && v1.y == v2.y;
		public static bool operator !=(SourceVector2 v1, SourceVector2 v2) => !(v1 == v2);

		public override bool Equals(object obj) => this == (SourceVector2)obj;
		public override int GetHashCode() => base.GetHashCode();

		// arithmetic operations
		public static SourceVector2 operator +(SourceVector2 v1, SourceVector2 v2) => new SourceVector2(v1.x + v2.x, v1.y + v2.y);
		public static SourceVector2 operator -(SourceVector2 v1, SourceVector2 v2) => new SourceVector2(v1.x - v2.x, v1.y - v2.y);
		public static SourceVector2 operator *(SourceVector2 v1, SourceVector2 v2) => new SourceVector2(v1.x * v2.x, v1.y * v2.y);
		public static SourceVector2 operator *(SourceVector2 v, float s) => new SourceVector2(v.x * s, v.y * s);
		public static SourceVector2 operator /(SourceVector2 v1, SourceVector2 v2)
		{
			return new SourceVector2(
				ix: v2.x != 0.0f ? v1.x / v2.x : 0f,
				iy: v2.y != 0.0f ? v1.y / v2.y : 0f
				);
		}
		public static SourceVector2 operator /(SourceVector2 v, float fl)
		{
			if (fl == 0.0f)
				return new SourceVector2();
			float oofl = 1.0f / fl;
			return new SourceVector2(v.x * oofl, v.y * oofl);
		}

		// negate the Vector2D components
		public void Negate()
		{
			x = -x;
			y = -y;
		}

		// Get the Vector2D's magnitude.
		public float Length() => (float)Math.Sqrt(LengthSqr());

		// Get the Vector2D's magnitude squared.
		public float LengthSqr() => (x * x + y * y);

		// return true if this vector is (0,0) within tolerance
		public bool IsZero(float tolerance = 0.01f)
		{
			return x > -tolerance && x < tolerance &&
					y > -tolerance && y < tolerance;
		}

		// Compare length.
		public bool IsLengthGreaterThan(float val) => LengthSqr() > val * val;
		public bool IsLengthLessThan(float val) => LengthSqr() < val * val;

		// Get the distance from this Vector2D to the other one.
		public float DistTo(SourceVector2 vOther)
		{
			return new SourceVector2(
				ix: x - vOther.x,
				iy: y - vOther.y
				).Length();
		}

		// Get the distance from this Vector2D to the other one squared.
		public float DistToSqr(SourceVector2 vOther)
		{
			return new SourceVector2(
				ix: x - vOther.x,
				iy: y - vOther.y
				).LengthSqr();
		}

		// Multiply, add, and assign to this (ie: *this = a + b * scalar). This
		// is about 12% faster than the actual Vector2D equation (because it's done per-component
		// rather than per-Vector2D).
		public void MulAdd(SourceVector2 a, SourceVector2 b, float scalar)
		{
			x = a.x + b.x * scalar;
			y = a.y + b.y * scalar;
		}

		// Dot product.
		public float Dot(SourceVector2 vOther) => (x * vOther.x + y * vOther.y);

		// Returns a SourceVector2 with the min or max in X, Y, and Z.
		public SourceVector2 Min(SourceVector2 vOther)
		{
			return new SourceVector2(x < vOther.x ? x : vOther.x,
									y < vOther.y ? y : vOther.y);
		}
		public SourceVector2 Max(SourceVector2 vOther)
		{
			return new SourceVector2(x > vOther.x ? x : vOther.x,
									y > vOther.y ? y : vOther.y);
		}

	public override string ToString() => $"{{{x}; {y}}}";

#if (UNITY)
        public UnityEngine.Vector2 toVector2() => new UnityEngine.Vector2(x, y);
#else
        public System.Numerics.Vector2 toVector2() => new System.Numerics.Vector2(x, y);
#endif
    }
}
