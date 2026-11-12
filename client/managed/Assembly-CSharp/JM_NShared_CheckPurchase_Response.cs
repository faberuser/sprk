using System.Collections;
using MiniJSON;
using NShared;
using NShared.CheckPurchase;

public static class JM_NShared_CheckPurchase_Response
{
	public static NShared.CheckPurchase.Response Parse(string s)
	{
		return Parse(s, new NShared.CheckPurchase.Response());
	}

	public static NShared.CheckPurchase.Response Parse(string s, NShared.CheckPurchase.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.CheckPurchase.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.CheckPurchase.Response());
	}

	public static NShared.CheckPurchase.Response Parse(IDictionary d, NShared.CheckPurchase.Response o)
	{
		o.CurrencyResults = BaseJsonMarshaler.ParseArray(d, "CurrencyResults", JM_NShared_CurrencyResultInfo3.Parse);
		o.purchaseTimeDuration = BaseJsonMarshaler.ParseObject(d, "purchaseTimeDuration", JM_NShared_PlayerPurchaseTimeDurationInfo.Parse);
		o.firstPurchase = BaseJsonMarshaler.ParseObject(d, "firstPurchase", JM_NShared_PlayerFirstPurchaseInfo.Parse);
		o.PlayerProductPurchaseInfo = BaseJsonMarshaler.ParseObject(d, "PlayerProductPurchaseInfo", JM_NShared_PlayerProductPurchaseInfo.Parse);
		o.ItemResults = BaseJsonMarshaler.ParseArray(d, "ItemResults", JM_NShared_ItemResultInfo.Parse);
		o.EquipItemInfos = BaseJsonMarshaler.ParseArray(d, "EquipItemInfos", JM_NShared_EquipItemInfo.Parse);
		o.HeroAddResults = BaseJsonMarshaler.ParseArray(d, "HeroAddResults", JM_NShared_HeroAddResultInfo.Parse);
		o.Attendance = BaseJsonMarshaler.ParseObject(d, "Attendance", JM_NShared_AttendanceInfo.Parse);
		o.AttendanceDatas = BaseJsonMarshaler.ParseArray(d, "AttendanceDatas", JM_NShared_AttendanceData.Parse);
		o.AttendanceRewardableDays = BaseJsonMarshaler.ParseArrayCast(d, "AttendanceRewardableDays", BaseJsonMarshaler.CastToInt);
		o.ItemTimeDurationInfo = BaseJsonMarshaler.ParseObject(d, "ItemTimeDurationInfo", JM_NShared_PlayerItemTimeDurationInfo.Parse);
		o.PurchaseMarketingResult = BaseJsonMarshaler.ParseObject(d, "PurchaseMarketingResult", JM_NShared_PurchaseMarketingResultInfo.Parse);
		o.NewbieMissionInfos = BaseJsonMarshaler.ParseArray(d, "NewbieMissionInfos", JM_NShared_NewbieMissionInfo.Parse);
		o.PlayerAnyMiscs = BaseJsonMarshaler.ParseArray(d, "PlayerAnyMiscs", JM_NShared_PlayerAnyMisc.Parse);
		o.OpendMissionCategories = BaseJsonMarshaler.ParseArray(d, "OpendMissionCategories", JM_NShared_MissionCategoryInfo.Parse);
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
