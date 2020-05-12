using System;

namespace SourceFormatParser.Common
{
	/// <summary>Degree Euler angle aligned to axis (NOT ROLL/PITCH/YAW)</summary>
	public class SourceDegreeEuler
	{
		public SourceDegreeEuler() { }
		public SourceDegreeEuler(float X, float Y, float Z) { x = X; y = Y; z = Z; }
		public SourceDegreeEuler(SourceQuaternion q)
		{
			SourceRadianEuler radians = new SourceRadianEuler(q);
			Init(MathUtils.RAD2DEG(radians.x), MathUtils.RAD2DEG(radians.y), MathUtils.RAD2DEG(radians.z));
		}
		public SourceDegreeEuler(SourceQAngle angles)
		{
			Init(angles.z, angles.x, angles.y);
		}
		public SourceDegreeEuler(SourceRadianEuler angles)
		{
			Init(MathUtils.RAD2DEG(angles.x), MathUtils.RAD2DEG(angles.y), MathUtils.RAD2DEG(angles.z));
		}

		// Initialization
		public void Init(float ix = 0.0f, float iy = 0.0f, float iz = 0.0f) { x = ix; y = iy; z = iz; }

		public SourceQAngle ToQAngle() => new SourceQAngle(y, z, x);

		//	conversion to qangle
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
