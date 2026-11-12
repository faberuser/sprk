using System;
using System.Globalization;

namespace NShared;

[JsonSerializable]
public class RequestLog
{
	public DateTime ParsedTime;

	public string Service { get; set; }

	public string Argument { get; set; }

	public string Time { get; set; }

	public void Init()
	{
		string format = "yyyy-MM-dd HH:mm:ss";
		_ = DateTime.Now;
		DateTime.TryParseExact(Time, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out ParsedTime);
	}
}
