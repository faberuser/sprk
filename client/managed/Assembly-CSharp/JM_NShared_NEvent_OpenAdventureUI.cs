using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NEvent;

public static class JM_NShared_NEvent_OpenAdventureUI
{
	public static OpenAdventureUI Parse(string s)
	{
		return Parse(s, new OpenAdventureUI());
	}

	public static OpenAdventureUI Parse(string s, OpenAdventureUI o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static OpenAdventureUI Parse(IDictionary d)
	{
		return Parse(d, new OpenAdventureUI());
	}

	public static OpenAdventureUI Parse(IDictionary d, OpenAdventureUI o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.SkipTrigger = BaseJsonMarshaler.ParseValueBool(d, "SkipTrigger");
		return o;
	}

	public static string EncodeJsonString(OpenAdventureUI o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(OpenAdventureUI o)
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
