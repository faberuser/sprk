using System.Collections;
using MiniJSON;
using NShared;
using NShared.GetAdeventureNotice;

public static class JM_NShared_GetAdeventureNotice_Response
{
	public static NShared.GetAdeventureNotice.Response Parse(string s)
	{
		return Parse(s, new NShared.GetAdeventureNotice.Response());
	}

	public static NShared.GetAdeventureNotice.Response Parse(string s, NShared.GetAdeventureNotice.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetAdeventureNotice.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetAdeventureNotice.Response());
	}

	public static NShared.GetAdeventureNotice.Response Parse(IDictionary d, NShared.GetAdeventureNotice.Response o)
	{
		o.WorldBossInfos = BaseJsonMarshaler.ParseArray(d, "WorldBossInfos", JM_NShared_NWorldBoss_WorldBossInfo.Parse);
		o.GuildRaidInfos = BaseJsonMarshaler.ParseArray(d, "GuildRaidInfos", JM_NShared_GuildRaidDungeonInfo.Parse);
		o.GuildSuppressApplied = BaseJsonMarshaler.ParseValueBool(d, "GuildSuppressApplied");
		o.GuildSuppressSessionInfo = BaseJsonMarshaler.ParseObject(d, "GuildSuppressSessionInfo", JM_NShared_GuildSuppressSessionInfo.Parse);
		o.MatchSeasonData = BaseJsonMarshaler.ParseObject(d, "MatchSeasonData", JM_NShared_MatchSeasonData.Parse);
		o.BanPickSeasonData = BaseJsonMarshaler.ParseObject(d, "BanPickSeasonData", JM_NShared_MatchSeasonData.Parse);
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
