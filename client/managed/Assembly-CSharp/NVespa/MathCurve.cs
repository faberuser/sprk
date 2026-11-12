using UnityEngine;

namespace NVespa;

public static class MathCurve
{
	public static Keyframe CreateKeyframe(float t, Keyframe keyframe0, Keyframe keyframe1)
	{
		float num = keyframe1.time - keyframe0.time;
		float value = Evaluate(t, keyframe0, keyframe1);
		float num2 = EvaluateDelta(t, keyframe0, keyframe1);
		return new Keyframe
		{
			inTangent = num2 / num,
			outTangent = num2 / num,
			tangentMode = 0,
			time = Mathf.Lerp(keyframe0.time, keyframe1.time, t),
			value = value
		};
	}

	public static float Evaluate(float t, Keyframe keyframe0, Keyframe keyframe1)
	{
		float num = keyframe1.time - keyframe0.time;
		float num2 = keyframe0.outTangent * num;
		float num3 = keyframe1.inTangent * num;
		float value = keyframe0.value;
		float value2 = keyframe1.value;
		float num4 = t * t;
		float num5 = num4 * t;
		float num6 = 2f * num5 - 3f * num4 + 1f;
		float num7 = num5 - 2f * num4 + t;
		float num8 = num5 - num4;
		float num9 = -2f * num5 + 3f * num4;
		return num6 * value + num7 * num2 + num8 * num3 + num9 * value2;
	}

	public static float EvaluateDelta(float t, Keyframe keyframe0, Keyframe keyframe1)
	{
		float num = keyframe1.time - keyframe0.time;
		float num2 = keyframe0.outTangent * num;
		float num3 = keyframe1.inTangent * num;
		float value = keyframe0.value;
		float value2 = keyframe1.value;
		float num4 = t * t;
		float num5 = 6f * value + 3f * num2 + 3f * num3 - 6f * value2;
		float num6 = -6f * value - 4f * num2 - 2f * num3 + 6f * value2;
		float num7 = num2;
		return num5 * num4 + num6 * t + num7;
	}

	public static bool EvaluateCriticalPointFrame(Keyframe keyframe0, Keyframe keyframe1, out float t0, out float t1)
	{
		float num = keyframe1.time - keyframe0.time;
		float num2 = keyframe0.outTangent * num;
		float num3 = keyframe1.inTangent * num;
		if (float.IsInfinity(num2) || float.IsInfinity(num3) || float.IsNaN(num2) || float.IsNaN(num3))
		{
			t0 = 0f;
			t1 = 0f;
			return false;
		}
		float value = keyframe0.value;
		float value2 = keyframe1.value;
		float num4 = 6f * value + 3f * num2 + 3f * num3 - 6f * value2;
		float num5 = -6f * value - 4f * num2 - 2f * num3 + 6f * value2;
		float num6 = num2;
		float num7 = num5 * num5 - 4f * num4 * num6;
		if (num7 < 0f)
		{
			t0 = 0f;
			t1 = 0f;
			return false;
		}
		if (Mathf.Approximately(num4, 0f))
		{
			t0 = 0f;
			t1 = 0f;
			return false;
		}
		t0 = (0f - num5 + Mathf.Sqrt(num7)) / (2f * num4);
		t1 = (0f - num5 - Mathf.Sqrt(num7)) / (2f * num4);
		return true;
	}

	public static void EvaluateMinMaxValue(Keyframe keyframe0, Keyframe keyframe1, out float oMin, out float oMax)
	{
		oMin = Mathf.Min(keyframe0.value, keyframe1.value);
		oMax = Mathf.Max(keyframe0.value, keyframe1.value);
		if (EvaluateCriticalPointFrame(keyframe0, keyframe1, out var t, out var t2))
		{
			t = Mathf.Clamp01(t);
			t2 = Mathf.Clamp01(t2);
			float num = Evaluate(t, keyframe0, keyframe1);
			float num2 = Evaluate(t2, keyframe0, keyframe1);
			oMin = Mathf.Min(oMin, num, num2);
			oMax = Mathf.Max(oMax, num, num2);
		}
	}
}
