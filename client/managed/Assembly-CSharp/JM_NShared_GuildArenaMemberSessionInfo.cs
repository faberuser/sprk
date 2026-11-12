using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildArenaMemberSessionInfo
{
	public static GuildArenaMemberSessionInfo Parse(string s)
	{
		return Parse(s, new GuildArenaMemberSessionInfo());
	}

	public static GuildArenaMemberSessionInfo Parse(string s, GuildArenaMemberSessionInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildArenaMemberSessionInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildArenaMemberSessionInfo());
	}

	public static GuildArenaMemberSessionInfo Parse(IDictionary d, GuildArenaMemberSessionInfo o)
	{
		o.SessionIndex = BaseJsonMarshaler.ParseValueInt(d, "SessionIndex");
		o.ServerGroup = BaseJsonMarshaler.ParseValueString(d, "ServerGroup");
		o.GuildId = BaseJsonMarshaler.ParseValueLong(d, "GuildId");
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.SessionScore = BaseJsonMarshaler.ParseValueInt(d, "SessionScore");
		o.MatchWinCount = BaseJsonMarshaler.ParseValueInt(d, "MatchWinCount");
		o.MatchLoseCount = BaseJsonMarshaler.ParseValueInt(d, "MatchLoseCount");
		o.DefenceWinCount = BaseJsonMarshaler.ParseValueInt(d, "DefenceWinCount");
		o.DefenceLoseCount = BaseJsonMarshaler.ParseValueInt(d, "DefenceLoseCount");
		o.DeckCount = BaseJsonMarshaler.ParseValueInt(d, "DeckCount");
		o.LoseDeck1 = BaseJsonMarshaler.ParseValueInt(d, "LoseDeck1");
		o.LoseDeck2 = BaseJsonMarshaler.ParseValueInt(d, "LoseDeck2");
		o.LoseDeck3 = BaseJsonMarshaler.ParseValueInt(d, "LoseDeck3");
		o.LoseDeck4 = BaseJsonMarshaler.ParseValueInt(d, "LoseDeck4");
		o.LoseDeck5 = BaseJsonMarshaler.ParseValueInt(d, "LoseDeck5");
		o.BlindDeck1 = BaseJsonMarshaler.ParseValueInt(d, "BlindDeck1");
		o.BlindDeck2 = BaseJsonMarshaler.ParseValueInt(d, "BlindDeck2");
		o.BlindDeck3 = BaseJsonMarshaler.ParseValueInt(d, "BlindDeck3");
		o.BlindDeck4 = BaseJsonMarshaler.ParseValueInt(d, "BlindDeck4");
		o.BlindDeck5 = BaseJsonMarshaler.ParseValueInt(d, "BlindDeck5");
		return o;
	}

	public static string EncodeJsonString(GuildArenaMemberSessionInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildArenaMemberSessionInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("SessionIndex", o.SessionIndex.ToString());
		if (o.ServerGroup != null)
		{
			dictionary.Add("ServerGroup", o.ServerGroup);
		}
		dictionary.Add("GuildId", o.GuildId.ToString());
		dictionary.Add("AccountId", o.AccountId.ToString());
		dictionary.Add("SessionScore", o.SessionScore.ToString());
		dictionary.Add("MatchWinCount", o.MatchWinCount.ToString());
		dictionary.Add("MatchLoseCount", o.MatchLoseCount.ToString());
		dictionary.Add("DefenceWinCount", o.DefenceWinCount.ToString());
		dictionary.Add("DefenceLoseCount", o.DefenceLoseCount.ToString());
		dictionary.Add("DeckCount", o.DeckCount.ToString());
		dictionary.Add("LoseDeck1", o.LoseDeck1.ToString());
		dictionary.Add("LoseDeck2", o.LoseDeck2.ToString());
		dictionary.Add("LoseDeck3", o.LoseDeck3.ToString());
		dictionary.Add("LoseDeck4", o.LoseDeck4.ToString());
		dictionary.Add("LoseDeck5", o.LoseDeck5.ToString());
		dictionary.Add("BlindDeck1", o.BlindDeck1.ToString());
		dictionary.Add("BlindDeck2", o.BlindDeck2.ToString());
		dictionary.Add("BlindDeck3", o.BlindDeck3.ToString());
		dictionary.Add("BlindDeck4", o.BlindDeck4.ToString());
		dictionary.Add("BlindDeck5", o.BlindDeck5.ToString());
		return dictionary;
	}
}
