using NVespa;

public static class Vespa_CurveCombo_Extension_Method
{
	public static float EVALUATE(this CurveCombo i_this, float i_frame, float i_default)
	{
		return i_this?.Evaluate(i_frame, i_default) ?? i_default;
	}
}
