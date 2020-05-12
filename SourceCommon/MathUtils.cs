using System;

namespace SourceFormatParser.Common
{
    public static class MathUtils
    {
        public const float M_PI = 3.14159265358979323846f;	// matches value in gcc v2 math.h
        public const float M_PI_F = ((float)(M_PI));
        public static float RAD2DEG(float x) => ((float)(x) * (float)(180.0f / M_PI_F));
        public static float DEG2RAD(float x) => ((float)(x) * (float)(M_PI_F / 180.0f));

        public static float Clamp(float val, float min, float max)
        {
            if (val <= min) return min;
            if (val >= max) return max;
            return val;
        }

        public static float Avg(params float[] fs)
        {
            float avg = 0;
            foreach (float f in fs) avg += f;
            avg /= fs.Length;
            return avg;
        }
        public static byte Avg(params byte[] bs)
        {
            int avg = 0;
            foreach (byte b in bs) avg += b;
            avg /= bs.Length;
            return (byte)avg;
        }

        public static SourceMatrix3x4 AngleMatrix(SourceQAngle angles)
        {
            //float sr, sp, sy, cr, cp, cy;

            //SinCos(DEG2RAD(angles[YAW]), &sy, &cy);
            //SinCos(DEG2RAD(angles[PITCH]), &sp, &cp);
            //SinCos(DEG2RAD(angles[ROLL]), &sr, &cr);

            //SourceMatrix3x4 matrix;
            //// matrix = (YAW * PITCH) * ROLL
            //matrix[0][0] = cp * cy;
            //matrix[1][0] = cp * sy;
            //matrix[2][0] = -sp;

            //// NOTE: Do not optimize this to reduce multiplies! optimizer bug will screw this up.
            //matrix[0][1] = sr * sp * cy + cr * -sy;
            //matrix[1][1] = sr * sp * sy + cr * cy;
            //matrix[2][1] = sr * cp;
            //matrix[0][2] = (cr * sp * cy + -sr * -sy);
            //matrix[1][2] = (cr * sp * sy + -sr * cy);
            //matrix[2][2] = cr * cp;

            //matrix[0][3] = 0.0f;
            //matrix[1][3] = 0.0f;
            //matrix[2][3] = 0.0f;

            //return matrix;
            throw new NotImplementedException();
        }

        public static SourceQuaternion MatrixQuaternion(SourceMatrix3x4 mat)
        {
            SourceQuaternion tmp = new SourceQuaternion();
            MatrixQuaternion(mat, ref tmp);
            return tmp;
        }

        public static void MatrixQuaternion(SourceMatrix3x4 mat, ref SourceQuaternion q)
        {
            SourceQAngle angles = new SourceQAngle();
            MatrixAngles(mat, ref angles);
            AngleQuaternion(angles, ref q);
        }

        public static void MatrixAngles(SourceMatrix3x4 matrix, ref SourceQAngle angles)
        {
            //MatrixAngles(matrix, out angles.x);
            throw new NotImplementedException();
        }

        public static void AngleQuaternion(SourceQAngle angles, ref SourceQuaternion outQuat)
        {
            float sr, sp, sy, cr, cp, cy;

            SinCos(DEG2RAD(angles.y) * 0.5f, out sy, out cy);
            SinCos(DEG2RAD(angles.x) * 0.5f, out sp, out cp);
            SinCos(DEG2RAD(angles.z) * 0.5f, out sr, out cr);

            // NJS: for some reason VC6 wasn't recognizing the common subexpressions:
            float srXcp = sr * cp, crXsp = cr * sp;
            outQuat.x = srXcp * cy - crXsp * sy; // X
            outQuat.y = crXsp * cy + srXcp * sy; // Y

            float crXcp = cr * cp, srXsp = sr * sp;
            outQuat.z = crXcp * sy - srXsp * cy; // Z
            outQuat.w = crXcp * cy + srXsp * sy; // W (real component)
        }

        public static void SinCos(float x, out float s, out float c)
        {
            c = (float)Math.Cos(x);
            s = (float)Math.Sin(x);
        }

