using System.Collections;
using MiniJSON;
using NShared;
using NShared.ExecEquipGacha;

public static class JM_NShared_ExecEquipGacha_Response
{
	public static NShared.ExecEquipGacha.Response Parse(string s)
	{
		return Parse(s, new NShared.ExecEquipGacha.Response());
	}

	public static NShared.ExecEquipGacha.Response Parse(string s, NShared.ExecEquipGacha.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ExecEquipGacha.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.ExecEquipGacha.Response());
	}

	public static NShared.ExecEquipGacha.Response Parse(IDictionary d, NShared.ExecEquipGacha.Response o)
	{
		o.GachaInfo = BaseJsonMarshaler.ParseObject(d, "GachaInfo", JM_NShared_FreeEquipGachaInfo.Parse);
		o.EquipGachaInfo = BaseJsonMarshaler.ParseObject(d, "EquipGachaInfo", JM_NShared_EquipGachaInfo.Parse);
		o.CurrencyResults = BaseJsonMarshaler.ParseArray(d, "CurrencyResults", JM_NShared_CurrencyResultInfo3.Parse);
		o.GachaItemResults = BaseJsonMarshaler.ParseArray(d, "GachaItemResults", JM_NShared_GachaItemResultInfo.Parse);
		o.ItemResult = BaseJsonMarshaler.ParseObject(d, "ItemResult", JM_NShared_ItemResultInfo.Parse);
		o.StepUpRewardInfo = BaseJsonMarshaler.ParseObject(d, "StepUpRewardInfo", JM_NShared_RewardResultInfo.Parse);
		o.StarterPickupRewardInfo = BaseJsonMarshaler.ParseObject(d, "StarterPickupRewardInfo", JM_NShared_RewardResultInfo.Parse);
		o.StepUpEquipGachaInfo = BaseJsonMarshaler.ParseObject(d, "StepUpEquipGachaInfo", JM_NShared_StepUpEquipGachaInfo.Parse);
		o.StarterPickupEquipGachaInfo = BaseJsonMarshaler.ParseObject(d, "StarterPickupEquipGachaInfo", JM_NShared_StarterPickupEquipGachaInfo.Parse);
		o.PetGachaCountInfos = BaseJsonMarshaler.ParseArray(d, "PetGachaCountInfos", JM_NShared_PetGachaCountInfo.Parse);
		o.PetRewardInfo = BaseJsonMarshaler.ParseObject(d, "PetRewardInfo", JM_NShared_RewardResultInfo.Parse);
		o.GachaCeilingCountInfos = BaseJsonMarshaler.ParseArray(d, "GachaCeilingCountInfos", JM_NShared_GachaCeilingCountInfo.Parse);
		o.GachaCeilingRewardInfo = BaseJsonMarshaler.ParseObject(d, "GachaCeilingRewardInfo", JM_NShared_RewardResultInfo.Parse);
		o.PlayerProductPurchaseInfo = BaseJsonMarshaler.ParseObject(d, "PlayerProductPurchaseInfo", JM_NShared_PlayerProductPurchaseInfo.Parse);
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
