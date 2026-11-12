using System.Collections;
using MiniJSON;
using NShared;
using NShared.GetGuildSuppressSessionInfo;

public static class JM_NShared_GetGuildSuppressSessionInfo_Response
{
	public static NShared.GetGuildSuppressSessionInfo.Response Parse(string s)
	{
		return Parse(s, new NShared.GetGuildSuppressSessionInfo.Response());
	}

	public static NShared.GetGuildSuppressSessionInfo.Response Parse(string s, NShared.GetGuildSuppressSessionInfo.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GetGuildSuppressSessionInfo.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.GetGuildSuppressSessionInfo.Response());
	}

	public static NShared.GetGuildSuppressSessionInfo.Response Parse(IDictionary d, NShared.GetGuildSuppressSessionInfo.Response o)
	{
		RestoredPortal.AcceptServerConquestSessions(d);
		o.GuildSuppressSessionInfo = BaseJsonMarshaler.ParseObject(d, "GuildSuppressSessionInfo", JM_NShared_GuildSuppressSessionInfo.Parse);
		o.GuildSuppressSeasonInfo = BaseJsonMarshaler.ParseObject(d, "GuildSuppressSeasonInfo", JM_NShared_GuildSuppressSeasonInfo.Parse);
		o.GuildSuppressApplied = BaseJsonMarshaler.ParseValueBool(d, "GuildSuppressApplied");
		o.GuildSuppressInfo = BaseJsonMarshaler.ParseObject(d, "GuildSuppressInfo", JM_NShared_GuildSuppressInfo.Parse);
		o.GuildSuppressPlayInfos = BaseJsonMarshaler.ParseArray(d, "GuildSuppressPlayInfos", JM_NShared_GuildSuppressPlayInfo.Parse);
		o.StaminaResult = BaseJsonMarshaler.ParseObject(d, "StaminaResult", JM_NShared_StaminaResultInfo.Parse);
		o.AutoApplySuppress = BaseJsonMarshaler.ParseValueInt(d, "AutoApplySuppress");
		o.GuildSuppressBanInfo = BaseJsonMarshaler.ParseObject(d, "GuildSuppressBanInfo", JM_NShared_GuildSuppressBanInfo.Parse);
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
