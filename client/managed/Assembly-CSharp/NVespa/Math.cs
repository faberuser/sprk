using System;
using UnityEngine;

namespace NVespa;

public static class Math
{
	public const float Epsilon = 1.4013E-40f;

	public static DateTime CreateDateTime(int year, int month, int day)
	{
		return CreateDateTime(year, month, day, 0, 0, 0);
	}

	public static DateTime CreateDateTime(int year, int month, int day, int hour, int minute, int second)
	{
		try
		{
			return new DateTime(year, month, day, hour, minute, second);
		}
		catch (Exception)
		{
		}
		year = Mathf.Clamp(year, 1, 9999);
		return new DateTime(year, 1, 1, 0, 0, 0).AddMonths(month - 1).AddDays(day - 1).AddHours(hour)
			.AddMinutes(minute)
			.AddSeconds(second);
	}

	public static void Swap<T>(ref T lhs, ref T rhs)
	{
		T val = lhs;
		lhs = rhs;
		rhs = val;
	}

	public static Vector3 ConvertFromMax(Vector3 max)
	{
		return new Vector3(0f - max.x, max.z, 0f - max.y);
	}

	public static bool Approximately(float a, float b)
	{
		if (a - b < 1.4013E-40f)
		{
			return a - b > -1.4013E-40f;
		}
		return false;
	}

	public static bool Approximately(Vector2 a, Vector2 b)
	{
		if (Approximately(a.x, b.x))
		{
			return Approximately(a.y, b.y);
		}
		return false;
	}

	public static bool Approximately(VectorXZ a, VectorXZ b)
	{
		if (Approximately(a.x, b.x))
		{
			return Approximately(a.z, b.z);
		}
		return false;
	}

	public static bool Approximately(Vector3 a, Vector3 b)
	{
		if (Approximately(a.x, b.x) && Approximately(a.y, b.y))
		{
			return Approximately(a.z, b.z);
		}
		return false;
	}

	public static bool Approximately(Vector4 a, Vector4 b)
	{
		if (Approximately(a.x, b.x) && Approximately(a.y, b.y) && Approximately(a.z, b.z))
		{
			return Approximately(a.w, b.w);
		}
		return false;
	}

	public static bool Approximately(Color a, Color b)
	{
		if (Approximately(a.r, b.r) && Approximately(a.g, b.g) && Approximately(a.b, b.b))
		{
			return Approximately(a.a, b.a);
		}
		return false;
	}

	public static bool Approximately(Rect a, Rect b)
	{
		if (Approximately(a.center, b.center))
		{
			return Approximately(a.size, b.size);
		}
		return false;
	}

	public static Vector3 Vec3FromArray(float[] valueArray)
	{
		return new Vector3(Vespa_Generic_IList_Extension_Method.GetValidValue(valueArray, 0, 0f), Vespa_Generic_IList_Extension_Method.GetValidValue(valueArray, 1, 0f), Vespa_Generic_IList_Extension_Method.GetValidValue(valueArray, 2, 0f));
	}

	public static bool IsInvalidFloat(float value)
	{
		if (!float.IsInfinity(value))
		{
			return float.IsNaN(value);
		}
		return true;
	}

	public static bool IsInvalidVec3(Vector3 value)
	{
		if (!IsInvalidFloat(value.x) && !IsInvalidFloat(value.y))
		{
			return IsInvalidFloat(value.z);
		}
		return true;
	}

	public static Bounds MergeBounds(Bounds i_lhs, Bounds i_rhs)
	{
		Bounds result = default(Bounds);
		result.SetMinMax(Vector3.Min(i_lhs.min, i_rhs.min), Vector3.Max(i_lhs.max, i_rhs.max));
		return result;
	}

	public static Quaternion CalcVerticalBillboard(Camera camera)
	{
		if (camera == null)
		{
			return Quaternion.identity;
		}
		Vector3 forward = camera.transform.forward;
		forward.y = 0f;
		if (forward.sqrMagnitude < Mathf.Epsilon)
		{
			return Quaternion.identity;
		}
		return Quaternion.LookRotation(forward);
	}

	public static Quaternion LookRotation(Vector3 i_from, Vector3 i_to)
	{
		Vector3 forward = i_to - i_from;
		if (forward.magnitude < Mathf.Epsilon)
		{
			return Quaternion.identity;
		}
		return Quaternion.LookRotation(forward);
	}

