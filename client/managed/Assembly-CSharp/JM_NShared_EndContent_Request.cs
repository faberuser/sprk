using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.EndContent;

public static class JM_NShared_EndContent_Request
{
	public static NShared.EndContent.Request Parse(string s)
	{
		return Parse(s, new NShared.EndContent.Request());
	}

	public static NShared.EndContent.Request Parse(string s, NShared.EndContent.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.EndContent.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.EndContent.Request());
	}

	public static NShared.EndContent.Request Parse(IDictionary d, NShared.EndContent.Request o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.DungeonDifficulty = BaseJsonMarshaler.ParseValueInt(d, "DungeonDifficulty");
		o.Completed = BaseJsonMarshaler.ParseValueBool(d, "Completed");
		o.Star = BaseJsonMarshaler.ParseValueInt(d, "Star");
		o.MonsterIndices = BaseJsonMarshaler.ParseArrayCast(d, "MonsterIndices", BaseJsonMarshaler.CastToInt);
		o.KillCounts = BaseJsonMarshaler.ParseArrayCast(d, "KillCounts", BaseJsonMarshaler.CastToInt);
		o.AliveHeroIndices = BaseJsonMarshaler.ParseArrayCast(d, "AliveHeroIndices", BaseJsonMarshaler.CastToInt);
		o.BattleLogString = BaseJsonMarshaler.ParseValueString(d, "BattleLogString");
		o.CreatureInfoString = BaseJsonMarshaler.ParseValueString(d, "CreatureInfoString");
		o.PureBattleTime = BaseJsonMarshaler.ParseValueInt(d, "PureBattleTime");
		o.ElapsedBattleTime = BaseJsonMarshaler.ParseValueLong(d, "ElapsedBattleTime");
		o.TotalDamage = BaseJsonMarshaler.ParseValueLong(d, "TotalDamage");
		o.GroupIndex = BaseJsonMarshaler.ParseValueInt(d, "GroupIndex");
		o.DungeonType = BaseJsonMarshaler.ParseEnum<EPunishmentDungeonType>(d, "DungeonType");
		o.ClearWave = BaseJsonMarshaler.ParseValueInt(d, "ClearWave");
		o.EndWave = BaseJsonMarshaler.ParseValueInt(d, "EndWave");
		o.EndKillCount = BaseJsonMarshaler.ParseValueInt(d, "EndKillCount");
		o.TotalKillCount = BaseJsonMarshaler.ParseValueInt(d, "TotalKillCount");
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.EndContent.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.EndContent.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("DungeonDifficulty", o.DungeonDifficulty.ToString());
		dictionary.Add("Completed", o.Completed.ToString());
		dictionary.Add("Star", o.Star.ToString());
		dictionary.Add("MonsterIndices", BaseJsonMarshaler.EncodeArray(o.MonsterIndices));
		dictionary.Add("KillCounts", BaseJsonMarshaler.EncodeArray(o.KillCounts));
		dictionary.Add("AliveHeroIndices", BaseJsonMarshaler.EncodeArray(o.AliveHeroIndices));
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
		dictionary.Add("TotalDamage", o.TotalDamage.ToString());
		dictionary.Add("GroupIndex", o.GroupIndex.ToString());
		dictionary.Add("DungeonType", o.DungeonType.ToString());
		dictionary.Add("ClearWave", o.ClearWave.ToString());
		dictionary.Add("EndWave", o.EndWave.ToString());
		dictionary.Add("EndKillCount", o.EndKillCount.ToString());
		dictionary.Add("TotalKillCount", o.TotalKillCount.ToString());
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
