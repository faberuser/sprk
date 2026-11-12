using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_RecordsOfHonorRankerInfo
{
	public static RecordsOfHonorRankerInfo Parse(string s)
	{
		return Parse(s, new RecordsOfHonorRankerInfo());
	}

	public static RecordsOfHonorRankerInfo Parse(string s, RecordsOfHonorRankerInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RecordsOfHonorRankerInfo Parse(IDictionary d)
	{
		return Parse(d, new RecordsOfHonorRankerInfo());
	}

	public static RecordsOfHonorRankerInfo Parse(IDictionary d, RecordsOfHonorRankerInfo o)
	{
		o.ContentType = BaseJsonMarshaler.ParseEnum<RecordsOfHonorContentType>(d, "ContentType");
		o.RankingType = BaseJsonMarshaler.ParseEnum<RecordsOfHonorRankingType>(d, "RankingType");
		o.Season = BaseJsonMarshaler.ParseValueInt(d, "Season");
		o.Ranker = BaseJsonMarshaler.ParseArray(d, "Ranker", JM_NShared_RecordsOfHonorRanker.Parse);
		return o;
	}

	public static string EncodeJsonString(RecordsOfHonorRankerInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RecordsOfHonorRankerInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ContentType", o.ContentType.ToString());
		dictionary.Add("RankingType", o.RankingType.ToString());
		dictionary.Add("Season", o.Season.ToString());
		dictionary.Add("Ranker", BaseJsonMarshaler.EncodeArray(o.Ranker, JM_NShared_RecordsOfHonorRanker.EncodeJsonObject));
		return dictionary;
	}
}
