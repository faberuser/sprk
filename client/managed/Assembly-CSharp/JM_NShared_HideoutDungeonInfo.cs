using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_HideoutDungeonInfo
{
	public static HideoutDungeonInfo Parse(string s)
	{
		return Parse(s, new HideoutDungeonInfo());
	}

	public static HideoutDungeonInfo Parse(string s, HideoutDungeonInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static HideoutDungeonInfo Parse(IDictionary d)
	{
		return Parse(d, new HideoutDungeonInfo());
	}

	public static HideoutDungeonInfo Parse(IDictionary d, HideoutDungeonInfo o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.CompletedTime = BaseJsonMarshaler.ParseValueString(d, "CompletedTime");
		o.DailyCompletedCount = BaseJsonMarshaler.ParseValueInt(d, "DailyCompletedCount");
		o.ResetCount = BaseJsonMarshaler.ParseValueInt(d, "ResetCount");
		o.NextResetRemainTime = BaseJsonMarshaler.ParseValueInt(d, "NextResetRemainTime");
		return o;
	}

	public static string EncodeJsonString(HideoutDungeonInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(HideoutDungeonInfo o)
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
