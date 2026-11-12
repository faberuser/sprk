using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NEvent;

public static class JM_NShared_NEvent_BattleInitDeck
{
	public static BattleInitDeck Parse(string s)
	{
		return Parse(s, new BattleInitDeck());
	}

	public static BattleInitDeck Parse(string s, BattleInitDeck o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleInitDeck Parse(IDictionary d)
	{
		return Parse(d, new BattleInitDeck());
	}

	public static BattleInitDeck Parse(IDictionary d, BattleInitDeck o)
	{
		o.MainHeroInfos = BaseJsonMarshaler.ParseArray(d, "MainHeroInfos", JM_NShared_CachedHeroInfo.Parse);
		o.SubHeroInfos = BaseJsonMarshaler.ParseArray(d, "SubHeroInfos", JM_NShared_CachedHeroInfo.Parse);
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.SkipTrigger = BaseJsonMarshaler.ParseValueBool(d, "SkipTrigger");
		return o;
	}

	public static string EncodeJsonString(BattleInitDeck o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleInitDeck o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("MainHeroInfos", BaseJsonMarshaler.EncodeArray(o.MainHeroInfos, JM_NShared_CachedHeroInfo.EncodeJsonObject));
		dictionary.Add("SubHeroInfos", BaseJsonMarshaler.EncodeArray(o.SubHeroInfos, JM_NShared_CachedHeroInfo.EncodeJsonObject));
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("SkipTrigger", o.SkipTrigger.ToString());
		return dictionary;
	}
}
