using System.Collections;
using MiniJSON;
using NShared;
using NShared.EndCampaign;

public static class JM_NShared_EndCampaign_Response
{
	public static NShared.EndCampaign.Response Parse(string s)
	{
		return Parse(s, new NShared.EndCampaign.Response());
	}

	public static NShared.EndCampaign.Response Parse(string s, NShared.EndCampaign.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.EndCampaign.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.EndCampaign.Response());
	}

	public static NShared.EndCampaign.Response Parse(IDictionary d, NShared.EndCampaign.Response o)
	{
		RestoredPortal.AcceptServerConquestSessions(d);
		o.ForwardHost = BaseJsonMarshaler.ParseValueString(d, "ForwardHost");
		o.CurrencyResults = BaseJsonMarshaler.ParseArray(d, "CurrencyResults", JM_NShared_CurrencyResultInfo3.Parse);
		o.CampaignResults = BaseJsonMarshaler.ParseArray(d, "CampaignResults", JM_NShared_ChapterDungeonInfo.Parse);
		o.ExpResult = BaseJsonMarshaler.ParseObject(d, "ExpResult", JM_NShared_ExpResultInfo.Parse);
		o.ExpResultsByGetHero = BaseJsonMarshaler.ParseArray(d, "ExpResultsByGetHero", JM_NShared_ExpResultInfo.Parse);
		o.StaminaResult = BaseJsonMarshaler.ParseObject(d, "StaminaResult", JM_NShared_StaminaResultInfo.Parse);
		o.ItemResults = BaseJsonMarshaler.ParseArray(d, "ItemResults", JM_NShared_ItemResultInfo.Parse);
		o.EquipItemInfos = BaseJsonMarshaler.ParseArray(d, "EquipItemInfos", JM_NShared_EquipItemInfo.Parse);
		o.EquipItemBoostInfos = BaseJsonMarshaler.ParseArray(d, "EquipItemBoostInfos", JM_NShared_EquipItemBoostInfo.Parse);
		o.HeroExpResults = BaseJsonMarshaler.ParseArray(d, "HeroExpResults", JM_NShared_HeroExpResultInfo.Parse);
		o.HeroInfos = BaseJsonMarshaler.ParseArray(d, "HeroInfos", JM_NShared_HeroInfo.Parse);
		o.TowerInfos = BaseJsonMarshaler.ParseArray(d, "TowerInfos", JM_NShared_TowerInfo.Parse);
		o.TowerHeroInfos = BaseJsonMarshaler.ParseArray(d, "TowerHeroInfos", JM_NShared_ResultCreatureInfo.Parse);
		o.UndergroundPrisonKeyResult = BaseJsonMarshaler.ParseObject(d, "UndergroundPrisonKeyResult", JM_NShared_StaminaResultInfo.Parse);
		o.FlaskResults = BaseJsonMarshaler.ParseArray(d, "FlaskResults", JM_NShared_FlaskEndCampaignResultInfo.Parse);
		o.FlaskItemResults = BaseJsonMarshaler.ParseArray(d, "FlaskItemResults", JM_NShared_ItemResultInfo.Parse);
		o.CurrencyResults2 = BaseJsonMarshaler.ParseArray(d, "CurrencyResults2", JM_NShared_CurrencyResultInfo3.Parse);
		o.ExpResult2 = BaseJsonMarshaler.ParseObject(d, "ExpResult2", JM_NShared_ExpResultInfo.Parse);
		o.HeroInfos2 = BaseJsonMarshaler.ParseArray(d, "HeroInfos2", JM_NShared_HeroInfo.Parse);
		o.StaminaResult2 = BaseJsonMarshaler.ParseObject(d, "StaminaResult2", JM_NShared_StaminaResultInfo.Parse);
		o.ItemResults2 = BaseJsonMarshaler.ParseArray(d, "ItemResults2", JM_NShared_ItemResultInfo.Parse);
		o.EquipItemInfos2 = BaseJsonMarshaler.ParseArray(d, "EquipItemInfos2", JM_NShared_EquipItemInfo.Parse);
		o.EquipItemBoostInfos2 = BaseJsonMarshaler.ParseArray(d, "EquipItemBoostInfos2", JM_NShared_EquipItemBoostInfo.Parse);
		o.HideoutDungeonResult = BaseJsonMarshaler.ParseObject(d, "HideoutDungeonResult", JM_NShared_HideoutDungeonInfo.Parse);
		o.ItemBoosterResult = BaseJsonMarshaler.ParseObject(d, "ItemBoosterResult", JM_NShared_ItemBoosterResultInfo.Parse);
		o.DailyCompletedCount = BaseJsonMarshaler.ParseValueInt(d, "DailyCompletedCount");
		o.Diff = BaseJsonMarshaler.ParseValueInt(d, "Diff");
		o.WorldMapEventTimeInfos = BaseJsonMarshaler.ParseArray(d, "WorldMapEventTimeInfos", JM_NShared_WorldMapEventTimeInfo.Parse);
		o.WorldMapEventInfos = BaseJsonMarshaler.ParseArray(d, "WorldMapEventInfos", JM_NShared_WorldMapEventInfo.Parse);
		o.RemovedWorldMapEventInfos = BaseJsonMarshaler.ParseArray(d, "RemovedWorldMapEventInfos", JM_NShared_WorldMapEventInfo.Parse);
		o.CompletedRaidInfo = BaseJsonMarshaler.ParseObject(d, "CompletedRaidInfo", JM_NShared_RaidInfo.Parse);
		o.RaidResult = BaseJsonMarshaler.ParseObject(d, "RaidResult", JM_NShared_RaidResultInfo.Parse);
		o.MultiplayMemberIds = BaseJsonMarshaler.ParseArrayCast(d, "MultiplayMemberIds", BaseJsonMarshaler.CastToLong);
		o.MultiplayRewardCalculated = BaseJsonMarshaler.ParseValueBool(d, "MultiplayRewardCalculated");
		o.MultiplayRemainTimeoutTime = BaseJsonMarshaler.ParseValueInt(d, "MultiplayRemainTimeoutTime");
		o.MultiplayRewardItems = BaseJsonMarshaler.ParseArray(d, "MultiplayRewardItems", JM_NShared_RewardItemInfo.Parse);
		o.MultiplayMemberResults = BaseJsonMarshaler.ParseArray(d, "MultiplayMemberResults", JM_NShared_MultiplayMemberResultInfo.Parse);
		o.GuildPointResult = BaseJsonMarshaler.ParseObject(d, "GuildPointResult", JM_NShared_GuildPointResultInfo.Parse);
		o.GuildActivityPointResult = BaseJsonMarshaler.ParseObject(d, "GuildActivityPointResult", JM_NShared_GuildActivityPointResultInfo.Parse);
		o.GuildMemberAccActivityPointResult = BaseJsonMarshaler.ParseObject(d, "GuildMemberAccActivityPointResult", JM_NShared_GuildMemberAccActivityPointResultInfo.Parse);
		o.GuildCurrencyResults = BaseJsonMarshaler.ParseArray(d, "GuildCurrencyResults", JM_NShared_CurrencyResultInfo3.Parse);
		o.GuildRaidInfos = BaseJsonMarshaler.ParseArray(d, "GuildRaidInfos", JM_NShared_GuildRaidDungeonInfo.Parse);
		o.GuildRaidMemberTotalScoreInfo = BaseJsonMarshaler.ParseObject(d, "GuildRaidMemberTotalScoreInfo", JM_NShared_GuildRaidMemberTotalScoreInfo.Parse);
		o.GuildRaidBootyItemRewardInfos = BaseJsonMarshaler.ParseArray(d, "GuildRaidBootyItemRewardInfos", JM_NShared_IndexedRewardItemInfo.Parse);
		o.WorldBossInfo = BaseJsonMarshaler.ParseObject(d, "WorldBossInfo", JM_NShared_NWorldBoss_WorldBossInfo.Parse);
		o.WorldBossRankInfo = BaseJsonMarshaler.ParseObject(d, "WorldBossRankInfo", JM_NShared_NWorldBoss_WorldBossRankerInfo.Parse);
		o.WorldBossKilledByMe = BaseJsonMarshaler.ParseValueBool(d, "WorldBossKilledByMe");
		o.WorldBossKilledRewardInfo = BaseJsonMarshaler.ParseObject(d, "WorldBossKilledRewardInfo", JM_NShared_PolledRewardInfo.Parse);
		o.EventWorldBossInfo = BaseJsonMarshaler.ParseObject(d, "EventWorldBossInfo", JM_NShared_NWorldBoss_EventWorldBossInfo.Parse);
		o.EventWorldBossKilledByMe = BaseJsonMarshaler.ParseValueBool(d, "EventWorldBossKilledByMe");
		o.EventWorldBossScoreInfo = BaseJsonMarshaler.ParseObject(d, "EventWorldBossScoreInfo", JM_NShared_NWorldBoss_EventWorldBossScoreInfo.Parse);
		o.ChallengeRaidRankerInfo = BaseJsonMarshaler.ParseObject(d, "ChallengeRaidRankerInfo", JM_NShared_NRaid_ChallengeRaidRankerInfo.Parse);
		o.GuildSuppressInfo = BaseJsonMarshaler.ParseObject(d, "GuildSuppressInfo", JM_NShared_GuildSuppressInfo.Parse);
		o.GuildSuppressScoreInfo = BaseJsonMarshaler.ParseObject(d, "GuildSuppressScoreInfo", JM_NShared_GuildSuppressScoreInfo.Parse);
		o.GuildSuppressPlayInfo = BaseJsonMarshaler.ParseObject(d, "GuildSuppressPlayInfo", JM_NShared_GuildSuppressPlayInfo.Parse);
		o.WearCostumeCount = BaseJsonMarshaler.ParseValueInt(d, "WearCostumeCount");
		o.ConquestDungeonResult = BaseJsonMarshaler.ParseObject(d, "ConquestDungeonResult", JM_NShared_ConquestDungeonInfo.Parse);
		o.GodkingTrialDungeonInfo = BaseJsonMarshaler.ParseObject(d, "GodkingTrialDungeonInfo", JM_NShared_GodkingTrialDungeonInfo.Parse);
		o.EclipseStaminaResult = BaseJsonMarshaler.ParseObject(d, "EclipseStaminaResult", JM_NShared_StaminaResultInfo.Parse);
		o.EclipseDungeonInfo = BaseJsonMarshaler.ParseObject(d, "EclipseDungeonInfo", JM_NShared_EclipseDungeonInfo.Parse);
		o.UnderPrisonInfos = BaseJsonMarshaler.ParseArray(d, "UnderPrisonInfos", JM_NShared_UnderPrisonInfo.Parse);
		o.SelectedRewardItemCodes = BaseJsonMarshaler.ParseArrayCast(d, "SelectedRewardItemCodes", BaseJsonMarshaler.CastToString);
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
