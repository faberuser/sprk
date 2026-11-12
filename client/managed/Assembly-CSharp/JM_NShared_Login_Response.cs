using System.Collections;
using MiniJSON;
using NShared;
using NShared.Login;

public static class JM_NShared_Login_Response
{
	public static NShared.Login.Response Parse(string s)
	{
		return Parse(s, new NShared.Login.Response());
	}

	public static NShared.Login.Response Parse(string s, NShared.Login.Response o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static NShared.Login.Response Parse(IDictionary d)
	{
		return Parse(d, new NShared.Login.Response());
	}

	public static NShared.Login.Response Parse(IDictionary d, NShared.Login.Response o)
	{
		o.ServerPrivateIp = BaseJsonMarshaler.ParseValueString(d, "ServerPrivateIp");
		o.ServerPort = BaseJsonMarshaler.ParseValueInt(d, "ServerPort");
		o.AdminLevel = BaseJsonMarshaler.ParseValueInt(d, "AdminLevel");
		o.StickyHost = BaseJsonMarshaler.ParseValueString(d, "StickyHost");
		o.AlternativeHost = BaseJsonMarshaler.ParseValueString(d, "AlternativeHost");
		o.IsNewUser = BaseJsonMarshaler.ParseValueBool(d, "IsNewUser");
		o.ServerLocalTime = BaseJsonMarshaler.ParseValueString(d, "ServerLocalTime");
		o.ServerUTCTime = BaseJsonMarshaler.ParseValueString(d, "ServerUTCTime");
		o.ServerUTCOffsetHour = BaseJsonMarshaler.ParseValueInt(d, "ServerUTCOffsetHour");
		o.LoginId = BaseJsonMarshaler.ParseValueString(d, "LoginId");
		o.FaceBookId = BaseJsonMarshaler.ParseValueString(d, "FaceBookId");
		o.GooglePlusId = BaseJsonMarshaler.ParseValueString(d, "GooglePlusId");
		o.GameCenterId = BaseJsonMarshaler.ParseValueString(d, "GameCenterId");
		o.AppleId = BaseJsonMarshaler.ParseValueString(d, "AppleId");
		o.LoginIdByMethodType = BaseJsonMarshaler.ParseArray(d, "LoginIdByMethodType", JM_NShared_LoginIdByMethodType.Parse);
		o.EventPush = BaseJsonMarshaler.ParseValueBool(d, "EventPush");
		o.GuildRaidPush = BaseJsonMarshaler.ParseValueBool(d, "GuildRaidPush");
		o.GuildSuppressPush = BaseJsonMarshaler.ParseValueBool(d, "GuildSuppressPush");
		o.NightPush = BaseJsonMarshaler.ParseValueBool(d, "NightPush");
		o.UserInfo = BaseJsonMarshaler.ParseObject(d, "UserInfo", JM_NShared_UserInfo.Parse);
		o.BattleInfo = BaseJsonMarshaler.ParseObject(d, "BattleInfo", JM_NShared_PlayerBattleInfo.Parse);
		o.BattleBanPickInfo = BaseJsonMarshaler.ParseObject(d, "BattleBanPickInfo", JM_NShared_PlayerBattleBanPickInfo.Parse);
		o.BattleLuckyInfo = BaseJsonMarshaler.ParseObject(d, "BattleLuckyInfo", JM_NShared_PlayerBattleLuckyInfo.Parse);
		o.MiscInfo = BaseJsonMarshaler.ParseObject(d, "MiscInfo", JM_NShared_PlayerMiscInfo.Parse);
		o.GuildId = BaseJsonMarshaler.ParseValueLong(d, "GuildId");
		o.Heroes = BaseJsonMarshaler.ParseArray(d, "Heroes", JM_NShared_HeroInfo.Parse);
		o.HeroRunePageInfos = BaseJsonMarshaler.ParseArray(d, "HeroRunePageInfos", JM_NShared_HeroRunePageInfo.Parse);
		o.Items = BaseJsonMarshaler.ParseArray(d, "Items", JM_NShared_ItemInfo.Parse);
		o.EquipItems = BaseJsonMarshaler.ParseArray(d, "EquipItems", JM_NShared_EquipItemInfo.Parse);
		o.SoulWeaponInfos = BaseJsonMarshaler.ParseArray(d, "SoulWeaponInfos", JM_NShared_SoulWeaponInfo.Parse);
		o.ChapterDungeons = BaseJsonMarshaler.ParseArray(d, "ChapterDungeons", JM_NShared_ChapterDungeonInfo.Parse);
		o.Towers = BaseJsonMarshaler.ParseArray(d, "Towers", JM_NShared_TowerInfo.Parse);
		o.ShopRestockTimes = BaseJsonMarshaler.ParseArray(d, "ShopRestockTimes", JM_NShared_ShopRestockTimeInfo.Parse);
		o.ShopListItems = BaseJsonMarshaler.ParseArray(d, "ShopListItems", JM_NShared_ShopListItemInfo.Parse);
		o.MessageServerInfo = BaseJsonMarshaler.ParseObject(d, "MessageServerInfo", JM_NShared_MessageServerInfo.Parse);
		o.FriendInfos = BaseJsonMarshaler.ParseArray(d, "FriendInfos", JM_NShared_FriendInfo.Parse);
		o.FriendInvitorInfos = BaseJsonMarshaler.ParseArray(d, "FriendInvitorInfos", JM_NShared_FriendUserInfo.Parse);
		o.Tutorials = BaseJsonMarshaler.ParseArray(d, "Tutorials", JM_NShared_TutorialInfo.Parse);
		o.ItemTimeDurations = BaseJsonMarshaler.ParseArray(d, "ItemTimeDurations", JM_NShared_PlayerItemTimeDurationInfo.Parse);
		o.FirstPurchases = BaseJsonMarshaler.ParseArray(d, "FirstPurchases", JM_NShared_PlayerFirstPurchaseInfo.Parse);
		o.PurchaseTimeDurations = BaseJsonMarshaler.ParseArray(d, "PurchaseTimeDurations", JM_NShared_PlayerPurchaseTimeDurationInfo.Parse);
		o.PlayerProductPurchaseInfos = BaseJsonMarshaler.ParseArray(d, "PlayerProductPurchaseInfos", JM_NShared_PlayerProductPurchaseInfo.Parse);
		o.WorldMapEventTimeInfos = BaseJsonMarshaler.ParseArray(d, "WorldMapEventTimeInfos", JM_NShared_WorldMapEventTimeInfo.Parse);
		o.WorldMapEventInfos = BaseJsonMarshaler.ParseArray(d, "WorldMapEventInfos", JM_NShared_WorldMapEventInfo.Parse);
		o.WantedQuestInfos = BaseJsonMarshaler.ParseArray(d, "WantedQuestInfos", JM_NShared_WantedQuestInfo.Parse);
		o.AttendanceInfos = BaseJsonMarshaler.ParseArray(d, "AttendanceInfos", JM_NShared_AttendanceInfo.Parse);
		o.RaidInfos = BaseJsonMarshaler.ParseArray(d, "RaidInfos", JM_NShared_RaidInfo.Parse);
		o.DeckInfos = BaseJsonMarshaler.ParseArray(d, "DeckInfos", JM_NShared_DeckInfo.Parse);
		o.CraftSlotInfos = BaseJsonMarshaler.ParseArray(d, "CraftSlotInfos", JM_NShared_CraftSlotInfo.Parse);
		o.CostumeInfos = BaseJsonMarshaler.ParseArray(d, "CostumeInfos", JM_NShared_CostumeInfo.Parse);
		o.PlayerAccessoryCostumeInfos = BaseJsonMarshaler.ParseArray(d, "PlayerAccessoryCostumeInfos", JM_NShared_PlayerAccessoryCostumeInfo.Parse);
		o.WeaponCostumeInfos = BaseJsonMarshaler.ParseArray(d, "WeaponCostumeInfos", JM_NShared_WeaponCostumeInfo.Parse);
		o.HairCostumeInfos = BaseJsonMarshaler.ParseArray(d, "HairCostumeInfos", JM_NShared_HairCostumeInfo.Parse);
		o.CostumeStorageSlotInfos = BaseJsonMarshaler.ParseArray(d, "CostumeStorageSlotInfos", JM_NShared_CostumeStorageSlotInfo.Parse);
		o.PlayerAvatarHeroInfo = BaseJsonMarshaler.ParseObject(d, "PlayerAvatarHeroInfo", JM_NShared_PlayerAvatarHeroInfo.Parse);
		o.PurchaseMarketingInfos = BaseJsonMarshaler.ParseArray(d, "PurchaseMarketingInfos", JM_NShared_PurchaseMarketingInfo.Parse);
		o.PlayerItemUseInfos = BaseJsonMarshaler.ParseArray(d, "PlayerItemUseInfos", JM_NShared_PlayerItemUseInfo.Parse);
		o.HideoutDungeons = BaseJsonMarshaler.ParseArray(d, "HideoutDungeons", JM_NShared_HideoutDungeonInfo.Parse);
		o.ConquestDungeons = BaseJsonMarshaler.ParseArray(d, "ConquestDungeons", JM_NShared_ConquestDungeonInfo.Parse);
		o.GodkingTrialDungeons = BaseJsonMarshaler.ParseArray(d, "GodkingTrialDungeons", JM_NShared_GodkingTrialDungeonInfo.Parse);
		o.UnderPrisonInfos = BaseJsonMarshaler.ParseArray(d, "UnderPrisonInfos", JM_NShared_UnderPrisonInfo.Parse);
		o.PlayRecordInfos = BaseJsonMarshaler.ParseArray(d, "PlayRecordInfos", JM_NShared_PlayRecordInfo.Parse);
		o.LoginDailyInfos = BaseJsonMarshaler.ParseArray(d, "LoginDailyInfos", JM_NShared_LoginDailyInfo.Parse);
		o.PlayerArchiveInfos = BaseJsonMarshaler.ParseArray(d, "PlayerArchiveInfos", JM_NShared_PlayerArchiveInfo.Parse);
		o.PlayerCurrencyInfos = BaseJsonMarshaler.ParseArray(d, "PlayerCurrencyInfos", JM_NShared_PlayerCurrencyInfo.Parse);
		o.NewbieMissionInfos = BaseJsonMarshaler.ParseArray(d, "NewbieMissionInfos", JM_NShared_NewbieMissionInfo.Parse);
		o.Notices = BaseJsonMarshaler.ParseArray(d, "Notices", JM_NShared_Notice.Parse);
		o.DropBonusEvents = BaseJsonMarshaler.ParseArray(d, "DropBonusEvents", JM_NShared_DropBonusEvent.Parse);
		o.PayShopItemEventInfos = BaseJsonMarshaler.ParseArray(d, "PayShopItemEventInfos", JM_NShared_PayShopItemEventInfo.Parse);
		o.ImageInfos = BaseJsonMarshaler.ParseArray(d, "ImageInfos", JM_NShared_PayShopImageUrlInfo.Parse);
		o.freeEquipGachaInfos = BaseJsonMarshaler.ParseArray(d, "freeEquipGachaInfos", JM_NShared_FreeEquipGachaInfo.Parse);
		o.EquipGachaInfos = BaseJsonMarshaler.ParseArray(d, "EquipGachaInfos", JM_NShared_EquipGachaInfo.Parse);
		o.chapterRewardInfos = BaseJsonMarshaler.ParseArray(d, "chapterRewardInfos", JM_NShared_ChapterRewardInfo.Parse);
		o.StaminaResults = BaseJsonMarshaler.ParseArray(d, "StaminaResults", JM_NShared_StaminaResultInfo.Parse);
		o.ContentsStatuses = BaseJsonMarshaler.ParseArray(d, "ContentsStatuses", JM_NShared_ContentsStatusInfo.Parse);
		o.ContentsValues = BaseJsonMarshaler.ParseArray(d, "ContentsValues", JM_NShared_ContentsValueInfo.Parse);
		o.ContentsNoticeInfos = BaseJsonMarshaler.ParseArray(d, "ContentsNoticeInfos", JM_NShared_ContentsNoticePopupInfo.Parse);
		o.PatchNote = BaseJsonMarshaler.ParseObject(d, "PatchNote", JM_NShared_PatchNoteInfo.Parse);
		o.PayShopProductInfos = BaseJsonMarshaler.ParseArray(d, "PayShopProductInfos", JM_NShared_PayShopProductInfo.Parse);
		o.LatestIAPInfo = BaseJsonMarshaler.ParseObject(d, "LatestIAPInfo", JM_NShared_PlayerInAppPurchaseInfo.Parse);
		o.NotCompletedIAPInfos = BaseJsonMarshaler.ParseArray(d, "NotCompletedIAPInfos", JM_NShared_PlayerInAppPurchaseInfo.Parse);
		o.HeroFriendlyInfo = BaseJsonMarshaler.ParseObject(d, "HeroFriendlyInfo", JM_NShared_PlayerHeroFriendlyInfo.Parse);
		o.SendRecvFriendshipPointInfos = BaseJsonMarshaler.ParseArray(d, "SendRecvFriendshipPointInfos", JM_NShared_FriendshipPointInfo.Parse);
		o.SentFriendshipPointInfos = BaseJsonMarshaler.ParseArray(d, "SentFriendshipPointInfos", JM_NShared_FriendshipPointInfo.Parse);
		o.GuildPointInfo = BaseJsonMarshaler.ParseObject(d, "GuildPointInfo", JM_NShared_PlayerGuildPointInfo.Parse);
		o.GuildInfo = BaseJsonMarshaler.ParseObject(d, "GuildInfo", JM_NShared_MyGuildInfo.Parse);
		o.GuildArenaSessionInfo = BaseJsonMarshaler.ParseArray(d, "GuildArenaSessionInfo", JM_NShared_GuildArenaSessionInfo.Parse);
		o.NPCFriendlyInfos = BaseJsonMarshaler.ParseArray(d, "NPCFriendlyInfos", JM_NShared_NPCFriendlyInfo.Parse);
		o.ChatBanInfos = BaseJsonMarshaler.ParseArray(d, "ChatBanInfos", JM_NShared_ChatBanInfo.Parse);
		o.CreatedTime = BaseJsonMarshaler.ParseValueString(d, "CreatedTime");
		o.WebNotice = BaseJsonMarshaler.ParseArray(d, "WebNotice", JM_NShared_WebNoticeInfo.Parse);
		o.GuildContentsRestrictedTime = BaseJsonMarshaler.ParseValueString(d, "GuildContentsRestrictedTime");
		o.AutoOpenInGameEvent = BaseJsonMarshaler.ParseValueBool(d, "AutoOpenInGameEvent");
		o.ServerName = BaseJsonMarshaler.ParseValueString(d, "ServerName");
		o.IsTutorialSkip = BaseJsonMarshaler.ParseValueBool(d, "IsTutorialSkip");
		o.UseRenewalPayShop = BaseJsonMarshaler.ParseValueInt(d, "UseRenewalPayShop");
		o.ClassBuffPointInfos = BaseJsonMarshaler.ParseArray(d, "ClassBuffPointInfos", JM_NShared_ClassBuffPointInfo.Parse);
		o.ClassBuffInfos = BaseJsonMarshaler.ParseArray(d, "ClassBuffInfos", JM_NShared_ClassBuffInfo.Parse);
		o.DispatchBattleInfos = BaseJsonMarshaler.ParseArray(d, "DispatchBattleInfos", JM_NShared_DispatchBattleInfo.Parse);
		o.RecordsOfHonorTopInfos = BaseJsonMarshaler.ParseArray(d, "RecordsOfHonorTopInfos", JM_NShared_RecordsOfHonorTopInfo.Parse);
		o.MonthlyHeroInfos = BaseJsonMarshaler.ParseArray(d, "MonthlyHeroInfos", JM_NShared_MonthlyHeroInfo.Parse);
		o.SubQuestInfos = BaseJsonMarshaler.ParseArray(d, "SubQuestInfos", JM_NShared_SubQuestInfo.Parse);
		o.MainQuestInfo = BaseJsonMarshaler.ParseObject(d, "MainQuestInfo", JM_NShared_MainQuestInfo.Parse);
		o.EclipseDungeonInfos = BaseJsonMarshaler.ParseArray(d, "EclipseDungeonInfos", JM_NShared_EclipseDungeonInfo.Parse);
		o.ClearMissionInfos = BaseJsonMarshaler.ParseArray(d, "ClearMissionInfos", JM_NShared_ClearMissionInfo.Parse);
		o.SignOutDoneTime = BaseJsonMarshaler.ParseValueString(d, "SignOutDoneTime");
		o.AppGuardAuthId = BaseJsonMarshaler.ParseValueString(d, "AppGuardAuthId");
		o.LastLoginTime = BaseJsonMarshaler.ParseValueString(d, "LastLoginTime");
		o.InactiveTime = BaseJsonMarshaler.ParseValueString(d, "InactiveTime");
		o.RestoreAccountTimeSec = BaseJsonMarshaler.ParseValueInt(d, "RestoreAccountTimeSec");
		o.PlayerAnyMiscs = BaseJsonMarshaler.ParseArray(d, "PlayerAnyMiscs", JM_NShared_PlayerAnyMisc.Parse);
		o.DuplicateUserInfos = BaseJsonMarshaler.ParseArray(d, "DuplicateUserInfos", JM_NShared_UserInfo.Parse);
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
