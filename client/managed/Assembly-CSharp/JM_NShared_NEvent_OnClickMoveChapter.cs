using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NEvent;

public static class JM_NShared_NEvent_OnClickMoveChapter
{
	public static OnClickMoveChapter Parse(string s)
	{
		return Parse(s, new OnClickMoveChapter());
	}

	public static OnClickMoveChapter Parse(string s, OnClickMoveChapter o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static OnClickMoveChapter Parse(IDictionary d)
	{
		return Parse(d, new OnClickMoveChapter());
	}

	public static OnClickMoveChapter Parse(IDictionary d, OnClickMoveChapter o)
	{
		o.ChapterIdx = BaseJsonMarshaler.ParseValueInt(d, "ChapterIdx");
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.SkipTrigger = BaseJsonMarshaler.ParseValueBool(d, "SkipTrigger");
		return o;
	}

	public static string EncodeJsonString(OnClickMoveChapter o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(OnClickMoveChapter o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChapterIdx", o.ChapterIdx.ToString());
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("SkipTrigger", o.SkipTrigger.ToString());
		return dictionary;
	}
}
