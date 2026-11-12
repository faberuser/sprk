using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_MatchRankerInfo
{
	public static MatchRankerInfo Parse(string s)
	{
		return Parse(s, new MatchRankerInfo());
	}

	public static MatchRankerInfo Parse(string s, MatchRankerInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static MatchRankerInfo Parse(IDictionary d)
	{
		return Parse(d, new MatchRankerInfo());
	}

	public static MatchRankerInfo Parse(IDictionary d, MatchRankerInfo o)
	{
		o.Rank = BaseJsonMarshaler.ParseValueInt(d, "Rank");
		o.MatchScore = BaseJsonMarshaler.ParseValueInt(d, "MatchScore");
		o.CountryCode = BaseJsonMarshaler.ParseValueString(d, "CountryCode");
		o.UserInfo = BaseJsonMarshaler.ParseObject(d, "UserInfo", JM_NShared_CachedUserInfo.Parse);
		o.HeroInfos = BaseJsonMarshaler.ParseArray(d, "HeroInfos", JM_NShared_DeckHeroInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(MatchRankerInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(MatchRankerInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Rank", o.Rank.ToString());
		dictionary.Add("MatchScore", o.MatchScore.ToString());
		if (o.CountryCode != null)
		{
			dictionary.Add("CountryCode", o.CountryCode);
		}
		dictionary.Add("UserInfo", JM_NShared_CachedUserInfo.EncodeJsonObject(o.UserInfo));
		dictionary.Add("HeroInfos", BaseJsonMarshaler.EncodeArray(o.HeroInfos, JM_NShared_DeckHeroInfo.EncodeJsonObject));
		return dictionary;
	}
}
