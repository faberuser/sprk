using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_WorldBossDailyScore
{
	public static WorldBossDailyScore Parse(string s)
	{
		return Parse(s, new WorldBossDailyScore());
	}

	public static WorldBossDailyScore Parse(string s, WorldBossDailyScore o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static WorldBossDailyScore Parse(IDictionary d)
	{
		return Parse(d, new WorldBossDailyScore());
	}

	public static WorldBossDailyScore Parse(IDictionary d, WorldBossDailyScore o)
	{
		o.WorldBossIndex = BaseJsonMarshaler.ParseValueInt(d, "WorldBossIndex");
		o.Season = BaseJsonMarshaler.ParseValueInt(d, "Season");
		o.Score = BaseJsonMarshaler.ParseValueLong(d, "Score");
		o.IsGetDailyParticipantReward = BaseJsonMarshaler.ParseValueInt(d, "IsGetDailyParticipantReward");
		o.GetDailyAchievementStep = BaseJsonMarshaler.ParseValueInt(d, "GetDailyAchievementStep");
		o.UpdateTime = BaseJsonMarshaler.ParseValueString(d, "UpdateTime");
		return o;
	}

	public static string EncodeJsonString(WorldBossDailyScore o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(WorldBossDailyScore o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("WorldBossIndex", o.WorldBossIndex.ToString());
		dictionary.Add("Season", o.Season.ToString());
		dictionary.Add("Score", o.Score.ToString());
		dictionary.Add("IsGetDailyParticipantReward", o.IsGetDailyParticipantReward.ToString());
		dictionary.Add("GetDailyAchievementStep", o.GetDailyAchievementStep.ToString());
		if (o.UpdateTime != null)
		{
			dictionary.Add("UpdateTime", o.UpdateTime);
		}
		return dictionary;
	}
}
