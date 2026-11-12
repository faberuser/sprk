using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildRankingBoardRankerInfo
{
	public static GuildRankingBoardRankerInfo Parse(string s)
	{
		return Parse(s, new GuildRankingBoardRankerInfo());
	}

	public static GuildRankingBoardRankerInfo Parse(string s, GuildRankingBoardRankerInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildRankingBoardRankerInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildRankingBoardRankerInfo());
	}

	public static GuildRankingBoardRankerInfo Parse(IDictionary d, GuildRankingBoardRankerInfo o)
	{
		o.Rank = BaseJsonMarshaler.ParseValueInt(d, "Rank");
		o.Logo = BaseJsonMarshaler.ParseValueByte(d, "Logo");
		o.Level = BaseJsonMarshaler.ParseValueInt(d, "Level");
		o.Name = BaseJsonMarshaler.ParseValueString(d, "Name");
		o.CurrentMemberCount = BaseJsonMarshaler.ParseValueInt(d, "CurrentMemberCount");
		o.Score = BaseJsonMarshaler.ParseValueLong(d, "Score");
		o.GuildId = BaseJsonMarshaler.ParseValueLong(d, "GuildId");
		o.CountryCode = BaseJsonMarshaler.ParseValueString(d, "CountryCode");
		o.ServerGroup = BaseJsonMarshaler.ParseValueString(d, "ServerGroup");
		return o;
	}

	public static string EncodeJsonString(GuildRankingBoardRankerInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildRankingBoardRankerInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Rank", o.Rank.ToString());
		dictionary.Add("Logo", o.Logo.ToString());
		dictionary.Add("Level", o.Level.ToString());
		if (o.Name != null)
		{
			dictionary.Add("Name", o.Name);
		}
		dictionary.Add("CurrentMemberCount", o.CurrentMemberCount.ToString());
		dictionary.Add("Score", o.Score.ToString());
		dictionary.Add("GuildId", o.GuildId.ToString());
		if (o.CountryCode != null)
		{
			dictionary.Add("CountryCode", o.CountryCode);
		}
		if (o.ServerGroup != null)
		{
			dictionary.Add("ServerGroup", o.ServerGroup);
		}
		return dictionary;
	}
}
