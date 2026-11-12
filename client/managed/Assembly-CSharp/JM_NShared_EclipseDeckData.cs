using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_EclipseDeckData
{
	public static EclipseDeckData Parse(string s)
	{
		return Parse(s, new EclipseDeckData());
	}

	public static EclipseDeckData Parse(string s, EclipseDeckData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EclipseDeckData Parse(IDictionary d)
	{
		return Parse(d, new EclipseDeckData());
	}

	public static EclipseDeckData Parse(IDictionary d, EclipseDeckData o)
	{
		o.DeckIndex = BaseJsonMarshaler.ParseValueInt(d, "DeckIndex");
		o.HeroInfos = BaseJsonMarshaler.ParseArray(d, "HeroInfos", JM_NShared_CachedHeroInfo.Parse);
		o.HeroConditionInfos = BaseJsonMarshaler.ParseArray(d, "HeroConditionInfos", JM_NShared_EclipseHeroConditionInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(EclipseDeckData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EclipseDeckData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("DeckIndex", o.DeckIndex.ToString());
		dictionary.Add("HeroInfos", BaseJsonMarshaler.EncodeArray(o.HeroInfos, JM_NShared_CachedHeroInfo.EncodeJsonObject));
		dictionary.Add("HeroConditionInfos", BaseJsonMarshaler.EncodeArray(o.HeroConditionInfos, JM_NShared_EclipseHeroConditionInfo.EncodeJsonObject));
		return dictionary;
	}
}
