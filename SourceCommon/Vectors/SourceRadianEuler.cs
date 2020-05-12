using System;

namespace SourceFormatParser.Common
{
	/// <summary>
	/// RadianEuler x/y/z (float)
	/// Size: 12 bytes (4 bytes per axis)
	/// </summary>
	public class SourceRadianEuler
	{
		public SourceRadianEuler() { }
		public SourceRadianEuler(float X, float Y, float Z) { x = X; y = Y; z = Z; }
		public SourceRadianEuler(SourceQuaternion q)
		{
			var t = this;
			MathUtils.QuaternionAngles(q, ref t);
			this.x = t.x;
			this.y = t.y;
			this.z = t.z;
		}
		public SourceRadianEuler(SourceQAngle angles)
		{
			Init(angles.z * 3.14159265358979323846f / 180.0f,
				angles.x * 3.14159265358979323846f / 180.0f,
				angles.y * 3.14159265358979323846f / 180.0f);
		}
		public SourceRadianEuler(SourceDegreeEuler angles)
		{
			Init(MathUtils.DEG2RAD(angles.x), MathUtils.DEG2RAD(angles.y), MathUtils.DEG2RAD(angles.z));
		}

		// Initialization
		public void Init(float ix = 0.0f, float iy = 0.0f, float iz = 0.0f) { x = ix; y = iy; z = iz; }

		//	conversion to qangle
		public SourceQAngle ToQAngle() => new SourceQAngle(MathUtils.DEG2RAD(y), MathUtils.DEG2RAD(z), MathUtils.DEG2RAD(x));
		public bool IsValid() { throw new NotImplementedException(); }
		public void Invalidate() { throw new NotImplementedException(); }

		public float Base() { return x; }

		// array access...
		public float this[int i]
		{
			get
			{
				switch (i)
				{
					case 0: return x;
					case 1: return y;
					case 2: return z;
					default: throw new Exception("wrong index [0..2]");
				}
			}
			set
			{
				switch (i)
				{
					case 0:
						x = value;
						break;
					case 1:
						y = value;
						break;
					case 2:
						z = value;
						break;
					default:
						throw new Exception("wrong index [0..2]");
				}
			}
		}

		public float x, y, z;
	}
}
