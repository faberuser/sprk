using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_RecommendDeckInfo
{
	public static RecommendDeckInfo Parse(string s)
	{
		return Parse(s, new RecommendDeckInfo());
	}

	public static RecommendDeckInfo Parse(string s, RecommendDeckInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RecommendDeckInfo Parse(IDictionary d)
	{
		return Parse(d, new RecommendDeckInfo());
	}

	public static RecommendDeckInfo Parse(IDictionary d, RecommendDeckInfo o)
	{
		o.AccountId = BaseJsonMarshaler.ParseValueString(d, "AccountId");
		o.Type = BaseJsonMarshaler.ParseEnum<RecommendDeckType>(d, "Type");
		o.Rank = BaseJsonMarshaler.ParseValueInt(d, "Rank");
		o.PlayTime = BaseJsonMarshaler.ParseValueInt(d, "PlayTime");
		o.Heroes = BaseJsonMarshaler.ParseArray(d, "Heroes", JM_NShared_ClearPartyHeroInfo.Parse);
		o.EquipItems = BaseJsonMarshaler.ParseArray(d, "EquipItems", JM_NShared_EquipItemInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(RecommendDeckInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RecommendDeckInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.AccountId != null)
		{
			dictionary.Add("AccountId", o.AccountId);
		}
		dictionary.Add("Type", o.Type.ToString());
		dictionary.Add("Rank", o.Rank.ToString());
		dictionary.Add("PlayTime", o.PlayTime.ToString());
		dictionary.Add("Heroes", BaseJsonMarshaler.EncodeArray(o.Heroes, JM_NShared_ClearPartyHeroInfo.EncodeJsonObject));
		dictionary.Add("EquipItems", BaseJsonMarshaler.EncodeArray(o.EquipItems, JM_NShared_EquipItemInfo.EncodeJsonObject));
		return dictionary;
	}
}
