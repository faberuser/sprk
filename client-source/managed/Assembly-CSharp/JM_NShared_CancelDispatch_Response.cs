using System.Collections;
using MiniJSON;
using NShared;
using NShared.CancelDispatch;

public static class JM_NShared_CancelDispatch_Response
{
	public static NShared.CancelDispatch.Response Parse(string s)
	{
		return Parse(s, new NShared.CancelDispatch.Response());
	}

	public static NShared.CancelDispatch.Response Parse(string s, NShared.CancelDispatch.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.CancelDispatch.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.CancelDispatch.Response());
	}

	public static NShared.CancelDispatch.Response Parse(IDictionary d, NShared.CancelDispatch.Response o)
	{
		o.DispatchBattleInfo = BaseJsonMarshaler.ParseObject(d, "DispatchBattleInfo", JM_NShared_DispatchBattleInfo.Parse);
		o.CurrencyResults = BaseJsonMarshaler.ParseArray(d, "CurrencyResults", JM_NShared_CurrencyResultInfo3.Parse);
		o.ItemResults = BaseJsonMarshaler.ParseArray(d, "ItemResults", JM_NShared_ItemResultInfo.Parse);
		o.StaminaResult = BaseJsonMarshaler.ParseObject(d, "StaminaResult", JM_NShared_StaminaResultInfo.Parse);
		o.EclipseStaminaResult = BaseJsonMarshaler.ParseObject(d, "EclipseStaminaResult", JM_NShared_StaminaResultInfo.Parse);
		o.TeamExpResult = BaseJsonMarshaler.ParseObject(d, "TeamExpResult", JM_NShared_ExpResultInfo.Parse);
		o.HeroExpResults = BaseJsonMarshaler.ParseArray(d, "HeroExpResults", JM_NShared_HeroExpResultInfo.Parse);
		o.FlaskResults = BaseJsonMarshaler.ParseArray(d, "FlaskResults", JM_NShared_FlaskEndCampaignResultInfo.Parse);
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
