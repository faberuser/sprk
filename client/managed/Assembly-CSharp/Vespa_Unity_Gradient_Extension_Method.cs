using UnityEngine;

public static class Vespa_Unity_Gradient_Extension_Method
{
	public static Color EVALUATE(this Gradient i_this, float i_frame, Color i_default)
	{
		return i_this?.Evaluate(i_frame) ?? i_default;
	}

	public static void Reset(this Gradient This, Color color)
	{
		This?.SetKeys(new GradientColorKey[1]
		{
			new GradientColorKey(color, 0f)
		}, new GradientAlphaKey[1]
		{
			new GradientAlphaKey(color.a, 0f)
		});
	}
}
