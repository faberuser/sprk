using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_WorldMapEventTimeInfo
{
	public static WorldMapEventTimeInfo Parse(string s)
	{
		return Parse(s, new WorldMapEventTimeInfo());
	}

	public static WorldMapEventTimeInfo Parse(string s, WorldMapEventTimeInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static WorldMapEventTimeInfo Parse(IDictionary d)
	{
		return Parse(d, new WorldMapEventTimeInfo());
	}

	public static WorldMapEventTimeInfo Parse(IDictionary d, WorldMapEventTimeInfo o)
	{
		o.EventIndex = BaseJsonMarshaler.ParseValueInt(d, "EventIndex");
		o.LastCoolCheckTime = BaseJsonMarshaler.ParseValueString(d, "LastCoolCheckTime");
		o.ReqDungeonCount = BaseJsonMarshaler.ParseValueInt(d, "ReqDungeonCount");
		return o;
	}

	public static string EncodeJsonString(WorldMapEventTimeInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(WorldMapEventTimeInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("EventIndex", o.EventIndex.ToString());
		if (o.LastCoolCheckTime != null)
		{
			dictionary.Add("LastCoolCheckTime", o.LastCoolCheckTime);
		}
		dictionary.Add("ReqDungeonCount", o.ReqDungeonCount.ToString());
		return dictionary;
	}
}
