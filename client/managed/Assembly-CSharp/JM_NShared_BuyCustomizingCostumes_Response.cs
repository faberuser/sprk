using System.Collections;
using MiniJSON;
using NShared;
using NShared.BuyCustomizingCostumes;

public static class JM_NShared_BuyCustomizingCostumes_Response
{
	public static NShared.BuyCustomizingCostumes.Response Parse(string s)
	{
		return Parse(s, new NShared.BuyCustomizingCostumes.Response());
	}

	public static NShared.BuyCustomizingCostumes.Response Parse(string s, NShared.BuyCustomizingCostumes.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.BuyCustomizingCostumes.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.BuyCustomizingCostumes.Response());
	}

	public static NShared.BuyCustomizingCostumes.Response Parse(IDictionary d, NShared.BuyCustomizingCostumes.Response o)
	{
		o.CurrencyResults = BaseJsonMarshaler.ParseArray(d, "CurrencyResults", JM_NShared_CurrencyResultInfo3.Parse);
		o.CostumeInfos = BaseJsonMarshaler.ParseArray(d, "CostumeInfos", JM_NShared_CostumeInfo.Parse);
		o.PlayerAccessoryCostumeInfos = BaseJsonMarshaler.ParseArray(d, "PlayerAccessoryCostumeInfos", JM_NShared_PlayerAccessoryCostumeInfo.Parse);
		o.WeaponCostumeInfo = BaseJsonMarshaler.ParseObject(d, "WeaponCostumeInfo", JM_NShared_WeaponCostumeInfo.Parse);
		o.HairCostumeInfo = BaseJsonMarshaler.ParseObject(d, "HairCostumeInfo", JM_NShared_HairCostumeInfo.Parse);
		o.UnsetAccessoryCostumeIndices = BaseJsonMarshaler.ParseArrayCast(d, "UnsetAccessoryCostumeIndices", BaseJsonMarshaler.CastToInt);
		o.HeroCostumeResultInfo = BaseJsonMarshaler.ParseObject(d, "HeroCostumeResultInfo", JM_NShared_HeroCostumeResultInfo.Parse);
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