        public static void AngleMatrix(SourceQAngle angles, SourceVector position, ref SourceMatrix3x4 matrix)
        {
            AngleMatrix(angles, ref matrix);
            MatrixSetColumn(position, 3, ref matrix);
        }

        public static void AngleMatrix(SourceQAngle angles, ref SourceMatrix3x4 matrix)
        {
            float sr, sp, sy, cr, cp, cy;

            SinCos(DEG2RAD(angles[1]), out sy, out cy);
            SinCos(DEG2RAD(angles[0]), out sp, out cp);
            SinCos(DEG2RAD(angles[2]), out sr, out cr);

            // matrix = (YAW * PITCH) * ROLL
            matrix[0][0] = cp * cy;
            matrix[1][0] = cp * sy;
            matrix[2][0] = -sp;

            // NOTE: Do not optimize this to reduce multiplies! optimizer bug will screw this up.
            matrix[0][1] = sr * sp * cy + cr * -sy;
            matrix[1][1] = sr * sp * sy + cr * cy;
            matrix[2][1] = sr * cp;
            matrix[0][2] = (cr * sp * cy + -sr * -sy);
            matrix[1][2] = (cr * sp * sy + -sr * cy);
            matrix[2][2] = cr * cp;

            matrix[0][3] = 0.0f;
            matrix[1][3] = 0.0f;
            matrix[2][3] = 0.0f;
        }

        public static void MatrixSetColumn(SourceVector _in, int column, ref SourceMatrix3x4 _out)
        {
            _out[0][column] = _in.x;
            _out[1][column] = _in.y;
            _out[2][column] = _in.z;
        }

        public static void QuaternionAngles(SourceQuaternion q, ref SourceRadianEuler angles)
        {
            // FIXME: doing it this way calculates too much data, needs to do an optimized version...
            SourceMatrix3x4 matrix = new SourceMatrix3x4();
            QuaternionMatrix(q, ref matrix);
            MatrixAngles(matrix, ref angles);
        }

        public static void QuaternionMatrix(SourceQuaternion q, ref SourceMatrix3x4 matrix)
        {
            // Original code
            // This should produce the same code as below with optimization, but looking at the assmebly,
            // it doesn't.  There are 7 extra multiplies in the release build of this, go figure.

            matrix[0][0] = 1.0f - 2.0f * q.y * q.y - 2.0f * q.z * q.z;
            matrix[1][0] = 2.0f * q.x * q.y + 2.0f * q.w * q.z;
            matrix[2][0] = 2.0f * q.x * q.z - 2.0f * q.w * q.y;

            matrix[0][1] = 2.0f * q.x * q.y - 2.0f * q.w * q.z;
            matrix[1][1] = 1.0f - 2.0f * q.x * q.x - 2.0f * q.z * q.z;
            matrix[2][1] = 2.0f * q.y * q.z + 2.0f * q.w * q.x;

            matrix[0][2] = 2.0f * q.x * q.z + 2.0f * q.w * q.y;
            matrix[1][2] = 2.0f * q.y * q.z - 2.0f * q.w * q.x;
            matrix[2][2] = 1.0f - 2.0f * q.x * q.x - 2.0f * q.y * q.y;

            matrix[0][3] = 0.0f;
            matrix[1][3] = 0.0f;
            matrix[2][3] = 0.0f;
        }

        public static void MatrixAngles(SourceMatrix3x4 matrix, ref SourceRadianEuler angles)
        {
            float[] a = new float[3];
            for (int i = 0; i < 3; i++) a[i] = angles[i];
            MatrixAngles(matrix, ref a);
            for (int i = 0; i < 3; i++) angles[i] = a[i];
        }

