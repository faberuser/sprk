using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_RankResultInfo
{
	public static RankResultInfo Parse(string s)
	{
		return Parse(s, new RankResultInfo());
	}

	public static RankResultInfo Parse(string s, RankResultInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RankResultInfo Parse(IDictionary d)
	{
		return Parse(d, new RankResultInfo());
	}

	public static RankResultInfo Parse(IDictionary d, RankResultInfo o)
	{
		o.TierRank = BaseJsonMarshaler.ParseValueInt(d, "TierRank");
		o.TotalRank = BaseJsonMarshaler.ParseValueInt(d, "TotalRank");
		o.TotalRankerCount = BaseJsonMarshaler.ParseValueInt(d, "TotalRankerCount");
		return o;
	}

	public static string EncodeJsonString(RankResultInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RankResultInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("TierRank", o.TierRank.ToString());
		dictionary.Add("TotalRank", o.TotalRank.ToString());
		dictionary.Add("TotalRankerCount", o.TotalRankerCount.ToString());
		return dictionary;
	}
}
