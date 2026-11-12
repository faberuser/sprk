using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_EclipseDeckResult
{
	public static EclipseDeckResult Parse(string s)
	{
		return Parse(s, new EclipseDeckResult());
	}

	public static EclipseDeckResult Parse(string s, EclipseDeckResult o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EclipseDeckResult Parse(IDictionary d)
	{
		return Parse(d, new EclipseDeckResult());
	}

	public static EclipseDeckResult Parse(IDictionary d, EclipseDeckResult o)
	{
		o.DeckIndex = BaseJsonMarshaler.ParseValueInt(d, "DeckIndex");
		o.HeroIndices = BaseJsonMarshaler.ParseValueString(d, "HeroIndices");
		o.CachedHeroInfos = BaseJsonMarshaler.ParseArray(d, "CachedHeroInfos", JM_NShared_CachedHeroInfo.Parse);
		o.UpdatedTime = BaseJsonMarshaler.ParseValueString(d, "UpdatedTime");
		o.ClearMaxWaveIndex = BaseJsonMarshaler.ParseValueInt(d, "ClearMaxWaveIndex");
		return o;
	}

	public static string EncodeJsonString(EclipseDeckResult o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EclipseDeckResult o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("DeckIndex", o.DeckIndex.ToString());
		if (o.HeroIndices != null)
		{
			dictionary.Add("HeroIndices", o.HeroIndices);
		}
		dictionary.Add("CachedHeroInfos", BaseJsonMarshaler.EncodeArray(o.CachedHeroInfos, JM_NShared_CachedHeroInfo.EncodeJsonObject));
		if (o.UpdatedTime != null)
		{
			dictionary.Add("UpdatedTime", o.UpdatedTime);
		}
		dictionary.Add("ClearMaxWaveIndex", o.ClearMaxWaveIndex.ToString());
		return dictionary;
	}
}
