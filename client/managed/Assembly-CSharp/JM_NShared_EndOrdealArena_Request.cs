using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.EndOrdealArena;

public static class JM_NShared_EndOrdealArena_Request
{
	public static NShared.EndOrdealArena.Request Parse(string s)
	{
		return Parse(s, new NShared.EndOrdealArena.Request());
	}

	public static NShared.EndOrdealArena.Request Parse(string s, NShared.EndOrdealArena.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.EndOrdealArena.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.EndOrdealArena.Request());
	}

	public static NShared.EndOrdealArena.Request Parse(IDictionary d, NShared.EndOrdealArena.Request o)
	{
		o.NodeIndex = BaseJsonMarshaler.ParseValueInt(d, "NodeIndex");
		o.HeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "HeroIndices", BaseJsonMarshaler.CastToInt);
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.Completed = BaseJsonMarshaler.ParseValueBool(d, "Completed");
		o.BattleLogString = BaseJsonMarshaler.ParseValueString(d, "BattleLogString");
		o.CreatureInfoString = BaseJsonMarshaler.ParseValueString(d, "CreatureInfoString");
		o.PureBattleTime = BaseJsonMarshaler.ParseValueInt(d, "PureBattleTime");
		o.ElapsedBattleTime = BaseJsonMarshaler.ParseValueLong(d, "ElapsedBattleTime");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.EndOrdealArena.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.EndOrdealArena.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("NodeIndex", o.NodeIndex.ToString());
		dictionary.Add("HeroIndices", BaseJsonMarshaler.EncodeArray(o.HeroIndices));
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("Completed", o.Completed.ToString());
		if (o.BattleLogString != null)
		{
			dictionary.Add("BattleLogString", o.BattleLogString);
		}
		if (o.CreatureInfoString != null)
		{
			dictionary.Add("CreatureInfoString", o.CreatureInfoString);
		}
		dictionary.Add("PureBattleTime", o.PureBattleTime.ToString());
		dictionary.Add("ElapsedBattleTime", o.ElapsedBattleTime.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
