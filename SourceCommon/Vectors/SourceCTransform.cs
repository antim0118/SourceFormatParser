using System;

namespace SourceFormatParser.Common
{
	public class SourceCTransform
	{
		public SourceCTransform() { }
		public SourceCTransform(SourceVector v, SourceQuaternion q)
		{
			m_vPosition = v;
			m_orientation = q;
		}
		public SourceCTransform(SourceVector v, SourceQAngle a)
		{
			m_vPosition = v;
			MathUtils.AngleQuaternion(a, ref m_orientation);
		}

		public SourceVector m_vPosition;
		public SourceQuaternion m_orientation;

		public bool IsValid()
		{
			//return m_vPosition.IsValid() && m_orientation.IsValid();
			throw new NotImplementedException();
		}

		public static bool operator ==(SourceCTransform c1, SourceCTransform c2) => c1.m_vPosition == c2.m_vPosition && c1.m_orientation == c2.m_orientation; //< exact equality check
		public static bool operator !=(SourceCTransform c1, SourceCTransform c2) => !(c1 == c2);

		public override bool Equals(object obj) => this == (SourceCTransform)obj;
		public override int GetHashCode() => base.GetHashCode();

		// for API compatibility with matrix3x4_t
		public void InitFromQAngles(SourceQAngle angles, SourceVector vPosition)
		{
			MathUtils.AngleQuaternion(angles, ref m_orientation);
			m_vPosition = vPosition;
		}
		public void InitFromMatrix(SourceMatrix3x4 transform)
		{
			m_orientation = MathUtils.MatrixQuaternion(transform);
			m_vPosition = transform.GetOrigin();
		}
		public void InitFromQuaternion(SourceQuaternion orientation, SourceVector vPosition)
		{
			m_orientation = orientation;
			m_vPosition = vPosition;
		}

		public SourceQuaternion ToQuaternion() => m_orientation;
		public SourceQAngle ToQAngle()
		{
			SourceQAngle angles = new SourceQAngle();
			MathUtils.QuaternionAngles(m_orientation, ref angles);
			return angles;
		}
		public SourceMatrix3x4 ToMatrix() => MathUtils.TransformMatrix(this);

		public void SetToIdentity()
		{
			m_vPosition = SourceVector.Empty;
			m_orientation = SourceQuaternion.Identity;
		}

		public void SetOrigin(SourceVector vPos) { m_vPosition = vPos; }
		public void SetAngles(SourceQAngle vAngles) { MathUtils.AngleQuaternion(vAngles, ref m_orientation); }
		public SourceVector GetOrigin() { return m_vPosition; }

		public void GetBasisVectorsFLU(ref SourceVector pForward, ref SourceVector pLeft, ref SourceVector pUp)
		{
			MathUtils.TransformVectorsFLU(this, ref pForward, ref pLeft, ref pUp);
		}
		public SourceVector GetForward()
		{
			SourceVector vForward = new SourceVector();
			MathUtils.TransformVectorsForward(this, ref vForward);
			return vForward;
		}
		public SourceVector TransformVector(SourceVector v0) => MathUtils.TransformPoint(this, v0);
		public SourceVector RotateVector(SourceVector v0)
		{
			SourceVector vOut = new SourceVector();
			MathUtils.RotatePoint(this, v0, ref vOut);
			return vOut;
		}
		public SourceVector TransformVectorByInverse(SourceVector v0)
		{
			SourceVector vOut = new SourceVector();
			MathUtils.VectorITransform(v0, this, ref vOut);
			return vOut;
		}
		public SourceVector RotateVectorByInverse(SourceVector v0)
		{
			SourceVector vOut = new SourceVector();
			MathUtils.VectorIRotate(v0, this, ref vOut);
			return vOut;
		}
		public SourceVector RotateExtents(SourceVector vBoxExtents) { throw new NotImplementedException(); } // these are extents and must remain positive/symmetric after rotation
		public void TransformAABB(SourceVector vecMinsIn, SourceVector vecMaxsIn, ref SourceVector vecMinsOut, ref SourceVector vecMaxsOut)
		{
			ToMatrix().TransformAABB(vecMinsIn, vecMaxsIn, ref vecMinsOut, ref vecMaxsOut);
		}
		public void TransformAABBByInverse(SourceVector vecMinsIn, SourceVector vecMaxsIn, ref SourceVector vecMinsOut, ref SourceVector vecMaxsOut)
		{
			ToMatrix().TransformAABBByInverse(vecMinsIn, vecMaxsIn, ref vecMinsOut, ref vecMaxsOut);
		}
		public void RotateAABB(SourceVector vecMinsIn, SourceVector vecMaxsIn, ref SourceVector vecMinsOut, ref SourceVector vecMaxsOut)
		{
			ToMatrix().RotateAABB(vecMinsIn, vecMaxsIn, ref vecMinsOut, ref vecMaxsOut);
		}
		public void RotateAABBByInverse(SourceVector vecMinsIn, SourceVector vecMaxsIn, ref SourceVector vecMinsOut, ref SourceVector vecMaxsOut)
		{
			ToMatrix().RotateAABBByInverse(vecMinsIn, vecMaxsIn, ref vecMinsOut, ref vecMaxsOut);
		}
		//inline void TransformPlane( const cplane_t &inPlane, cplane_t &outPlane ) const;
		//inline void InverseTransformPlane( const cplane_t &inPlane, cplane_t &outPlane ) const;

		/// Computes an inverse.  Uses the 'TR' naming to be consistent with the same method in matrix3x4_t (which only works with orthonormal matrices) 
		public void InverseTR(ref SourceCTransform _out)
		{
			SourceMatrix3x4 xForm = ToMatrix();
			_out = xForm.InverseTR().ToCTransform();
		}
	}
}
