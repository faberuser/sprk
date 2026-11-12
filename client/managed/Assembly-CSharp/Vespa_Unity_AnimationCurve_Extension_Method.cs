using System.Collections.Generic;
using UnityEngine;

public static class Vespa_Unity_AnimationCurve_Extension_Method
{
	private class KeyHolder
	{
		public Keyframe Key;
	}

	public static bool IsNullOrEmpty(this AnimationCurve i_this)
	{
		if (i_this == null)
		{
			return true;
		}
		if (i_this.length == 0)
		{
			return true;
		}
		return false;
	}

	public static float EVALUATE(this AnimationCurve i_this, float i_frame, float i_default)
	{
		if (IsNullOrEmpty(i_this))
		{
			return i_default;
		}
		return i_this.Evaluate(i_frame);
	}

	public static bool IsValidCurve(this AnimationCurve This)
	{
		if (IsNullOrEmpty(This))
		{
			return true;
		}
		for (int i = 0; i < This.length; i++)
		{
			Keyframe keyframe = This[i];
			if (float.IsNaN(keyframe.value) || float.IsInfinity(keyframe.value))
			{
				return false;
			}
		}
		return true;
	}

	public static bool IsValidScaleCurve(this AnimationCurve This)
	{
		if (IsNullOrEmpty(This))
		{
			return true;
		}
		for (int i = 0; i < This.length; i++)
		{
			Keyframe keyframe = This[i];
			if (float.IsNaN(keyframe.value) || float.IsInfinity(keyframe.value))
			{
				return false;
			}
			if (Mathf.Approximately(keyframe.value, 0f))
			{
				return false;
			}
		}
		return true;
	}

	public static AnimationCurve ScaleCurve(this AnimationCurve This, float scaleFactor)
	{
		if (IsNullOrEmpty(This))
		{
			return new AnimationCurve();
		}
		Keyframe[] array = new Keyframe[This.length];
		for (int i = 0; i < This.length; i++)
		{
			Keyframe keyframe = This[i];
			array[i].inTangent = keyframe.inTangent * scaleFactor;
			array[i].outTangent = keyframe.outTangent * scaleFactor;
			array[i].tangentMode = keyframe.tangentMode;
			array[i].time = keyframe.time;
			array[i].value = keyframe.value * scaleFactor;
		}
		return new AnimationCurve(array)
		{
			preWrapMode = This.preWrapMode,
			postWrapMode = This.postWrapMode
		};
	}

	public static AnimationCurve FilterCurve(this AnimationCurve This, float error)
	{
		if (IsNullOrEmpty(This))
		{
			return new AnimationCurve();
		}
		List<KeyHolder> list = new List<KeyHolder>();
		for (int i = 0; i < This.length; i++)
		{
			list.Add(new KeyHolder
			{
				Key = This[i]
			});
		}
		for (int j = 1; j < list.Count - 1; j++)
		{
			KeyHolder keyHolder = list[j - 1];
			KeyHolder keyHolder2 = list[j];
			KeyHolder keyHolder3 = list[j + 1];
			float num = Mathf.Abs(keyHolder.Key.value - keyHolder2.Key.value);
			float num2 = Mathf.Abs(keyHolder3.Key.value - keyHolder2.Key.value);
			float num3 = keyHolder2.Key.time - keyHolder.Key.time;
			float num4 = (Mathf.Abs(keyHolder.Key.outTangent) + Mathf.Abs(keyHolder2.Key.inTangent)) * num3 / 2f;
			float num5 = keyHolder3.Key.time - keyHolder2.Key.time;
			float num6 = (Mathf.Abs(keyHolder3.Key.outTangent) + Mathf.Abs(keyHolder2.Key.inTangent)) * num5 / 2f;
			float num7 = num + num2 + num4 + num6;
			if (num7 < error || num7 < Mathf.Epsilon)
			{
				float num8 = (keyHolder3.Key.value - keyHolder.Key.value) / (keyHolder3.Key.time - keyHolder.Key.time);
				keyHolder.Key.outTangent = num8;
				keyHolder3.Key.inTangent = num8;
				list.RemoveAt(j);
				j--;
			}
		}
		Keyframe[] array = new Keyframe[list.Count];
		for (int k = 0; k < array.Length; k++)
		{
			array[k] = list[k].Key;
		}
		return new AnimationCurve(array)
		{
			preWrapMode = This.preWrapMode,
			postWrapMode = This.postWrapMode
		};
	}

	private static float Calc(Keyframe key0, Keyframe key1, float t)
	{
		float time = key0.time;
		float value = key0.value;
		float outTangent = key0.outTangent;
		float time2 = key1.time;
		float value2 = key1.value;
		float inTangent = key1.inTangent;
		float num = time * time * time;
		float num2 = time * time;
		float num3 = time2 * time2 * time2;
		float num4 = time2 * time2;
		float num5 = num - num3 + 3f * time * num4 - 3f * num2 * time2;
		float num6 = (time * outTangent + time * inTangent - time2 * outTangent - time2 * inTangent - 2f * value + 2f * value2) / num5;
		float num7 = ((0f - num2) * outTangent - 2f * num2 * inTangent + 2f * num4 * outTangent + num4 * inTangent - time * time2 * outTangent + time * time2 * inTangent + 3f * time * value - 3f * time * value2 + 3f * value * time2 - 3f * time2 * value2) / num5;
		float num8 = (num * inTangent - num3 * outTangent - time * num4 * outTangent - 2f * time * num4 * inTangent + 2f * num2 * time2 * outTangent + num2 * time2 * inTangent - 6f * time * value * time2 + 6f * time * time2 * value2) / num5;
		float num9 = (time * num3 * outTangent - num2 * num4 * outTangent + num2 * num4 * inTangent - num * time2 * inTangent - value * num3 + num * value2 + 3f * time * value * num4 - 3f * num2 * time2 * value2) / num5;
		return num6 * t * t * t + num7 * t * t + num8 * t + num9;
	}
}
