using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GuildArenaMemberDeckInfo
{
	public static GuildArenaMemberDeckInfo Parse(string s)
	{
		return Parse(s, new GuildArenaMemberDeckInfo());
	}

	public static GuildArenaMemberDeckInfo Parse(string s, GuildArenaMemberDeckInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuildArenaMemberDeckInfo Parse(IDictionary d)
	{
		return Parse(d, new GuildArenaMemberDeckInfo());
	}

	public static GuildArenaMemberDeckInfo Parse(IDictionary d, GuildArenaMemberDeckInfo o)
	{
		o.DeckCount = BaseJsonMarshaler.ParseValueInt(d, "DeckCount");
		o.DefeatDeck = BaseJsonMarshaler.ParseArrayCast(d, "DefeatDeck", BaseJsonMarshaler.CastToInt);
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.ServerGroup = BaseJsonMarshaler.ParseValueString(d, "ServerGroup");
		o.Rank = BaseJsonMarshaler.ParseValueByte(d, "Rank");
		o.Nick = BaseJsonMarshaler.ParseValueString(d, "Nick");
		o.TeamLevel = BaseJsonMarshaler.ParseValueInt(d, "TeamLevel");
		o.AvatarHeroIndex = BaseJsonMarshaler.ParseValueInt(d, "AvatarHeroIndex");
		o.CountryCode = BaseJsonMarshaler.ParseValueString(d, "CountryCode");
		return o;
	}

	public static string EncodeJsonString(GuildArenaMemberDeckInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuildArenaMemberDeckInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("DeckCount", o.DeckCount.ToString());
		dictionary.Add("DefeatDeck", BaseJsonMarshaler.EncodeArray(o.DefeatDeck));
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