        public static void MatrixAngles(SourceMatrix3x4 matrix, ref float[] angles)
        {
            float[] forward = new float[3];
            float[] left = new float[3];
            float[] up = new float[3];

            //
            // Extract the basis vectors from the matrix. Since we only need the Z
            // component of the up vector, we don't get X and Y.
            //
            forward[0] = matrix[0][0];
            forward[1] = matrix[1][0];
            forward[2] = matrix[2][0];
            left[0] = matrix[0][1];
            left[1] = matrix[1][1];
            left[2] = matrix[2][1];
            up[2] = matrix[2][2];

            float xyDist = (float)(Math.Sqrt(forward[0] * forward[0] + forward[1] * forward[1]));

            // enough here to get angles?
            if (xyDist > 0.001f)
            {
                // (yaw)	y = ATAN( forward.y, forward.x );		-- in our space, forward is the X axis
                angles[1] = RAD2DEG((float)Math.Atan2(forward[1], forward[0]));

                // (pitch)	x = ATAN( -forward.z, sqrt(forward.x*forward.x+forward.y*forward.y) );
                angles[0] = RAD2DEG((float)Math.Atan2(-forward[2], xyDist));

                // (roll)	z = ATAN( left.z, up.z );
                angles[2] = RAD2DEG((float)Math.Atan2(left[2], up[2]));
            }
            else    // forward is mostly Z, gimbal lock-
            {
                // (yaw)	y = ATAN( -left.x, left.y );			-- forward is mostly z, so use right for yaw
                angles[1] = RAD2DEG((float)Math.Atan2(-left[0], left[1]));

                // (pitch)	x = ATAN( -forward.z, sqrt(forward.x*forward.x+forward.y*forward.y) );
                angles[0] = RAD2DEG((float)Math.Atan2(-forward[2], xyDist));

                // Assume no roll in this case as one degree of freedom has been lost (i.e. yaw == roll)
                angles[2] = 0;
            }
        }

        public static void QuaternionAngles(SourceQuaternion q, ref SourceQAngle angles)
        {
            // FIXME: doing it this way calculates too much data, needs to do an optimized version...
            SourceMatrix3x4 matrix = new SourceMatrix3x4();
            QuaternionMatrix(q, ref matrix);
            MatrixAngles(matrix, ref angles);
        }

        public static void AngleQuaternion(SourceRadianEuler angles, ref SourceQuaternion outQuat)
        {
            float sr, sp, sy, cr, cp, cy;

            SinCos(angles.z * 0.5f, out sy, out cy);
            SinCos(angles.y * 0.5f, out sp, out cp);
            SinCos(angles.x * 0.5f, out sr, out cr);

            // NJS: for some reason VC6 wasn't recognizing the common subexpressions:
            float srXcp = sr * cp, crXsp = cr * sp;
            outQuat.x = srXcp * cy - crXsp * sy; // X
            outQuat.y = crXsp * cy + srXcp * sy; // Y

            float crXcp = cr * cp, srXsp = sr * sp;
            outQuat.z = crXcp * sy - srXsp * cy; // Z
            outQuat.w = crXcp * cy + srXsp * sy; // W (real component)
        }

        public static SourceMatrix3x4 TransformMatrix(SourceCTransform _in)
        {
            SourceMatrix3x4 _out = new SourceMatrix3x4();
            TransformMatrix(_in, ref _out);
            return _out;
        }

        public static void TransformMatrix(SourceCTransform _in, ref SourceMatrix3x4 _out)
        {
            QuaternionMatrix(_in.m_orientation, _in.m_vPosition, ref _out);
        }

        public static void QuaternionMatrix(SourceQuaternion q, SourceVector pos, ref SourceMatrix3x4 matrix)
        {
            QuaternionMatrix(q, ref matrix);

            matrix[0][3] = pos.x;
            matrix[1][3] = pos.y;
            matrix[2][3] = pos.z;
        }

        public static void TransformVectorsFLU(SourceCTransform _in, ref SourceVector pForward, ref SourceVector pLeft, ref SourceVector pUp)
        {
            QuaternionVectorsFLU(_in.m_orientation, ref pForward, ref pLeft, ref pUp);
        }

        public static void QuaternionVectorsFLU(SourceQuaternion q, ref SourceVector pForward, ref SourceVector pLeft, ref SourceVector pUp)
        {
            // Note: it's pretty much identical to just computing the quaternion matrix and assigning its columns to the vectors
            pForward = q.GetForward();
            pLeft = q.GetLeft();
            pUp = q.GetUp();
        }

        public static void TransformVectorsForward(SourceCTransform _in, ref SourceVector pForward)
        {
            QuaternionVectorsForward(_in.m_orientation, ref pForward);
        }

