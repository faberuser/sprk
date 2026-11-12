using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_AchievementInfo
{
	public static AchievementInfo Parse(string s)
	{
		return Parse(s, new AchievementInfo());
	}

	public static AchievementInfo Parse(string s, AchievementInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static AchievementInfo Parse(IDictionary d)
	{
		return Parse(d, new AchievementInfo());
	}

	public static AchievementInfo Parse(IDictionary d, AchievementInfo o)
	{
		o.AchievementIndex = BaseJsonMarshaler.ParseValueInt(d, "AchievementIndex");
		o.LastStep = BaseJsonMarshaler.ParseValueInt(d, "LastStep");
		o.Achievement = BaseJsonMarshaler.ParseValueLong(d, "Achievement");
		o.ClearTeamLevel = BaseJsonMarshaler.ParseValueInt(d, "ClearTeamLevel");
		o.UpdatedTime = BaseJsonMarshaler.ParseValueString(d, "UpdatedTime");
		return o;
	}

	public static string EncodeJsonString(AchievementInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(AchievementInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("AchievementIndex", o.AchievementIndex.ToString());
		dictionary.Add("LastStep", o.LastStep.ToString());
		dictionary.Add("Achievement", o.Achievement.ToString());
		dictionary.Add("ClearTeamLevel", o.ClearTeamLevel.ToString());
		if (o.UpdatedTime != null)
		{
			dictionary.Add("UpdatedTime", o.UpdatedTime);
		}
		return dictionary;
	}
}
