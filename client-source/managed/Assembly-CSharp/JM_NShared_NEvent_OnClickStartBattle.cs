using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NEvent;

public static class JM_NShared_NEvent_OnClickStartBattle
{
	public static OnClickStartBattle Parse(string s)
	{
		return Parse(s, new OnClickStartBattle());
	}

	public static OnClickStartBattle Parse(string s, OnClickStartBattle o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static OnClickStartBattle Parse(IDictionary d)
	{
		return Parse(d, new OnClickStartBattle());
	}

	public static OnClickStartBattle Parse(IDictionary d, OnClickStartBattle o)
	{
		o.DifficultyType = BaseJsonMarshaler.ParseEnum<DungeonDifficultyType>(d, "DifficultyType");
		o.IsScenario = BaseJsonMarshaler.ParseValueBool(d, "IsScenario");
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.SkipTrigger = BaseJsonMarshaler.ParseValueBool(d, "SkipTrigger");
		return o;
	}

	public static string EncodeJsonString(OnClickStartBattle o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(OnClickStartBattle o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("DifficultyType", o.DifficultyType.ToString());
		dictionary.Add("IsScenario", o.IsScenario.ToString());
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("SkipTrigger", o.SkipTrigger.ToString());
		return dictionary;
	}
}
