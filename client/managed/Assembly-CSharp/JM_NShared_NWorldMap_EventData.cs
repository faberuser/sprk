using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NWorldMap;

public static class JM_NShared_NWorldMap_EventData
{
	public static EventData Parse(string s)
	{
		return Parse(s, new EventData());
	}

	public static EventData Parse(string s, EventData o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EventData Parse(IDictionary d)
	{
		return Parse(d, new EventData());
	}

	public static EventData Parse(IDictionary d, EventData o)
	{
		o.Index = BaseJsonMarshaler.ParseValueInt(d, "Index");
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.EventType = BaseJsonMarshaler.ParseEnum<EventType>(d, "EventType");
		o.DungeonIndex = BaseJsonMarshaler.ParseArrayCast(d, "DungeonIndex", BaseJsonMarshaler.CastToInt);
		o.CompletedDungeonOnly = BaseJsonMarshaler.ParseValueBool(d, "CompletedDungeonOnly");
		o.CurrentChapterOnly = BaseJsonMarshaler.ParseValueBool(d, "CurrentChapterOnly");
		o.ReqChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ReqChapterIndex");
		o.ReqDungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "ReqDungeonIndex");
		o.EventValue = BaseJsonMarshaler.ParseArrayCast(d, "EventValue", BaseJsonMarshaler.CastToInt);
		o.OccurCount = BaseJsonMarshaler.ParseValueInt(d, "OccurCount");
		o.OccurPercentage = BaseJsonMarshaler.ParseArrayCast(d, "OccurPercentage", BaseJsonMarshaler.CastToInt);
		o.OccurHour = BaseJsonMarshaler.ParseArrayCast(d, "OccurHour", BaseJsonMarshaler.CastToInt);
		o.Duration = BaseJsonMarshaler.ParseValueInt(d, "Duration");
		o.Cooltime = BaseJsonMarshaler.ParseValueInt(d, "Cooltime");
		return o;
	}

	public static string EncodeJsonString(EventData o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EventData o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Index", o.Index.ToString());
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("EventType", o.EventType.ToString());
		dictionary.Add("DungeonIndex", BaseJsonMarshaler.EncodeArray(o.DungeonIndex));
		dictionary.Add("CompletedDungeonOnly", o.CompletedDungeonOnly.ToString());
		dictionary.Add("CurrentChapterOnly", o.CurrentChapterOnly.ToString());
		dictionary.Add("ReqChapterIndex", o.ReqChapterIndex.ToString());
		dictionary.Add("ReqDungeonIndex", o.ReqDungeonIndex.ToString());
		dictionary.Add("EventValue", BaseJsonMarshaler.EncodeArray(o.EventValue));
		dictionary.Add("OccurCount", o.OccurCount.ToString());
		dictionary.Add("OccurPercentage", BaseJsonMarshaler.EncodeArray(o.OccurPercentage));
		dictionary.Add("OccurHour", BaseJsonMarshaler.EncodeArray(o.OccurHour));
		dictionary.Add("Duration", o.Duration.ToString());
		dictionary.Add("Cooltime", o.Cooltime.ToString());
		return dictionary;
	}
}
