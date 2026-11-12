using System;
using System.Globalization;
using NVespa;

namespace NGame2.NUtil.NStringProxy;

public class TimeDurationBuilder : IBuilder
{
	private int _beginHour;

	private int _beginMinute;

	private int _endHour;

	private int _endMinute;

	private bool _appendLine;

	bool IBuilder.AppendLine => _appendLine;

	public TimeDurationBuilder(int beginHour, int beginMinute, int endHour, int endMinute, bool appendLine)
	{
		_beginHour = beginHour;
		_beginMinute = beginMinute;
		_endHour = endHour;
		_endMinute = endMinute;
		_appendLine = appendLine;
	}

	public string Build()
	{
		string localHourString = GetLocalHourString(_beginHour, _beginMinute);
		string localHourString2 = GetLocalHourString(_endHour, _endMinute);
		return $"{localHourString}~{localHourString2}";
	}

	public static string GetLocalHourString(int hour, int minute)
	{
		DateTime now = DateTime.Now;
		DateTime utcNow = DateTime.UtcNow;
		TimeSpan timeSpan = now - utcNow;
		int year = DateTime.Now.Year;
		int month = DateTime.Now.Month;
		int day = DateTime.Now.Day;
		int hour2 = (hour + Convert.ToInt32(timeSpan.TotalHours) + 240) % 24;
		return NVespa.Math.CreateDateTime(year, month, day, hour2, minute, 0).ToString("tt hh:mm", CultureInfo.InvariantCulture);
	}
}
