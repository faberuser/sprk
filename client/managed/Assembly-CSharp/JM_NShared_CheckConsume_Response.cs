using System.Collections;
using MiniJSON;
using NShared;
using NShared.CheckConsume;

public static class JM_NShared_CheckConsume_Response
{
	public static NShared.CheckConsume.Response Parse(string s)
	{
		return Parse(s, new NShared.CheckConsume.Response());
	}

	public static NShared.CheckConsume.Response Parse(string s, NShared.CheckConsume.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.CheckConsume.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.CheckConsume.Response());
	}

	public static NShared.CheckConsume.Response Parse(IDictionary d, NShared.CheckConsume.Response o)
	{
		o.GemResult = BaseJsonMarshaler.ParseObject(d, "GemResult", JM_NShared_GemResultInfo.Parse);
		o.GoldResult = BaseJsonMarshaler.ParseObject(d, "GoldResult", JM_NShared_GoldResultInfo.Parse);
		o.PvpCoinResult = BaseJsonMarshaler.ParseObject(d, "PvpCoinResult", JM_NShared_PvpCoinResultInfo.Parse);
		o.purchaseTimeDuration = BaseJsonMarshaler.ParseObject(d, "purchaseTimeDuration", JM_NShared_PlayerPurchaseTimeDurationInfo.Parse);
		o.firstPurchase = BaseJsonMarshaler.ParseObject(d, "firstPurchase", JM_NShared_PlayerFirstPurchaseInfo.Parse);
		o.PlayerProductPurchaseInfo = BaseJsonMarshaler.ParseObject(d, "PlayerProductPurchaseInfo", JM_NShared_PlayerProductPurchaseInfo.Parse);
		o.MileageResult = BaseJsonMarshaler.ParseObject(d, "MileageResult", JM_NShared_MileageResultInfo.Parse);
		o.ItemResults = BaseJsonMarshaler.ParseArray(d, "ItemResults", JM_NShared_ItemResultInfo.Parse);
		o.EquipItemInfos = BaseJsonMarshaler.ParseArray(d, "EquipItemInfos", JM_NShared_EquipItemInfo.Parse);
		o.ItemTimeDurationInfo = BaseJsonMarshaler.ParseObject(d, "ItemTimeDurationInfo", JM_NShared_PlayerItemTimeDurationInfo.Parse);
		o.PurchaseMarketingResult = BaseJsonMarshaler.ParseObject(d, "PurchaseMarketingResult", JM_NShared_PurchaseMarketingResultInfo.Parse);
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
