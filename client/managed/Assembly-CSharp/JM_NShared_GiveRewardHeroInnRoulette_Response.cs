using System.Collections;
using MiniJSON;
using NShared;
using NShared.GiveRewardHeroInnRoulette;

public static class JM_NShared_GiveRewardHeroInnRoulette_Response
{
	public static NShared.GiveRewardHeroInnRoulette.Response Parse(string s)
	{
		return Parse(s, new NShared.GiveRewardHeroInnRoulette.Response());
	}

	public static NShared.GiveRewardHeroInnRoulette.Response Parse(string s, NShared.GiveRewardHeroInnRoulette.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.GiveRewardHeroInnRoulette.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.GiveRewardHeroInnRoulette.Response());
	}

	public static NShared.GiveRewardHeroInnRoulette.Response Parse(IDictionary d, NShared.GiveRewardHeroInnRoulette.Response o)
	{
		o.RouletteInfo = BaseJsonMarshaler.ParseObject(d, "RouletteInfo", JM_NShared_RouletteInfo.Parse);
		o.Section = BaseJsonMarshaler.ParseValueInt(d, "Section");
		o.CurrencyResults = BaseJsonMarshaler.ParseArray(d, "CurrencyResults", JM_NShared_CurrencyResultInfo3.Parse);
		o.itemResults = BaseJsonMarshaler.ParseArray(d, "itemResults", JM_NShared_ItemResultInfo.Parse);
		o.equipItemInfos = BaseJsonMarshaler.ParseArray(d, "equipItemInfos", JM_NShared_EquipItemInfo.Parse);
		o.staminaResult = BaseJsonMarshaler.ParseObject(d, "staminaResult", JM_NShared_StaminaResultInfo.Parse);
		o.expResult = BaseJsonMarshaler.ParseObject(d, "expResult", JM_NShared_ExpResultInfo.Parse);
		o.EventDungeonTicket = BaseJsonMarshaler.ParseObject(d, "EventDungeonTicket", JM_NShared_StaminaResultInfo.Parse);
		o.FriendshipPointResult = BaseJsonMarshaler.ParseObject(d, "FriendshipPointResult", JM_NShared_FriendshipPointResultInfo.Parse);
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
