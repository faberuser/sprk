using System;

public static class Vespa_DateTime_Extension_Method
{
	public static string NetworkFormat = "yyyy-MM-dd'T'HH:mm:ss";

	public static string NetworkFormat2 = "yyyy-MM-dd HH:mm:ss";

	public static string ToNetworkString(this DateTime i_dateTime)
	{
		return i_dateTime.ToString(NetworkFormat);
	}

	public static string ToNetworkString2(this DateTime i_dateTime)
	{
		return i_dateTime.ToString(NetworkFormat2);
	}

	public static string ToNetworkString(this DateTime i_dateTime, string Format)
	{
		return i_dateTime.ToString(Format);
	}

	public static DateTime NextDayOfWeek(this DateTime i_dateTime, DayOfWeek i_dayOfWeek)
	{
		int num = i_dayOfWeek - i_dateTime.DayOfWeek;
		if (num <= 0)
		{
			num += 7;
		}
		return i_dateTime.AddDays(num);
	}
}
