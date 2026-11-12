using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_ChapterDungeonInfo
{
	public static ChapterDungeonInfo Parse(string s)
	{
		return Parse(s, new ChapterDungeonInfo());
	}

	public static ChapterDungeonInfo Parse(string s, ChapterDungeonInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ChapterDungeonInfo Parse(IDictionary d)
	{
		return Parse(d, new ChapterDungeonInfo());
	}

	public static ChapterDungeonInfo Parse(IDictionary d, ChapterDungeonInfo o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.MaxStar = BaseJsonMarshaler.ParseValueShort(d, "MaxStar");
		o.FirstRewardedDiff = BaseJsonMarshaler.ParseValueShort(d, "FirstRewardedDiff");
		o.ScenarioComplete = BaseJsonMarshaler.ParseValueShort(d, "ScenarioComplete");
		o.VisitedTime = BaseJsonMarshaler.ParseValueString(d, "VisitedTime");
		o.CompletedTime = BaseJsonMarshaler.ParseValueString(d, "CompletedTime");
		o.DailyCompletedCount = BaseJsonMarshaler.ParseValueInt(d, "DailyCompletedCount");
		o.ResetCount = BaseJsonMarshaler.ParseValueInt(d, "ResetCount");
		return o;
	}

	public static string EncodeJsonString(ChapterDungeonInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ChapterDungeonInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("MaxStar", o.MaxStar.ToString());
		dictionary.Add("FirstRewardedDiff", o.FirstRewardedDiff.ToString());
		dictionary.Add("ScenarioComplete", o.ScenarioComplete.ToString());
		if (o.VisitedTime != null)
		{
			dictionary.Add("VisitedTime", o.VisitedTime);
		}
		if (o.CompletedTime != null)
		{
			dictionary.Add("CompletedTime", o.CompletedTime);
		}
		dictionary.Add("DailyCompletedCount", o.DailyCompletedCount.ToString());
		dictionary.Add("ResetCount", o.ResetCount.ToString());
		return dictionary;
	}
}
