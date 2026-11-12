using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NEvent;

public static class JM_NShared_NEvent_ScenarioWaveReady
{
	public static ScenarioWaveReady Parse(string s)
	{
		return Parse(s, new ScenarioWaveReady());
	}

	public static ScenarioWaveReady Parse(string s, ScenarioWaveReady o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ScenarioWaveReady Parse(IDictionary d)
	{
		return Parse(d, new ScenarioWaveReady());
	}

	public static ScenarioWaveReady Parse(IDictionary d, ScenarioWaveReady o)
	{
		o.WaveIndex = BaseJsonMarshaler.ParseValueInt(d, "WaveIndex");
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.SkipTrigger = BaseJsonMarshaler.ParseValueBool(d, "SkipTrigger");
		return o;
	}

	public static string EncodeJsonString(ScenarioWaveReady o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ScenarioWaveReady o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("WaveIndex", o.WaveIndex.ToString());
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("SkipTrigger", o.SkipTrigger.ToString());
		return dictionary;
	}
}
