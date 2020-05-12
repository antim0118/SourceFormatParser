using System;

namespace SourceFormatParser.Common
{
	/// <summary>
	/// Quaternion x/y/z/w (float)
	/// Size: 16 bytes (4 bytes per axis)
	/// </summary>
	public struct SourceQuaternion
	{
		// same data-layout as engine's vec4_t,
		// which is a vec_t[4]=float[4]

		public float x, y, z, w;

		public SourceQuaternion(float ix, float iy, float iz, float iw)
		{
			//Init(ix, iy, iz, iw);
			x = ix; y = iy; z = iz; w = iw;
		}
		public SourceQuaternion(SourceRadianEuler angle)
		{
			var t = new SourceQuaternion();
			MathUtils.AngleQuaternion(angle, ref t);
			this.x = t.x;
			this.y = t.y;
			this.z = t.z;
			this.w = t.w;
		}
		public SourceQuaternion(SourceDegreeEuler angle)
		{
			SourceRadianEuler radians = new SourceRadianEuler(angle);
			var t = new SourceQuaternion();
			MathUtils.AngleQuaternion(radians, ref t);
			this.x = t.x;
			this.y = t.y;
			this.z = t.z;
			this.w = t.w;
		}

		void Init(float ix = 0.0f, float iy = 0.0f, float iz = 0.0f, float iw = 0.0f) { x = ix; y = iy; z = iz; w = iw; }
		void Init(SourceVector vImaginaryPart, float flRealPart) { x = vImaginaryPart.x; y = vImaginaryPart.y; z = vImaginaryPart.z; w = flRealPart; }


		public static bool operator ==(SourceQuaternion v1, SourceQuaternion v2) => v1.x == v2.x && v1.y == v2.y && v1.z == v2.z && v1.w == v2.w;
		public static bool operator !=(SourceQuaternion v1, SourceQuaternion v2) => !(v1 == v2);

		public override bool Equals(object obj) => this == (SourceQuaternion)obj;
		public override int GetHashCode() => base.GetHashCode();

		public SourceQuaternion Conjugate() { return new SourceQuaternion(-x, -y, -z, w); }

		public SourceVector GetForward()
		{
			SourceVector vAxisX;
			vAxisX.x = 1.0f - 2.0f * y * y - 2.0f * z * z;
			vAxisX.y = 2.0f * x * y + 2.0f * w * z;
			vAxisX.z = 2.0f * x * z - 2.0f * w * y;
			return vAxisX;
		}
		public SourceVector GetLeft()
		{
			SourceVector vAxisY;
			vAxisY.x = 2.0f * x * y - 2.0f * w * z;
			vAxisY.y = 1.0f - 2.0f * x * x - 2.0f * z * z;
			vAxisY.z = 2.0f * y * z + 2.0f * w * x;
			return vAxisY;
		}
		public SourceVector GetUp()
		{
			SourceVector vAxisZ;
			vAxisZ.x = 2.0f * x * z + 2.0f * w * y;
			vAxisZ.y = 2.0f * y * z - 2.0f * w * x;
			vAxisZ.z = 1.0f - 2.0f * x * x - 2.0f * y * y;
			return vAxisZ;
		}


		public float RealPart() { return w; }
		public SourceQAngle ToQAngle()
		{
			SourceQAngle anglesOut = new SourceQAngle();
			MathUtils.QuaternionAngles(this, ref anglesOut);
			return anglesOut;
		}
		public SourceMatrix3x4 ToMatrix()
		{
			SourceMatrix3x4 mat = new SourceMatrix3x4();
			mat.InitFromQuaternion(this);
			return mat;
		}

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
					case 3: return w;
					default: throw new Exception("wrong index [0..3]");
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
					case 3:
						w = value;
						break;
					default:
						throw new Exception("wrong index [0..3]");
				}
			}
		}

		public static SourceQuaternion operator +(SourceQuaternion q1, SourceQuaternion q2) => new SourceQuaternion(q1.x + q2.x, q1.y + q2.y, q1.z + q2.z, q1.w + q2.w);
		public static SourceQuaternion operator -(SourceQuaternion q1, SourceQuaternion q2) => new SourceQuaternion(q1.x - q2.x, q1.y - q2.y, q1.z - q2.z, q1.w - q2.w);

		public static SourceQuaternion Identity = new SourceQuaternion();
	}
}