	public static Vector3 PointNearLine(Vector3 i_lineA, Vector3 i_lineB, Vector3 i_point)
	{
		float num = Vector3.Distance(i_lineA, i_lineB);
		if (num < Mathf.Epsilon)
		{
			return i_lineA;
		}
		float num2 = Vector3.Dot(i_point - i_lineA, i_lineB - i_lineA) / (num * num);
		if (num2 < 0f)
		{
			return i_lineA;
		}
		if (num2 > 1f)
		{
			return i_lineB;
		}
		return i_lineA + num2 * (i_lineB - i_lineA);
	}

	public static VectorXZ PointNearLine(VectorXZ i_lineA, VectorXZ i_lineB, VectorXZ i_point)
	{
		float num = VectorXZ.Distance(i_lineA, i_lineB);
		if (num < Mathf.Epsilon)
		{
			return i_lineA;
		}
		float num2 = VectorXZ.Dot(i_point - i_lineA, i_lineB - i_lineA) / (num * num);
		if (num2 < 0f)
		{
			return i_lineA;
		}
		if (num2 > 1f)
		{
			return i_lineB;
		}
		return i_lineA + num2 * (i_lineB - i_lineA);
	}

	public static float Angle(Vector2 dir)
	{
		return Mathf.Atan2(dir.y, dir.x) * 57.29578f;
	}

	public static float Angle(VectorXZ dir)
	{
		return Mathf.Atan2(dir.z, dir.x) * 57.29578f;
	}

	public static Vector2 Rotate(float i_degrees)
	{
		float x = Mathf.Cos(i_degrees * (MathF.PI / 180f));
		float y = Mathf.Sin(i_degrees * (MathF.PI / 180f));
		return new Vector2(x, y);
	}

	public static VectorXZ RotateXZ(float i_degrees)
	{
		float i_x = Mathf.Cos(i_degrees * (MathF.PI / 180f));
		float i_z = Mathf.Sin(i_degrees * (MathF.PI / 180f));
		return new VectorXZ(i_x, i_z);
	}

	public static Vector2 Rotate(Vector2 i_v, float i_degrees)
	{
		float num = Mathf.Sin(i_degrees * (MathF.PI / 180f));
		float num2 = Mathf.Cos(i_degrees * (MathF.PI / 180f));
		float x = i_v.x;
		float y = i_v.y;
		i_v.x = num2 * x - num * y;
		i_v.y = num * x + num2 * y;
		return i_v;
	}

	public static VectorXZ Rotate(VectorXZ i_v, float i_degrees)
	{
		float num = Mathf.Sin(i_degrees * (MathF.PI / 180f));
		float num2 = Mathf.Cos(i_degrees * (MathF.PI / 180f));
		float x = i_v.x;
		float z = i_v.z;
		i_v.x = num2 * x - num * z;
		i_v.z = num * x + num2 * z;
		return i_v;
	}

	public static Vector2 RandomVec2(Vector2 i_min, Vector2 i_max)
	{
		return new Vector2(UnityEngine.Random.Range(i_min.x, i_max.x), UnityEngine.Random.Range(i_min.y, i_max.y));
	}

	public static Vector3 RandomVec3(Vector3 i_min, Vector3 i_max)
	{
		return new Vector3(UnityEngine.Random.Range(i_min.x, i_max.x), UnityEngine.Random.Range(i_min.y, i_max.y), UnityEngine.Random.Range(i_min.z, i_max.z));
	}

	public static VectorXZ RandomVecXZ(VectorXZ i_min, VectorXZ i_max)
	{
		return new VectorXZ(UnityEngine.Random.Range(i_min.x, i_max.x), UnityEngine.Random.Range(i_min.z, i_max.z));
	}

	public static float CalcFrame(float current, float length)
	{
		return Mathf.Clamp01(current / Vespa_Float_Extension_Method.NoneZero(length));
	}

	public static float ValueForDivide(float i_value)
	{
		if (Mathf.Approximately(i_value, 0f))
		{
			return Mathf.Sign(i_value) * Mathf.Epsilon;
		}
		return i_value;
	}

	public static Bounds LerpBounds(Bounds i_from, Bounds i_to, float i_frame)
	{
		return new Bounds(Vector3.Lerp(i_from.center, i_to.center, i_frame), Vector3.Lerp(i_from.size, i_to.size, i_frame));
	}

