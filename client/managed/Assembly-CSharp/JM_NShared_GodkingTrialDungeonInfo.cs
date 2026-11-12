using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_GodkingTrialDungeonInfo
{
	public static GodkingTrialDungeonInfo Parse(string s)
	{
		return Parse(s, new GodkingTrialDungeonInfo());
	}

	public static GodkingTrialDungeonInfo Parse(string s, GodkingTrialDungeonInfo o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static GodkingTrialDungeonInfo Parse(IDictionary d)
	{
		return Parse(d, new GodkingTrialDungeonInfo());
	}

	public static GodkingTrialDungeonInfo Parse(IDictionary d, GodkingTrialDungeonInfo o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.IsOpen = BaseJsonMarshaler.ParseValueInt(d, "IsOpen");
		o.OpenedTime = BaseJsonMarshaler.ParseValueString(d, "OpenedTime");
		o.NextResetRemainTime = BaseJsonMarshaler.ParseValueInt(d, "NextResetRemainTime");
		return o;
	}

	public static string EncodeJsonString(GodkingTrialDungeonInfo o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(GodkingTrialDungeonInfo o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("IsOpen", o.IsOpen.ToString());
		if (o.OpenedTime != null)
		{
			dictionary.Add("OpenedTime", o.OpenedTime);
		}
		dictionary.Add("NextResetRemainTime", o.NextResetRemainTime.ToString());
		return dictionary;
	}
}
