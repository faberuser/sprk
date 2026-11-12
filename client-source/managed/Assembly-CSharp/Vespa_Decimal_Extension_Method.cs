using UnityEngine;

public static class Vespa_Decimal_Extension_Method
{
	private static readonly string[] SizeSuffix = new string[4] { "Byte", "KB", "MB", "GB" };

	public static string ToSizeString(this decimal This)
	{
		decimal num = This;
		int num2 = 0;
		for (num2 = 0; num2 < SizeSuffix.Length - 1; num2++)
		{
			if (num < 0m)
			{
				if (num > -1000m)
				{
					break;
				}
			}
			else if (num < 1000m)
			{
				break;
			}
			num /= 1000m;
		}
		return $"{num:F1}{SizeSuffix[num2]}";
	}

	public static decimal NoneZero(this decimal value)
	{
		if (Mathf.Approximately((float)value, 0f))
		{
			if (value < 0m)
			{
				return -(decimal)Mathf.Epsilon;
			}
			return (decimal)Mathf.Epsilon;
		}
		return value;
	}
}
