using UnityEngine;

namespace NVespa;

public class AnimationCurveCache
{
	private float[] _value = new float[1];

	public void Reset(AnimationCurve curve, int splitCount)
	{
		_value = new float[splitCount];
		for (int i = 0; i < splitCount; i++)
		{
			float num = (float)i / (float)(splitCount - 1);
			_value[i] = Vespa_Unity_AnimationCurve_Extension_Method.EVALUATE(curve, num, num);
		}
	}

	public float Evaluate(float frame)
	{
		float num = frame * (float)(_value.Length - 1);
		int num2 = Mathf.FloorToInt(num);
		if (num2 < 0)
		{
			return _value[0];
		}
		if (num2 >= _value.Length - 1)
		{
			return _value[_value.Length - 1];
		}
		float t = num - (float)num2;
		return Mathf.Lerp(_value[num2], _value[num2 + 1], t);
	}
}
