using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildAttendanceRewardData
{
	public static GuildAttendanceRewardData Parse(string s)
	{
		return Parse(s, new GuildAttendanceRewardData());
	}

	public static GuildAttendanceRewardData Parse(string s, GuildAttendanceRewardData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildAttendanceRewardData Parse(IDictionary d)
	{
		return Parse(d, new GuildAttendanceRewardData());
	}

	public static GuildAttendanceRewardData Parse(IDictionary d, GuildAttendanceRewardData o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.Day = BaseJsonMarshaler.ParseValueInt(d, "Day");
		o.Step = BaseJsonMarshaler.ParseValueInt(d, "Step");
		o.RewardIndex = BaseJsonMarshaler.ParseValueInt(d, "RewardIndex");
		return o;
	}

	public static string EncodeJsonString(GuildAttendanceRewardData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildAttendanceRewardData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("Day", o.Day.ToString());
		dictionary.Add("Step", o.Step.ToString());
		dictionary.Add("RewardIndex", o.RewardIndex.ToString());
		return dictionary;
	}
}
