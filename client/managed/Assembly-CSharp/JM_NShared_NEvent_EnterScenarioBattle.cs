using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NEvent;

public static class JM_NShared_NEvent_EnterScenarioBattle
{
	public static EnterScenarioBattle Parse(string s)
	{
		return Parse(s, new EnterScenarioBattle());
	}

	public static EnterScenarioBattle Parse(string s, EnterScenarioBattle o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static EnterScenarioBattle Parse(IDictionary d)
	{
		return Parse(d, new EnterScenarioBattle());
	}

	public static EnterScenarioBattle Parse(IDictionary d, EnterScenarioBattle o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.SkipTrigger = BaseJsonMarshaler.ParseValueBool(d, "SkipTrigger");
		return o;
	}

	public static string EncodeJsonString(EnterScenarioBattle o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(EnterScenarioBattle o)
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
