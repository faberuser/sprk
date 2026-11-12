using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_PartyDeckHeroInfo
{
	public static PartyDeckHeroInfo Parse(string s)
	{
		return Parse(s, new PartyDeckHeroInfo());
	}

	public static PartyDeckHeroInfo Parse(string s, PartyDeckHeroInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PartyDeckHeroInfo Parse(IDictionary d)
	{
		return Parse(d, new PartyDeckHeroInfo());
	}

	public static PartyDeckHeroInfo Parse(IDictionary d, PartyDeckHeroInfo o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.HeroInfo = BaseJsonMarshaler.ParseObject(d, "HeroInfo", JM_NShared_CachedHeroInfo.Parse);
		o.DeckIndex = BaseJsonMarshaler.ParseValueInt(d, "DeckIndex");
		return o;
	}

	public static string EncodeJsonString(PartyDeckHeroInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PartyDeckHeroInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("HeroInfo", JM_NShared_CachedHeroInfo.EncodeJsonObject(o.HeroInfo));
		dictionary.Add("DeckIndex", o.DeckIndex.ToString());
		return dictionary;
	}
}
