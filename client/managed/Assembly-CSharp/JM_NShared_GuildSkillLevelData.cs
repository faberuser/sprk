using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildSkillLevelData
{
	public static GuildSkillLevelData Parse(string s)
	{
		return Parse(s, new GuildSkillLevelData());
	}

	public static GuildSkillLevelData Parse(string s, GuildSkillLevelData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildSkillLevelData Parse(IDictionary d)
	{
		return Parse(d, new GuildSkillLevelData());
	}

	public static GuildSkillLevelData Parse(IDictionary d, GuildSkillLevelData o)
	{
		o.ActivityPoint = BaseJsonMarshaler.ParseValueInt(d, "ActivityPoint");
		o.TotalActivityPoint = BaseJsonMarshaler.ParseValueInt(d, "TotalActivityPoint");
		o.Values = BaseJsonMarshaler.ParseArrayCast(d, "Values", BaseJsonMarshaler.CastToInt);
		o.SuppressPoint = BaseJsonMarshaler.ParseValueInt(d, "SuppressPoint");
		o.TotalSuppressPoint = BaseJsonMarshaler.ParseValueInt(d, "TotalSuppressPoint");
		o.TrainingCenterLevel = BaseJsonMarshaler.ParseValueInt(d, "TrainingCenterLevel");
		o.GuildSkillIndex = BaseJsonMarshaler.ParseValueInt(d, "GuildSkillIndex");
		o.SkillIndex = BaseJsonMarshaler.ParseValueInt(d, "SkillIndex");
		o.GuildSkillLevel = BaseJsonMarshaler.ParseValueByte(d, "GuildSkillLevel");
		return o;
	}

	public static string EncodeJsonString(GuildSkillLevelData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildSkillLevelData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ActivityPoint", o.ActivityPoint.ToString());
		dictionary.Add("TotalActivityPoint", o.TotalActivityPoint.ToString());
		dictionary.Add("Values", BaseJsonMarshaler.EncodeArray(o.Values));
		dictionary.Add("SuppressPoint", o.SuppressPoint.ToString());
		dictionary.Add("TotalSuppressPoint", o.TotalSuppressPoint.ToString());
		dictionary.Add("TrainingCenterLevel", o.TrainingCenterLevel.ToString());
		dictionary.Add("GuildSkillIndex", o.GuildSkillIndex.ToString());
		dictionary.Add("SkillIndex", o.SkillIndex.ToString());
		dictionary.Add("GuildSkillLevel", o.GuildSkillLevel.ToString());
		return dictionary;
	}
}
