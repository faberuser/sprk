using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NMessage.NProtocol;

public static class JM_NShared_NMessage_NProtocol_PartyDeckCostumeInfo
{
	public static PartyDeckCostumeInfo Parse(string s)
	{
		return Parse(s, new PartyDeckCostumeInfo());
	}

	public static PartyDeckCostumeInfo Parse(string s, PartyDeckCostumeInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static PartyDeckCostumeInfo Parse(IDictionary d)
	{
		return Parse(d, new PartyDeckCostumeInfo());
	}

	public static PartyDeckCostumeInfo Parse(IDictionary d, PartyDeckCostumeInfo o)
	{
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.CostumeInfo = BaseJsonMarshaler.ParseObject(d, "CostumeInfo", JM_NShared_CachedHeroCostumeInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(PartyDeckCostumeInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(PartyDeckCostumeInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		dictionary.Add("CostumeInfo", JM_NShared_CachedHeroCostumeInfo.EncodeJsonObject(o.CostumeInfo));
		return dictionary;
	}
}
