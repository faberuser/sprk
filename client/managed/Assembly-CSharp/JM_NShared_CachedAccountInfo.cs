using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_CachedAccountInfo
{
	public static CachedAccountInfo Parse(string s)
	{
		return Parse(s, new CachedAccountInfo());
	}

	public static CachedAccountInfo Parse(string s, CachedAccountInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static CachedAccountInfo Parse(IDictionary d)
	{
		return Parse(d, new CachedAccountInfo());
	}

	public static CachedAccountInfo Parse(IDictionary d, CachedAccountInfo o)
	{
		o.UserInfo = BaseJsonMarshaler.ParseObject(d, "UserInfo", JM_NShared_CachedUserInfo.Parse);
		o.HeroInfos = BaseJsonMarshaler.ParseDictionary<int, CachedHeroInfo, Dictionary<int, CachedHeroInfo>>(d, "HeroInfos", JM_NShared_CachedHeroInfo.Parse);
		o.AiHeroInfos = BaseJsonMarshaler.ParseDictionary<int, CachedHeroInfo, Dictionary<int, CachedHeroInfo>>(d, "AiHeroInfos", JM_NShared_CachedHeroInfo.Parse);
		o.GroupHeroInfos = BaseJsonMarshaler.ParseDictionary<int, CachedHeroInfo, Dictionary<int, CachedHeroInfo>>(d, "GroupHeroInfos", JM_NShared_CachedHeroInfo.Parse);
		o.DeckInfos = BaseJsonMarshaler.ParseDictionary<BattleType, DeckInfo, Dictionary<BattleType, DeckInfo>>(d, "DeckInfos", JM_NShared_DeckInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(CachedAccountInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(CachedAccountInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("UserInfo", JM_NShared_CachedUserInfo.EncodeJsonObject(o.UserInfo));
		dictionary.Add("HeroInfos", BaseJsonMarshaler.EncodeDictionary(o.HeroInfos, JM_NShared_CachedHeroInfo.EncodeJsonObject));
		dictionary.Add("AiHeroInfos", BaseJsonMarshaler.EncodeDictionary(o.AiHeroInfos, JM_NShared_CachedHeroInfo.EncodeJsonObject));
		dictionary.Add("GroupHeroInfos", BaseJsonMarshaler.EncodeDictionary(o.GroupHeroInfos, JM_NShared_CachedHeroInfo.EncodeJsonObject));
		dictionary.Add("DeckInfos", BaseJsonMarshaler.EncodeDictionary(o.DeckInfos, JM_NShared_DeckInfo.EncodeJsonObject));
		return dictionary;
	}
}
