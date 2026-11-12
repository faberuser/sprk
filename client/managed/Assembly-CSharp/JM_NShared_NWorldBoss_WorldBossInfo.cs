using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NWorldBoss;

public static class JM_NShared_NWorldBoss_WorldBossInfo
{
	public static WorldBossInfo Parse(string s)
	{
		return Parse(s, new WorldBossInfo());
	}

	public static WorldBossInfo Parse(string s, WorldBossInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static WorldBossInfo Parse(IDictionary d)
	{
		return Parse(d, new WorldBossInfo());
	}

	public static WorldBossInfo Parse(IDictionary d, WorldBossInfo o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.Season = BaseJsonMarshaler.ParseValueInt(d, "Season");
		o.MonsterHp0 = BaseJsonMarshaler.ParseValueLong(d, "MonsterHp0");
		o.MaxHp = BaseJsonMarshaler.ParseValueLong(d, "MaxHp");
		o.TotalDamage = BaseJsonMarshaler.ParseValueLong(d, "TotalDamage");
		o.Status = BaseJsonMarshaler.ParseEnum<WorldBossStatus>(d, "Status");
		o.CreatedTime = BaseJsonMarshaler.ParseValueString(d, "CreatedTime");
		o.EndedTime = BaseJsonMarshaler.ParseValueString(d, "EndedTime");
		o.RewardedTime = BaseJsonMarshaler.ParseValueString(d, "RewardedTime");
		o.DailyRewardedTime = BaseJsonMarshaler.ParseValueString(d, "DailyRewardedTime");
		o.DailyRewarding = BaseJsonMarshaler.ParseValueInt(d, "DailyRewarding");
		return o;
	}

	public static string EncodeJsonString(WorldBossInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(WorldBossInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("Season", o.Season.ToString());
		dictionary.Add("MonsterHp0", o.MonsterHp0.ToString());
		dictionary.Add("MaxHp", o.MaxHp.ToString());
		dictionary.Add("TotalDamage", o.TotalDamage.ToString());
		dictionary.Add("Status", o.Status.ToString());
		if (o.CreatedTime != null)
		{
			dictionary.Add("CreatedTime", o.CreatedTime);
		}
		if (o.EndedTime != null)
		{
			dictionary.Add("EndedTime", o.EndedTime);
		}
		if (o.RewardedTime != null)
		{
			dictionary.Add("RewardedTime", o.RewardedTime);
		}
		if (o.DailyRewardedTime != null)
		{
			dictionary.Add("DailyRewardedTime", o.DailyRewardedTime);
		}
		dictionary.Add("DailyRewarding", o.DailyRewarding.ToString());
		return dictionary;
	}
}
