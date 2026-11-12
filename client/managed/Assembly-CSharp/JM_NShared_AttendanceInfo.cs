using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_AttendanceInfo
{
	public static AttendanceInfo Parse(string s)
	{
		return Parse(s, new AttendanceInfo());
	}

	public static AttendanceInfo Parse(string s, AttendanceInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static AttendanceInfo Parse(IDictionary d)
	{
		return Parse(d, new AttendanceInfo());
	}

	public static AttendanceInfo Parse(IDictionary d, AttendanceInfo o)
	{
		o.AttendanceIndex = BaseJsonMarshaler.ParseValueInt(d, "AttendanceIndex");
		o.CompletedDay = BaseJsonMarshaler.ParseValueInt(d, "CompletedDay");
		o.CompletedTime = BaseJsonMarshaler.ParseValueString(d, "CompletedTime");
		o.EndTime = BaseJsonMarshaler.ParseValueString(d, "EndTime");
		o.LastRewardedDay = BaseJsonMarshaler.ParseValueInt(d, "LastRewardedDay");
		o.SuspendEndTime = BaseJsonMarshaler.ParseValueString(d, "SuspendEndTime");
		return o;
	}

	public static string EncodeJsonString(AttendanceInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(AttendanceInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AttendanceIndex", o.AttendanceIndex.ToString());
		dictionary.Add("CompletedDay", o.CompletedDay.ToString());
		if (o.CompletedTime != null)
		{
			dictionary.Add("CompletedTime", o.CompletedTime);
		}
		if (o.EndTime != null)
		{
			dictionary.Add("EndTime", o.EndTime);
		}
		dictionary.Add("LastRewardedDay", o.LastRewardedDay.ToString());
		if (o.SuspendEndTime != null)
		{
			dictionary.Add("SuspendEndTime", o.SuspendEndTime);
		}
		return dictionary;
	}
}
