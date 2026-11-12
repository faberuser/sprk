using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NEvent;

public static class JM_NShared_NEvent_ScenarioBattleBegin
{
	public static ScenarioBattleBegin Parse(string s)
	{
		return Parse(s, new ScenarioBattleBegin());
	}

	public static ScenarioBattleBegin Parse(string s, ScenarioBattleBegin o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ScenarioBattleBegin Parse(IDictionary d)
	{
		return Parse(d, new ScenarioBattleBegin());
	}

	public static ScenarioBattleBegin Parse(IDictionary d, ScenarioBattleBegin o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.SkipTrigger = BaseJsonMarshaler.ParseValueBool(d, "SkipTrigger");
		return o;
	}

	public static string EncodeJsonString(ScenarioBattleBegin o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ScenarioBattleBegin o)
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
