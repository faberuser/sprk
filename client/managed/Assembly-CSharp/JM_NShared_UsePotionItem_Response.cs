using System.Collections;
using MiniJSON;
using NShared;
using NShared.UsePotionItem;

public static class JM_NShared_UsePotionItem_Response
{
	public static NShared.UsePotionItem.Response Parse(string s)
	{
		return Parse(s, new NShared.UsePotionItem.Response());
	}

	public static NShared.UsePotionItem.Response Parse(string s, NShared.UsePotionItem.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.UsePotionItem.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.UsePotionItem.Response());
	}

	public static NShared.UsePotionItem.Response Parse(IDictionary d, NShared.UsePotionItem.Response o)
	{
		o.StaminaResult = BaseJsonMarshaler.ParseObject(d, "StaminaResult", JM_NShared_StaminaResultInfo.Parse);
		o.CurrencyResult = BaseJsonMarshaler.ParseObject(d, "CurrencyResult", JM_NShared_CurrencyResultInfo3.Parse);
		o.ItemResult = BaseJsonMarshaler.ParseObject(d, "ItemResult", JM_NShared_ItemResultInfo.Parse);
		o.HeroExpResult = BaseJsonMarshaler.ParseObject(d, "HeroExpResult", JM_NShared_HeroExpResultInfo.Parse);
		o.HeroTranscendResult = BaseJsonMarshaler.ParseObject(d, "HeroTranscendResult", JM_NShared_HeroTranscendResultInfo.Parse);
		o.ItemTimeDurationInfos = BaseJsonMarshaler.ParseArray(d, "ItemTimeDurationInfos", JM_NShared_PlayerItemTimeDurationInfo.Parse);
		o.HideoutDungeonInfo = BaseJsonMarshaler.ParseObject(d, "HideoutDungeonInfo", JM_NShared_HideoutDungeonInfo.Parse);
		o.ConquestDungeonInfo = BaseJsonMarshaler.ParseObject(d, "ConquestDungeonInfo", JM_NShared_ConquestDungeonInfo.Parse);
		o.NewInventoryExtend = BaseJsonMarshaler.ParseValueInt(d, "NewInventoryExtend");
		o.NewEquipStoageSlotExtend = BaseJsonMarshaler.ParseValueInt(d, "NewEquipStoageSlotExtend");
		o.NewChestExtend = BaseJsonMarshaler.ParseValueInt(d, "NewChestExtend");
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