        public static void QuaternionVectorsForward(SourceQuaternion q, ref SourceVector pForward)
        {
            pForward = q.GetForward();
        }

        public static SourceVector TransformPoint(SourceCTransform tm, SourceVector p)
        {
            return new SourceVector(
                tm.m_vPosition.x + (1.0f - 2.0f * tm.m_orientation.y * tm.m_orientation.y - 2.0f * tm.m_orientation.z * tm.m_orientation.z) * p.x + (2.0f * tm.m_orientation.x * tm.m_orientation.y - 2.0f * tm.m_orientation.w * tm.m_orientation.z) * p.y + (2.0f * tm.m_orientation.x * tm.m_orientation.z + 2.0f * tm.m_orientation.w * tm.m_orientation.y) * p.z,
                tm.m_vPosition.y + (2.0f * tm.m_orientation.x * tm.m_orientation.y + 2.0f * tm.m_orientation.w * tm.m_orientation.z) * p.x + (1.0f - 2.0f * tm.m_orientation.x * tm.m_orientation.x - 2.0f * tm.m_orientation.z * tm.m_orientation.z) * p.y + (2.0f * tm.m_orientation.y * tm.m_orientation.z - 2.0f * tm.m_orientation.w * tm.m_orientation.x) * p.z,
                tm.m_vPosition.z + (2.0f * tm.m_orientation.x * tm.m_orientation.z - 2.0f * tm.m_orientation.w * tm.m_orientation.y) * p.x + (2.0f * tm.m_orientation.y * tm.m_orientation.z + 2.0f * tm.m_orientation.w * tm.m_orientation.x) * p.y + (1.0f - 2.0f * tm.m_orientation.x * tm.m_orientation.x - 2.0f * tm.m_orientation.y * tm.m_orientation.y) * p.z
                );
        }

        public static void RotatePoint(SourceCTransform tm, SourceVector p, ref SourceVector _out)
        {
            _out.x = (1.0f - 2.0f * tm.m_orientation.y * tm.m_orientation.y - 2.0f * tm.m_orientation.z * tm.m_orientation.z) * p.x + (2.0f * tm.m_orientation.x * tm.m_orientation.y - 2.0f * tm.m_orientation.w * tm.m_orientation.z) * p.y + (2.0f * tm.m_orientation.x * tm.m_orientation.z + 2.0f * tm.m_orientation.w * tm.m_orientation.y) * p.z;
            _out.y = (2.0f * tm.m_orientation.x * tm.m_orientation.y + 2.0f * tm.m_orientation.w * tm.m_orientation.z) * p.x + (1.0f - 2.0f * tm.m_orientation.x * tm.m_orientation.x - 2.0f * tm.m_orientation.z * tm.m_orientation.z) * p.y + (2.0f * tm.m_orientation.y * tm.m_orientation.z - 2.0f * tm.m_orientation.w * tm.m_orientation.x) * p.z;
            _out.z = (2.0f * tm.m_orientation.x * tm.m_orientation.z - 2.0f * tm.m_orientation.w * tm.m_orientation.y) * p.x + (2.0f * tm.m_orientation.y * tm.m_orientation.z + 2.0f * tm.m_orientation.w * tm.m_orientation.x) * p.y + (1.0f - 2.0f * tm.m_orientation.x * tm.m_orientation.x - 2.0f * tm.m_orientation.y * tm.m_orientation.y) * p.z;
        }

        public static void VectorITransform(SourceVector v, SourceCTransform t, ref SourceVector _out)
        {
            // FIXME: Make work directly with the transform
            SourceMatrix3x4 m = new SourceMatrix3x4();
            TransformMatrix(t, ref m);
            VectorITransform(v, m, ref _out);
        }

        public static void VectorITransform(SourceVector in1, SourceMatrix3x4 in2, ref SourceVector _out)
        {
            throw new NotImplementedException();
            //VectorITransform(in1.x, in2, ref _out.x);
        }

        public static void VectorIRotate(SourceVector v, SourceCTransform t, ref SourceVector _out)
        {
            // FIXME: Make work directly with the transform
            SourceMatrix3x4 m = new SourceMatrix3x4();
            TransformMatrix(t, ref m);
            VectorIRotate(v, m, ref _out);
        }

