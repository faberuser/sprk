using UnityEngine;

public static class Vespa_Unity_Color_Extension_Method
{
	public static Color Alpha(this Color i_color, float i_alpha)
	{
		return new Color(i_color.r, i_color.g, i_color.b, i_alpha);
	}

	public static Color MultiplyAlpha(this Color i_color, float i_alpha)
	{
		return new Color(i_color.r, i_color.g, i_color.b, i_color.a * i_alpha);
	}

	public static float Brightness(this Color i_color)
	{
		return i_color.r * 0.3f + i_color.g * 0.59f + i_color.b * 0.11f;
	}

	public static Color Lerp(this Color i_color, Color i_target, float i_t)
	{
		return new Color(Mathf.Lerp(i_color.r, i_target.r, i_t), Mathf.Lerp(i_color.g, i_target.g, i_t), Mathf.Lerp(i_color.b, i_target.b, i_t), Mathf.Lerp(i_color.a, i_target.a, i_t));
	}

	public static string ToHex(this Color i_color)
	{
		return NGUIText.EncodeColor32(i_color);
	}
}
