using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NEvent;

public static class JM_NShared_NEvent_OpenTowerOfOrdeal
{
	public static OpenTowerOfOrdeal Parse(string s)
	{
		return Parse(s, new OpenTowerOfOrdeal());
	}

	public static OpenTowerOfOrdeal Parse(string s, OpenTowerOfOrdeal o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static OpenTowerOfOrdeal Parse(IDictionary d)
	{
		return Parse(d, new OpenTowerOfOrdeal());
	}

	public static OpenTowerOfOrdeal Parse(IDictionary d, OpenTowerOfOrdeal o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.SkipTrigger = BaseJsonMarshaler.ParseValueBool(d, "SkipTrigger");
		return o;
	}

	public static string EncodeJsonString(OpenTowerOfOrdeal o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(OpenTowerOfOrdeal o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("SkipTrigger", o.SkipTrigger.ToString());
		return dictionary;
	}
}
