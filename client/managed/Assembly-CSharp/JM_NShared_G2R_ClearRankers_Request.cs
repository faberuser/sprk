using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.G2R_ClearRankers;

public static class JM_NShared_G2R_ClearRankers_Request
{
	public static NShared.G2R_ClearRankers.Request Parse(string s)
	{
		return Parse(s, new NShared.G2R_ClearRankers.Request());
	}

	public static NShared.G2R_ClearRankers.Request Parse(string s, NShared.G2R_ClearRankers.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2R_ClearRankers.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2R_ClearRankers.Request());
	}

	public static NShared.G2R_ClearRankers.Request Parse(IDictionary d, NShared.G2R_ClearRankers.Request o)
	{
		o.Keyword = BaseJsonMarshaler.ParseValueString(d, "Keyword");
		return o;
	}

	public static string EncodeJsonString(NShared.G2R_ClearRankers.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.G2R_ClearRankers.Request o)
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
