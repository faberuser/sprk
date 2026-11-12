using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.NRoomMessage.NProtocol;

public static class JM_NShared_NRoomMessage_NProtocol_BattleStart
{
	public static BattleStart Parse(string s)
	{
		return Parse(s, new BattleStart());
	}

	public static BattleStart Parse(string s, BattleStart o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static BattleStart Parse(IDictionary d)
	{
		return Parse(d, new BattleStart());
	}

	public static BattleStart Parse(IDictionary d, BattleStart o)
	{
		o.Key = BaseJsonMarshaler.ParseValueInt(d, "Key");
		o.TowerFloor = BaseJsonMarshaler.ParseValueInt(d, "TowerFloor");
		o.IsTowerBattle = BaseJsonMarshaler.ParseValueBool(d, "IsTowerBattle");
		o.BattleType = BaseJsonMarshaler.ParseValueInt(d, "BattleType");
		o.BattleDefine = BaseJsonMarshaler.ParseObject(d, "BattleDefine", JM_NShared_BattleDefineData.Parse);
		o.BattleMenu = BaseJsonMarshaler.ParseEnum<EBattleMenu>(d, "BattleMenu");
		o.PreloadFieldname = BaseJsonMarshaler.ParseValueString(d, "PreloadFieldname");
		o.PreloadDungeonMonster = BaseJsonMarshaler.ParseValueBool(d, "PreloadDungeonMonster");
		o.PreloadCreatureInfos = BaseJsonMarshaler.ParseList<CachedHeroInfo, List<CachedHeroInfo>>(d, "PreloadCreatureInfos", JM_NShared_CachedHeroInfo.Parse);
		o.ChapterIndex = BaseJsonMarshaler.ParseValueInt(d, "ChapterIndex");
		o.DungeonIndex = BaseJsonMarshaler.ParseValueInt(d, "DungeonIndex");
		o.LinkedIndex = BaseJsonMarshaler.ParseValueInt(d, "LinkedIndex");
		o.WorldMapEnventIndex = BaseJsonMarshaler.ParseValueInt(d, "WorldMapEnventIndex");
		o.WorldMapNodeIndex = BaseJsonMarshaler.ParseValueInt(d, "WorldMapNodeIndex");
		o.IsSenarioDungeon = BaseJsonMarshaler.ParseValueBool(d, "IsSenarioDungeon");
		o.DifficultType = BaseJsonMarshaler.ParseEnum<DungeonDifficultyType>(d, "DifficultType");
		o.DropItems = BaseJsonMarshaler.ParseArray(d, "DropItems", JM_NShared_RewardItemInfo.Parse);
		o.DropGold = BaseJsonMarshaler.ParseValueInt(d, "DropGold");
		o.TowerIntialUserHeroes = BaseJsonMarshaler.ParseArray(d, "TowerIntialUserHeroes", JM_NShared_ResultCreatureInfo.Parse);
		o.TowerIntialEnemyHeroes = BaseJsonMarshaler.ParseArray(d, "TowerIntialEnemyHeroes", JM_NShared_ResultCreatureInfo.Parse);
		o.BattleLogs = BaseJsonMarshaler.ParseArray(d, "BattleLogs", JM_NShared_BaseBattleLog.Parse);
		o.ArenaMatchInfo = BaseJsonMarshaler.ParseObject(d, "ArenaMatchInfo", JM_NShared_MatchInfo.Parse);
		o.SeasonData = BaseJsonMarshaler.ParseObject(d, "SeasonData", JM_NShared_MatchSeasonData.Parse);
		o.ArenaReplayInfo = BaseJsonMarshaler.ParseObject(d, "ArenaReplayInfo", JM_NShared_ArenaReplayInfo.Parse);
		o.CampaignReplayInfo = BaseJsonMarshaler.ParseObject(d, "CampaignReplayInfo", JM_NShared_CampaignDungeonData.Parse);
		o.OfflineArenaLeftTeam = BaseJsonMarshaler.ParseObject(d, "OfflineArenaLeftTeam", JM_NShared_ReplayTeamInfo.Parse);
		o.MatchedNpcInfo = BaseJsonMarshaler.ParseObject(d, "MatchedNpcInfo", JM_NShared_MatchedAccountInfo.Parse);
		o.OfflineMyMatchInfo = BaseJsonMarshaler.ParseObject(d, "OfflineMyMatchInfo", JM_NShared_MatchedAccountInfo.Parse);
		o.OriginalContextTypeName = BaseJsonMarshaler.ParseValueString(d, "OriginalContextTypeName");
		return o;
	}

	public static string EncodeJsonString(BattleStart o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(BattleStart o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		dictionary.Add("Key", o.Key.ToString());
		dictionary.Add("TowerFloor", o.TowerFloor.ToString());
		dictionary.Add("IsTowerBattle", o.IsTowerBattle.ToString());
		dictionary.Add("BattleType", o.BattleType.ToString());
		dictionary.Add("BattleDefine", JM_NShared_BattleDefineData.EncodeJsonObject(o.BattleDefine));
		dictionary.Add("BattleMenu", o.BattleMenu.ToString());
		if (o.PreloadFieldname != null)
		{
			dictionary.Add("PreloadFieldname", o.PreloadFieldname);
		}
		dictionary.Add("PreloadDungeonMonster", o.PreloadDungeonMonster.ToString());
		dictionary.Add("PreloadCreatureInfos", BaseJsonMarshaler.EncodeArray(o.PreloadCreatureInfos, JM_NShared_CachedHeroInfo.EncodeJsonObject));
		dictionary.Add("ChapterIndex", o.ChapterIndex.ToString());
		dictionary.Add("DungeonIndex", o.DungeonIndex.ToString());
		dictionary.Add("LinkedIndex", o.LinkedIndex.ToString());
		dictionary.Add("WorldMapEnventIndex", o.WorldMapEnventIndex.ToString());
		dictionary.Add("WorldMapNodeIndex", o.WorldMapNodeIndex.ToString());
		dictionary.Add("IsSenarioDungeon", o.IsSenarioDungeon.ToString());
		dictionary.Add("DifficultType", o.DifficultType.ToString());
		dictionary.Add("DropItems", BaseJsonMarshaler.EncodeArray(o.DropItems, JM_NShared_RewardItemInfo.EncodeJsonObject));
		dictionary.Add("DropGold", o.DropGold.ToString());
		dictionary.Add("TowerIntialUserHeroes", BaseJsonMarshaler.EncodeArray(o.TowerIntialUserHeroes, JM_NShared_ResultCreatureInfo.EncodeJsonObject));
		dictionary.Add("TowerIntialEnemyHeroes", BaseJsonMarshaler.EncodeArray(o.TowerIntialEnemyHeroes, JM_NShared_ResultCreatureInfo.EncodeJsonObject));
		dictionary.Add("BattleLogs", BaseJsonMarshaler.EncodeArray(o.BattleLogs, JM_NShared_BaseBattleLog.EncodeJsonObject));
		dictionary.Add("ArenaMatchInfo", JM_NShared_MatchInfo.EncodeJsonObject(o.ArenaMatchInfo));
		dictionary.Add("SeasonData", JM_NShared_MatchSeasonData.EncodeJsonObject(o.SeasonData));
		dictionary.Add("ArenaReplayInfo", JM_NShared_ArenaReplayInfo.EncodeJsonObject(o.ArenaReplayInfo));
		dictionary.Add("CampaignReplayInfo", JM_NShared_CampaignDungeonData.EncodeJsonObject(o.CampaignReplayInfo));
		dictionary.Add("OfflineArenaLeftTeam", JM_NShared_ReplayTeamInfo.EncodeJsonObject(o.OfflineArenaLeftTeam));
		dictionary.Add("MatchedNpcInfo", JM_NShared_MatchedAccountInfo.EncodeJsonObject(o.MatchedNpcInfo));
		dictionary.Add("OfflineMyMatchInfo", JM_NShared_MatchedAccountInfo.EncodeJsonObject(o.OfflineMyMatchInfo));
		if (o.OriginalContextTypeName != null)
		{
			dictionary.Add("OriginalContextTypeName", o.OriginalContextTypeName);
		}
		return dictionary;
	}
}
