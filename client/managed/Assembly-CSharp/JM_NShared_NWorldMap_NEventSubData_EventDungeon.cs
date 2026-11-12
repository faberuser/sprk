using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NWorldMap.NEventSubData;

public static class JM_NShared_NWorldMap_NEventSubData_EventDungeon
{
	public static EventDungeon Parse(string s)
	{
		return Parse(s, new EventDungeon());
	}

	public static EventDungeon Parse(string s, EventDungeon o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EventDungeon Parse(IDictionary d)
	{
		return Parse(d, new EventDungeon());
	}

	public static EventDungeon Parse(IDictionary d, EventDungeon o)
	{
		o.EventDungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "EventDungeonIndex");
		return o;
	}

	public static string EncodeJsonString(EventDungeon o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EventDungeon o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("EventDungeonIndex", o.EventDungeonIndex.ToString());
		return dictionary;
	}
}
