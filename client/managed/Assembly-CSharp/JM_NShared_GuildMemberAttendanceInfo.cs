using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildMemberAttendanceInfo
{
	public static GuildMemberAttendanceInfo Parse(string s)
	{
		return Parse(s, new GuildMemberAttendanceInfo());
	}

	public static GuildMemberAttendanceInfo Parse(string s, GuildMemberAttendanceInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildMemberAttendanceInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildMemberAttendanceInfo());
	}

	public static GuildMemberAttendanceInfo Parse(IDictionary d, GuildMemberAttendanceInfo o)
	{
		o.GuildId = BaseJsonMarshaler.ParseValueLong(d, "GuildId");
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.LastAttendanceTime = BaseJsonMarshaler.ParseValueString(d, "LastAttendanceTime");
		o.Day1 = BaseJsonMarshaler.ParseValueInt(d, "Day1");
		o.Day2 = BaseJsonMarshaler.ParseValueInt(d, "Day2");
		o.Day3 = BaseJsonMarshaler.ParseValueInt(d, "Day3");
		o.Day4 = BaseJsonMarshaler.ParseValueInt(d, "Day4");
		o.Day5 = BaseJsonMarshaler.ParseValueInt(d, "Day5");
		o.Day6 = BaseJsonMarshaler.ParseValueInt(d, "Day6");
		o.Day7 = BaseJsonMarshaler.ParseValueInt(d, "Day7");
		o.Daily = BaseJsonMarshaler.ParseValueInt(d, "Daily");
		o.Weekly = BaseJsonMarshaler.ParseValueInt(d, "Weekly");
		o.Successive = BaseJsonMarshaler.ParseValueInt(d, "Successive");
		o.SuccessiveCount = BaseJsonMarshaler.ParseValueInt(d, "SuccessiveCount");
		o.MaxSuccessiveCount = BaseJsonMarshaler.ParseValueInt(d, "MaxSuccessiveCount");
		o.LastWeekFirstAttendanceTime = BaseJsonMarshaler.ParseValueString(d, "LastWeekFirstAttendanceTime");
		o.UpdatedTime = BaseJsonMarshaler.ParseValueString(d, "UpdatedTime");
		return o;
	}

	public static string EncodeJsonString(GuildMemberAttendanceInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildMemberAttendanceInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GuildId", o.GuildId.ToString());
		dictionary.Add("AccountId", o.AccountId.ToString());
		if (o.LastAttendanceTime != null)
		{
			dictionary.Add("LastAttendanceTime", o.LastAttendanceTime);
		}
		dictionary.Add("Day1", o.Day1.ToString());
		dictionary.Add("Day2", o.Day2.ToString());
		dictionary.Add("Day3", o.Day3.ToString());
		dictionary.Add("Day4", o.Day4.ToString());
		dictionary.Add("Day5", o.Day5.ToString());
		dictionary.Add("Day6", o.Day6.ToString());
		dictionary.Add("Day7", o.Day7.ToString());
		dictionary.Add("Daily", o.Daily.ToString());
		dictionary.Add("Weekly", o.Weekly.ToString());
		dictionary.Add("Successive", o.Successive.ToString());
		dictionary.Add("SuccessiveCount", o.SuccessiveCount.ToString());
		dictionary.Add("MaxSuccessiveCount", o.MaxSuccessiveCount.ToString());
		if (o.LastWeekFirstAttendanceTime != null)
		{
			dictionary.Add("LastWeekFirstAttendanceTime", o.LastWeekFirstAttendanceTime);
		}
		if (o.UpdatedTime != null)
		{
			dictionary.Add("UpdatedTime", o.UpdatedTime);
		}
		return dictionary;
	}
}
