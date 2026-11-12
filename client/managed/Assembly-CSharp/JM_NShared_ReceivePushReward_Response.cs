using System.Collections;
using MiniJSON;
using NShared;
using NShared.ReceivePushReward;

public static class JM_NShared_ReceivePushReward_Response
{
	public static NShared.ReceivePushReward.Response Parse(string s)
	{
		return Parse(s, new NShared.ReceivePushReward.Response());
	}

	public static NShared.ReceivePushReward.Response Parse(string s, NShared.ReceivePushReward.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.ReceivePushReward.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.ReceivePushReward.Response());
	}

	public static NShared.ReceivePushReward.Response Parse(IDictionary d, NShared.ReceivePushReward.Response o)
	{
		o.CurrencyResultInfos = BaseJsonMarshaler.ParseArray(d, "CurrencyResultInfos", JM_NShared_CurrencyResultInfo3.Parse);
		o.ItemResultInfos = BaseJsonMarshaler.ParseArray(d, "ItemResultInfos", JM_NShared_ItemResultInfo.Parse);
		o.StaminaResultInfos = BaseJsonMarshaler.ParseArray(d, "StaminaResultInfos", JM_NShared_StaminaResultInfo.Parse);
		o.FriendshipPointResultInfos = BaseJsonMarshaler.ParseArray(d, "FriendshipPointResultInfos", JM_NShared_FriendshipPointResultInfo.Parse);
		o.EquipItemResultInfos = BaseJsonMarshaler.ParseArray(d, "EquipItemResultInfos", JM_NShared_EquipItemInfo.Parse);
		o.HeroResultInfos = BaseJsonMarshaler.ParseArray(d, "HeroResultInfos", JM_NShared_HeroInfo.Parse);
		o.ExpResultInfos = BaseJsonMarshaler.ParseArray(d, "ExpResultInfos", JM_NShared_ExpResultInfo.Parse);
		o.EquipItemBoostResultInfos = BaseJsonMarshaler.ParseArray(d, "EquipItemBoostResultInfos", JM_NShared_EquipItemBoostInfo.Parse);
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
