using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NWorldMap.NEventSubData;

public static class JM_NShared_NWorldMap_NEventSubData_Raid
{
	public static Raid Parse(string s)
	{
		return Parse(s, new Raid());
	}

	public static Raid Parse(string s, Raid o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static Raid Parse(IDictionary d)
	{
		return Parse(d, new Raid());
	}

	public static Raid Parse(IDictionary d, Raid o)
	{
		o.RaidInfo = BaseJsonMarshaler.ParseObject(d, "RaidInfo", JM_NShared_RaidInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(Raid o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(Raid o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("RaidInfo", JM_NShared_RaidInfo.EncodeJsonObject(o.RaidInfo));
		return dictionary;
	}
}
