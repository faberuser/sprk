using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NEvent;

public static class JM_NShared_NEvent_BattleEnd
{
	public static BattleEnd Parse(string s)
	{
		return Parse(s, new BattleEnd());
	}

	public static BattleEnd Parse(string s, BattleEnd o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleEnd Parse(IDictionary d)
	{
		return Parse(d, new BattleEnd());
	}

	public static BattleEnd Parse(IDictionary d, BattleEnd o)
	{
		o.Winner = BaseJsonMarshaler.ParseEnum<TeamType>(d, "Winner");
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.SkipTrigger = BaseJsonMarshaler.ParseValueBool(d, "SkipTrigger");
		return o;
	}

	public static string EncodeJsonString(BattleEnd o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleEnd o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Winner", o.Winner.ToString());
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("SkipTrigger", o.SkipTrigger.ToString());
		return dictionary;
	}
}
