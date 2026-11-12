using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildAttendanceInfo
{
	public static GuildAttendanceInfo Parse(string s)
	{
		return Parse(s, new GuildAttendanceInfo());
	}

	public static GuildAttendanceInfo Parse(string s, GuildAttendanceInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildAttendanceInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildAttendanceInfo());
	}

	public static GuildAttendanceInfo Parse(IDictionary d, GuildAttendanceInfo o)
	{
		o.GuildId = BaseJsonMarshaler.ParseValueLong(d, "GuildId");
		o.Daily = BaseJsonMarshaler.ParseValueInt(d, "Daily");
		o.Weekly = BaseJsonMarshaler.ParseValueInt(d, "Weekly");
		o.UpdatedTime = BaseJsonMarshaler.ParseValueString(d, "UpdatedTime");
		o.LastDaily = BaseJsonMarshaler.ParseValueInt(d, "LastDaily");
		o.LastDailyUpdatedTime = BaseJsonMarshaler.ParseValueString(d, "LastDailyUpdatedTime");
		o.LastWeekly = BaseJsonMarshaler.ParseValueInt(d, "LastWeekly");
		o.LastWeeklyUpdatedTime = BaseJsonMarshaler.ParseValueString(d, "LastWeeklyUpdatedTime");
		return o;
	}

	public static string EncodeJsonString(GuildAttendanceInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildAttendanceInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GuildId", o.GuildId.ToString());
		dictionary.Add("Daily", o.Daily.ToString());
		dictionary.Add("Weekly", o.Weekly.ToString());
		if (o.UpdatedTime != null)
		{
			dictionary.Add("UpdatedTime", o.UpdatedTime);
		}
		dictionary.Add("LastDaily", o.LastDaily.ToString());
		if (o.LastDailyUpdatedTime != null)
		{
			dictionary.Add("LastDailyUpdatedTime", o.LastDailyUpdatedTime);
		}
		dictionary.Add("LastWeekly", o.LastWeekly.ToString());
		if (o.LastWeeklyUpdatedTime != null)
		{
			dictionary.Add("LastWeeklyUpdatedTime", o.LastWeeklyUpdatedTime);
		}
		return dictionary;
	}
}
