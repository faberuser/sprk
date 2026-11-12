using System.Globalization;
using NShared;
using UnityEngine;

public static class Vespa_Float_Extension_Method
{
	public static float NoneZero(this float value)
	{
		if (Mathf.Approximately(value, 0f))
		{
			return Mathf.Sign(value) * Mathf.Epsilon;
		}
		return value;
	}

	public static string ToCommaString(this float This)
	{
		CultureInfo cultureInfo = GameRoot.Instance.LocalizationValueDataContainer.GetCultureInfo();
		return This.ToString("#,#0.0", cultureInfo);
	}

	public static string ToExpString(this float value)
	{
		return $"{value:F1}%";
	}

	public static string ToStringF3(this float value)
	{
		return $"{value:F3}%";
	}
}
