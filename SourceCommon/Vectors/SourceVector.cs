using System;

namespace SourceFormatParser.Common
{
	/// <summary>
	/// Vector x/y/z (float)
	/// Size: 12 bytes (4 bytes per axis)
	/// </summary>
	public struct SourceVector
	{
		public float x, y, z;

		public SourceVector(float ix = 0.0f, float iy = 0.0f, float iz = 0.0f)
		{
			this.x = ix;
			this.y = iy;
			this.z = iz;
		}

		// Initialization methods
		public static SourceVector Random(float minVal, float maxVal)
		{
			Random rand = new Random();
			return new SourceVector(
				ix: minVal + ((float)rand.NextDouble()) * (float)(maxVal - minVal),
				iy: minVal + ((float)rand.NextDouble()) * (float)(maxVal - minVal),
				iz: minVal + ((float)rand.NextDouble()) * (float)(maxVal - minVal)
			);
		}
		public void Zero() => x = y = z = 0.0f; ///< zero out a vector

		// equality
		public static bool operator ==(SourceVector v1, SourceVector v2) => v1.x == v2.x && v1.y == v2.y && v1.z == v2.z;
		public static bool operator !=(SourceVector v1, SourceVector v2) => !(v1 == v2);

		public override bool Equals(object obj) => this == (SourceVector)obj;
		public override int GetHashCode() => base.GetHashCode();

		// arithmetic operations
		public static SourceVector operator +(SourceVector v1, SourceVector v2) => new SourceVector(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
		public static SourceVector operator -(SourceVector v1, SourceVector v2) => new SourceVector(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
		public static SourceVector operator *(SourceVector v1, SourceVector v2) => new SourceVector(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
		public static SourceVector operator *(SourceVector v, float s) => new SourceVector(v.x * s, v.y * s, v.z * s);
		public static SourceVector operator /(SourceVector v1, SourceVector v2)
		{
			return new SourceVector(
				ix: v2.x != 0.0f ? v1.x / v2.x : 0f,
				iy: v2.y != 0.0f ? v1.y / v2.y : 0f,
				iz: v2.z != 0.0f ? v1.z / v2.z : 0f
				);
		}
		public static SourceVector operator /(SourceVector v, float fl)
		{
			if (fl == 0.0f)
				return new SourceVector();
			float oofl = 1.0f / fl;
			return new SourceVector(v.x * oofl, v.y * oofl, v.z * oofl);
		}
		public static SourceVector operator +(SourceVector v, float s) => new SourceVector(v.x + s, v.y + s, v.z + s); //< broadcast add
		public static SourceVector operator -(SourceVector v, float s) => new SourceVector(v.x - s, v.y - s, v.z - s); //< broadcast sub			

		// negate the vector components
		public void Negate()
		{
			x = -x;
			y = -y;
			z = -z;
		}

		// Get the vector's magnitude.
		public float Length() => (float)Math.Sqrt(x * x + y * y + z * z);

		// Get the vector's magnitude squared.
		public float LengthSqr() => (x * x + y * y + z * z);

		// return true if this vector is (0,0,0) within tolerance
		public bool IsZero(float tolerance = 0.01f)
		{
			return x > -tolerance && x < tolerance &&
					y > -tolerance && y < tolerance &&
					z > -tolerance && z < tolerance;
		}

		public bool IsLengthGreaterThan(float val) => LengthSqr() > val * val;
		public bool IsLengthLessThan(float val) => LengthSqr() < val * val;

		// check if a vector is within the box defined by two other vectors // check a point against a box
		public bool WithinAABox(SourceVector boxmin, SourceVector boxmax)
		{
			return (x >= boxmin.x) && (x <= boxmax.x) &&
					(y >= boxmin.y) && (y <= boxmax.y) &&
					(z >= boxmin.z) && (z <= boxmax.z);
		}

		// Get the distance from this vector to the other one.
		public float DistTo(SourceVector vOther)
		{
			return new SourceVector(
				ix: x - vOther.x,
				iy: y - vOther.y,
				iz: z - vOther.z
				).Length();
		}

		// Get the distance from this vector to the other one squared.
		// NJS: note, VC wasn't inlining it correctly in several deeply nested inlines due to being an 'out of line' inline.  
		// may be able to tidy this up after switching to VC7
		public float DistToSqr(SourceVector vOther)
		{
			return new SourceVector(
				ix: x - vOther.x,
				iy: y - vOther.y,
				iz: z - vOther.z
				).LengthSqr();
		}

		// Multiply, add, and assign to this (ie: *this = a + b * scalar). This
		// is about 12% faster than the actual vector equation (because it's done per-component
		// rather than per-vector).
		public void MulAdd(SourceVector a, SourceVector b, float scalar)
		{
			x = a.x + b.x * scalar;
			y = a.y + b.y * scalar;
			z = a.z + b.z * scalar;
		}

		// Dot product.
		public float Dot(SourceVector vOther) => (x * vOther.x + y * vOther.y + z * vOther.z);


		// 2d
		public float Length2D() => (float)Math.Sqrt(Length2DSqr());
		public float Length2DSqr() => (x * x + y * y);


		// Cross product between two vectors.
		public SourceVector Cross(SourceVector vOther)
		{
			return new SourceVector(
				ix: y * vOther.z - z * vOther.y,
				iy: z * vOther.x - x * vOther.z,
				iz: x * vOther.y - y * vOther.x
				);
		}

		// Returns a vector with the min or max in X, Y, and Z.
		public SourceVector Min(SourceVector vOther)
		{
			return new SourceVector(x < vOther.x ? x : vOther.x,
									y < vOther.y ? y : vOther.y,
									z < vOther.z ? z : vOther.z);
		}
		public SourceVector Max(SourceVector vOther)
		{
			return new SourceVector(x > vOther.x ? x : vOther.x,
									y > vOther.y ? y : vOther.y,
									z > vOther.z ? z : vOther.z);
		}

		public override string ToString() => $"{{{x}; {y}; {z}}}";

#if (UNITY)
        public UnityEngine.Vector3 toVector3() => new UnityEngine.Vector3(x, y, z);
#else
		public System.Numerics.Vector3 toVector3() => new System.Numerics.Vector3(x, y, z);
#endif
	}
}
