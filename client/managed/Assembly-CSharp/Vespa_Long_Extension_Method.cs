using System;
using System.Globalization;
using NShared;
using NVespa;

public static class Vespa_Long_Extension_Method
{
	private const string KILLO_UNIT = "k";

	private const string MILLION_UNIT = "m";

	private const string BILLION_UNIT = "b";

	private const string TRILLION_UNIT = "t";

	private const long KILLO = 1000L;

	private const long MILLION = 1000000L;

	private const long BILLION = 1000000000L;

	private const long TRILLION = 1000000000000L;

	private const long KILLO_HIGHER = 10000L;

	private const long MILLION_HIGHER = 10000000L;

	private const long BILLION_HIGHER = 10000000000L;

	private const long TRILLION_HIGHER = 10000000000000L;

	private const long KILLO_LOWER = 100L;

	private const long MILLION_LOWER = 100000L;

	private const long BILLION_LOWER = 100000000L;

	private const long TRILLION_LOWER = 100000000000L;

	private static readonly string[] SizeSuffix = new string[4] { "Byte", "KB", "MB", "GB" };

	public static string Shorten(this long number)
	{
		if (number >= 10000000000000L)
		{
			number /= 100000000000L;
		}
		else if (number >= 10000000000L)
		{
			number /= 100000000;
		}
		else if (number >= 10000000)
		{
			number /= 100000;
		}
		else if (number >= 10000)
		{
			number /= 100;
		}
		return number.ToString();
	}

	public static string ShortenCommaUnit(this long number)
	{
		if (number == 0L)
		{
			return Vespa_Int_Extension_Method.ToStringCache(0);
		}
		if (!SystemOptionData.ShowDamageUnit)
		{
			return ToCommaString(number);
		}
		string arg = null;
		if (number >= 10000000000000L)
		{
			number /= 100000000000L;
			arg = "t";
		}
		else if (number >= 10000000000L)
		{
			number /= 100000000;
			arg = "b";
		}
		else if (number >= 10000000)
		{
			number /= 100000;
			arg = "m";
		}
		else
		{
			if (number < 10000)
			{
				return $"{ToCommaString(number)}{arg}";
			}
			number /= 100;
			arg = "k";
		}
		if (number >= 1000)
		{
			number /= 10;
			return $"{ToCommaString(number)}{arg}";
		}
		float num = (float)number / 10f;
		return $"{Vespa_Float_Extension_Method.ToCommaString(num)}{arg}";
	}

	public static string ShortenUnit(this long number)
	{
		string arg = null;
		if (number >= 10000000000000L)
		{
			number /= 100000000000L;
			arg = "t";
		}
		else if (number >= 10000000000L)
		{
			number /= 100000000;
			arg = "b";
		}
		else if (number >= 10000000)
		{
			number /= 100000;
			arg = "m";
		}
		else if (number >= 10000)
		{
			number /= 100;
			arg = "k";
		}
		if (number >= 1000)
		{
			number /= 10;
			return $"{number.ToString()}{arg}";
		}
		_ = (float)number / 10f;
		return $"{number.ToString()}{arg}";
	}

	public static string WithDamageUnit(this long number)
	{
		if (!SystemOptionData.ShowDamageUnit)
		{
			return ToCommaString(number);
		}
		return ShortenCommaUnit(number);
	}

	public static string ToStringCache(this long i_value)
	{
		return NVespa.ToStringCache.LongToString(i_value);
	}

	public static string ToCommaString(this long This)
	{
		CultureInfo cultureInfo = GameRoot.Instance.LocalizationValueDataContainer.GetCultureInfo();
		return This.ToString("#,##0", cultureInfo);
	}

	public static string ToSizeString(this long This)
	{
		return Vespa_Decimal_Extension_Method.ToSizeString(This);
	}

	public static string LongToByteFormat(this long This)
	{
		float num = (float)This / 1024f;
		float value = num / 1024f;
		if (System.Math.Abs(value) >= 1000f)
		{
			return value.ToString("#,##0") + " MB";
		}
		if ((double)System.Math.Abs(value) >= 0.1)
		{
			return value.ToString("0.#") + " MB";
		}
		if ((double)System.Math.Abs(num) >= 0.1)
		{
			return num.ToString("0.#") + " KB";
		}
		return ((double)System.Math.Sign(This) * 0.1).ToString("0.#") + " KB";
	}
}
