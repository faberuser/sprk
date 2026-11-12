using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NEvent;

public static class JM_NShared_NEvent_OnClickNextBattle
{
	public static OnClickNextBattle Parse(string s)
	{
		return Parse(s, new OnClickNextBattle());
	}

	public static OnClickNextBattle Parse(string s, OnClickNextBattle o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static OnClickNextBattle Parse(IDictionary d)
	{
		return Parse(d, new OnClickNextBattle());
	}

	public static OnClickNextBattle Parse(IDictionary d, OnClickNextBattle o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.SkipTrigger = BaseJsonMarshaler.ParseValueBool(d, "SkipTrigger");
		return o;
	}

	public static string EncodeJsonString(OnClickNextBattle o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(OnClickNextBattle o)
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
