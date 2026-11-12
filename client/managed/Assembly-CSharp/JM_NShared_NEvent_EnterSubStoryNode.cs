using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NEvent;

public static class JM_NShared_NEvent_EnterSubStoryNode
{
	public static EnterSubStoryNode Parse(string s)
	{
		return Parse(s, new EnterSubStoryNode());
	}

	public static EnterSubStoryNode Parse(string s, EnterSubStoryNode o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EnterSubStoryNode Parse(IDictionary d)
	{
		return Parse(d, new EnterSubStoryNode());
	}

	public static EnterSubStoryNode Parse(IDictionary d, EnterSubStoryNode o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.SkipTrigger = BaseJsonMarshaler.ParseValueBool(d, "SkipTrigger");
		return o;
	}

	public static string EncodeJsonString(EnterSubStoryNode o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EnterSubStoryNode o)
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
