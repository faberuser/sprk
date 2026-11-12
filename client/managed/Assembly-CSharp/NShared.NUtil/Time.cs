using System;

namespace NShared.NUtil;

public class Time
{
	public static TimeSpan GetEpochTime()
	{
		return DateTime.UtcNow - new DateTime(1970, 1, 1);
	}

	public static long GetEpochTimeMs()
	{
		return (long)GetEpochTime().TotalMilliseconds;
	}

	public static int GetMonthFromMillennium()
	{
		int year = DateTime.Now.Year;
		int month = DateTime.Now.Month;
		return (year - 2000) * 12 + (month - 1);
	}
}
