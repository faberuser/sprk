using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildRankingBoardServerTotalRankingInfo
{
	public static GuildRankingBoardServerTotalRankingInfo Parse(string s)
	{
		return Parse(s, new GuildRankingBoardServerTotalRankingInfo());
	}

	public static GuildRankingBoardServerTotalRankingInfo Parse(string s, GuildRankingBoardServerTotalRankingInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildRankingBoardServerTotalRankingInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildRankingBoardServerTotalRankingInfo());
	}

	public static GuildRankingBoardServerTotalRankingInfo Parse(IDictionary d, GuildRankingBoardServerTotalRankingInfo o)
	{
		o.Rank = BaseJsonMarshaler.ParseValueInt(d, "Rank");
		o.ServerGroup = BaseJsonMarshaler.ParseValueString(d, "ServerGroup");
		o.Score = BaseJsonMarshaler.ParseValueLong(d, "Score");
		return o;
	}

	public static string EncodeJsonString(GuildRankingBoardServerTotalRankingInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildRankingBoardServerTotalRankingInfo o)
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
