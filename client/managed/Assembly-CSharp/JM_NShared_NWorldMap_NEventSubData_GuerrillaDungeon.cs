using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NWorldMap.NEventSubData;

public static class JM_NShared_NWorldMap_NEventSubData_GuerrillaDungeon
{
	public static GuerrillaDungeon Parse(string s)
	{
		return Parse(s, new GuerrillaDungeon());
	}

	public static GuerrillaDungeon Parse(string s, GuerrillaDungeon o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GuerrillaDungeon Parse(IDictionary d)
	{
		return Parse(d, new GuerrillaDungeon());
	}

	public static GuerrillaDungeon Parse(IDictionary d, GuerrillaDungeon o)
	{
		o.GuerrillaDungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "GuerrillaDungeonIndex");
		return o;
	}

	public static string EncodeJsonString(GuerrillaDungeon o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GuerrillaDungeon o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("GuerrillaDungeonIndex", o.GuerrillaDungeonIndex.ToString());
		return dictionary;
	}
}
