using System.Globalization;
using System.Text;
using NShared;
using NVespa;

public static class Vespa_Int_Extension_Method
{
	public static string ToStringCache(this int i_value)
	{
		return NVespa.ToStringCache.IntToString(i_value);
	}

	public static string ToCommaString(this long i_value)
	{
		CultureInfo cultureInfo = GameRoot.Instance.LocalizationValueDataContainer.GetCultureInfo();
		return i_value.ToString("#,##0", cultureInfo);
	}

	public static string ToSizeString(this int This)
	{
		return Vespa_Decimal_Extension_Method.ToSizeString(This);
	}

	public static int Lerp(this int a, int b, float t)
	{
		return a + (int)((float)(b - a) * t);
	}

	public static string ToStringLineUp(this int[] i_value, char separator)
	{
		if (i_value == null || i_value.Length == 0)
		{
			return string.Empty;
		}
		if (i_value.Length == 1)
		{
			return i_value[0].ToString();
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < i_value.Length; i++)
		{
			stringBuilder.Append(i_value[i]);
			if (i != i_value.Length - 1)
			{
				stringBuilder.Append(separator);
			}
		}
		return stringBuilder.ToString();
	}
}
