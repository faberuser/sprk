using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_RecommendRankInfo
{
	public static RecommendRankInfo Parse(string s)
	{
		return Parse(s, new RecommendRankInfo());
	}

	public static RecommendRankInfo Parse(string s, RecommendRankInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RecommendRankInfo Parse(IDictionary d)
	{
		return Parse(d, new RecommendRankInfo());
	}

	public static RecommendRankInfo Parse(IDictionary d, RecommendRankInfo o)
	{
		o.BattleIndex = BaseJsonMarshaler.ParseValueString(d, "BattleIndex");
		o.RankInfos = BaseJsonMarshaler.ParseArray(d, "RankInfos", JM_NShared_RecommendDeckInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(RecommendRankInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RecommendRankInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.BattleIndex != null)
		{
			dictionary.Add("BattleIndex", o.BattleIndex);
		}
		dictionary.Add("RankInfos", BaseJsonMarshaler.EncodeArray(o.RankInfos, JM_NShared_RecommendDeckInfo.EncodeJsonObject));
		return dictionary;
	}
}
