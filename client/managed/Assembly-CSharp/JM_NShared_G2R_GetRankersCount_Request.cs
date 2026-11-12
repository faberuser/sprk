using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.G2R_GetRankersCount;

public static class JM_NShared_G2R_GetRankersCount_Request
{
	public static NShared.G2R_GetRankersCount.Request Parse(string s)
	{
		return Parse(s, new NShared.G2R_GetRankersCount.Request());
	}

	public static NShared.G2R_GetRankersCount.Request Parse(string s, NShared.G2R_GetRankersCount.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2R_GetRankersCount.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2R_GetRankersCount.Request());
	}

	public static NShared.G2R_GetRankersCount.Request Parse(IDictionary d, NShared.G2R_GetRankersCount.Request o)
	{
		o.Keyword = BaseJsonMarshaler.ParseValueString(d, "Keyword");
		return o;
	}

	public static string EncodeJsonString(NShared.G2R_GetRankersCount.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.G2R_GetRankersCount.Request o)
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
		return dictionary;
	}
}
