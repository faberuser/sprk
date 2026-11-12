using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetGuildRaidGuildRanking;

public static class JM_NShared_GetGuildRaidGuildRanking_GuildRankingInfo
{
	public static GuildRankingInfo Parse(string s)
	{
		return Parse(s, new GuildRankingInfo());
	}

	public static GuildRankingInfo Parse(string s, GuildRankingInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildRankingInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildRankingInfo());
	}

	public static GuildRankingInfo Parse(IDictionary d, GuildRankingInfo o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.Difficulty = BaseJsonMarshaler.ParseValueInt(d, "Difficulty");
		o.GuildId = BaseJsonMarshaler.ParseValueInt(d, "GuildId");
		o.GuildName = BaseJsonMarshaler.ParseValueString(d, "GuildName");
		o.GuildLogo = BaseJsonMarshaler.ParseValueInt(d, "GuildLogo");
		o.ClearTime = BaseJsonMarshaler.ParseValueInt(d, "ClearTime");
		return o;
	}

	public static string EncodeJsonString(GuildRankingInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildRankingInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("Difficulty", o.Difficulty.ToString());
		dictionary.Add("GuildId", o.GuildId.ToString());
		if (o.GuildName != null)
		{
			dictionary.Add("GuildName", o.GuildName);
		}
		dictionary.Add("GuildLogo", o.GuildLogo.ToString());
		dictionary.Add("ClearTime", o.ClearTime.ToString());
		return dictionary;
	}
}
