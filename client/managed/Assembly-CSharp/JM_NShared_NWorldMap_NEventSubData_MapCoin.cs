using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared.NWorldMap.NEventSubData;

public static class JM_NShared_NWorldMap_NEventSubData_MapCoin
{
	public static MapCoin Parse(string s)
	{
		return Parse(s, new MapCoin());
	}

	public static MapCoin Parse(string s, MapCoin o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static MapCoin Parse(IDictionary d)
	{
		return Parse(d, new MapCoin());
	}

	public static MapCoin Parse(IDictionary d, MapCoin o)
	{
		return o;
	}

	public static string EncodeJsonString(MapCoin o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(MapCoin o)
	{
		return new Dictionary<string, object>();
	}
}
