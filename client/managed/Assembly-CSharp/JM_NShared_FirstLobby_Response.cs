using System.Collections;
using MiniJSON;
using NShared;
using NShared.FirstLobby;

public static class JM_NShared_FirstLobby_Response
{
	public static NShared.FirstLobby.Response Parse(string s)
	{
		return Parse(s, new NShared.FirstLobby.Response());
	}

	public static NShared.FirstLobby.Response Parse(string s, NShared.FirstLobby.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.FirstLobby.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.FirstLobby.Response());
	}

	public static NShared.FirstLobby.Response Parse(IDictionary d, NShared.FirstLobby.Response o)
	{
		o.PlayerBookMarkHeroInfo = BaseJsonMarshaler.ParseObject(d, "PlayerBookMarkHeroInfo", JM_NShared_PlayerBookMarkHeroInfo.Parse);
		o.PayShopUserGroup = BaseJsonMarshaler.ParseEnum<PayShopUserGroup>(d, "PayShopUserGroup");
		o.UserTypeEndTime = BaseJsonMarshaler.ParseValueString(d, "UserTypeEndTime");
		o.HeroPresetStorages = BaseJsonMarshaler.ParseArray(d, "HeroPresetStorages", JM_NShared_HeroPresetStorageInfo.Parse);
		o.EquipPresetInfos = BaseJsonMarshaler.ParseArray(d, "EquipPresetInfos", JM_NShared_EquipPresetInfo.Parse);
		o.EquipItemIdentifiedInfos = BaseJsonMarshaler.ParseArray(d, "EquipItemIdentifiedInfos", JM_NShared_EquipItemIdentifiedInfo.Parse);
		o.TechnoMagicEquipItems = BaseJsonMarshaler.ParseArrayCast(d, "TechnoMagicEquipItems", BaseJsonMarshaler.CastToInt);
		o.PetInfos = BaseJsonMarshaler.ParseArray(d, "PetInfos", JM_NShared_PetInfo.Parse);
		o.PetMiscInfos = BaseJsonMarshaler.ParseArray(d, "PetMiscInfos", JM_NShared_PetMiscInfo.Parse);
		o.PetGachaCountInfos = BaseJsonMarshaler.ParseArray(d, "PetGachaCountInfos", JM_NShared_PetGachaCountInfo.Parse);
		o.GachaCeilingCountInfos = BaseJsonMarshaler.ParseArray(d, "GachaCeilingCountInfos", JM_NShared_GachaCeilingCountInfo.Parse);
		o.KingsPassSeaonInfo = BaseJsonMarshaler.ParseObject(d, "KingsPassSeaonInfo", JM_NShared_KingsPassSeaonInfo.Parse);
		o.KingsPassMissionInfos = BaseJsonMarshaler.ParseArray(d, "KingsPassMissionInfos", JM_NShared_KingsPassMission.Parse);
		o.OpenPunishmentRaidInfos = BaseJsonMarshaler.ParseArray(d, "OpenPunishmentRaidInfos", JM_NShared_OpenPunishmentRaidInfo.Parse);
		o.PunishmentRaidInfos = BaseJsonMarshaler.ParseArray(d, "PunishmentRaidInfos", JM_NShared_PunishmentRaidInfo.Parse);
		o.EventCraftItemInfos = BaseJsonMarshaler.ParseArray(d, "EventCraftItemInfos", JM_NShared_EventCraftItemInfo.Parse);
		o.PetExploreInfos = BaseJsonMarshaler.ParseArray(d, "PetExploreInfos", JM_NShared_PetExploreInfo.Parse);
		o.TeamLevelBuffInfos = BaseJsonMarshaler.ParseArray(d, "TeamLevelBuffInfos", JM_NShared_TeamLevelBuff.Parse);
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
