using System.Collections;
using MiniJSON;
using NShared;
using NShared.CompleteMonthlyHero;

public static class JM_NShared_CompleteMonthlyHero_Response
{
	public static NShared.CompleteMonthlyHero.Response Parse(string s)
	{
		return Parse(s, new NShared.CompleteMonthlyHero.Response());
	}

	public static NShared.CompleteMonthlyHero.Response Parse(string s, NShared.CompleteMonthlyHero.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.CompleteMonthlyHero.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.CompleteMonthlyHero.Response());
	}

	public static NShared.CompleteMonthlyHero.Response Parse(IDictionary d, NShared.CompleteMonthlyHero.Response o)
	{
		o.Info = BaseJsonMarshaler.ParseObject(d, "Info", JM_NShared_MonthlyHeroInfo.Parse);
		o.CurrencyResults = BaseJsonMarshaler.ParseArray(d, "CurrencyResults", JM_NShared_CurrencyResultInfo3.Parse);
		o.ItemResults = BaseJsonMarshaler.ParseArray(d, "ItemResults", JM_NShared_ItemResultInfo.Parse);
		o.EquipItemResults = BaseJsonMarshaler.ParseArray(d, "EquipItemResults", JM_NShared_EquipItemInfo.Parse);
		o.HeroInfos = BaseJsonMarshaler.ParseArray(d, "HeroInfos", JM_NShared_HeroInfo.Parse);
		o.TeamExpResultInfos = BaseJsonMarshaler.ParseArray(d, "TeamExpResultInfos", JM_NShared_ExpResultInfo.Parse);
		o.StaminaResultInfos = BaseJsonMarshaler.ParseArray(d, "StaminaResultInfos", JM_NShared_StaminaResultInfo.Parse);
		o.DungeonInfos = BaseJsonMarshaler.ParseArray(d, "DungeonInfos", JM_NShared_ChapterDungeonInfo.Parse);
		o.FreeEquipGachaInfo = BaseJsonMarshaler.ParseObject(d, "FreeEquipGachaInfo", JM_NShared_FreeEquipGachaInfo.Parse);
		o.EquipGachaInfo = BaseJsonMarshaler.ParseObject(d, "EquipGachaInfo", JM_NShared_EquipGachaInfo.Parse);
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
