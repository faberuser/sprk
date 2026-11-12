using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_WorldBossPlayInfo
{
	public static WorldBossPlayInfo Parse(string s)
	{
		return Parse(s, new WorldBossPlayInfo());
	}

	public static WorldBossPlayInfo Parse(string s, WorldBossPlayInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static WorldBossPlayInfo Parse(IDictionary d)
	{
		return Parse(d, new WorldBossPlayInfo());
	}

	public static WorldBossPlayInfo Parse(IDictionary d, WorldBossPlayInfo o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.Season = BaseJsonMarshaler.ParseValueInt(d, "Season");
		o.TryCount = BaseJsonMarshaler.ParseValueInt(d, "TryCount");
		o.ResetCount = BaseJsonMarshaler.ParseValueInt(d, "ResetCount");
		o.MaxDamage = BaseJsonMarshaler.ParseValueLong(d, "MaxDamage");
		o.TotalDamage = BaseJsonMarshaler.ParseValueLong(d, "TotalDamage");
		o.MaxDamageTime = BaseJsonMarshaler.ParseValueString(d, "MaxDamageTime");
		o.LastBattleTime = BaseJsonMarshaler.ParseValueString(d, "LastBattleTime");
		o.RewardedTime = BaseJsonMarshaler.ParseValueString(d, "RewardedTime");
		return o;
	}

	public static string EncodeJsonString(WorldBossPlayInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(WorldBossPlayInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("Season", o.Season.ToString());
		dictionary.Add("TryCount", o.TryCount.ToString());
		dictionary.Add("ResetCount", o.ResetCount.ToString());
		dictionary.Add("MaxDamage", o.MaxDamage.ToString());
		dictionary.Add("TotalDamage", o.TotalDamage.ToString());
		if (o.MaxDamageTime != null)
		{
			dictionary.Add("MaxDamageTime", o.MaxDamageTime);
		}
		if (o.LastBattleTime != null)
		{
			dictionary.Add("LastBattleTime", o.LastBattleTime);
		}
		if (o.RewardedTime != null)
		{
			dictionary.Add("RewardedTime", o.RewardedTime);
		}
		return dictionary;
	}
}
