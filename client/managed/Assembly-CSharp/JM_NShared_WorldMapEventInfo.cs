using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NWorldMap;

public static class JM_NShared_WorldMapEventInfo
{
	public static WorldMapEventInfo Parse(string s)
	{
		return Parse(s, new WorldMapEventInfo());
	}

	public static WorldMapEventInfo Parse(string s, WorldMapEventInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static WorldMapEventInfo Parse(IDictionary d)
	{
		return Parse(d, new WorldMapEventInfo());
	}

	public static WorldMapEventInfo Parse(IDictionary d, WorldMapEventInfo o)
	{
		o.EventIndex = BaseJsonMarshaler.ParseValueInt(d, "EventIndex");
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.EventType = BaseJsonMarshaler.ParseEnum<EventType>(d, "EventType");
		o.SerializedSubData = BaseJsonMarshaler.ParseValueString(d, "SerializedSubData");
		o.RewardItems = BaseJsonMarshaler.ParseValueString(d, "RewardItems");
		o.RewardGold = BaseJsonMarshaler.ParseValueInt(d, "RewardGold");
		o.RewardGem = BaseJsonMarshaler.ParseValueInt(d, "RewardGem");
		o.OccuredTime = BaseJsonMarshaler.ParseValueString(d, "OccuredTime");
		o.Duration = BaseJsonMarshaler.ParseValueInt(d, "Duration");
		o.Completed = BaseJsonMarshaler.ParseValueBool(d, "Completed");
		o.NoExpire = BaseJsonMarshaler.ParseValueBool(d, "NoExpire");
		o.NoExpireOnComplete = BaseJsonMarshaler.ParseValueBool(d, "NoExpireOnComplete");
		o.ProgressCount = BaseJsonMarshaler.ParseValueInt(d, "ProgressCount");
		o.RequireCount = BaseJsonMarshaler.ParseValueInt(d, "RequireCount");
		return o;
	}

	public static string EncodeJsonString(WorldMapEventInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(WorldMapEventInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("EventIndex", o.EventIndex.ToString());
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("EventType", o.EventType.ToString());
		if (o.SerializedSubData != null)
		{
			dictionary.Add("SerializedSubData", o.SerializedSubData);
		}
		if (o.RewardItems != null)
		{
			dictionary.Add("RewardItems", o.RewardItems);
		}
		dictionary.Add("RewardGold", o.RewardGold.ToString());
		dictionary.Add("RewardGem", o.RewardGem.ToString());
		if (o.OccuredTime != null)
		{
			dictionary.Add("OccuredTime", o.OccuredTime);
		}
		dictionary.Add("Duration", o.Duration.ToString());
		dictionary.Add("Completed", o.Completed.ToString());
		dictionary.Add("NoExpire", o.NoExpire.ToString());
		dictionary.Add("NoExpireOnComplete", o.NoExpireOnComplete.ToString());
		dictionary.Add("ProgressCount", o.ProgressCount.ToString());
		dictionary.Add("RequireCount", o.RequireCount.ToString());
		return dictionary;
	}
}
