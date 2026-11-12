using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NWorldBoss;

public static class JM_NShared_NWorldBoss_WorldBossServerTotalRankingInfo
{
	public static WorldBossServerTotalRankingInfo Parse(string s)
	{
		return Parse(s, new WorldBossServerTotalRankingInfo());
	}

	public static WorldBossServerTotalRankingInfo Parse(string s, WorldBossServerTotalRankingInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static WorldBossServerTotalRankingInfo Parse(IDictionary d)
	{
		return Parse(d, new WorldBossServerTotalRankingInfo());
	}

	public static WorldBossServerTotalRankingInfo Parse(IDictionary d, WorldBossServerTotalRankingInfo o)
	{
		o.Rank = BaseJsonMarshaler.ParseValueInt(d, "Rank");
		o.ServerGroup = BaseJsonMarshaler.ParseValueString(d, "ServerGroup");
		o.Score = BaseJsonMarshaler.ParseValueLong(d, "Score");
		return o;
	}

	public static string EncodeJsonString(WorldBossServerTotalRankingInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(WorldBossServerTotalRankingInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Rank", o.Rank.ToString());
		if (o.ServerGroup != null)
		{
			dictionary.Add("ServerGroup", o.ServerGroup);
		}
		dictionary.Add("Score", o.Score.ToString());
		return dictionary;
	}
}
