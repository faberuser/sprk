using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;

public static class JM_NShared_WaveReadyBattleLog
{
	public static WaveReadyBattleLog Parse(string s)
	{
		return Parse(s, new WaveReadyBattleLog());
	}

	public static WaveReadyBattleLog Parse(string s, WaveReadyBattleLog o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static WaveReadyBattleLog Parse(IDictionary d)
	{
		return Parse(d, new WaveReadyBattleLog());
	}

	public static WaveReadyBattleLog Parse(IDictionary d, WaveReadyBattleLog o)
	{
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.Difficulty = BaseJsonMarshaler.ParseEnum<DungeonDifficultyType>(d, "Difficulty");
		o.Scenario = BaseJsonMarshaler.ParseValueBool(d, "Scenario");
		o.WaveIndex = BaseJsonMarshaler.ParseValueInt(d, "WaveIndex");
		o.MaxWaveIndex = BaseJsonMarshaler.ParseValueInt(d, "MaxWaveIndex");
		o.MaxStageLevel = BaseJsonMarshaler.ParseValueInt(d, "MaxStageLevel");
		o.TotalWaveIndex = BaseJsonMarshaler.ParseValueInt(d, "TotalWaveIndex");
		o.MonsterCreateInfos = BaseJsonMarshaler.ParseArray(d, "MonsterCreateInfos", JM_NShared_BattleCreatureCreateInfo.Parse);
		o.AllianceCreateInfos = BaseJsonMarshaler.ParseArray(d, "AllianceCreateInfos", JM_NShared_BattleCreatureCreateInfo.Parse);
		o.WaveCenterPositionInfos = BaseJsonMarshaler.ParseArray(d, "WaveCenterPositionInfos", JM_NShared_WaveCenterPositionInfo.Parse);
		return SprkConquest.ReadSeed(o, d);
	}

	public static string EncodeJsonString(WaveReadyBattleLog o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(WaveReadyBattleLog o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("Difficulty", o.Difficulty.ToString());
		dictionary.Add("Scenario", o.Scenario.ToString());
		dictionary.Add("WaveIndex", o.WaveIndex.ToString());
		dictionary.Add("MaxWaveIndex", o.MaxWaveIndex.ToString());
		dictionary.Add("MaxStageLevel", o.MaxStageLevel.ToString());
		dictionary.Add("TotalWaveIndex", o.TotalWaveIndex.ToString());
		dictionary.Add("MonsterCreateInfos", BaseJsonMarshaler.EncodeArray(o.MonsterCreateInfos, JM_NShared_BattleCreatureCreateInfo.EncodeJsonObject));
		dictionary.Add("AllianceCreateInfos", BaseJsonMarshaler.EncodeArray(o.AllianceCreateInfos, JM_NShared_BattleCreatureCreateInfo.EncodeJsonObject));
		dictionary.Add("WaveCenterPositionInfos", BaseJsonMarshaler.EncodeArray(o.WaveCenterPositionInfos, JM_NShared_WaveCenterPositionInfo.EncodeJsonObject));
		return dictionary;
	}
}