	public static int Lerp(int i_from, int i_to, int i_tValue, int i_tMax)
	{
		if (i_tMax == 0)
		{
			if (i_tValue <= 0)
			{
				return i_from;
			}
			return i_to;
		}
		return (int)((long)(i_to - i_from) * (long)i_tValue / i_tMax + i_from);
	}

	public static int Mid(int i_value0, int i_value1, int i_value2)
	{
		if (i_value1 < i_value2)
		{
			return Mathf.Clamp(i_value0, i_value1, i_value2);
		}
		return Mathf.Clamp(i_value0, i_value2, i_value1);
	}

	public static long Max(long i_valueA, long i_valueB)
	{
		if (i_valueA <= i_valueB)
		{
			return i_valueB;
		}
		return i_valueA;
	}

	public static long Min(long i_valueA, long i_valueB)
	{
		if (i_valueA >= i_valueB)
		{
			return i_valueB;
		}
		return i_valueA;
	}

	public static float LerpOver(float i_from, float i_to, float i_frame)
	{
		return i_from * (1f - i_frame) + i_to * i_frame;
	}

	public static Vector2 LerpOver(Vector2 i_from, Vector2 i_to, float i_frame)
	{
		return i_from * (1f - i_frame) + i_to * i_frame;
	}

	public static Vector3 LerpOver(Vector3 i_from, Vector3 i_to, float i_frame)
	{
		return i_from * (1f - i_frame) + i_to * i_frame;
	}

	public static VectorXZ LerpOver(VectorXZ i_from, VectorXZ i_to, float i_frame)
	{
		return i_from * (1f - i_frame) + i_to * i_frame;
	}

	public static int ClampOver(int value, int min, int max)
	{
		return Mathf.Clamp(value, min, max - 1);
	}

	public static Vector2 InverseLerp(Vector2 from, Vector2 to, Vector2 value)
	{
		return new Vector2(Mathf.InverseLerp(from.x, to.x, value.x), Mathf.InverseLerp(from.y, to.y, value.y));
	}

	public static Vector3 InverseLerp(Vector3 from, Vector3 to, Vector3 value)
	{
		return new Vector3(Mathf.InverseLerp(from.x, to.x, value.x), Mathf.InverseLerp(from.y, to.y, value.y), Mathf.InverseLerp(from.z, to.z, value.z));
	}

	public static float InverseLerpInc(float from, float to, float value)
	{
		if (Mathf.Approximately(to, from))
		{
			if (value < from)
			{
				return 0f;
			}
			return 1f;
		}
		return Mathf.InverseLerp(from, to, value);
	}

	public static float InverseLerpOver(float from, float to, float value)
	{
		float num = to - from;
		if (Mathf.Abs(num) < Mathf.Epsilon)
		{
			if (from < to)
			{
				if (value <= from)
				{
					return 0f;
				}
				return 1f;
			}
			if (value <= to)
			{
				return 1f;
			}
			return 0f;
		}
		return (value - from) / num;
	}

	public static Vector3 PointOnCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
	{
		Vector3 result = default(Vector3);
		float num = ((0f - t + 2f) * t - 1f) * t * 0.5f;
		float num2 = ((3f * t - 5f) * t * t + 2f) * 0.5f;
		float num3 = ((-3f * t + 4f) * t + 1f) * t * 0.5f;
		float num4 = (t - 1f) * t * t * 0.5f;
		result.x = p0.x * num + p1.x * num2 + p2.x * num3 + p3.x * num4;
		result.y = p0.y * num + p1.y * num2 + p2.y * num3 + p3.y * num4;
		result.z = p0.z * num + p1.z * num2 + p2.z * num3 + p3.z * num4;
		return result;
	}

	public static Vector3 HermitSpline(Vector3 p0, Vector3 m0, Vector3 p1, Vector3 m1, float t)
	{
		float num = t * t;
		float num2 = num * t;
		return (2f * num2 - 3f * num + 1f) * p0 + (num2 - 2f * num + t) * m0 + (-2f * num2 + 3f * num) * p1 + (num2 - num) * m1;
	}

	public static Vector3 HermitSplineDelta(Vector3 p0, Vector3 m0, Vector3 p1, Vector3 m1, float t)
	{
		float num = t * t;
		float num2 = 3f * num;
		float num3 = 2f * t;
		float num4 = 1f;
		return (2f * num2 - 3f * num3) * p0 + (num2 - 2f * num3 + num4) * m0 + (-2f * num2 + 3f * num3) * p1 + (num2 - num3) * m1;
	}

