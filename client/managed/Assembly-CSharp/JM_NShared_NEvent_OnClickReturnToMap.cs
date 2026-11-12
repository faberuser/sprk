using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NEvent;

public static class JM_NShared_NEvent_OnClickReturnToMap
{
	public static OnClickReturnToMap Parse(string s)
	{
		return Parse(s, new OnClickReturnToMap());
	}

	public static OnClickReturnToMap Parse(string s, OnClickReturnToMap o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static OnClickReturnToMap Parse(IDictionary d)
	{
		return Parse(d, new OnClickReturnToMap());
	}

	public static OnClickReturnToMap Parse(IDictionary d, OnClickReturnToMap o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.SkipTrigger = BaseJsonMarshaler.ParseValueBool(d, "SkipTrigger");
		return o;
	}

	public static string EncodeJsonString(OnClickReturnToMap o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(OnClickReturnToMap o)
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
