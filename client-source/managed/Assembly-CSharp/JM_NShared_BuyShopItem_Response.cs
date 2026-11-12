using System.Collections;
using MiniJSON;
using NShared;
using NShared.BuyShopItem;

public static class JM_NShared_BuyShopItem_Response
{
	public static NShared.BuyShopItem.Response Parse(string s)
	{
		return Parse(s, new NShared.BuyShopItem.Response());
	}

	public static NShared.BuyShopItem.Response Parse(string s, NShared.BuyShopItem.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.BuyShopItem.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.BuyShopItem.Response());
	}

	public static NShared.BuyShopItem.Response Parse(IDictionary d, NShared.BuyShopItem.Response o)
	{
		o.CurrencyResults = BaseJsonMarshaler.ParseArray(d, "CurrencyResults", JM_NShared_CurrencyResultInfo3.Parse);
		o.RoyalPointResult = BaseJsonMarshaler.ParseObject(d, "RoyalPointResult", JM_NShared_RoyalPointResultInfo.Parse);
		o.ItemResult = BaseJsonMarshaler.ParseObject(d, "ItemResult", JM_NShared_ItemResultInfo.Parse);
		o.GuildPointResult = BaseJsonMarshaler.ParseObject(d, "GuildPointResult", JM_NShared_GuildPointResultInfo.Parse);
		o.FriendshipPointResult = BaseJsonMarshaler.ParseObject(d, "FriendshipPointResult", JM_NShared_FriendshipPointResultInfo.Parse);
		o.EquipItems = BaseJsonMarshaler.ParseArray(d, "EquipItems", JM_NShared_EquipItemInfo.Parse);
		o.ItemTimeDurationInfo = BaseJsonMarshaler.ParseObject(d, "ItemTimeDurationInfo", JM_NShared_PlayerItemTimeDurationInfo.Parse);
		o.heroInfo = BaseJsonMarshaler.ParseObject(d, "heroInfo", JM_NShared_HeroInfo.Parse);
		o.firstPurchase = BaseJsonMarshaler.ParseObject(d, "firstPurchase", JM_NShared_PlayerFirstPurchaseInfo.Parse);
		o.ShopItem = BaseJsonMarshaler.ParseObject(d, "ShopItem", JM_NShared_ShopItemInfo.Parse);
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
