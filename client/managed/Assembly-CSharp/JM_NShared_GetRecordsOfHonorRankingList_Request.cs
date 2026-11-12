using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.GetRecordsOfHonorRankingList;

public static class JM_NShared_GetRecordsOfHonorRankingList_Request
{
	public static NShared.GetRecordsOfHonorRankingList.Request Parse(string s)
	{
		return Parse(s, new NShared.GetRecordsOfHonorRankingList.Request());
	}

	public static NShared.GetRecordsOfHonorRankingList.Request Parse(string s, NShared.GetRecordsOfHonorRankingList.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetRecordsOfHonorRankingList.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetRecordsOfHonorRankingList.Request());
	}

	public static NShared.GetRecordsOfHonorRankingList.Request Parse(IDictionary d, NShared.GetRecordsOfHonorRankingList.Request o)
	{
		o.ContentType = BaseJsonMarshaler.ParseEnum<RecordsOfHonorContentType>(d, "ContentType");
		o.Season = BaseJsonMarshaler.ParseValueInt(d, "Season");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.GetRecordsOfHonorRankingList.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.GetRecordsOfHonorRankingList.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ContentType", o.ContentType.ToString());
		dictionary.Add("Season", o.Season.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
