using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.G2R_GetRankers;

public static class JM_NShared_G2R_GetRankers_Request
{
	public static NShared.G2R_GetRankers.Request Parse(string s)
	{
		return Parse(s, new NShared.G2R_GetRankers.Request());
	}

	public static NShared.G2R_GetRankers.Request Parse(string s, NShared.G2R_GetRankers.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2R_GetRankers.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2R_GetRankers.Request());
	}

	public static NShared.G2R_GetRankers.Request Parse(IDictionary d, NShared.G2R_GetRankers.Request o)
	{
		o.Keyword = BaseJsonMarshaler.ParseValueString(d, "Keyword");
		o.StartRank = BaseJsonMarshaler.ParseValueInt(d, "StartRank");
		o.RankerCount = BaseJsonMarshaler.ParseValueInt(d, "RankerCount");
		o.ServerGroup = BaseJsonMarshaler.ParseValueString(d, "ServerGroup");
		return o;
	}

	public static string EncodeJsonString(NShared.G2R_GetRankers.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.G2R_GetRankers.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.Keyword != null)
		{
			dictionary.Add("Keyword", o.Keyword);
		}
		dictionary.Add("StartRank", o.StartRank.ToString());
		dictionary.Add("RankerCount", o.RankerCount.ToString());
		if (o.ServerGroup != null)
		{
			dictionary.Add("ServerGroup", o.ServerGroup);
		}
		return dictionary;
	}
}
