using System;
using System.Collections.Generic;
using System.Globalization;

public static class Vespa_String_Extension_Method2
{
	private static Dictionary<string, DateTime> _time2Cache = new Dictionary<string, DateTime>();

	public static bool TryParseNetworkTime(this string i_str, out DateTime o_dateTime)
	{
		if (String_Extension_Method.IsNullOrEmpty(i_str))
		{
			o_dateTime = DateTime.UtcNow;
			return false;
		}
		return DateTime.TryParseExact(i_str, Vespa_DateTime_Extension_Method.NetworkFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out o_dateTime);
	}

	public static bool TryParseNetworkTime(this string i_str, string format, out DateTime o_dateTime)
	{
		if (String_Extension_Method.IsNullOrEmpty(i_str))
		{
			o_dateTime = DateTime.UtcNow;
			return false;
		}
		return DateTime.TryParseExact(i_str, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out o_dateTime);
	}

	public static bool TryParseNetworkTime2(this string i_str, out DateTime o_dateTime)
	{
		if (String_Extension_Method.IsNullOrEmpty(i_str))
		{
			o_dateTime = DateTime.UtcNow;
			return false;
		}
		if (_time2Cache.TryGetValue(i_str, out o_dateTime))
		{
			return true;
		}
		if (TryParseNetworkTime2NoCache(i_str, out o_dateTime))
		{
			_time2Cache[i_str] = o_dateTime;
			return true;
		}
		return false;
	}

	public static bool TryParseNetworkTime2NoCache(this string i_str, out DateTime o_dateTime)
	{
		if (String_Extension_Method.IsNullOrEmpty(i_str))
		{
			o_dateTime = DateTime.UtcNow;
			return false;
		}
		if (_time2Cache.Count > 100 && _time2Cache.Count > 500)
		{
			_time2Cache.Clear();
		}
		return DateTime.TryParseExact(i_str, Vespa_DateTime_Extension_Method.NetworkFormat2, CultureInfo.InvariantCulture, DateTimeStyles.None, out o_dateTime);
	}

	public static int GetElapsedNetworkTime2(this string i_str)
	{
		TryParseNetworkTime2(i_str, out var o_dateTime);
		return (int)(DateTime.UtcNow - o_dateTime).TotalSeconds;
	}

	public static byte calculateLRC(this string i_str)
	{
		byte[] array = Vespa_String_Extension_Method.ToByteArray(i_str);
		byte b = 0;
		for (int i = 0; i < array.Length; i++)
		{
			b = (byte)((b + array[i]) & 0xFF);
		}
		return (byte)(((b ^ 0xFF) + 1) & 0xFF);
	}
}