        public static void VectorIRotate(SourceVector in1, SourceMatrix3x4 in2, ref SourceVector _out)
        {
            throw new NotImplementedException();
            //VectorIRotate( in1.x, in2, ref _out.x );
        }

        public static void TransformAABB(SourceMatrix3x4 transform, SourceVector vecMinsIn, SourceVector vecMaxsIn, ref SourceVector vecMinsOut, ref SourceVector vecMaxsOut)
        {
            throw new NotImplementedException(); //can't find some of methods

            //SourceVector localCenter;
            //VectorAdd(vecMinsIn, vecMaxsIn, localCenter);
            //localCenter *= 0.5f;

            //SourceVector localExtents;
            //VectorSubtract(vecMaxsIn, localCenter, localExtents);

            //SourceVector worldCenter;
            //VectorTransform(localCenter, transform, worldCenter);

            //SourceVector worldExtents;
            //worldExtents.x = DotProductAbs(localExtents, transform[0]);
            //worldExtents.y = DotProductAbs(localExtents, transform[1]);
            //worldExtents.z = DotProductAbs(localExtents, transform[2]);

            //VectorSubtract(worldCenter, worldExtents, vecMinsOut);
            //VectorAdd(worldCenter, worldExtents, vecMaxsOut);
        }

        public static void ITransformAABB(SourceMatrix3x4 transform, SourceVector vecMinsIn, SourceVector vecMaxsIn, ref SourceVector vecMinsOut, ref SourceVector vecMaxsOut)
        {
            throw new NotImplementedException(); //can't find some of methods

            //      SourceVector worldCenter;
            //  VectorAdd(vecMinsIn, vecMaxsIn, worldCenter );
            //  worldCenter *= 0.5f;

            //      SourceVector worldExtents;
            //  VectorSubtract(vecMaxsIn, worldCenter, worldExtents );

            //  Vector localCenter;
            //  VectorITransform(worldCenter, transform, localCenter );

            //  Vector localExtents;
            //  localExtents.x =	FloatMakePositive(worldExtents.x* transform[0][0] ) + 
            //FloatMakePositive(worldExtents.y* transform[1][0] ) + 
            //FloatMakePositive(worldExtents.z* transform[2][0] );
            //  localExtents.y =	FloatMakePositive(worldExtents.x* transform[0][1] ) + 
            //FloatMakePositive(worldExtents.y* transform[1][1] ) + 
            //FloatMakePositive(worldExtents.z* transform[2][1] );
            //  localExtents.z =	FloatMakePositive(worldExtents.x* transform[0][2] ) + 
            //FloatMakePositive(worldExtents.y* transform[1][2] ) + 
            //FloatMakePositive(worldExtents.z* transform[2][2] );

            //  VectorSubtract(localCenter, localExtents, vecMinsOut );
            //  VectorAdd(localCenter, localExtents, vecMaxsOut );
        }

        public static void RotateAABB(SourceMatrix3x4 transform, SourceVector vecMinsIn, SourceVector vecMaxsIn, ref SourceVector vecMinsOut, ref SourceVector vecMaxsOut)
        {
            throw new NotImplementedException(); //can't find some of methods

            //SourceVector localCenter;
            //VectorAdd(vecMinsIn, vecMaxsIn, localCenter);
            //localCenter *= 0.5f;

            //SourceVector localExtents;
            //VectorSubtract(vecMaxsIn, localCenter, localExtents);

            //SourceVector newCenter;
            //VectorRotate(localCenter, transform, newCenter);

            //VecSourceVectortor newExtents;
            //newExtents.x = DotProductAbs(localExtents, transform[0]);
            //newExtents.y = DotProductAbs(localExtents, transform[1]);
            //newExtents.z = DotProductAbs(localExtents, transform[2]);

            //VectorSubtract(newCenter, newExtents, vecMinsOut);
            //VectorAdd(newCenter, newExtents, vecMaxsOut);
        }

