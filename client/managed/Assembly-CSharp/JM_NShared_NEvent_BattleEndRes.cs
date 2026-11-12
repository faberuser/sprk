using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NEvent;

public static class JM_NShared_NEvent_BattleEndRes
{
	public static BattleEndRes Parse(string s)
	{
		return Parse(s, new BattleEndRes());
	}

	public static BattleEndRes Parse(string s, BattleEndRes o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleEndRes Parse(IDictionary d)
	{
		return Parse(d, new BattleEndRes());
	}

	public static BattleEndRes Parse(IDictionary d, BattleEndRes o)
	{
		o.BattleInfo = BaseJsonMarshaler.ParseObject(d, "BattleInfo", JM_NShared_PlayerBattleInfo.Parse);
		o.MatchResult = BaseJsonMarshaler.ParseObject(d, "MatchResult", JM_NShared_MatchResultInfo.Parse);
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.SkipTrigger = BaseJsonMarshaler.ParseValueBool(d, "SkipTrigger");
		return o;
	}

	public static string EncodeJsonString(BattleEndRes o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleEndRes o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("BattleInfo", JM_NShared_PlayerBattleInfo.EncodeJsonObject(o.BattleInfo));
		dictionary.Add("MatchResult", JM_NShared_MatchResultInfo.EncodeJsonObject(o.MatchResult));
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("SkipTrigger", o.SkipTrigger.ToString());
		return dictionary;
	}
}
