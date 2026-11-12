using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_RecordsOfHonorMyInfo
{
	public static RecordsOfHonorMyInfo Parse(string s)
	{
		return Parse(s, new RecordsOfHonorMyInfo());
	}

	public static RecordsOfHonorMyInfo Parse(string s, RecordsOfHonorMyInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static RecordsOfHonorMyInfo Parse(IDictionary d)
	{
		return Parse(d, new RecordsOfHonorMyInfo());
	}

	public static RecordsOfHonorMyInfo Parse(IDictionary d, RecordsOfHonorMyInfo o)
	{
		o.ContentType = BaseJsonMarshaler.ParseEnum<RecordsOfHonorContentType>(d, "ContentType");
		o.RankingType = BaseJsonMarshaler.ParseEnum<RecordsOfHonorRankingType>(d, "RankingType");
		o.Season = BaseJsonMarshaler.ParseValueInt(d, "Season");
		o.Rank = BaseJsonMarshaler.ParseValueInt(d, "Rank");
		return o;
	}

	public static string EncodeJsonString(RecordsOfHonorMyInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(RecordsOfHonorMyInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ContentType", o.ContentType.ToString());
		dictionary.Add("RankingType", o.RankingType.ToString());
		dictionary.Add("Season", o.Season.ToString());
		dictionary.Add("Rank", o.Rank.ToString());
		return dictionary;
	}
}
