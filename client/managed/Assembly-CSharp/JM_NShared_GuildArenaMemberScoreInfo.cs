using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildArenaMemberScoreInfo
{
	public static GuildArenaMemberScoreInfo Parse(string s)
	{
		return Parse(s, new GuildArenaMemberScoreInfo());
	}

	public static GuildArenaMemberScoreInfo Parse(string s, GuildArenaMemberScoreInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildArenaMemberScoreInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildArenaMemberScoreInfo());
	}

	public static GuildArenaMemberScoreInfo Parse(IDictionary d, GuildArenaMemberScoreInfo o)
	{
		o.TotalBattleCount = BaseJsonMarshaler.ParseValueInt(d, "TotalBattleCount");
		o.ScoreRank = BaseJsonMarshaler.ParseValueInt(d, "ScoreRank");
		o.MatchWinCount = BaseJsonMarshaler.ParseValueInt(d, "MatchWinCount");
		o.DefenceWinCount = BaseJsonMarshaler.ParseValueInt(d, "DefenceWinCount");
		o.WinCount = BaseJsonMarshaler.ParseValueInt(d, "WinCount");
		o.LoseCount = BaseJsonMarshaler.ParseValueInt(d, "LoseCount");
		o.SessionNumber = BaseJsonMarshaler.ParseValueInt(d, "SessionNumber");
		o.SessionScore = BaseJsonMarshaler.ParseValueInt(d, "SessionScore");
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.ServerGroup = BaseJsonMarshaler.ParseValueString(d, "ServerGroup");
		o.Rank = BaseJsonMarshaler.ParseValueByte(d, "Rank");
		o.Nick = BaseJsonMarshaler.ParseValueString(d, "Nick");
		o.TeamLevel = BaseJsonMarshaler.ParseValueInt(d, "TeamLevel");
		o.AvatarHeroIndex = BaseJsonMarshaler.ParseValueInt(d, "AvatarHeroIndex");
		o.CountryCode = BaseJsonMarshaler.ParseValueString(d, "CountryCode");
		return o;
	}

	public static string EncodeJsonString(GuildArenaMemberScoreInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildArenaMemberScoreInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("TotalBattleCount", o.TotalBattleCount.ToString());
		dictionary.Add("ScoreRank", o.ScoreRank.ToString());
		dictionary.Add("MatchWinCount", o.MatchWinCount.ToString());
		dictionary.Add("DefenceWinCount", o.DefenceWinCount.ToString());
		dictionary.Add("WinCount", o.WinCount.ToString());
		dictionary.Add("LoseCount", o.LoseCount.ToString());
		dictionary.Add("SessionNumber", o.SessionNumber.ToString());
		dictionary.Add("SessionScore", o.SessionScore.ToString());
		dictionary.Add("AccountId", o.AccountId.ToString());
		if (o.ServerGroup != null)
		{
			dictionary.Add("ServerGroup", o.ServerGroup);
		}
		dictionary.Add("Rank", o.Rank.ToString());
		if (o.Nick != null)
		{
			dictionary.Add("Nick", o.Nick);
		}
		dictionary.Add("TeamLevel", o.TeamLevel.ToString());
		dictionary.Add("AvatarHeroIndex", o.AvatarHeroIndex.ToString());
		if (o.CountryCode != null)
		{
			dictionary.Add("CountryCode", o.CountryCode);
		}
		return dictionary;
	}
}
