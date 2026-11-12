using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NEvent;

public static class JM_NShared_NEvent_WaveEnd
{
	public static WaveEnd Parse(string s)
	{
		return Parse(s, new WaveEnd());
	}

	public static WaveEnd Parse(string s, WaveEnd o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static WaveEnd Parse(IDictionary d)
	{
		return Parse(d, new WaveEnd());
	}

	public static WaveEnd Parse(IDictionary d, WaveEnd o)
	{
		o.WaveIndex = BaseJsonMarshaler.ParseValueInt(d, "WaveIndex");
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.SkipTrigger = BaseJsonMarshaler.ParseValueBool(d, "SkipTrigger");
		return o;
	}

	public static string EncodeJsonString(WaveEnd o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(WaveEnd o)
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
