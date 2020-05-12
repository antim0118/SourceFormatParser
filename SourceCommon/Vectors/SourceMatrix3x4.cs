namespace SourceFormatParser.Common
{
	/// <summary>
	/// Matrix 3x4 (float)
	/// Size: 48 bytes (4 bytes per val)
	/// </summary>
	public class SourceMatrix3x4
	{
		public SourceMatrix3x4() { }
		public SourceMatrix3x4(float m00, float m01, float m02, float m03,
			float m10, float m11, float m12, float m13,
			float m20, float m21, float m22, float m23)
		{
			m_flMatVal[0][0] = m00; m_flMatVal[0][1] = m01; m_flMatVal[0][2] = m02; m_flMatVal[0][3] = m03;
			m_flMatVal[1][0] = m10; m_flMatVal[1][1] = m11; m_flMatVal[1][2] = m12; m_flMatVal[1][3] = m13;
			m_flMatVal[2][0] = m20; m_flMatVal[2][1] = m21; m_flMatVal[2][2] = m22; m_flMatVal[2][3] = m23;
		}

		/// Creates a matrix where the X axis = forward the Y axis = left, and the Z axis = up
		void InitXYZ(SourceVector xAxis, SourceVector yAxis, SourceVector zAxis, SourceVector vecOrigin)
		{
			m_flMatVal[0][0] = xAxis.x; m_flMatVal[0][1] = yAxis.x; m_flMatVal[0][2] = zAxis.x; m_flMatVal[0][3] = vecOrigin.x;
			m_flMatVal[1][0] = xAxis.y; m_flMatVal[1][1] = yAxis.y; m_flMatVal[1][2] = zAxis.y; m_flMatVal[1][3] = vecOrigin.y;
			m_flMatVal[2][0] = xAxis.z; m_flMatVal[2][1] = yAxis.z; m_flMatVal[2][2] = zAxis.z; m_flMatVal[2][3] = vecOrigin.z;
		}

		//-----------------------------------------------------------------------------
		// Creates a matrix where the X axis = forward
		// the Y axis = left, and the Z axis = up
		//-----------------------------------------------------------------------------
		void Init(SourceVector xAxis, SourceVector yAxis, SourceVector zAxis, SourceVector vecOrigin)
		{
			m_flMatVal[0][0] = xAxis.x; m_flMatVal[0][1] = yAxis.x; m_flMatVal[0][2] = zAxis.x; m_flMatVal[0][3] = vecOrigin.x;
			m_flMatVal[1][0] = xAxis.y; m_flMatVal[1][1] = yAxis.y; m_flMatVal[1][2] = zAxis.y; m_flMatVal[1][3] = vecOrigin.y;
			m_flMatVal[2][0] = xAxis.z; m_flMatVal[2][1] = yAxis.z; m_flMatVal[2][2] = zAxis.z; m_flMatVal[2][3] = vecOrigin.z;
		}

		//-----------------------------------------------------------------------------
		// Creates a matrix where the X axis = forward
		// the Y axis = left, and the Z axis = up
		//-----------------------------------------------------------------------------
		public SourceMatrix3x4(SourceVector xAxis, SourceVector yAxis, SourceVector zAxis, SourceVector vecOrigin)
		{
			Init(xAxis, yAxis, zAxis, vecOrigin);
		}

		public void InitFromQAngles(SourceQAngle angles, SourceVector vPosition)
		{
			var t = this;
			MathUtils.AngleMatrix(angles, vPosition, ref t);
			this.m_flMatVal = t.m_flMatVal;
		}
		public void InitFromQAngles(SourceQAngle angles) { InitFromQAngles(angles, SourceVector.Empty); }
		public void InitFromRadianEuler(SourceRadianEuler angles, SourceVector vPosition)
		{
			var t = this;
			MathUtils.AngleMatrix(angles, vPosition, ref t);
			this.m_flMatVal = t.m_flMatVal;
		}
		public void InitFromRadianEuler(SourceRadianEuler angles) { InitFromRadianEuler(angles, SourceVector.Empty); }
		public void InitFromCTransform(SourceCTransform transform)
		{
			var t = this;
			MathUtils.TransformMatrix(transform, ref t);
			this.m_flMatVal = t.m_flMatVal;
		}
		public void InitFromQuaternion(SourceQuaternion orientation, SourceVector vPosition)
		{
			var t = this;
			MathUtils.QuaternionMatrix(orientation, vPosition, ref t);
			this.m_flMatVal = t.m_flMatVal;
		}
		public void InitFromQuaternion(SourceQuaternion orientation) { InitFromQuaternion(orientation, SourceVector.Empty); }
		public void InitFromDiagonal(SourceVector vDiagonal)
		{
			SetToIdentity();
			m_flMatVal[0][0] = vDiagonal.x;
			m_flMatVal[1][1] = vDiagonal.y;
			m_flMatVal[2][2] = vDiagonal.z;
		}

		public SourceQuaternion ToQuaternion() => MathUtils.MatrixQuaternion(this);
		public SourceQAngle ToQAngle()
		{
			SourceQAngle tmp = new SourceQAngle();
			MathUtils.MatrixAngles(this, ref tmp);
			return tmp;
		}
		public SourceCTransform ToCTransform() => MathUtils.MatrixTransform(this);

		public void SetToIdentity()
		{
			for (int i1 = 0; i1 < m_flMatVal.Length; i1++)
				for (int i2 = 0; i2 < m_flMatVal[i1].Length; i2++)
					m_flMatVal[i1][i2] = 0.0f;
			m_flMatVal[0][0] = 1.0f;
			m_flMatVal[1][1] = 1.0f;
			m_flMatVal[2][2] = 1.0f;
		}

		/// multiply the scale/rot part of the matrix by a constant. This doesn't init the matrix ,
		/// just scale in place. So if you want to construct a scaling matrix, init to identity and
		/// then call this.
		public void ScaleUpper3x3Matrix(float flScale)
		{
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					m_flMatVal[i][j] *= flScale;
				}
			}
		}

		/// modify the origin
		public void SetOrigin(SourceVector p)
		{
			m_flMatVal[0][3] = p.x;
			m_flMatVal[1][3] = p.y;
			m_flMatVal[2][3] = p.z;
		}

		/// return the origin
		public SourceVector GetOrigin()
		{
			return new SourceVector(m_flMatVal[0][3], m_flMatVal[1][3], m_flMatVal[2][3]);
		}

		void Invalidate()
		{
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					m_flMatVal[i][j] = float.NaN;
				}
			}
		}

		/// check all components for invalid floating point values
		bool IsValid()
		{
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					if (m_flMatVal[i][j] == float.NaN)
						return false;
				}
			}
			return true;
		}

		public static bool operator ==(SourceMatrix3x4 m1, SourceMatrix3x4 m2)
		{
			bool equals = true;
			for (int i1 = 0; i1 < 3; i1++)
			{
				if (!equals) break;
				for (int i2 = 0; i2 < 4; i2++)
				{
					if (m1[i1][i2] != m2[i1][i2])
					{
						equals = false;
						break;
					}
				}
			}
			return equals;
		}
		public static bool operator !=(SourceMatrix3x4 m1, SourceMatrix3x4 m2) => !(m1 == m2);

		public override bool Equals(object obj) => this == (SourceMatrix3x4)obj;
		public override int GetHashCode() => base.GetHashCode();

		//	bool IsEqualTo( const SourceMatrix3x4 &other, float flTolerance = 1e-5f) const;

		//void GetBasisVectorsFLU(Vector* pForward, Vector* pLeft, Vector* pUp) const;
		//SourceVector TransformVector( const Vector &v0 ) const;
		//SourceVector RotateVector( const Vector &v0 ) const;
		//SourceVector TransformVectorByInverse( const Vector &v0 ) const;
		//SourceVector RotateVectorByInverse( const Vector &v0 ) const;
		//SourceVector RotateExtents( const Vector &vBoxExtents ) const; // these are extents and must remain positive/symmetric after rotation
		public void TransformAABB(SourceVector vecMinsIn, SourceVector vecMaxsIn, ref SourceVector vecMinsOut, ref SourceVector vecMaxsOut)
		{
			MathUtils.TransformAABB(this, vecMinsIn, vecMaxsIn, ref vecMinsOut, ref vecMaxsOut);
		}
		public void TransformAABBByInverse(SourceVector vecMinsIn, SourceVector vecMaxsIn, ref SourceVector vecMinsOut, ref SourceVector vecMaxsOut)
		{
			MathUtils.ITransformAABB(this, vecMinsIn, vecMaxsIn, ref vecMinsOut, ref vecMaxsOut);
		}
		public void RotateAABB(SourceVector vecMinsIn, SourceVector vecMaxsIn, ref SourceVector vecMinsOut, ref SourceVector vecMaxsOut)
		{
			MathUtils.RotateAABB(this, vecMinsIn, vecMaxsIn, ref vecMinsOut, ref vecMaxsOut);
		}
		public void RotateAABBByInverse(SourceVector vecMinsIn, SourceVector vecMaxsIn, ref SourceVector vecMinsOut, ref SourceVector vecMaxsOut)
		{
			MathUtils.IRotateAABB(this, vecMinsIn, vecMaxsIn, ref vecMinsOut, ref vecMaxsOut);
		}
		//void TransformPlane( const cplane_t &inPlane, cplane_t &outPlane ) const;
		//void TransformPlaneByInverse( const cplane_t &inPlane, cplane_t &outPlane ) const;
		//float GetOrthogonalityError() const;
		//float GetDeterminant()const;
		//float GetSylvestersCriterion()const; // for symmetrical matrices only: should be >0 iff it's a positive definite matrix

		//SourceVector GetColumn(MatrixAxisType_t nColumn) const;
		//void SetColumn( const Vector &vColumn, MatrixAxisType_t nColumn);
		//SourceVector GetForward() const { return GetColumn(FORWARD_AXIS ); }
		//	SourceVector GetLeft() const { return GetColumn(LEFT_AXIS ); }
		//	SourceVector GetUp() const { return GetColumn(UP_AXIS ); }
		//	SourceVector GetRow(int nRow) const { return * (Vector*) (m_flMatVal[nRow]); }
		//	void SetRow(int nRow, const Vector &vRow ) { m_flMatVal[nRow][0] = vRow.x; m_flMatVal[nRow][1] = vRow.y; m_flMatVal[nRow][2] = vRow.z; }

		public void InverseTR(ref SourceMatrix3x4 _out)
		{
			MathUtils.MatrixInvert(this, ref _out);
		}
		public SourceMatrix3x4 InverseTR()
		{
			SourceMatrix3x4 _out = new SourceMatrix3x4();
			InverseTR(ref _out);
			return _out;
		}

		public float[] this[int i]
		{
			get => m_flMatVal[i];
			set => m_flMatVal[i] = value;
		}

		public float Base() { return m_flMatVal[0][0]; }

		float[][] _m_flMatVal;
		public float[][] m_flMatVal//[3][4];
		{
			get
			{
				if (_m_flMatVal == null)
				{
					_m_flMatVal = new float[3][];
					for (int v = 0; v < 3; v++)
						_m_flMatVal[v] = new float[4];
				}
				return _m_flMatVal;
			}
			set
			{
				_m_flMatVal = value;
			}
		}
	}
}
