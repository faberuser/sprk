using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.EndCampaign;

public static class JM_NShared_EndCampaign_Request
{
	public static NShared.EndCampaign.Request Parse(string s)
	{
		return Parse(s, new NShared.EndCampaign.Request());
	}

	public static NShared.EndCampaign.Request Parse(string s, NShared.EndCampaign.Request o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.EndCampaign.Request Parse(IDictionary d)
	{
		return Parse(d, new NShared.EndCampaign.Request());
	}

	public static NShared.EndCampaign.Request Parse(IDictionary d, NShared.EndCampaign.Request o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.DungeonDifficulty = BaseJsonMarshaler.ParseValueInt(d, "DungeonDifficulty");
		o.ScenarioDungeon = BaseJsonMarshaler.ParseValueBool(d, "ScenarioDungeon");
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
		o.SessionId = BaseJsonMarshaler.ParseValueString(d, "SessionId");
		return o;
	}

	public static string EncodeJsonString(NShared.EndCampaign.Request o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(NShared.EndCampaign.Request o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("DungeonDifficulty", o.DungeonDifficulty.ToString());
		dictionary.Add("ScenarioDungeon", o.ScenarioDungeon.ToString());
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
		if (o.SessionId != null)
		{
			dictionary.Add("SessionId", o.SessionId);
		}
		return dictionary;
	}
}
