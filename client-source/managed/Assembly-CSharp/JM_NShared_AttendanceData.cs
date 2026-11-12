using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_AttendanceData
{
	public static AttendanceData Parse(string s)
	{
		return Parse(s, new AttendanceData());
	}

	public static AttendanceData Parse(string s, AttendanceData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static AttendanceData Parse(IDictionary d)
	{
		return Parse(d, new AttendanceData());
	}

	public static AttendanceData Parse(IDictionary d, AttendanceData o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.StartDate = BaseJsonMarshaler.ParseValueString(d, "StartDate");
		o.EndDate = BaseJsonMarshaler.ParseValueString(d, "EndDate");
		o.Keyword = BaseJsonMarshaler.ParseEnum<EAttendanceKeyword>(d, "Keyword");
		o.Title = BaseJsonMarshaler.ParseValueString(d, "Title");
		o.ManualStart = BaseJsonMarshaler.ParseValueByte(d, "ManualStart");
		o.RewardEndDay = BaseJsonMarshaler.ParseValueByte(d, "RewardEndDay");
		o.Repeatable = BaseJsonMarshaler.ParseValueInt(d, "Repeatable");
		o.PageSize = BaseJsonMarshaler.ParseValueInt(d, "PageSize");
		o.InitialPage = BaseJsonMarshaler.ParseValueInt(d, "InitialPage");
		o.OpenLevel = BaseJsonMarshaler.ParseValueInt(d, "OpenLevel");
		o.NextAttendanceIndex = BaseJsonMarshaler.ParseValueInt(d, "NextAttendanceIndex");
		o.ApplyAttendanceTime = BaseJsonMarshaler.ParseValueString(d, "ApplyAttendanceTime");
		o.GroupIndex = BaseJsonMarshaler.ParseValueInt(d, "GroupIndex");
		o.Reward = BaseJsonMarshaler.ParseArray(d, "Reward", JM_NShared_AttendanceRewards.Parse);
		o.AttendanceRewardableDays = BaseJsonMarshaler.ParseValueInt(d, "AttendanceRewardableDays");
		o.SendMail = BaseJsonMarshaler.ParseValueInt(d, "SendMail");
		return o;
	}

	public static string EncodeJsonString(AttendanceData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(AttendanceData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		if (o.StartDate != null)
		{
			dictionary.Add("StartDate", o.StartDate);
		}
		if (o.EndDate != null)
		{
			dictionary.Add("EndDate", o.EndDate);
		}
		dictionary.Add("Keyword", o.Keyword.ToString());
		if (o.Title != null)
		{
			dictionary.Add("Title", o.Title);
		}
		dictionary.Add("ManualStart", o.ManualStart.ToString());
		dictionary.Add("RewardEndDay", o.RewardEndDay.ToString());
		dictionary.Add("Repeatable", o.Repeatable.ToString());
		dictionary.Add("PageSize", o.PageSize.ToString());
		dictionary.Add("InitialPage", o.InitialPage.ToString());
		dictionary.Add("OpenLevel", o.OpenLevel.ToString());
		dictionary.Add("NextAttendanceIndex", o.NextAttendanceIndex.ToString());
		if (o.ApplyAttendanceTime != null)
		{
			dictionary.Add("ApplyAttendanceTime", o.ApplyAttendanceTime);
		}
		dictionary.Add("GroupIndex", o.GroupIndex.ToString());
		dictionary.Add("Reward", BaseJsonMarshaler.EncodeArray(o.Reward, JM_NShared_AttendanceRewards.EncodeJsonObject));
		dictionary.Add("AttendanceRewardableDays", o.AttendanceRewardableDays.ToString());
		dictionary.Add("SendMail", o.SendMail.ToString());
		return dictionary;
	}
}
