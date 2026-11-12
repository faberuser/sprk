using System.Collections;
using MiniJSON;
using NShared;
using NShared.BeginCampaign;

public static class JM_NShared_BeginCampaign_Response
{
	public static NShared.BeginCampaign.Response Parse(string s)
	{
		return Parse(s, new NShared.BeginCampaign.Response());
	}

	public static NShared.BeginCampaign.Response Parse(string s, NShared.BeginCampaign.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.BeginCampaign.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.BeginCampaign.Response());
	}

	public static NShared.BeginCampaign.Response Parse(IDictionary d, NShared.BeginCampaign.Response o)
	{
		RestoredPortal.AcceptServerConquestSessions(d);
		o.DropItems = BaseJsonMarshaler.ParseArray(d, "DropItems", JM_NShared_RewardItemInfo.Parse);
		o.DropGold = BaseJsonMarshaler.ParseValueInt(d, "DropGold");
		o.StaminaResult = BaseJsonMarshaler.ParseObject(d, "StaminaResult", JM_NShared_StaminaResultInfo.Parse);
		o.CurrencyResults2 = BaseJsonMarshaler.ParseArray(d, "CurrencyResults2", JM_NShared_CurrencyResultInfo3.Parse);
		o.GemResult = BaseJsonMarshaler.ParseObject(d, "GemResult", JM_NShared_GemResultInfo.Parse);
		o.CurrencyResults = BaseJsonMarshaler.ParseArray(d, "CurrencyResults", JM_NShared_CurrencyResultInfo.Parse);
		o.RechargeTime = BaseJsonMarshaler.ParseValueString(d, "RechargeTime");
		o.BattleServerAddress = BaseJsonMarshaler.ParseValueString(d, "BattleServerAddress");
		o.BattleServerPort = BaseJsonMarshaler.ParseValueInt(d, "BattleServerPort");
		o.CheckIntegrity = BaseJsonMarshaler.ParseValueBool(d, "CheckIntegrity");
		o.TowerIntialUserHeroes = BaseJsonMarshaler.ParseArray(d, "TowerIntialUserHeroes", JM_NShared_ResultCreatureInfo.Parse);
		o.TowerIntialEnemyHeroes = BaseJsonMarshaler.ParseArray(d, "TowerIntialEnemyHeroes", JM_NShared_ResultCreatureInfo.Parse);
		o.WorldBossInfo = BaseJsonMarshaler.ParseObject(d, "WorldBossInfo", JM_NShared_NWorldBoss_WorldBossInfo.Parse);
		o.WorldBossDailyScore = BaseJsonMarshaler.ParseObject(d, "WorldBossDailyScore", JM_NShared_WorldBossDailyScore.Parse);
		o.EventWorldBossInfo = BaseJsonMarshaler.ParseObject(d, "EventWorldBossInfo", JM_NShared_NWorldBoss_EventWorldBossInfo.Parse);
		o.GodkingTrialDungeonInfo = BaseJsonMarshaler.ParseObject(d, "GodkingTrialDungeonInfo", JM_NShared_GodkingTrialDungeonInfo.Parse);
		o.ItemBoosterResult = BaseJsonMarshaler.ParseObject(d, "ItemBoosterResult", JM_NShared_ItemBoosterResultInfo.Parse);
		o.ItemTimeDurationInfo = BaseJsonMarshaler.ParseObject(d, "ItemTimeDurationInfo", JM_NShared_PlayerItemTimeDurationInfo.Parse);
		o.GuildSuppressInfo = BaseJsonMarshaler.ParseObject(d, "GuildSuppressInfo", JM_NShared_GuildSuppressInfo.Parse);
		o.GuildSuppressScoreInfo = BaseJsonMarshaler.ParseObject(d, "GuildSuppressScoreInfo", JM_NShared_GuildSuppressScoreInfo.Parse);
		o.GuildSuppressPlayInfo = BaseJsonMarshaler.ParseObject(d, "GuildSuppressPlayInfo", JM_NShared_GuildSuppressPlayInfo.Parse);
		o.WearCostumeCount = BaseJsonMarshaler.ParseValueInt(d, "WearCostumeCount");
		o.GuildRaidDungeonInfo = BaseJsonMarshaler.ParseObject(d, "GuildRaidDungeonInfo", JM_NShared_GuildRaidDungeonInfo.Parse);
		o.EclipseBattleInfo = BaseJsonMarshaler.ParseObject(d, "EclipseBattleInfo", JM_NShared_EclipseBattleInfo.Parse);
		o.Result = BaseJsonMarshaler.ParseEnum<ResultType>(d, "Result");
		o.BaseResult = BaseJsonMarshaler.ParseEnum<BaseResultType>(d, "BaseResult");
		o.InternalErrorMessage = BaseJsonMarshaler.ParseValueString(d, "InternalErrorMessage");
		o.AchievementInfos = BaseJsonMarshaler.ParseArray(d, "AchievementInfos", JM_NShared_AchievementInfo.Parse);
		o.ReservedScenarioQuestInfo = BaseJsonMarshaler.ParseObject(d, "ReservedScenarioQuestInfo", JM_NShared_ScenarioQuestInfo.Parse);
		o.ReservedStaminaResults = BaseJsonMarshaler.ParseArray(d, "ReservedStaminaResults", JM_NShared_StaminaResultInfo.Parse);
		o.ReservedPlayRecordInfos = BaseJsonMarshaler.ParseArray(d, "ReservedPlayRecordInfos", JM_NShared_PlayRecordInfo.Parse);
		o.ReservedPlayerArchiveInfos = BaseJsonMarshaler.ParseArray(d, "ReservedPlayerArchiveInfos", JM_NShared_PlayerArchiveInfo.Parse);
		o.ReservedMainQuestInfo = BaseJsonMarshaler.ParseObject(d, "ReservedMainQuestInfo", JM_NShared_MainQuestInfo.Parse);
		o.ReservedSubQuestInfos = BaseJsonMarshaler.ParseArray(d, "ReservedSubQuestInfos", JM_NShared_SubQuestInfo.Parse);
		o.ReservedClearMissionInfos = BaseJsonMarshaler.ParseArray(d, "ReservedClearMissionInfos", JM_NShared_ClearMissionInfo.Parse);
		o.ReservedUpdatedEquipItemInfos = BaseJsonMarshaler.ParseArray(d, "ReservedUpdatedEquipItemInfos", JM_NShared_UpdatedEquipItemInfo.Parse);
		o.ReservedPurchaseMarketingInfos = BaseJsonMarshaler.ParseArray(d, "ReservedPurchaseMarketingInfos", JM_NShared_PurchaseMarketingInfo.Parse);
		o.ReservePlayerKingsPassExpInfo = BaseJsonMarshaler.ParseObject(d, "ReservePlayerKingsPassExpInfo", JM_NShared_PlayerKingsPassExp.Parse);
		o.ReservedKingsPassMissionInfos = BaseJsonMarshaler.ParseArray(d, "ReservedKingsPassMissionInfos", JM_NShared_KingsPassMission.Parse);
		o.ReservedCurrencyResults = BaseJsonMarshaler.ParseArray(d, "ReservedCurrencyResults", JM_NShared_CurrencyResultInfo3.Parse);
		o.ReservedNoticeInfos = BaseJsonMarshaler.ParseArray(d, "ReservedNoticeInfos", JM_NShared_Notice.Parse);
		o.ReserveTeamLevelBuffInfos = BaseJsonMarshaler.ParseArray(d, "ReserveTeamLevelBuffInfos", JM_NShared_TeamLevelBuff.Parse);
		return o;
	}
}