	public static float BezierSpline(float p0, float p1, float p2, float p3, float t)
	{
		BezierSplineCalculate(t, out var o_weightP, out var o_weightP2, out var o_weightP3, out var o_weightP4);
		return p0 * o_weightP + p1 * o_weightP2 + p2 * o_weightP3 + p3 * o_weightP4;
	}

	public static Vector2 BezierSpline(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
	{
		BezierSplineCalculate(t, out var o_weightP, out var o_weightP2, out var o_weightP3, out var o_weightP4);
		return p0 * o_weightP + p1 * o_weightP2 + p2 * o_weightP3 + p3 * o_weightP4;
	}

	public static VectorXZ BezierSpline(VectorXZ p0, VectorXZ p1, VectorXZ p2, VectorXZ p3, float t)
	{
		BezierSplineCalculate(t, out var o_weightP, out var o_weightP2, out var o_weightP3, out var o_weightP4);
		return p0 * o_weightP + p1 * o_weightP2 + p2 * o_weightP3 + p3 * o_weightP4;
	}

	public static Vector3 BezierSpline(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
	{
		BezierSplineCalculate(t, out var o_weightP, out var o_weightP2, out var o_weightP3, out var o_weightP4);
		return p0 * o_weightP + p1 * o_weightP2 + p2 * o_weightP3 + p3 * o_weightP4;
	}

	public static float BezierSplineDelta(float p0, float p1, float p2, float p3, float t)
	{
		BezierSplineDeltaCalculate(t, out var o_weightP, out var o_weightP2, out var o_weightP3, out var o_weightP4);
		return o_weightP * p0 + o_weightP2 * p1 + o_weightP3 * p2 + o_weightP4 * p3;
	}

	public static Vector2 BezierSplineDelta(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
	{
		BezierSplineDeltaCalculate(t, out var o_weightP, out var o_weightP2, out var o_weightP3, out var o_weightP4);
		return o_weightP * p0 + o_weightP2 * p1 + o_weightP3 * p2 + o_weightP4 * p3;
	}

	public static VectorXZ BezierSplineDelta(VectorXZ p0, VectorXZ p1, VectorXZ p2, VectorXZ p3, float t)
	{
		BezierSplineDeltaCalculate(t, out var o_weightP, out var o_weightP2, out var o_weightP3, out var o_weightP4);
		return o_weightP * p0 + o_weightP2 * p1 + o_weightP3 * p2 + o_weightP4 * p3;
	}

	public static Vector3 BezierSplineDelta(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
	{
		BezierSplineDeltaCalculate(t, out var o_weightP, out var o_weightP2, out var o_weightP3, out var o_weightP4);
		return o_weightP * p0 + o_weightP2 * p1 + o_weightP3 * p2 + o_weightP4 * p3;
	}

	private static void BezierSplineCalculate(float t, out float o_weightP0, out float o_weightP1, out float o_weightP2, out float o_weightP3)
	{
		float num = 1f - t;
		float num2 = num * num;
		float num3 = num2 * num;
		float num4 = t * t;
		float num5 = num4 * t;
		o_weightP0 = num3;
		o_weightP1 = 3f * num2 * t;
		o_weightP2 = 3f * num * num4;
		o_weightP3 = num5;
	}

	public static void BezierSplineDeltaCalculate(float t, out float o_weightP0, out float o_weightP1, out float o_weightP2, out float o_weightP3)
	{
		float num = t * t;
		o_weightP0 = -0.5f * (1f - 2f * t + num);
		o_weightP1 = 0.5f * (1f - 4f * t + 3f * num);
		o_weightP2 = 0.5f * (2f * t - 3f * num);
		o_weightP3 = 0.5f * num;
	}

	public static void BezierSplineDeltaCalculate_Orig(float t, out float o_weightP0, out float o_weightP1, out float o_weightP2, out float o_weightP3)
	{
		float num = t * t;
		o_weightP0 = -3f * (1f - 2f * t + num);
		o_weightP1 = 3f * (1f - 4f * t + 3f * num);
		o_weightP2 = 3f * (2f * t - 3f * num);
		o_weightP3 = 3f * num;
	}
}
