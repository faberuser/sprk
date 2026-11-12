using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ConquestDungeonInfo
{
	public static ConquestDungeonInfo Parse(string s)
	{
		return Parse(s, new ConquestDungeonInfo());
	}

	public static ConquestDungeonInfo Parse(string s, ConquestDungeonInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ConquestDungeonInfo Parse(IDictionary d)
	{
		return Parse(d, new ConquestDungeonInfo());
	}

	public static ConquestDungeonInfo Parse(IDictionary d, ConquestDungeonInfo o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.CompletedTime = BaseJsonMarshaler.ParseValueString(d, "CompletedTime");
		o.DailyCompletedCount = BaseJsonMarshaler.ParseValueInt(d, "DailyCompletedCount");
		o.ResetCount = BaseJsonMarshaler.ParseValueInt(d, "ResetCount");
		o.NextResetRemainTime = BaseJsonMarshaler.ParseValueInt(d, "NextResetRemainTime");
		return o;
	}

	public static string EncodeJsonString(ConquestDungeonInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ConquestDungeonInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		if (o.CompletedTime != null)
		{
			dictionary.Add("CompletedTime", o.CompletedTime);
		}
		dictionary.Add("DailyCompletedCount", o.DailyCompletedCount.ToString());
		dictionary.Add("ResetCount", o.ResetCount.ToString());
		dictionary.Add("NextResetRemainTime", o.NextResetRemainTime.ToString());
		return dictionary;
	}
}
