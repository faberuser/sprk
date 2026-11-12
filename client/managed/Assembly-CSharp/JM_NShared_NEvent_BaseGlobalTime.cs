using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NEvent;

public static class JM_NShared_NEvent_BaseGlobalTime
{
	public static BaseGlobalTime Parse(string s)
	{
		return Parse(s, new BaseGlobalTime());
	}

	public static BaseGlobalTime Parse(string s, BaseGlobalTime o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BaseGlobalTime Parse(IDictionary d)
	{
		return Parse(d, new BaseGlobalTime());
	}

	public static BaseGlobalTime Parse(IDictionary d, BaseGlobalTime o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.WaveIndex = BaseJsonMarshaler.ParseValueInt(d, "WaveIndex");
		return o;
	}

	public static string EncodeJsonString(BaseGlobalTime o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BaseGlobalTime o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("WaveIndex", o.WaveIndex.ToString());
		return dictionary;
	}
}
