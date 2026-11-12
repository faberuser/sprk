using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NEvent;

public static class JM_NShared_NEvent_ScenarioWaveEnd
{
	public static ScenarioWaveEnd Parse(string s)
	{
		return Parse(s, new ScenarioWaveEnd());
	}

	public static ScenarioWaveEnd Parse(string s, ScenarioWaveEnd o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ScenarioWaveEnd Parse(IDictionary d)
	{
		return Parse(d, new ScenarioWaveEnd());
	}

	public static ScenarioWaveEnd Parse(IDictionary d, ScenarioWaveEnd o)
	{
		o.WaveIndex = BaseJsonMarshaler.ParseValueInt(d, "WaveIndex");
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.SkipTrigger = BaseJsonMarshaler.ParseValueBool(d, "SkipTrigger");
		return o;
	}

	public static string EncodeJsonString(ScenarioWaveEnd o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ScenarioWaveEnd o)
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
