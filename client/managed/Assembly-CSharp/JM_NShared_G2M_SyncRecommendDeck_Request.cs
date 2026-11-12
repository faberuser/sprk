using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.G2M_SyncRecommendDeck;

public static class JM_NShared_G2M_SyncRecommendDeck_Request
{
	public static NShared.G2M_SyncRecommendDeck.Request Parse(string s)
	{
		return Parse(s, new NShared.G2M_SyncRecommendDeck.Request());
	}

	public static NShared.G2M_SyncRecommendDeck.Request Parse(string s, NShared.G2M_SyncRecommendDeck.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2M_SyncRecommendDeck.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2M_SyncRecommendDeck.Request());
	}

	public static NShared.G2M_SyncRecommendDeck.Request Parse(IDictionary d, NShared.G2M_SyncRecommendDeck.Request o)
	{
		o.Host = BaseJsonMarshaler.ParseValueString(d, "Host");
		o.RecommendLevelRanks = BaseJsonMarshaler.ParseArray(d, "RecommendLevelRanks", JM_NShared_RecommendRankInfo.Parse);
		o.RecommendTimeRanks = BaseJsonMarshaler.ParseArray(d, "RecommendTimeRanks", JM_NShared_RecommendRankInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(NShared.G2M_SyncRecommendDeck.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.G2M_SyncRecommendDeck.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Host != null)
		{
			dictionary.Add("Host", o.Host);
		}
		dictionary.Add("RecommendLevelRanks", BaseJsonMarshaler.EncodeArray(o.RecommendLevelRanks, JM_NShared_RecommendRankInfo.EncodeJsonObject));
		dictionary.Add("RecommendTimeRanks", BaseJsonMarshaler.EncodeArray(o.RecommendTimeRanks, JM_NShared_RecommendRankInfo.EncodeJsonObject));
		return dictionary;
	}
}