        public static void IRotateAABB(SourceMatrix3x4 transform, SourceVector vecMinsIn, SourceVector vecMaxsIn, ref SourceVector vecMinsOut, ref SourceVector vecMaxsOut)
        {
            throw new NotImplementedException(); //can't find some of methods

            //SourceVector oldCenter;
            //VectorAdd(vecMinsIn, vecMaxsIn, oldCenter);
            //oldCenter *= 0.5f;

            //SourceVector oldExtents;
            //VectorSubtract(vecMaxsIn, oldCenter, oldExtents);

            //SourceVector newCenter;
            //VectorIRotate(oldCenter, transform, newCenter);

            //SourceVector newExtents;
            //newExtents.x = FloatMakePositive(oldExtents.x * transform[0][0]) +
            //            FloatMakePositive(oldExtents.y * transform[1][0]) +
            //            FloatMakePositive(oldExtents.z * transform[2][0]);
            //newExtents.y = FloatMakePositive(oldExtents.x * transform[0][1]) +
            //            FloatMakePositive(oldExtents.y * transform[1][1]) +
            //            FloatMakePositive(oldExtents.z * transform[2][1]);
            //newExtents.z = FloatMakePositive(oldExtents.x * transform[0][2]) +
            //            FloatMakePositive(oldExtents.y * transform[1][2]) +
            //            FloatMakePositive(oldExtents.z * transform[2][2]);

            //VectorSubtract(newCenter, newExtents, vecMinsOut);
            //VectorAdd(newCenter, newExtents, vecMaxsOut);
        }

        public static void MatrixInvert(SourceMatrix3x4 _in, ref SourceMatrix3x4 _out)
        {
            if (_in == _out)
            {
                V_swap(ref _out[0][1], ref _out[1][0]);
                V_swap(ref _out[0][2], ref _out[2][0]);
                V_swap(ref _out[1][2], ref _out[2][1]);
            }
            else
            {
                // transpose the matrix
                _out[0][0] = _in[0][0];
                _out[0][1] = _in[1][0];
                _out[0][2] = _in[2][0];

                _out[1][0] = _in[0][1];
                _out[1][1] = _in[1][1];
                _out[1][2] = _in[2][1];

                _out[2][0] = _in[0][2];
                _out[2][1] = _in[1][2];
                _out[2][2] = _in[2][2];
            }

            // now fix up the translation to be in the other space
            float[] tmp = new float[3];
            tmp[0] = _in[0][3];
            tmp[1] = _in[1][3];
            tmp[2] = _in[2][3];

            _out[0][3] = -DotProduct(tmp, _out[0]);
            _out[1][3] = -DotProduct(tmp, _out[1]);
            _out[2][3] = -DotProduct(tmp, _out[2]);
        }

        public static void V_swap<T>(ref T x, ref T y)
        {
            T temp = x;
            x = y;
            y = temp;
        }

        public static float DotProduct(float[] a, float[] b) => a[0] * b[0] + a[1] * b[1] + a[2] * b[2];

        public static void AngleMatrix(SourceRadianEuler angles, SourceVector position, ref SourceMatrix3x4 matrix)
        {
            AngleMatrix(angles, ref matrix);
            MatrixSetColumn(position, 3, ref matrix);
        }

        public static void AngleMatrix(SourceRadianEuler angles, ref SourceMatrix3x4 matrix)
        {
            SourceQAngle quakeEuler = new SourceQAngle(RAD2DEG(angles.y), RAD2DEG(angles.z), RAD2DEG(angles.x));

            AngleMatrix(quakeEuler, ref matrix);
        }

        public static SourceCTransform MatrixTransform(SourceMatrix3x4 _in)
        {
            SourceCTransform _out = new SourceCTransform();
            MatrixTransform(_in, ref _out);
            return _out;
        }

        public static void MatrixTransform(SourceMatrix3x4 _in, ref SourceCTransform _out)
        {
            MatrixQuaternion(_in, ref _out.m_orientation);
            MatrixGetColumn(_in, 3, ref _out.m_vPosition);
        }

        public static void MatrixGetColumn(SourceMatrix3x4 _in, int column, ref SourceVector _out)
        {
            _out.x = _in[0][column];
            _out.y = _in[1][column];
            _out.z = _in[2][column];
        }
    }
}