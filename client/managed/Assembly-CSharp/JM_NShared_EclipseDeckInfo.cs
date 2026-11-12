using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_EclipseDeckInfo
{
	public static EclipseDeckInfo Parse(string s)
	{
		return Parse(s, new EclipseDeckInfo());
	}

	public static EclipseDeckInfo Parse(string s, EclipseDeckInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EclipseDeckInfo Parse(IDictionary d)
	{
		return Parse(d, new EclipseDeckInfo());
	}

	public static EclipseDeckInfo Parse(IDictionary d, EclipseDeckInfo o)
	{
		o.DeckIndex = BaseJsonMarshaler.ParseValueInt(d, "DeckIndex");
		o.HeroIndex = BaseJsonMarshaler.ParseValueInt(d, "HeroIndex");
		o.HeroInfo = BaseJsonMarshaler.ParseValueString(d, "HeroInfo");
		o.UpdatedTime = BaseJsonMarshaler.ParseValueString(d, "UpdatedTime");
		return o;
	}

	public static string EncodeJsonString(EclipseDeckInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EclipseDeckInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("DeckIndex", o.DeckIndex.ToString());
		dictionary.Add("HeroIndex", o.HeroIndex.ToString());
		if (o.HeroInfo != null)
		{
			dictionary.Add("HeroInfo", o.HeroInfo);
		}
		if (o.UpdatedTime != null)
		{
			dictionary.Add("UpdatedTime", o.UpdatedTime);
		}
		return dictionary;
	}
}
