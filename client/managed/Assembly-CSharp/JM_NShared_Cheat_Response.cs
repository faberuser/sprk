using System.Collections;
using MiniJSON;
using NShared;
using NShared.Cheat;

public static class JM_NShared_Cheat_Response
{
	public static NShared.Cheat.Response Parse(string s)
	{
		return Parse(s, new NShared.Cheat.Response());
	}

	public static NShared.Cheat.Response Parse(string s, NShared.Cheat.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.Cheat.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.Cheat.Response());
	}

	public static NShared.Cheat.Response Parse(IDictionary d, NShared.Cheat.Response o)
	{
		o.Message = BaseJsonMarshaler.ParseValueString(d, "Message");
		o.EquipItemResults = BaseJsonMarshaler.ParseArray(d, "EquipItemResults", JM_NShared_EquipItemInfo.Parse);
		o.RemovedEquipItemSlotIndices = BaseJsonMarshaler.ParseArrayCast(d, "RemovedEquipItemSlotIndices", BaseJsonMarshaler.CastToInt);
		o.ItemResults = BaseJsonMarshaler.ParseArray(d, "ItemResults", JM_NShared_ItemResultInfo.Parse);
		o.SoulWeaponInfo = BaseJsonMarshaler.ParseObject(d, "SoulWeaponInfo", JM_NShared_SoulWeaponInfo.Parse);
		o.CurrencyResults = BaseJsonMarshaler.ParseArray(d, "CurrencyResults", JM_NShared_CurrencyResultInfo3.Parse);
		o.FriendshipPointResult = BaseJsonMarshaler.ParseObject(d, "FriendshipPointResult", JM_NShared_FriendshipPointResultInfo.Parse);
		o.HeroInfos = BaseJsonMarshaler.ParseArray(d, "HeroInfos", JM_NShared_HeroInfo.Parse);
		o.TeamExpResult = BaseJsonMarshaler.ParseObject(d, "TeamExpResult", JM_NShared_ExpResultInfo.Parse);
		o.TowerInfos = BaseJsonMarshaler.ParseArray(d, "TowerInfos", JM_NShared_TowerInfo.Parse);
		o.DungeonInfos = BaseJsonMarshaler.ParseArray(d, "DungeonInfos", JM_NShared_ChapterDungeonInfo.Parse);
		o.TutorialInfos = BaseJsonMarshaler.ParseArray(d, "TutorialInfos", JM_NShared_TutorialInfo.Parse);
		o.StaminaResults = BaseJsonMarshaler.ParseArray(d, "StaminaResults", JM_NShared_StaminaResultInfo.Parse);
		o.BattleInfo = BaseJsonMarshaler.ParseObject(d, "BattleInfo", JM_NShared_PlayerBattleInfo.Parse);
		o.RaidInfos = BaseJsonMarshaler.ParseArray(d, "RaidInfos", JM_NShared_RaidInfo.Parse);
		o.MiscInfo = BaseJsonMarshaler.ParseObject(d, "MiscInfo", JM_NShared_PlayerMiscInfo.Parse);
		o.MyGuildInfo = BaseJsonMarshaler.ParseObject(d, "MyGuildInfo", JM_NShared_MyGuildInfo.Parse);
		o.GuildBuildingInfo = BaseJsonMarshaler.ParseObject(d, "GuildBuildingInfo", JM_NShared_GuildBuildingInfo.Parse);
		o.GuildActivityPointResult = BaseJsonMarshaler.ParseObject(d, "GuildActivityPointResult", JM_NShared_GuildActivityPointResultInfo.Parse);
		o.GuildSuppressPointResult = BaseJsonMarshaler.ParseObject(d, "GuildSuppressPointResult", JM_NShared_GuildSuppressPointResultInfo.Parse);
		o.HeroFriendlyInfo = BaseJsonMarshaler.ParseObject(d, "HeroFriendlyInfo", JM_NShared_PlayerHeroFriendlyInfo.Parse);
		o.PurchaseTimeDurations = BaseJsonMarshaler.ParseArray(d, "PurchaseTimeDurations", JM_NShared_PlayerPurchaseTimeDurationInfo.Parse);
		o.GuildPointResult = BaseJsonMarshaler.ParseObject(d, "GuildPointResult", JM_NShared_GuildPointResultInfo.Parse);
		o.GuildRaidInfo = BaseJsonMarshaler.ParseObject(d, "GuildRaidInfo", JM_NShared_GuildRaidDungeonInfo.Parse);
		o.WorldBossPointResult = BaseJsonMarshaler.ParseObject(d, "WorldBossPointResult", JM_NShared_WorldBossPointResultInfo.Parse);
		o.NpcFriendlyInfo = BaseJsonMarshaler.ParseObject(d, "NpcFriendlyInfo", JM_NShared_NPCFriendlyInfo.Parse);
		o.RemovedHeroIndex = BaseJsonMarshaler.ParseValueInt(d, "RemovedHeroIndex");
		o.ShopItemInfos = BaseJsonMarshaler.ParseArray(d, "ShopItemInfos", JM_NShared_ShopItemInfo.Parse);
		o.StepUpEquipGachaInfo = BaseJsonMarshaler.ParseObject(d, "StepUpEquipGachaInfo", JM_NShared_StepUpEquipGachaInfo.Parse);
		o.PetInfos = BaseJsonMarshaler.ParseArray(d, "PetInfos", JM_NShared_PetInfo.Parse);
		o.PetMiscInfos = BaseJsonMarshaler.ParseArray(d, "PetMiscInfos", JM_NShared_PetMiscInfo.Parse);
		o.PetGachaCountInfos = BaseJsonMarshaler.ParseArray(d, "PetGachaCountInfos", JM_NShared_PetGachaCountInfo.Parse);
		o.PlayerTreasureHouseInfo = BaseJsonMarshaler.ParseObject(d, "PlayerTreasureHouseInfo", JM_NShared_PlayerTreasureHouseInfo.Parse);
		o.PunishmentRaidInfos = BaseJsonMarshaler.ParseArray(d, "PunishmentRaidInfos", JM_NShared_PunishmentRaidInfo.Parse);
		o.OpenPunishmentRaidInfo = BaseJsonMarshaler.ParseObject(d, "OpenPunishmentRaidInfo", JM_NShared_OpenPunishmentRaidInfo.Parse);
		o.PetExploreInfos = BaseJsonMarshaler.ParseArray(d, "PetExploreInfos", JM_NShared_PetExploreInfo.Parse);
		o.PayShopDiscountInfos = BaseJsonMarshaler.ParseArray(d, "PayShopDiscountInfos", JM_NShared_PayShopDiscountInfo.Parse);
		o.PlayerProductPurchaseInfo = BaseJsonMarshaler.ParseArray(d, "PlayerProductPurchaseInfo", JM_NShared_PlayerProductPurchaseInfo.Parse);
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
