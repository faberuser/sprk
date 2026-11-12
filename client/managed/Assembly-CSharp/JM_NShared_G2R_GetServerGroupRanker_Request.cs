using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.G2R_GetServerGroupRanker;

public static class JM_NShared_G2R_GetServerGroupRanker_Request
{
	public static NShared.G2R_GetServerGroupRanker.Request Parse(string s)
	{
		return Parse(s, new NShared.G2R_GetServerGroupRanker.Request());
	}

	public static NShared.G2R_GetServerGroupRanker.Request Parse(string s, NShared.G2R_GetServerGroupRanker.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.G2R_GetServerGroupRanker.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.G2R_GetServerGroupRanker.Request());
	}

	public static NShared.G2R_GetServerGroupRanker.Request Parse(IDictionary d, NShared.G2R_GetServerGroupRanker.Request o)
	{
		o.__Header__ = BaseJsonMarshaler.ParseObject(d, "__Header__", JM_NShared_G2R_GetServerGroupRanker_Header.Parse);
		return o;
	}

	public static string EncodeJsonString(NShared.G2R_GetServerGroupRanker.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.G2R_GetServerGroupRanker.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("__Header__", JM_NShared_G2R_GetServerGroupRanker_Header.EncodeJsonObject(o.__Header__));
		return dictionary;
	}
}
