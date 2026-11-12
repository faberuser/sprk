using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_AttendanceDayInfo
{
	public static AttendanceDayInfo Parse(string s)
	{
		return Parse(s, new AttendanceDayInfo());
	}

	public static AttendanceDayInfo Parse(string s, AttendanceDayInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static AttendanceDayInfo Parse(IDictionary d)
	{
		return Parse(d, new AttendanceDayInfo());
	}

	public static AttendanceDayInfo Parse(IDictionary d, AttendanceDayInfo o)
	{
		o.AttendanceIndex = BaseJsonMarshaler.ParseValueInt(d, "AttendanceIndex");
		o.Day = BaseJsonMarshaler.ParseValueInt(d, "Day");
		return o;
	}

	public static string EncodeJsonString(AttendanceDayInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(AttendanceDayInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AttendanceIndex", o.AttendanceIndex.ToString());
		dictionary.Add("Day", o.Day.ToString());
		return dictionary;
	}
}
