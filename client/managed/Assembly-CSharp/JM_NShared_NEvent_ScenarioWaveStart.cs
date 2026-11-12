using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NEvent;

public static class JM_NShared_NEvent_ScenarioWaveStart
{
	public static ScenarioWaveStart Parse(string s)
	{
		return Parse(s, new ScenarioWaveStart());
	}

	public static ScenarioWaveStart Parse(string s, ScenarioWaveStart o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ScenarioWaveStart Parse(IDictionary d)
	{
		return Parse(d, new ScenarioWaveStart());
	}

	public static ScenarioWaveStart Parse(IDictionary d, ScenarioWaveStart o)
	{
		o.WaveIndex = BaseJsonMarshaler.ParseValueInt(d, "WaveIndex");
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.SkipTrigger = BaseJsonMarshaler.ParseValueBool(d, "SkipTrigger");
		return o;
	}

	public static string EncodeJsonString(ScenarioWaveStart o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ScenarioWaveStart o)
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
