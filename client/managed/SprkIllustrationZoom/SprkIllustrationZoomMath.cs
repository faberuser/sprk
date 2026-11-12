using System;

public static class SprkIllustrationZoomMath
{
	public static float NextScale(float current, float wheel)
	{
		if (float.IsNaN(current) || float.IsInfinity(current) || current <= 0f)
		{
			current = 1f;
		}
		if (float.IsNaN(wheel) || float.IsInfinity(wheel))
		{
			wheel = 0f;
		}
		wheel = Math.Max(-20f, Math.Min(20f, wheel));
		return (float)Math.Max(0.5, Math.Min(8.0, (double)current * Math.Pow(1.15, wheel * 10f)));
	}

	public static float ClampOffset(float offset, float scale, float imageSize, float viewportSize)
	{
		float num = Math.Abs(scale * imageSize - viewportSize) * 0.5f;
		return Math.Max(0f - num, Math.Min(num, offset));
	}
}
