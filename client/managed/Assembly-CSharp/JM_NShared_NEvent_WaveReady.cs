using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NEvent;

public static class JM_NShared_NEvent_WaveReady
{
	public static NShared.NEvent.WaveReady Parse(string s)
	{
		return Parse(s, new NShared.NEvent.WaveReady());
	}

	public static NShared.NEvent.WaveReady Parse(string s, NShared.NEvent.WaveReady o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.NEvent.WaveReady Parse(IDictionary d)
	{
		return Parse(d, new NShared.NEvent.WaveReady());
	}

	public static NShared.NEvent.WaveReady Parse(IDictionary d, NShared.NEvent.WaveReady o)
	{
		o.WaveIndex = BaseJsonMarshaler.ParseValueInt(d, "WaveIndex");
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.SkipTrigger = BaseJsonMarshaler.ParseValueBool(d, "SkipTrigger");
		return o;
	}

	public static string EncodeJsonString(NShared.NEvent.WaveReady o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.NEvent.WaveReady o)
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
