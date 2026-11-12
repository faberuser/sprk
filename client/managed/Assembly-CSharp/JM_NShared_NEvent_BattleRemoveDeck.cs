using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NEvent;

public static class JM_NShared_NEvent_BattleRemoveDeck
{
	public static BattleRemoveDeck Parse(string s)
	{
		return Parse(s, new BattleRemoveDeck());
	}

	public static BattleRemoveDeck Parse(string s, BattleRemoveDeck o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleRemoveDeck Parse(IDictionary d)
	{
		return Parse(d, new BattleRemoveDeck());
	}

	public static BattleRemoveDeck Parse(IDictionary d, BattleRemoveDeck o)
	{
		o.RemoveHeroIndex = BaseJsonMarshaler.ParseValueInt(d, "RemoveHeroIndex");
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.SkipTrigger = BaseJsonMarshaler.ParseValueBool(d, "SkipTrigger");
		return o;
	}

	public static string EncodeJsonString(BattleRemoveDeck o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleRemoveDeck o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("RemoveHeroIndex", o.RemoveHeroIndex.ToString());
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("SkipTrigger", o.SkipTrigger.ToString());
		return dictionary;
	}
}
