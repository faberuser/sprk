using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NEvent;

public static class JM_NShared_NEvent_BattleInsertDeck
{
	public static BattleInsertDeck Parse(string s)
	{
		return Parse(s, new BattleInsertDeck());
	}

	public static BattleInsertDeck Parse(string s, BattleInsertDeck o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleInsertDeck Parse(IDictionary d)
	{
		return Parse(d, new BattleInsertDeck());
	}

	public static BattleInsertDeck Parse(IDictionary d, BattleInsertDeck o)
	{
		o.IsMain = BaseJsonMarshaler.ParseValueBool(d, "IsMain");
		o.InsertHeroInfo = BaseJsonMarshaler.ParseObject(d, "InsertHeroInfo", JM_NShared_CachedHeroInfo.Parse);
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.SkipTrigger = BaseJsonMarshaler.ParseValueBool(d, "SkipTrigger");
		return o;
	}

	public static string EncodeJsonString(BattleInsertDeck o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleInsertDeck o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("IsMain", o.IsMain.ToString());
		dictionary.Add("InsertHeroInfo", JM_NShared_CachedHeroInfo.EncodeJsonObject(o.InsertHeroInfo));
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("SkipTrigger", o.SkipTrigger.ToString());
		return dictionary;
	}
}
