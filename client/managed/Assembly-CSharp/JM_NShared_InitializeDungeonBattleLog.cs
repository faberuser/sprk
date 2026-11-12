using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_InitializeDungeonBattleLog
{
	public static InitializeDungeonBattleLog Parse(string s)
	{
		return Parse(s, new InitializeDungeonBattleLog());
	}

	public static InitializeDungeonBattleLog Parse(string s, InitializeDungeonBattleLog o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static InitializeDungeonBattleLog Parse(IDictionary d)
	{
		return Parse(d, new InitializeDungeonBattleLog());
	}

	public static InitializeDungeonBattleLog Parse(IDictionary d, InitializeDungeonBattleLog o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.PlayerId = BaseJsonMarshaler.ParseValueLong(d, "PlayerId");
		o.CreateInfos = BaseJsonMarshaler.ParseArray(d, "CreateInfos", JM_NShared_BattleCreatureCreateInfo.Parse);
		return o;
	}

	public static string EncodeJsonString(InitializeDungeonBattleLog o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(InitializeDungeonBattleLog o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("PlayerId", o.PlayerId.ToString());
		dictionary.Add("CreateInfos", BaseJsonMarshaler.EncodeArray(o.CreateInfos, JM_NShared_BattleCreatureCreateInfo.EncodeJsonObject));
		return dictionary;
	}
}
