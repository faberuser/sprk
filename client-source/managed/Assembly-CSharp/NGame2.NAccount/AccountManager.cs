using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount.NNotice;
using NGame2.NChatting;
using NGame2.NConsole;
using NGame2.NContents;
using NGame2.NContentsInfo;
using NGame2.NDropBonusEvent;
using NGame2.NEditor.NViewer;
using NGame2.NGuild;
using NGame2.NHonoraryChamber;
using NGame2.NLogin;
using NGame2.NMessageLog;
using NGame2.NNotice;
using NGame2.NNotification;
using NGame2.NParty;
using NGame2.NPayShopItemEvent;
using NGame2.NPet;
using NGame2.NQuest;
using NGame2.NSocket;
using NGame2.NSubStory;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUtil.NWrappedInfo;
using NGameAsset;
using NShared;
using NShared.BeginCampaign;
using NShared.BeginContent;
using NShared.CompleteScenarioDungeon;
using NShared.EndCampaign;
using NShared.EndContent;
using NShared.FirstLobby;
using NShared.Login;
using NShared.NMessage;
using NShared.NMessage.NProtocol;
using NShared.NRaid;
using NVespa;
using NVespa.NGlobal;
using NVespa.NGlobal.NGuiManager;
using NVespa.NScene;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NAccount;

public class AccountManager : GlobalAutoBehaviour<AccountManager>, IAccountInfo
{
	private class LoginInfo
	{
		public string LoginId { get; set; }

		public string FaceBookId { get; set; }

		public string GooglePlusId { get; set; }

		public (string id, string legacyId) GameCenterId { get; set; }

		public string AppleId { get; set; }

		public string TwitterId { get; set; }

		public string LineId { get; set; }

		public string ServerPrivateIp { get; set; }

		public int ServerPort { get; set; }

		public string ServerHost => ServerPrivateIp + ":" + ServerPort;

		public int AdminLevel { get; set; }

		public string CreatedTime { get; set; }

		public string Nick { get; set; }

		public int TeamLevel { get; set; }
	}

	private class Managers
	{
		public UserManager UserManager;

		public UserHeroManager HeroManager;

		public UserEquipItemManager EquipItemManager;

		public UserItemManager ItemManager;

		public UserCampaignManager CampaignManager;

		public UserTowerManager TowerManager;

		public UserShopItemManager ShopItemManager;

		public UserShopRestockTimeManager ShopRestockTimeManager;

		public UserFriendManager FriendManager;

		public UserAchievementManager AchievementManager;

		public UserUnderPrisonManager UnderPrisonManager;

		public UserTutorialManager TutorialManager;

		public NewUserPayShopProductManager NewPayShopProductManager;

		public UserPlayerProductPurchaseManager PlayerProductPurchaseManager;

		public UserFirstPurchaseManager FirstPurchaseManager;

		public UserPurchaseTimeDurationManager PurchaseTimeDurationManager;

		public UserItemTimeDurationManager ItemTimeDurationManager;

		public UserWorldMapEventManager WorldMapEventManager;

		public UserWorldMapEventTimeManager WorldMapEventTimeManager;

		public UserWantedQuestManager WantedQuestManager;

		public UserAttendanceManager AttendanceManager;

		public UserFreeEquipGachaManager FreeEquipGachaManager;

		public UserEquipGachaManager EquipGachaManager;

		public UserStepUpEquipGachaManager StepUpEquipGachaManager;

		public UserChapterRewardManager ChapterRewardManager;

		public UserHeroFriendlyManager HeroFriendlyManager;

		public UserNPCFriendlyManager NPCFriendlyManager;

		public UserSendRecvFriendshipPointManager SendRecvFriendshipPointManager;

		public UserRaidManager RaidManager;

		public UserDeckManager DeckManager;

		public UserCraftSlotManager CraftSlotManager;

		public UserCostumeManager CostumeManager;

		public UserAccessoryCostumeManager AccessoryCostumeManager;

		public UserWeaponCostumeManager WeaponCostumeManager;

		public UserHairCostumeManager HairCostumeManager;

		public UserCostumeStorageManager CostumeStorageManager;

		public UserPurchaseMarketingManager PurchaseMarketingManager;

		public UserItemUseManager ItemUseManager;

		public UserHideoutDungeonManager HideoutDungeonManager;

		public UserPlayRecordManager PlayRecordManager;

		public UserAccumulateLoginManager AccumulateLoginManager;

		public UserArchiveManager ArchiveManager;

		public UserRouletteManager RouletteManager;

		public UserPlayerCurrencyManager PlayerCurrencyManager;

		public UserTeamLevelBuffManager TeamLevelBuffManager;

		public UserClassBuffManager ClassBuffManager;

		public UserKeyTypeDungeonManager KeyTypeDungeonManager;

		public UserQuestManager QuestManager;

		public UserDispatchBattleInfoManager DispatchBattleInfoManager;

		public UserMonthlyHeroManager MonthlyHeroManager;

		public UserNewbieMissionManager NewbieMissionManager;

		public UserGodkingTrialDungeonInfoManager GodkingTrialDungeonInfoManager;

		public UserSoulWeaponManager SoulWeaponManager;

		public UserEclipseManager EclipseManager;

		public SoulWeaponNoticeManager SoulWeaponNoticeManager;

		public UserAdventureNoticeManager AdventureNoticeManager;

		public UserClearMissionManager ClearMissionManager;

		public UserStarterGachaManager StarterGachaManager;

		public UserHeroPresetStorageManager HeroPresetStorageManager;

		public UserEquipPresetStorageManager EquipPresetStorageManager;

		public UserEquipItemIdentifiedInfoManager EquipItemIdentifiedInfoManager;

		public UserPetManager PetManager;

		public UserPetGachaCountManager PetGachaCountManager;

		public UserPetIncubatorManager PetIncubatorManager;

		public UserPetIncubatorSlotManager PetIncubatorSlotManager;

		public EquipGachaCeilingManager EquipGachaCeilingManager;

		public UserRightAwayProductManager RightAwayProductManager;

		public UserGachaSelectShopManager GachaSelectShopManager;

		public UserKingsPassManager KingsPassManager;

		public UserEventCraftManager EventCraftManager;

		public UserPetExploreManager PetExploreManager;

		public UserEventGrowWorldTreeManager EventGrowWorldTreeManager;

		public UserPayShopDiscountProductManager PayShopDiscountProductManager;

		public UserOpenPunishmentRaidManager OpenPunishmentRaidManager;

		public UserAnyMiscManager AnyMiscManager;
	}

	public const int MaxLoginRetryCount = 5;

	private NShared.Login.Response loginResponse;

	public bool AutoOpenInGameEvent;

	public string CountryServerName;

	public bool IsTutorialSkip;

	private LoginInfo _loginInfo;

	private Managers _managers;

	private DateTime _utcTime_when_SetServerTime = DateTime.UtcNow;

	private DateTime _serverLocalTime = DateTime.UtcNow;

	private DateTime _serverUTCTime = DateTime.UtcNow;

	private int ServerUTCOffsetHour = (int)(DateTime.UtcNow - DateTime.Now).TotalHours;

	public ViewerContainer ViewerContainer = new ViewerContainer();

	public string LoginId
	{
		get
		{
			if (_loginInfo == null)
			{
				return null;
			}
			return _loginInfo.LoginId;
		}
	}

	public string FaceBookId
	{
		get
		{
			if (_loginInfo == null)
			{
				return null;
			}
			return _loginInfo.FaceBookId;
		}
		set
		{
			_loginInfo.FaceBookId = value;
		}
	}

	public string GooglePlusId
	{
		get
		{
			if (_loginInfo == null)
			{
				return null;
			}
			return _loginInfo.GooglePlusId;
		}
		set
		{
			_loginInfo.GooglePlusId = value;
		}
	}

	public (string id, string legacyId) GameCenterId
	{
		get
		{
			if (_loginInfo == null)
			{
				return (id: null, legacyId: null);
			}
			return _loginInfo.GameCenterId;
		}
		set
		{
			_loginInfo.GameCenterId = value;
		}
	}

	public string AppleId
	{
		get
		{
			if (_loginInfo == null)
			{
				return null;
			}
			return _loginInfo.AppleId;
		}
		set
		{
			_loginInfo.AppleId = value;
		}
	}

	public string TwitterId
	{
		get
		{
			if (_loginInfo == null)
			{
				return null;
			}
			return _loginInfo.TwitterId;
		}
		set
		{
			_loginInfo.TwitterId = value;
		}
	}

	public string LineId
	{
		get
		{
			if (_loginInfo == null)
			{
				return null;
			}
			return _loginInfo.LineId;
		}
		set
		{
			_loginInfo.LineId = value;
		}
	}

	public string ServerPrivateIp
	{
		get
		{
			if (_loginInfo == null)
			{
				return null;
			}
			return _loginInfo.ServerPrivateIp;
		}
	}

	public int ServerPort
	{
		get
		{
			if (_loginInfo == null)
			{
				return 0;
			}
			return _loginInfo.ServerPort;
		}
	}

	public string ServerHost
	{
		get
		{
			if (_loginInfo == null)
			{
				return null;
			}
			return _loginInfo.ServerHost;
		}
	}

	public int AdminLevel
	{
		get
		{
			if (_loginInfo == null)
			{
				return 0;
			}
			return _loginInfo.AdminLevel;
		}
	}

	public string CreatedTime
	{
		get
		{
			if (_loginInfo == null)
			{
				return null;
			}
			return _loginInfo.CreatedTime;
		}
	}

	public string Nick
	{
		get
		{
			if (_loginInfo == null)
			{
				return null;
			}
			return _loginInfo.Nick;
		}
	}

	public int TeamLevel
	{
		get
		{
			if (_loginInfo == null)
			{
				return 0;
			}
			return _loginInfo.TeamLevel;
		}
	}

	public string UniqueUserString { get; set; }

	public UserInfo UserInfo
	{
		get
		{
			if (UserManager == null)
			{
				return null;
			}
			return UserManager.UserInfo;
		}
	}

	public UserManager UserManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.UserManager;
		}
	}

	public UserHeroManager HeroManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.HeroManager;
		}
	}

	public UserEquipItemManager EquipItemManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.EquipItemManager;
		}
	}

	public UserItemManager ItemManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.ItemManager;
		}
	}

	public UserCampaignManager CampaignManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.CampaignManager;
		}
	}

	public UserTowerManager TowerManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.TowerManager;
		}
	}

	public UserShopItemManager ShopItemManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.ShopItemManager;
		}
	}

	public UserShopRestockTimeManager ShopRestockTimeManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.ShopRestockTimeManager;
		}
	}

	public UserFriendManager FriendManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.FriendManager;
		}
	}

	public UserAchievementManager AchievementManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.AchievementManager;
		}
	}

	public UserUnderPrisonManager UnderPrisonManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.UnderPrisonManager;
		}
	}

	public UserTutorialManager TutorialManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.TutorialManager;
		}
	}

	public NewUserPayShopProductManager NewPayShopProductManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.NewPayShopProductManager;
		}
	}

	public UserPlayerProductPurchaseManager PlayerProductPurchaseManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.PlayerProductPurchaseManager;
		}
	}

	public UserFirstPurchaseManager FirstPurchaseManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.FirstPurchaseManager;
		}
	}

	public UserPurchaseTimeDurationManager PurchaseTimeDurationManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.PurchaseTimeDurationManager;
		}
	}

	public UserItemTimeDurationManager ItemTimeDurationManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.ItemTimeDurationManager;
		}
	}

	public UserWorldMapEventManager WorldMapEventManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.WorldMapEventManager;
		}
	}

	public UserWorldMapEventTimeManager WorldMapEventTimeManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.WorldMapEventTimeManager;
		}
	}

	public UserWantedQuestManager WantedQuestManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.WantedQuestManager;
		}
	}

	public UserAttendanceManager AttendanceManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.AttendanceManager;
		}
	}

	public UserFreeEquipGachaManager FreeEquipGachaManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.FreeEquipGachaManager;
		}
	}

	public UserEquipGachaManager EquipGachaManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.EquipGachaManager;
		}
	}

	public UserStepUpEquipGachaManager StepUpEquipGachamanager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.StepUpEquipGachaManager;
		}
	}

	public UserChapterRewardManager ChapterRewardManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.ChapterRewardManager;
		}
	}

	public UserSendRecvFriendshipPointManager SendRecvFriendshipPointManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.SendRecvFriendshipPointManager;
		}
	}

	public UserRaidManager RaidManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.RaidManager;
		}
	}

	public UserDeckManager DeckManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.DeckManager;
		}
	}

	public UserHeroFriendlyManager HeroFriendlyManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.HeroFriendlyManager;
		}
	}

	public UserNPCFriendlyManager NPCFriendlyManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.NPCFriendlyManager;
		}
	}

	public UserCraftSlotManager CraftSlotManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.CraftSlotManager;
		}
	}

	public UserCostumeManager CostumeManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.CostumeManager;
		}
	}

	public UserCostumeStorageManager CostumeStorageManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.CostumeStorageManager;
		}
	}

	public UserWeaponCostumeManager WeaponCostumeManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.WeaponCostumeManager;
		}
	}

	public UserHairCostumeManager HairCostumeManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.HairCostumeManager;
		}
	}

	public UserAccessoryCostumeManager AccessoryCostumeManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.AccessoryCostumeManager;
		}
	}

	public UserPurchaseMarketingManager PurchaseMarketingManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.PurchaseMarketingManager;
		}
	}

	public UserItemUseManager ItemUseManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.ItemUseManager;
		}
	}

	public UserHideoutDungeonManager HideoutDungeonManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.HideoutDungeonManager;
		}
	}

	public UserPlayRecordManager PlayRecordManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.PlayRecordManager;
		}
	}

	public UserArchiveManager ArchiveManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.ArchiveManager;
		}
	}

	public UserAccumulateLoginManager AccumulateLoginManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.AccumulateLoginManager;
		}
	}

	public UserRouletteManager RouletteManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.RouletteManager;
		}
	}

	public UserPlayerCurrencyManager PlayerCurrencyManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.PlayerCurrencyManager;
		}
	}

	public UserTeamLevelBuffManager TeamLevelBuffManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.TeamLevelBuffManager;
		}
	}

	public UserClassBuffManager ClassBuffManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.ClassBuffManager;
		}
	}

	public UserKeyTypeDungeonManager KeyTypeDungeonManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.KeyTypeDungeonManager;
		}
	}

	public UserMonthlyHeroManager MonthlyHeroManager
	{
		get
		{
			if (AccountManagers == null)
			{
				return null;
			}
			return AccountManagers.MonthlyHeroManager;
		}
	}

	public UserDispatchBattleInfoManager DispatchBattleInfoManager
	{
		get
		{
			if (_managers == null)
			{
				return null;
			}
			return _managers.DispatchBattleInfoManager;
		}
	}

	public UserNewbieMissionManager NewbieMissionManager
	{
		get
		{
			if (_managers == null)
			{
				return null;
			}
			return _managers.NewbieMissionManager;
		}
	}

	public UserQuestManager QuestManager
	{
		get
		{
			if (_managers == null)
			{
				return null;
			}
			return _managers.QuestManager;
		}
	}

	public UserGodkingTrialDungeonInfoManager GodkingTrialDungeonInfoManager
	{
		get
		{
			if (_managers == null)
			{
				return null;
			}
			return _managers.GodkingTrialDungeonInfoManager;
		}
	}

	public UserSoulWeaponManager SoulWeaponManager
	{
		get
		{
			if (_managers == null)
			{
				return null;
			}
			return _managers.SoulWeaponManager;
		}
	}

	public UserEclipseManager EclipseManager
	{
		get
		{
			if (_managers == null)
			{
				return null;
			}
			return _managers.EclipseManager;
		}
	}

	public SoulWeaponNoticeManager SoulWeaponNoticeManager
	{
		get
		{
			if (_managers == null)
			{
				return null;
			}
			return _managers.SoulWeaponNoticeManager;
		}
	}

	public UserAdventureNoticeManager AdventureNoticeManager
	{
		get
		{
			if (_managers == null)
			{
				return null;
			}
			return _managers.AdventureNoticeManager;
		}
	}

	public UserClearMissionManager ClearMissionManager
	{
		get
		{
			if (_managers == null)
			{
				return null;
			}
			return _managers.ClearMissionManager;
		}
	}

	public UserHeroPresetStorageManager HeroPresetStorageManager
	{
		get
		{
			if (_managers == null)
			{
				return null;
			}
			return _managers.HeroPresetStorageManager;
		}
	}

	public UserEquipPresetStorageManager EquipPresetSotrageManger
	{
		get
		{
			if (_managers == null)
			{
				return null;
			}
			return _managers.EquipPresetStorageManager;
		}
	}

	public UserStarterGachaManager StarterGachaManager
	{
		get
		{
			if (_managers == null)
			{
				return null;
			}
			return _managers.StarterGachaManager;
		}
	}

	public UserEquipItemIdentifiedInfoManager EquipItemIdentifiedInfoManager
	{
		get
		{
			if (_managers == null)
			{
				return null;
			}
			return _managers.EquipItemIdentifiedInfoManager;
		}
	}

	public UserPetManager PetManager
	{
		get
		{
			if (_managers == null)
			{
				return null;
			}
			return _managers.PetManager;
		}
	}

	public UserPetGachaCountManager PetGachaCountManager
	{
		get
		{
			if (_managers == null)
			{
				return null;
			}
			return _managers.PetGachaCountManager;
		}
	}

	public UserPetIncubatorManager PetIncubatorManager
	{
		get
		{
			if (_managers == null)
			{
				return null;
			}
			return _managers.PetIncubatorManager;
		}
	}

	public UserPetIncubatorSlotManager PetIncubatorSlotManager
	{
		get
		{
			if (_managers == null)
			{
				return null;
			}
			return _managers.PetIncubatorSlotManager;
		}
	}

	public EquipGachaCeilingManager EquipGachaCeilingManager
	{
		get
		{
			if (_managers == null)
			{
				return null;
			}
			return _managers.EquipGachaCeilingManager;
		}
	}

	public UserRightAwayProductManager RightAwayProductManager
	{
		get
		{
			if (_managers == null)
			{
				return null;
			}
			return _managers.RightAwayProductManager;
		}
	}

	public UserGachaSelectShopManager GachaSelectShopManager
	{
		get
		{
			if (_managers == null)
			{
				return null;
			}
			return _managers.GachaSelectShopManager;
		}
	}

	public UserKingsPassManager KingsPassManager
	{
		get
		{
			if (_managers == null)
			{
				return null;
			}
			return _managers.KingsPassManager;
		}
	}

	public UserEventCraftManager EventCraftManager
	{
		get
		{
			if (_managers == null)
			{
				return null;
			}
			return _managers.EventCraftManager;
		}
	}

	public UserPetExploreManager PetExploreManager
	{
		get
		{
			if (_managers == null)
			{
				return null;
			}
			return _managers.PetExploreManager;
		}
	}

	public UserEventGrowWorldTreeManager EventGrowWorldTreeManager
	{
		get
		{
			if (_managers == null)
			{
				return null;
			}
			return _managers.EventGrowWorldTreeManager;
		}
	}

	public UserPayShopDiscountProductManager PayShopDiscountProductManager
	{
		get
		{
			if (_managers == null)
			{
				return null;
			}
			return _managers.PayShopDiscountProductManager;
		}
	}

	public UserOpenPunishmentRaidManager OpenPunishmentRaidManager
	{
		get
		{
			if (_managers == null)
			{
				return null;
			}
			return _managers.OpenPunishmentRaidManager;
		}
	}

	public UserAnyMiscManager UserAnyMiscManager
	{
		get
		{
			if (_managers == null)
			{
				return null;
			}
			return _managers.AnyMiscManager;
		}
	}

	public PlayerInAppPurchaseInfo IncompleteInAppPurchaseInfo { get; private set; }

	public PlayerInAppPurchaseInfo[] NotCompletedInAppPurchaseInfos { get; private set; }

	public int RewardableScenarioQuestIndex { get; private set; }

	public PatchNoteInfo PatchNote { get; private set; }

	public int LastGuildRaidRank { get; private set; }

	public long LastGuildRaidScore { get; private set; }

	public WebNoticeInfo[] WebNotices { get; private set; }

	public DateTime GuildContentsRestrictedTime { get; private set; }

	public int LastGuildSuppressRank { get; private set; }

	public long LastGuildSuppressScore { get; private set; }

	public int LastGuildSuppressLevel { get; private set; }

	public int LastChallengeRaidRank { get; private set; }

	public long LastChallengeRaidBestScore { get; private set; }

	public TopClearDungeonInfo[] TopClearDungeonInfos { get; private set; }

	public bool LoginResponseComplete { get; private set; }

	private Managers AccountManagers
	{
		get
		{
			if (_managers == null)
			{
				_managers = new Managers();
			}
			return _managers;
		}
		set
		{
			_managers = value;
		}
	}

	public List<PlayGuideInfo> ListPlayGuideInfo { get; set; }

	public List<PlayGuideInfo> ListSortedPlayGuideInfo { get; set; }

	protected override void Awake()
	{
		base.Awake();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		Close();
	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
		Close();
	}

	public void AddEditorViewer()
	{
		SceneAutoBehaviour<NVespa.NScene.GuiManager>.instance.Add("AccountManager", this, delegate
		{
			ViewerContainer.DrawEditor();
		});
	}

	public void InitUserInfo(string loginId, string createdTime, UserInfo userInfo, PlayerMiscInfo miscInfo, int adminLevel, PlayerGuildPointInfo guildPointInfo)
	{
		LocalAccountDefine.AccountId = userInfo.AccountId;
		SingletonAuto<UserCampaignManager.ChapterDungeonInfo>.instance.Reset();
		_loginInfo = _loginInfo ?? new LoginInfo();
		_loginInfo.LoginId = loginId;
		_loginInfo.AdminLevel = adminLevel;
		_loginInfo.CreatedTime = createdTime;
		AccountManagers.UserManager = new UserManager(this, userInfo, miscInfo, guildPointInfo);
		SetUniqueUserString(userInfo);
		SetSentryUserInfo(userInfo);
		GameConsole.InputListen = SingletonAuto<ConsoleInputManager>.instance.OnConsoleInput;
	}

	private void SetSentryUserInfo(UserInfo userInfo)
	{
		SentrySdk.SetUserInfo((userInfo != null && !String_Extension_Method.IsNullOrEmpty(userInfo.Nick)) ? userInfo.Nick : string.Empty, UniqueUserString);
	}

	public void SetUniqueUserString(UserInfo userInfo)
	{
		if (String_Extension_Method.IsNullOrEmpty(UniqueUserString))
		{
			string arg = ((userInfo != null) ? userInfo.AccountId.ToString() : string.Empty);
			string arg2 = (SingletonUpdater<LoginManager>.isValidInstance ? SingletonUpdater<LoginManager>.instance.ServerName : string.Empty);
			LoginMethodType lastLoginMethod = LoginData.LastLoginMethod;
			UniqueUserString = $"{arg}@{arg2}.{lastLoginMethod}";
		}
	}

	public void InitNewUser(string createdTime)
	{
		SingletonUpdater<LoginManager>.instance.IsNewUser = false;
		if (String_Extension_Method.IsNullOrEmpty(createdTime))
		{
			SingletonUpdater<LoginManager>.instance.IsNewUser = true;
			return;
		}
		DateTime dateTime = DateTime.Parse(createdTime);
		DateTime currentServerUTCTime = GetCurrentServerUTCTime();
		if (DateTime.Compare(dateTime, currentServerUTCTime) > 0)
		{
			if ((dateTime - currentServerUTCTime).TotalHours < 24.0)
			{
				SingletonUpdater<LoginManager>.instance.IsNewUser = true;
			}
		}
		else if ((currentServerUTCTime - dateTime).TotalHours < 24.0)
		{
			SingletonUpdater<LoginManager>.instance.IsNewUser = true;
		}
		if (NGameAsset.Define.FullDownloadUser.Value)
		{
			SingletonUpdater<LoginManager>.instance.IsNewUser = false;
		}
		if (Convert.ToBoolean(SingletonUpdater<LoginManager>.instance.VersionInfo.OffBackgroundDownload))
		{
			SingletonUpdater<LoginManager>.instance.IsNewUser = false;
		}
	}

	private void Close()
	{
		foreach (IUserObserverManager userObserverManager in GetUserObserverManagers())
		{
			userObserverManager.ClearObserver();
		}
		Reset();
	}

	private void Update()
	{
		if (UserInfo != null && _loginInfo != null && AccountManagers != null)
		{
			if (AccountManagers.UserManager != null)
			{
				AccountManagers.UserManager.Update();
			}
			if (AccountManagers.HideoutDungeonManager != null)
			{
				AccountManagers.HideoutDungeonManager.Update();
			}
			if (AccountManagers.KeyTypeDungeonManager != null)
			{
				AccountManagers.KeyTypeDungeonManager.Update();
			}
			if (AccountManagers.PlayerCurrencyManager != null)
			{
				AccountManagers.PlayerCurrencyManager.Update();
			}
		}
	}

	public static bool IsLogined()
	{
		if (IsLogin())
		{
			return GlobalAutoBehaviour<AccountManager>.instance.LoginResponseComplete;
		}
		return false;
	}

	public static bool IsLogin()
	{
		return !String_Extension_Method.IsNullOrEmpty(LoginData.LastAccessToken);
	}

	public static void Logout()
	{
		if (GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			AutoBattle.Stop();
			GlobalAutoBehaviour<AccountManager>.instance._loginInfo = null;
		}
		SprkAccounts.ClearSession();
	}

	public static void Reset()
	{
		if (!SystemStatus.IsApplicationQuit && GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			GlobalAutoBehaviour<AccountManager>.instance._loginInfo = null;
			GlobalAutoBehaviour<AccountManager>.instance.LoginResponseComplete = false;
			GlobalAutoBehaviour<AccountManager>.instance.loginResponse = null;
		}
	}

	public bool HasMinLevelHero(int MinLevel)
	{
		foreach (CachedHeroInfo heroInfo in GetHeroInfoList(sort: true))
		{
			if (heroInfo.Level >= MinLevel)
			{
				return true;
			}
		}
		return false;
	}

	public void RemoveGoogleGiftCardPromotion(string productId)
	{
	}

	public void RemoveGooglePlayPointReward(string productId)
	{
	}

	public AttendanceKeywordGroup GetCurReceivedAttendanceKeyword()
	{
		AttendanceKeywordGroup result = new AttendanceKeywordGroup(EAttendanceKeyword.ReturnUser);
		if (result.IsValid_AttendanceInfo())
		{
			return result;
		}
		result = new AttendanceKeywordGroup(EAttendanceKeyword.BeginUser201110);
		if (result.IsValid_AttendanceInfo())
		{
			return result;
		}
		result = new AttendanceKeywordGroup(EAttendanceKeyword.Common);
		if (result.IsValid_AttendanceData())
		{
			return result;
		}
		return default(AttendanceKeywordGroup);
	}

	public void InitStaminaInfo(StaminaResultInfo[] infos)
	{
		AccountManagers.UserManager.Apply(infos);
	}

	public void InitHeroInfos(HeroInfo[] infos)
	{
		AccountManagers.HeroManager = new UserHeroManager();
		AccountManagers.HeroManager.Set(infos);
	}

	public void InitRunePageInfos(HeroRunePageInfo[] infos)
	{
		AccountManagers.HeroManager.Apply(infos, isRefresh: true);
	}

	public void InitEquipItemInfos(EquipItemInfo[] infos)
	{
		AccountManagers.EquipItemManager = new UserEquipItemManager();
		AccountManagers.EquipItemManager.Set(infos);
	}

	public void InitItemInfos(ItemInfo[] infos)
	{
		AccountManagers.ItemManager = new UserItemManager();
		AccountManagers.ItemManager.Set(infos);
	}

	public void InitChapterDungeonInfos(ChapterDungeonInfo[] infos)
	{
		AccountManagers.CampaignManager = new UserCampaignManager();
		AccountManagers.CampaignManager.Set(infos);
	}

	public void InitTowerInfos(TowerInfo[] infos)
	{
		AccountManagers.TowerManager = new UserTowerManager();
		AccountManagers.TowerManager.Set(infos);
	}

	public void InitShopRestockTimeInfos(ShopRestockTimeInfo[] infos)
	{
		AccountManagers.ShopRestockTimeManager = new UserShopRestockTimeManager();
		AccountManagers.ShopRestockTimeManager.Set(infos);
	}

	public void InitShopItemInfos(ShopListItemInfo[] infos)
	{
		AccountManagers.ShopItemManager = new UserShopItemManager();
		AccountManagers.ShopItemManager.Set(infos);
	}

	public void InitWorldMapEventInfo(WorldMapEventInfo[] infos)
	{
		AccountManagers.WorldMapEventManager = new UserWorldMapEventManager();
		AccountManagers.WorldMapEventManager.Set(infos);
	}

	public void InitWorldMapEventTimeInfo(WorldMapEventTimeInfo[] infos)
	{
		AccountManagers.WorldMapEventTimeManager = new UserWorldMapEventTimeManager();
		AccountManagers.WorldMapEventTimeManager.Set(infos);
	}

	public void InitFriendManager(FriendInfo[] infos, FriendUserInfo[] detailInfos)
	{
		if (AccountManagers.FriendManager == null)
		{
			AccountManagers.FriendManager = new UserFriendManager();
		}
		AccountManagers.FriendManager.Set(infos, detailInfos);
	}

	public void InitAchievementManager(AchievementInfo[] infos)
	{
		if (AccountManagers.AchievementManager == null)
		{
			AccountManagers.AchievementManager = new UserAchievementManager();
		}
		AccountManagers.AchievementManager.Set(infos);
	}

	public void InitScenarioQuestInfo(ScenarioQuestInfo info)
	{
	}

	public void InitTutorialInfos(TutorialInfo[] infos)
	{
		if (AccountManagers.TutorialManager == null)
		{
			AccountManagers.TutorialManager = new UserTutorialManager();
		}
		AccountManagers.TutorialManager.Reset(infos);
	}

	public void SetUnderPrisonInfos(UnderPrisonInfo[] infos, bool init = false)
	{
		if (AccountManagers.UnderPrisonManager == null)
		{
			AccountManagers.UnderPrisonManager = new UserUnderPrisonManager();
		}
		AccountManagers.UnderPrisonManager.Reset(infos);
		if (init)
		{
			AccountManagers.UnderPrisonManager.InitNew();
		}
	}

	public void SetNewPayShopProductInfo(NewPayShopProductInfo[] infos)
	{
		if (AccountManagers.NewPayShopProductManager == null)
		{
			AccountManagers.NewPayShopProductManager = new NewUserPayShopProductManager();
		}
		AccountManagers.NewPayShopProductManager.Set(infos);
	}

	public void SetPlayerProductPurchaseInfo(PlayerProductPurchaseInfo[] info)
	{
		if (AccountManagers.PlayerProductPurchaseManager == null)
		{
			AccountManagers.PlayerProductPurchaseManager = new UserPlayerProductPurchaseManager();
		}
		AccountManagers.PlayerProductPurchaseManager.Set(info);
	}

	public void SetTopClearDungeonInfos(TopClearDungeonInfo[] infos)
	{
		TopClearDungeonInfos = infos;
	}

	public void InitHeroFriendlyInfo(PlayerHeroFriendlyInfo info)
	{
		if (AccountManagers.HeroFriendlyManager == null)
		{
			AccountManagers.HeroFriendlyManager = new UserHeroFriendlyManager();
		}
		AccountManagers.HeroFriendlyManager.Set(info);
	}

	public void SetHeroFriendlyInfo(PlayerHeroFriendlyInfo info)
	{
		if (AccountManagers.HeroFriendlyManager == null)
		{
			AccountManagers.HeroFriendlyManager = new UserHeroFriendlyManager();
		}
		AccountManagers.HeroFriendlyManager.Add(info);
	}

	public void InitNPCFriendlyInfo(NPCFriendlyInfo[] infos)
	{
		if (AccountManagers.NPCFriendlyManager == null)
		{
			AccountManagers.NPCFriendlyManager = new UserNPCFriendlyManager();
		}
		AccountManagers.NPCFriendlyManager.Set(infos);
	}

	public void SetNPCFriendlyInfo(NPCFriendlyInfo info)
	{
		if (AccountManagers.NPCFriendlyManager == null)
		{
			AccountManagers.NPCFriendlyManager = new UserNPCFriendlyManager();
		}
		AccountManagers.NPCFriendlyManager.Add(info);
	}

	public void InitPlayerPurchaseTimeDurationInfo(PlayerPurchaseTimeDurationInfo[] infos)
	{
		if (AccountManagers.PurchaseTimeDurationManager == null)
		{
			AccountManagers.PurchaseTimeDurationManager = new UserPurchaseTimeDurationManager();
		}
		AccountManagers.PurchaseTimeDurationManager.Set(infos);
	}

	public void SetPlayerPurchaseTimeDurationInfo(PlayerPurchaseTimeDurationInfo info)
	{
		if (AccountManagers.PurchaseTimeDurationManager == null)
		{
			AccountManagers.PurchaseTimeDurationManager = new UserPurchaseTimeDurationManager();
		}
		AccountManagers.PurchaseTimeDurationManager.Add(info);
	}

	public void SetPlayerPurchaseTimeDurationInfo(PlayerPurchaseTimeDurationInfo[] infos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			SetPlayerPurchaseTimeDurationInfo(infos[i]);
		}
	}

	public void InitPlayerItemTimeDurationInfo(PlayerItemTimeDurationInfo[] infos)
	{
		if (AccountManagers.ItemTimeDurationManager == null)
		{
			AccountManagers.ItemTimeDurationManager = new UserItemTimeDurationManager();
		}
		AccountManagers.ItemTimeDurationManager.Apply(infos);
	}

	public void InitWantedQuestInfo(WantedQuestInfo[] infos)
	{
		if (AccountManagers.WantedQuestManager == null)
		{
			AccountManagers.WantedQuestManager = new UserWantedQuestManager();
		}
		AccountManagers.WantedQuestManager.Set(infos);
	}

	public void InitAttendanceInfo(AttendanceInfo[] infos)
	{
		if (AccountManagers.AttendanceManager == null)
		{
			AccountManagers.AttendanceManager = new UserAttendanceManager();
		}
		AccountManagers.AttendanceManager.Set(infos);
	}

	public void InitFreeEquipGachaInfo(FreeEquipGachaInfo[] infos)
	{
		if (AccountManagers.FreeEquipGachaManager == null)
		{
			AccountManagers.FreeEquipGachaManager = new UserFreeEquipGachaManager();
		}
		AccountManagers.FreeEquipGachaManager.Set(infos);
	}

	public void InitEquipGachaInfo(EquipGachaInfo[] infos)
	{
		if (AccountManagers.EquipGachaManager == null)
		{
			AccountManagers.EquipGachaManager = new UserEquipGachaManager();
		}
		AccountManagers.EquipGachaManager.Set(infos);
	}

	public void InitStepUpEquipGachaInfo(StepUpEquipGachaInfo[] infos)
	{
		if (AccountManagers.StepUpEquipGachaManager == null)
		{
			AccountManagers.StepUpEquipGachaManager = new UserStepUpEquipGachaManager();
		}
		AccountManagers.StepUpEquipGachaManager.Set(infos);
	}

	public void InitChapterRewardInfo(ChapterRewardInfo[] infos)
	{
		if (AccountManagers.ChapterRewardManager == null)
		{
			AccountManagers.ChapterRewardManager = new UserChapterRewardManager();
		}
		AccountManagers.ChapterRewardManager.Set(infos);
	}

	public void InitSendRecvFriendshipPointManager(FriendshipPointInfo[] friendshipPointInfos, FriendshipPointInfo[] recvFriendshipPointInfos)
	{
		if (AccountManagers.SendRecvFriendshipPointManager == null)
		{
			AccountManagers.SendRecvFriendshipPointManager = new UserSendRecvFriendshipPointManager();
		}
		AccountManagers.SendRecvFriendshipPointManager.Set(friendshipPointInfos, recvFriendshipPointInfos);
	}

	public void InitRaidManager(NShared.RaidInfo[] infos)
	{
		if (AccountManagers.RaidManager == null)
		{
			AccountManagers.RaidManager = new UserRaidManager();
		}
		AccountManagers.RaidManager.Set(infos);
	}

	public void InitDeckManager(DeckInfo[] infos)
	{
		if (AccountManagers.DeckManager == null)
		{
			AccountManagers.DeckManager = new UserDeckManager();
		}
		AccountManagers.DeckManager.Set(infos);
	}

	public void InitGuildInfo(MyGuildInfo info)
	{
		GlobalAutoBehaviour<GuildManager>.instance.SetMyGuildInfo(info);
	}

	public void InitCraftSlotInfo(CraftSlotInfo[] infos)
	{
		if (AccountManagers.CraftSlotManager == null)
		{
			AccountManagers.CraftSlotManager = new UserCraftSlotManager();
		}
		AccountManagers.CraftSlotManager.Apply(infos);
	}

	public void InitCostumeInfo(CostumeInfo[] infos)
	{
		if (AccountManagers.CostumeManager == null)
		{
			AccountManagers.CostumeManager = new UserCostumeManager();
		}
		AccountManagers.CostumeManager.Apply(infos);
	}

	public void InitCostumeStorageInfo(CostumeStorageSlotInfo[] infos)
	{
		if (AccountManagers.CostumeStorageManager == null)
		{
			AccountManagers.CostumeStorageManager = new UserCostumeStorageManager();
		}
		AccountManagers.CostumeStorageManager.Apply(infos);
	}

	public void InitWeaponCostumeInfo(WeaponCostumeInfo[] infos)
	{
		if (AccountManagers.WeaponCostumeManager == null)
		{
			AccountManagers.WeaponCostumeManager = new UserWeaponCostumeManager();
		}
		AccountManagers.WeaponCostumeManager.Apply(infos);
	}

	public void IniHairCostumeInfo(HairCostumeInfo[] infos)
	{
		if (AccountManagers.HairCostumeManager == null)
		{
			AccountManagers.HairCostumeManager = new UserHairCostumeManager();
		}
		AccountManagers.HairCostumeManager.Apply(infos);
	}

	public void InitAccessoryInfo(PlayerAccessoryCostumeInfo[] infos)
	{
		if (AccountManagers.AccessoryCostumeManager == null)
		{
			AccountManagers.AccessoryCostumeManager = new UserAccessoryCostumeManager();
		}
		AccountManagers.AccessoryCostumeManager.Apply(infos);
	}

	public void InitGuildContentsRestrictedTime(string time)
	{
		DateTime o_dateTime;
		if (String_Extension_Method.IsNullOrEmpty(time))
		{
			o_dateTime = DateTime.MinValue;
		}
		else
		{
			Vespa_String_Extension_Method2.TryParseNetworkTime2(time, out o_dateTime);
		}
		GuildContentsRestrictedTime = o_dateTime;
	}

	public void InitPurchaseMarketingInfo(PurchaseMarketingInfo[] infos)
	{
		if (AccountManagers.PurchaseMarketingManager == null)
		{
			AccountManagers.PurchaseMarketingManager = new UserPurchaseMarketingManager();
		}
		AccountManagers.PurchaseMarketingManager.Apply(infos);
	}

	public void InitItemUseInfo(PlayerItemUseInfo[] infos)
	{
		if (AccountManagers.ItemUseManager == null)
		{
			AccountManagers.ItemUseManager = new UserItemUseManager();
		}
		AccountManagers.ItemUseManager.Apply(infos);
	}

	public void InitHideoutDungeonInfos(HideoutDungeonInfo[] infos)
	{
		if (AccountManagers.HideoutDungeonManager == null)
		{
			AccountManagers.HideoutDungeonManager = new UserHideoutDungeonManager();
		}
		AccountManagers.HideoutDungeonManager.Set(infos);
	}

	public void InitKeyTypeDungeonInfos(ConquestDungeonInfo[] infos)
	{
		if (AccountManagers.KeyTypeDungeonManager == null)
		{
			AccountManagers.KeyTypeDungeonManager = new UserKeyTypeDungeonManager();
		}
		AccountManagers.KeyTypeDungeonManager.Apply(infos);
	}

	public void InitPlayRecordInfos(PlayRecordInfo[] infos)
	{
		if (AccountManagers.PlayRecordManager == null)
		{
			AccountManagers.PlayRecordManager = new UserPlayRecordManager();
		}
		AccountManagers.PlayRecordManager.Reset(infos);
	}

	public void InitLoginDailyInfos(LoginDailyInfo[] infos)
	{
		if (AccountManagers.AccumulateLoginManager == null)
		{
			AccountManagers.AccumulateLoginManager = new UserAccumulateLoginManager();
		}
		AccountManagers.AccumulateLoginManager.Set(infos);
	}

	public void InitPlayArchiveInfos(PlayerArchiveInfo[] infos)
	{
		if (AccountManagers.ArchiveManager == null)
		{
			AccountManagers.ArchiveManager = new UserArchiveManager();
		}
		AccountManagers.ArchiveManager.Apply(infos);
	}

	public void InitRouletteInfos(RouletteInfo[] infos)
	{
		if (AccountManagers.RouletteManager == null)
		{
			AccountManagers.RouletteManager = new UserRouletteManager();
		}
		AccountManagers.RouletteManager.Set(infos);
	}

	public void InitPlayerCurrencyInfos(PlayerCurrencyInfo[] infos)
	{
		if (AccountManagers.PlayerCurrencyManager == null)
		{
			AccountManagers.PlayerCurrencyManager = new UserPlayerCurrencyManager();
		}
		AccountManagers.PlayerCurrencyManager.Set(infos);
	}

	public void InitTeamLevelBuffInfos(TeamLevelBuff[] buffInfos)
	{
		if (AccountManagers.TeamLevelBuffManager == null)
		{
			AccountManagers.TeamLevelBuffManager = new UserTeamLevelBuffManager();
		}
		AccountManagers.TeamLevelBuffManager.Set(buffInfos);
	}

	public void InitClassBuffInfos(ClassBuffPointInfo[] pointInfos, ClassBuffInfo[] infos)
	{
		if (AccountManagers.ClassBuffManager == null)
		{
			AccountManagers.ClassBuffManager = new UserClassBuffManager();
		}
		AccountManagers.ClassBuffManager.Set(infos);
		AccountManagers.ClassBuffManager.InitClassBuffInfo(infos);
		AccountManagers.ClassBuffManager.InitClassBuffPoint(pointInfos);
	}

	public void InitQuestInfos(MainQuestInfo mainQuestInfo, SubQuestInfo[] subQuestInfos)
	{
		if (AccountManagers.QuestManager == null)
		{
			AccountManagers.QuestManager = new UserQuestManager();
		}
		AccountManagers.QuestManager.Apply(mainQuestInfo);
		AccountManagers.QuestManager.InitSubQuestInfos(subQuestInfos);
	}

	public void InitUserDispatchBattleInfoManager(DispatchBattleInfo[] infos)
	{
		if (infos == null)
		{
			infos = new DispatchBattleInfo[0];
		}
		List<WCL_DispatchBattleInfo> list = new List<WCL_DispatchBattleInfo>(infos.Length);
		int i = 0;
		for (int num = infos.Length; i < num; i++)
		{
			if (Vespa_Generic_IList_Extension_Method.IsValidIndex(infos, i) && infos[i] != null)
			{
				list.Add(new WCL_DispatchBattleInfo().Copy(infos[i]));
			}
		}
		if (AccountManagers.DispatchBattleInfoManager == null)
		{
			AccountManagers.DispatchBattleInfoManager = new UserDispatchBattleInfoManager();
		}
		AccountManagers.DispatchBattleInfoManager.Set(list.ToArray());
	}

	public void InitUserGodkingTrialDungeonInfoManager(GodkingTrialDungeonInfo[] infos)
	{
		if (AccountManagers.GodkingTrialDungeonInfoManager == null)
		{
			AccountManagers.GodkingTrialDungeonInfoManager = new UserGodkingTrialDungeonInfoManager();
		}
		AccountManagers.GodkingTrialDungeonInfoManager.Apply(infos);
	}

	public void InitUserSoulWeaponManager(SoulWeaponInfo[] infos)
	{
		if (AccountManagers.SoulWeaponManager == null)
		{
			AccountManagers.SoulWeaponManager = new UserSoulWeaponManager();
		}
		AccountManagers.SoulWeaponManager.Set(infos);
	}

	public void InitEclipseManager(EclipseDungeonInfo[] eclipseDungeonInfos)
	{
		if (AccountManagers.EclipseManager == null)
		{
			AccountManagers.EclipseManager = new UserEclipseManager();
		}
		if (eclipseDungeonInfos != null && eclipseDungeonInfos.Count() > 0)
		{
			AccountManagers.EclipseManager.Apply(eclipseDungeonInfos[0]);
		}
	}

	public void InitSoulWeaponNoticeManager()
	{
		if (_managers.SoulWeaponNoticeManager == null)
		{
			_managers.SoulWeaponNoticeManager = new SoulWeaponNoticeManager();
		}
	}

	public void InitAdventureNoticeManager()
	{
		if (_managers.AdventureNoticeManager == null)
		{
			_managers.AdventureNoticeManager = new UserAdventureNoticeManager();
		}
	}

	public void InitMonthlyHeroInfos(MonthlyHeroInfo[] infos)
	{
		if (AccountManagers.MonthlyHeroManager == null)
		{
			AccountManagers.MonthlyHeroManager = new UserMonthlyHeroManager();
		}
		AccountManagers.MonthlyHeroManager.Set(infos);
	}

	public void InitNewbieMissionInfos(NewbieMissionInfo[] infos)
	{
		if (AccountManagers.NewbieMissionManager == null)
		{
			AccountManagers.NewbieMissionManager = new UserNewbieMissionManager();
		}
		AccountManagers.NewbieMissionManager.Set(infos);
	}

	public void InitClearMissionInfos(ClearMissionInfo[] infos)
	{
		if (_managers.ClearMissionManager == null)
		{
			_managers.ClearMissionManager = new UserClearMissionManager();
		}
		_managers.ClearMissionManager.Set(infos);
	}

	public void InitStaterGachaInfos(StarterPickupEquipGachaInfo[] infos)
	{
		if (_managers.StarterGachaManager == null)
		{
			_managers.StarterGachaManager = new UserStarterGachaManager();
		}
		_managers.StarterGachaManager.Set(infos);
	}

	public void InitHeroPresetStorageInfos(HeroPresetStorageInfo[] infos)
	{
		if (_managers.HeroPresetStorageManager == null)
		{
			_managers.HeroPresetStorageManager = new UserHeroPresetStorageManager();
		}
		_managers.HeroPresetStorageManager.Set(infos);
	}

	public void InitEquipPresetStorageInfos(EquipPresetInfo[] infos)
	{
		if (_managers.EquipPresetStorageManager == null)
		{
			_managers.EquipPresetStorageManager = new UserEquipPresetStorageManager();
		}
		_managers.EquipPresetStorageManager.Set(infos);
	}

	public void InitEquipItemIdentifiedInfos(EquipItemIdentifiedInfo[] infos)
	{
		if (_managers.EquipItemIdentifiedInfoManager == null)
		{
			_managers.EquipItemIdentifiedInfoManager = new UserEquipItemIdentifiedInfoManager();
		}
		_managers.EquipItemIdentifiedInfoManager.Set(infos);
	}

	public void InitPetInfos(PetInfo[] infos)
	{
		if (_managers.PetManager == null)
		{
			_managers.PetManager = new UserPetManager();
		}
		_managers.PetManager.Set(infos);
	}

	public void InitPetMiscInfos(PetMiscInfo[] infos)
	{
		if (_managers.PetManager == null)
		{
			_managers.PetManager = new UserPetManager();
		}
		_managers.PetManager.Apply(infos);
	}

	public void InitPetIncubatorManager()
	{
		if (_managers.PetIncubatorManager == null)
		{
			_managers.PetIncubatorManager = new UserPetIncubatorManager();
		}
	}

	public void InitPetIncubatorSlotManager()
	{
		if (_managers.PetIncubatorSlotManager == null)
		{
			_managers.PetIncubatorSlotManager = new UserPetIncubatorSlotManager();
		}
	}

	public void InitPetGachaCountInfos(PetGachaCountInfo[] infos)
	{
		if (_managers.PetGachaCountManager == null)
		{
			_managers.PetGachaCountManager = new UserPetGachaCountManager();
		}
		_managers.PetGachaCountManager.Apply(infos);
	}

	public void InitGachaCeilingCountInfos(GachaCeilingCountInfo[] infos)
	{
		if (_managers.EquipGachaCeilingManager == null)
		{
			_managers.EquipGachaCeilingManager = new EquipGachaCeilingManager();
		}
		_managers.EquipGachaCeilingManager.Apply(infos);
	}

	public void InitRightAwayProductManager()
	{
		if (_managers.RightAwayProductManager == null)
		{
			_managers.RightAwayProductManager = new UserRightAwayProductManager();
		}
	}

	public void SetRightAwayProductInfos(RightAwayProductInfo[] infos)
	{
		if (_managers.RightAwayProductManager == null)
		{
			_managers.RightAwayProductManager = new UserRightAwayProductManager();
		}
		_managers.RightAwayProductManager.Reset(infos);
	}

	public void InitGachaSelectShopManager()
	{
		if (_managers.GachaSelectShopManager == null)
		{
			_managers.GachaSelectShopManager = new UserGachaSelectShopManager();
		}
	}

	public void SetGachaSelectShopNoticeInfo(SelectShopNoticeInfo[] infos)
	{
		InitGachaSelectShopManager();
		_managers.GachaSelectShopManager.ApplySelectShopNoticeInfo(infos);
	}

	public void SetGachaSelectShopInfo(SelectShopInfo[] infos)
	{
		InitGachaSelectShopManager();
		_managers.GachaSelectShopManager.Set(infos);
	}

	public void InitKingsPassManager(KingsPassMission[] infos)
	{
		if (_managers.KingsPassManager == null)
		{
			_managers.KingsPassManager = new UserKingsPassManager();
		}
		_managers.KingsPassManager.Apply(infos);
	}

	public void SetPlayerKingsPassExp(PlayerKingsPassExp info)
	{
		if (_managers.KingsPassManager == null)
		{
			_managers.KingsPassManager = new UserKingsPassManager();
		}
		_managers.KingsPassManager.Set(info);
	}

	public void SetKingsPassSeaonInfo(KingsPassSeaonInfo info)
	{
		if (_managers.KingsPassManager == null)
		{
			_managers.KingsPassManager = new UserKingsPassManager();
		}
		_managers.KingsPassManager.Set(info);
	}

	public void InitEventCraftManager(EventCraftItemInfo[] infos)
	{
		if (_managers.EventCraftManager == null)
		{
			_managers.EventCraftManager = new UserEventCraftManager();
		}
		_managers.EventCraftManager.Apply(infos);
	}

	public void InitPetExploreManager(PetExploreInfo[] infos)
	{
		if (_managers.PetExploreManager == null)
		{
			_managers.PetExploreManager = new UserPetExploreManager();
		}
		_managers.PetExploreManager.Set(infos);
	}

	public void InitEventGrowWorldTreeManager()
	{
		if (_managers.EventGrowWorldTreeManager == null)
		{
			_managers.EventGrowWorldTreeManager = new UserEventGrowWorldTreeManager();
		}
	}

	public void InitLegendCostumeManager()
	{
		if (_managers.PayShopDiscountProductManager == null)
		{
			_managers.PayShopDiscountProductManager = new UserPayShopDiscountProductManager();
		}
	}

	public void InitPunishmentRaidManager()
	{
		if (_managers.OpenPunishmentRaidManager == null)
		{
			_managers.OpenPunishmentRaidManager = new UserOpenPunishmentRaidManager();
		}
	}

	public void InitAnyMiscInfos(PlayerAnyMisc[] infos)
	{
		if (_managers.AnyMiscManager == null)
		{
			_managers.AnyMiscManager = new UserAnyMiscManager();
		}
		_managers.AnyMiscManager.Set(infos);
	}

	public void Apply(AppliableResponse response)
	{
	}

	private IEnumerable<IUserObserverManager> GetUserObserverManagers()
	{
		if (UserManager != null)
		{
			yield return UserManager.ObserverManager;
		}
		if (EquipItemManager != null)
		{
			yield return EquipItemManager.ObserverManager;
		}
		if (ItemManager != null)
		{
			yield return ItemManager.ObserverManager;
		}
		if (ItemManager != null)
		{
			yield return ItemManager.ItemCountObserverManager;
		}
		if (AchievementManager != null)
		{
			yield return AchievementManager.ObserverManager;
		}
		if (ItemTimeDurationManager != null)
		{
			yield return ItemTimeDurationManager.ObserverManager;
		}
		if (HeroManager != null)
		{
			yield return HeroManager.ObserverManager;
		}
		if (CampaignManager != null)
		{
			yield return CampaignManager.ObserverManager;
		}
		if (FriendManager != null)
		{
			yield return FriendManager.ObserverManager;
		}
		if (FreeEquipGachaManager != null)
		{
			yield return FreeEquipGachaManager.ObserverManager;
		}
		if (EquipGachaManager != null)
		{
			yield return EquipGachaManager.ObserverManager;
		}
		if (AttendanceManager != null)
		{
			yield return AttendanceManager.ObserverManager;
		}
		if (SendRecvFriendshipPointManager != null)
		{
			yield return SendRecvFriendshipPointManager.ObserverManager;
		}
		if (RaidManager != null)
		{
			yield return RaidManager.ObserverManager;
		}
		if (CraftSlotManager != null)
		{
			yield return CraftSlotManager.ObserverManager;
		}
		if (PurchaseTimeDurationManager != null)
		{
			yield return PurchaseTimeDurationManager.ObserverManager;
		}
		if (FirstPurchaseManager != null)
		{
			yield return FirstPurchaseManager.ObserverManager;
		}
		if (NPCFriendlyManager != null)
		{
			yield return NPCFriendlyManager.ObserverManager;
		}
		if (PurchaseMarketingManager != null)
		{
			yield return PurchaseMarketingManager.ObserverManager;
		}
		if (ItemUseManager != null)
		{
			yield return ItemUseManager.ObserverManager;
		}
		if (PlayerCurrencyManager != null)
		{
			yield return PlayerCurrencyManager.ObserverManager;
		}
		if (QuestManager != null)
		{
			yield return QuestManager.MainQuestObserverManager;
		}
		if (QuestManager != null)
		{
			yield return QuestManager.SubQuestObserverManager;
		}
		if (SoulWeaponManager != null)
		{
			yield return SoulWeaponManager.ObserverManager;
		}
		if (SoulWeaponNoticeManager != null)
		{
			yield return SoulWeaponNoticeManager.ObserverManager;
		}
		if (DispatchBattleInfoManager != null)
		{
			yield return DispatchBattleInfoManager.ObserverManager;
		}
	}

	public void AddUserObserver(IUserObserver userObserver)
	{
		if (userObserver == null)
		{
			return;
		}
		foreach (IUserObserverManager userObserverManager in GetUserObserverManagers())
		{
			userObserverManager.AddObserver(userObserver);
		}
	}

	public void RemoveUserObserver(IUserObserver userObserver)
	{
		if (userObserver == null)
		{
			return;
		}
		foreach (IUserObserverManager userObserverManager in GetUserObserverManagers())
		{
			userObserverManager.RemoveObserver(userObserver);
		}
	}

	public void OnLoginResponse(NShared.Login.Response response)
	{
		if (loginResponse == null)
		{
			loginResponse = new NShared.Login.Response();
		}
		loginResponse = response;
		SetServerTime(response.ServerLocalTime, response.ServerUTCOffsetHour);
		_loginInfo = _loginInfo ?? new LoginInfo();
		_loginInfo.ServerPrivateIp = response.ServerPrivateIp;
		_loginInfo.ServerPort = response.ServerPort;
		_loginInfo.FaceBookId = response.FaceBookId;
		_loginInfo.GooglePlusId = response.GooglePlusId;
		_loginInfo.GameCenterId = (id: response.GameCenterId, legacyId: string.Empty);
		_loginInfo.AppleId = response.AppleId;
		if (!object.Equals(response.LoginIdByMethodType, null))
		{
			for (int i = 0; i < response.LoginIdByMethodType.Length; i++)
			{
				LoginIdByMethodType loginIdByMethodType = response.LoginIdByMethodType[i];
				switch (loginIdByMethodType.LoginMethod)
				{
				case LoginMethodType.Steam:
					_loginInfo.FaceBookId = loginIdByMethodType.LoginId;
					break;
				case LoginMethodType.GooglePlus:
					_loginInfo.GooglePlusId = loginIdByMethodType.LoginId;
					break;
				case LoginMethodType.GameCenter:
					_loginInfo.GameCenterId = (id: loginIdByMethodType.LoginId, legacyId: string.Empty);
					break;
				case LoginMethodType.Apple:
					_loginInfo.AppleId = loginIdByMethodType.LoginId;
					break;
				case LoginMethodType.Twitter:
					_loginInfo.TwitterId = loginIdByMethodType.LoginId;
					break;
				case LoginMethodType.Line:
					_loginInfo.LineId = loginIdByMethodType.LoginId;
					break;
				}
			}
		}
		_loginInfo.LoginId = response.LoginId;
		_loginInfo.CreatedTime = response.CreatedTime;
		if (response.UserInfo != null)
		{
			_loginInfo.Nick = response.UserInfo.Nick;
			_loginInfo.TeamLevel = response.UserInfo.TeamLevel;
		}
		InitNewUser(response.CreatedTime);
		NotCompletedInAppPurchaseInfos = response.NotCompletedIAPInfos;
		PatchNote = response.PatchNote;
		WebNotices = response.WebNotice;
		GlobalAutoBehaviour<GuildManager>.instance.SetGuildId(response.GuildId);
		for (int j = 0; j < __Extension_Method_ICollection__.COUNT(response.Notices); j++)
		{
			SingletonAuto<NoticeManager>.instance.AddNotice(response.Notices[j]);
		}
		AutoOpenInGameEvent = response.AutoOpenInGameEvent;
		CountryServerName = string.Empty;
		SingletonAuto<HonoraryChamberManager>.instance.Set(response.RecordsOfHonorTopInfos);
		IsTutorialSkip = response.IsTutorialSkip;
		SingletonUpdater<LoginManager>.instance.FirstLogined.Value = true;
	}

	private void UpdateContentValues()
	{
		SingletonUpdater<AsyncManager>.instance.ConfigAsyncLoad(SingletonAuto<ContentsValueManager>.instance.GetContentsBoolValue("UseSyncAssetLoad", defaultValue: true));
		SingletonUpdater<AssetCacheService>.instance.ConfigCache(SingletonAuto<ContentsValueManager>.instance.GetContentsIntValue("BundleCacheSize", 250), SingletonAuto<ContentsValueManager>.instance.GetContentsIntValue("ResourceCacheSize", 20), SingletonAuto<ContentsValueManager>.instance.GetContentsIntValue("CacheRetainTicks", 180000), SingletonAuto<ContentsValueManager>.instance.GetContentsIntValue("CacheDisableMemSizeInMb", 2000));
		ProfileManager.IsLogEnabled = SingletonAuto<ContentsValueManager>.instance.GetContentsBoolValue("EnableProfiler");
		int defaultValue = 10;
		if (ProfileManager.IsLogEnabled)
		{
			GlobalAutoBehaviour<ProfileManager>.instance._httpRttThreshold = SingletonAuto<ContentsValueManager>.instance.GetContentsIntValue("HttpRttThreshold", 1);
			GlobalAutoBehaviour<ProfileManager>.instance.ProfileReportBasisPoint = SingletonAuto<ContentsValueManager>.instance.GetContentsIntValue("ProfileReportBasisPoint", defaultValue);
		}
		SkillModifierContainer.UseCacheOnBattleLogic = SingletonAuto<ContentsValueManager>.instance.GetContentsBoolValue("UseCacheOnBattleLogic", defaultValue: true);
	}

	public void AddViewer()
	{
		ViewerContainer.AddAccountViewer(OpenPunishmentRaidManager);
		ViewerContainer.AddListViewer(OpenPunishmentRaidManager.GetPunishmentRaidInfos());
		ViewerContainer.AddAccountViewer(AttendanceManager);
		ViewerContainer.AddAccountViewer(PetManager);
		ViewerContainer.AddAccountViewer(PetGachaCountManager);
		ViewerContainer.AddAccountViewer(PetIncubatorSlotManager);
		ViewerContainer.AddAccountViewer(PetIncubatorManager);
		ViewerContainer.AddAccountViewer(PetExploreManager);
		ViewerContainer.AddAccountViewer(HeroManager);
		ViewerContainer.AddAccountViewer(EquipItemManager);
		ViewerContainer.AddAccountViewer(ItemManager);
		ViewerContainer.AddAccountViewer(CampaignManager);
		ViewerContainer.AddAccountViewer(TowerManager);
		ViewerContainer.AddAccountViewer(ShopItemManager);
		ViewerContainer.AddAccountViewer(ShopRestockTimeManager);
		ViewerContainer.AddAccountViewer(FriendManager);
		ViewerContainer.AddAccountViewer(AchievementManager);
		ViewerContainer.AddAccountViewer(UnderPrisonManager);
		ViewerContainer.AddAccountViewer(TutorialManager);
		ViewerContainer.AddAccountViewer(NewPayShopProductManager);
		ViewerContainer.AddAccountViewer(FirstPurchaseManager);
		ViewerContainer.AddAccountViewer(PurchaseTimeDurationManager);
		ViewerContainer.AddAccountViewer(ItemTimeDurationManager);
		ViewerContainer.AddAccountViewer(WorldMapEventManager);
		ViewerContainer.AddAccountViewer(WorldMapEventTimeManager);
		ViewerContainer.AddAccountViewer(WantedQuestManager);
		ViewerContainer.AddAccountViewer(FreeEquipGachaManager);
		ViewerContainer.AddAccountViewer(EquipGachaManager);
		ViewerContainer.AddAccountViewer(StepUpEquipGachamanager);
		ViewerContainer.AddAccountViewer(StarterGachaManager);
		ViewerContainer.AddAccountViewer(EquipGachaCeilingManager);
		ViewerContainer.AddAccountViewer(ChapterRewardManager);
		ViewerContainer.AddAccountViewer(SendRecvFriendshipPointManager);
		ViewerContainer.AddAccountViewer(RaidManager);
		ViewerContainer.AddAccountViewer(DeckManager);
		ViewerContainer.AddAccountViewer(HeroFriendlyManager);
		ViewerContainer.AddAccountViewer(NPCFriendlyManager);
		ViewerContainer.AddAccountViewer(CraftSlotManager);
		ViewerContainer.AddAccountViewer(CostumeManager);
		ViewerContainer.AddAccountViewer(CostumeStorageManager);
		ViewerContainer.AddAccountViewer(WeaponCostumeManager);
		ViewerContainer.AddAccountViewer(HairCostumeManager);
		ViewerContainer.AddAccountViewer(AccessoryCostumeManager);
		ViewerContainer.AddAccountViewer(PurchaseMarketingManager);
		ViewerContainer.AddAccountViewer(ItemUseManager);
		ViewerContainer.AddAccountViewer(HideoutDungeonManager);
		ViewerContainer.AddAccountViewer(PlayRecordManager);
		ViewerContainer.AddAccountViewer(ArchiveManager);
		ViewerContainer.AddAccountViewer(AccumulateLoginManager);
		ViewerContainer.AddAccountViewer(RouletteManager);
		ViewerContainer.AddAccountViewer(PlayerCurrencyManager);
		ViewerContainer.AddAccountViewer(TeamLevelBuffManager);
		ViewerContainer.AddAccountViewer(ClassBuffManager);
		ViewerContainer.AddAccountViewer(MonthlyHeroManager);
		ViewerContainer.AddAccountViewer(DispatchBattleInfoManager);
		ViewerContainer.AddAccountViewer(NewbieMissionManager);
		ViewerContainer.AddAccountViewer(QuestManager);
		ViewerContainer.AddAccountViewer(GodkingTrialDungeonInfoManager);
		ViewerContainer.AddAccountViewer(SoulWeaponManager);
		ViewerContainer.AddAccountViewer(ClearMissionManager);
		ViewerContainer.AddAccountViewer(HeroPresetStorageManager);
		ViewerContainer.AddAccountViewer(EquipPresetSotrageManger);
		ViewerContainer.AddAccountViewer(EquipItemIdentifiedInfoManager);
		ViewerContainer.AddAccountViewer(RightAwayProductManager);
		ViewerContainer.AddAccountViewer(KingsPassManager);
		ViewerContainer.AddAccountViewer(EventCraftManager);
	}

	private void SetPlayerAvatarHero(PlayerAvatarHeroInfo playerAvatarHeroInfo)
	{
		if (UserManager != null)
		{
			UserManager.SetPlayerAvaterHeroInfo(playerAvatarHeroInfo);
		}
	}

	public void TableLoadComplete()
	{
		AfterTableLoad_OnLoginResponse(loginResponse);
	}

	private void AfterTableLoad_OnLoginResponse(NShared.Login.Response response)
	{
		LoginResponseComplete = true;
		_managers = null;
		InitUserInfo(response.LoginId, response.CreatedTime, response.UserInfo, response.MiscInfo, response.AdminLevel, response.GuildPointInfo);
		InitStaminaInfo(response.StaminaResults);
		InitEquipItemInfos(response.EquipItems);
		InitEquipPresetStorageInfos(null);
		InitAccessoryInfo(response.PlayerAccessoryCostumeInfos);
		InitHeroInfos(response.Heroes);
		InitRunePageInfos(response.HeroRunePageInfos);
		InitItemInfos(response.Items);
		InitChapterDungeonInfos(response.ChapterDungeons);
		InitHideoutDungeonInfos(response.HideoutDungeons);
		InitKeyTypeDungeonInfos(response.ConquestDungeons);
		InitTowerInfos(response.Towers);
		InitShopItemInfos(response.ShopListItems);
		InitShopRestockTimeInfos(response.ShopRestockTimes);
		InitFriendManager(response.FriendInfos, response.FriendInvitorInfos);
		InitAchievementManager(response.AchievementInfos);
		InitScenarioQuestInfo(response.ReservedScenarioQuestInfo);
		InitTutorialInfos(response.Tutorials);
		InitPlayerItemTimeDurationInfo(response.ItemTimeDurations);
		InitPlayerPurchaseTimeDurationInfo(response.PurchaseTimeDurations);
		InitWorldMapEventTimeInfo(response.WorldMapEventTimeInfos);
		InitWorldMapEventInfo(response.WorldMapEventInfos);
		InitWantedQuestInfo(response.WantedQuestInfos);
		InitAttendanceInfo(response.AttendanceInfos);
		InitFreeEquipGachaInfo(response.freeEquipGachaInfos);
		InitEquipGachaInfo(response.EquipGachaInfos);
		InitStepUpEquipGachaInfo(null);
		InitChapterRewardInfo(response.chapterRewardInfos);
		InitHeroFriendlyInfo(response.HeroFriendlyInfo);
		InitNPCFriendlyInfo(response.NPCFriendlyInfos);
		InitSendRecvFriendshipPointManager(response.SendRecvFriendshipPointInfos, response.SentFriendshipPointInfos);
		InitRaidManager(response.RaidInfos);
		InitDeckManager(response.DeckInfos);
		InitGuildInfo(response.GuildInfo);
		InitCraftSlotInfo(response.CraftSlotInfos);
		InitCostumeInfo(response.CostumeInfos);
		InitWeaponCostumeInfo(response.WeaponCostumeInfos);
		IniHairCostumeInfo(response.HairCostumeInfos);
		InitCostumeStorageInfo(response.CostumeStorageSlotInfos);
		InitGuildContentsRestrictedTime(response.GuildContentsRestrictedTime);
		InitPurchaseMarketingInfo(response.PurchaseMarketingInfos);
		InitItemUseInfo(response.PlayerItemUseInfos);
		InitPlayRecordInfos(response.PlayRecordInfos);
		InitLoginDailyInfos(response.LoginDailyInfos);
		InitPlayArchiveInfos(response.PlayerArchiveInfos);
		InitRouletteInfos(null);
		InitPlayerCurrencyInfos(response.PlayerCurrencyInfos);
		InitClassBuffInfos(response.ClassBuffPointInfos, response.ClassBuffInfos);
		InitNewbieMissionInfos(response.NewbieMissionInfos);
		InitQuestInfos(response.MainQuestInfo, response.SubQuestInfos);
		InitUserDispatchBattleInfoManager(response.DispatchBattleInfos);
		InitUserGodkingTrialDungeonInfoManager(response.GodkingTrialDungeons);
		InitUserSoulWeaponManager(response.SoulWeaponInfos);
		InitSoulWeaponNoticeManager();
		InitAdventureNoticeManager();
		InitEclipseManager(response.EclipseDungeonInfos);
		InitMonthlyHeroInfos(response.MonthlyHeroInfos);
		SetNewPayShopProductInfo(null);
		SetPlayerProductPurchaseInfo(response.PlayerProductPurchaseInfos);
		SetPlayerAvatarHero(response.PlayerAvatarHeroInfo);
		InitClearMissionInfos(response.ClearMissionInfos);
		SetUnderPrisonInfos(response.UnderPrisonInfos, init: true);
		InitTeamLevelBuffInfos(null);
		InitStaterGachaInfos(null);
		InitHeroPresetStorageInfos(null);
		InitEquipItemIdentifiedInfos(null);
		InitPetInfos(null);
		InitKingsPassManager(null);
		InitEventCraftManager(null);
		InitPetExploreManager(null);
		InitEventGrowWorldTreeManager();
		InitLegendCostumeManager();
		InitPunishmentRaidManager();
		InitAnyMiscInfos(response.PlayerAnyMiscs);
		InitCurrencyInfo();
		InitRightAwayProductManager();
		InitGachaSelectShopManager();
		InitPetIncubatorManager();
		InitPetIncubatorSlotManager();
		SingletonAuto<ContentsStatusManager>.instance.Add(response.ContentsStatuses);
		SingletonAuto<ContentsValueManager>.instance.Add(response.ContentsValues);
		SingletonAuto<ContentsNoticePopupManager>.instance.Add(response.ContentsNoticeInfos);
		RegisterGlobalObserver();
		GlobalAutoBehaviour<PayShopItemEventManager>.instance.Set(response.PayShopItemEventInfos);
		GlobalAutoBehaviour<DropBonusEventManager>.instance.Set(response.DropBonusEvents);
		GlobalAutoBehaviour<ChattingManager>.instance.SetChatBanInfos(response.ChatBanInfos);
		ShopManagement.ResetShopItemPurchaseCountDictionary();
		GlobalAutoBehaviour<BoosterManager>.instance.BindAccount();
		SingletonAuto<MatchArena>.instance.ApplyBattleInfo(response.BattleInfo, response.BattleBanPickInfo);
		InitMessageConnection(response);
		SingletonAuto<NotificationManager>.instance.AddObserver();
		SingletonAuto<NotificationManager>.instance.SetNotificationDefaultValuesOnce();
		if (response.GuildInfo != null && !String_Extension_Method.IsNullOrEmpty(response.GuildInfo.Notice))
		{
			string stringDirect = LocalizationManager.GetStringDirect("GUILD_NOTICE_TEXT", response.GuildInfo.Notice);
			GlobalAutoBehaviour<ChattingManager>.instance.AddSystemChat(stringDirect, ChatType.Guild, showPreview: true, 0L);
		}
		UpdateContentValues();
		RestoredPortal.EnsureNativeShaderVariables();
	}

	private void RegisterGlobalObserver()
	{
		_managers.UserManager.ObserverManager.AddObserver(GlobalAutoBehaviour<PartyManager>.NewInstance());
		AddUserObserver(SingletonUpdater<NGame2.NQuest.QuestManager>.NewInstance());
		AddUserObserver(GlobalAutoBehaviour<CostumeOpenChecker>.NewInstance());
		if (SoulWeaponNoticeManager != null)
		{
			SoulWeaponNoticeManager.AddObserver();
		}
	}

	public DateTime GetCurrentServerLocalTime()
	{
		double elapsedTimeSinceLastEnterLobby = GetElapsedTimeSinceLastEnterLobby();
		return _serverLocalTime.AddSeconds(elapsedTimeSinceLastEnterLobby);
	}

	public DateTime GetCurrentServerUTCTime()
	{
		double elapsedTimeSinceLastEnterLobby = GetElapsedTimeSinceLastEnterLobby();
		return _serverUTCTime.AddSeconds(elapsedTimeSinceLastEnterLobby);
	}

	private void InitCurrencyInfo()
	{
		if (PlayerCurrencyManager != null)
		{
			UserInfo.EventOrvelPoint = PlayerCurrencyManager.GetCurrencyValue(UserManager.UserStatType.EventOrvelPoint);
		}
	}

	public double GetElapsedTimeSinceLastEnterLobby()
	{
		return (DateTime.UtcNow - _utcTime_when_SetServerTime).TotalSeconds;
	}

	public DateTime ServerLocalToUTC(DateTime time)
	{
		return DateTime.SpecifyKind(time.AddHours(-ServerUTCOffsetHour), DateTimeKind.Utc);
	}

	public DateTime ServerUTCToLocal(DateTime time)
	{
		return DateTime.SpecifyKind(time.AddHours(ServerUTCOffsetHour), DateTimeKind.Local);
	}

	public DateTime GetUTCOffsetNextMidnightTime()
	{
		DateTime dateTime = GetCurrentServerLocalTime().AddDays(1.0);
		DateTime time = NVespa.Math.CreateDateTime(dateTime.Year, dateTime.Month, dateTime.Day);
		return ServerLocalToUTC(time);
	}

	public void SetServerTime(string serverLocalTime, int serverUtcOffsetHour)
	{
		_utcTime_when_SetServerTime = DateTime.UtcNow;
		Vespa_String_Extension_Method2.TryParseNetworkTime2(serverLocalTime, out _serverLocalTime);
		ServerUTCOffsetHour = serverUtcOffsetHour;
		_serverUTCTime = _serverLocalTime.AddHours(-ServerUTCOffsetHour);
	}

	public void SetCreateTime(string createdTime)
	{
		_loginInfo.CreatedTime = createdTime;
	}

	public int GetServerUTCOffsetHour()
	{
		return ServerUTCOffsetHour;
	}

	private void InitMessageConnection(NShared.Login.Response response)
	{
		MessageSocketConnector messageConnector = GlobalAutoBehaviour<SocketServiceManager>.instance.MessageConnector;
		messageConnector.Init(guildId: response.GuildId, friendIds: FriendManager.GetFriendIds(), accountId: response.UserInfo.AccountId, serverInfo: response.MessageServerInfo);
		messageConnector.SetMessageLogHandler(SingletonAuto<MessageLogHandler>.instance);
		messageConnector.OnMessageServerConnected = OnMessageServerConnected;
		messageConnector.OnMessageServerDisconnected = OnMessageServerDisconnected;
		messageConnector.OnEnterChannel = OnEnterChannel;
	}

	private static void OnMessageServerConnected(LoginRes loginRes)
	{
		GlobalAutoBehaviour<ChattingManager>.instance.OnChattingServerConnected(loginRes);
		if (GlobalAutoBehaviour<PartyManager>.isValidInstance)
		{
			GlobalAutoBehaviour<PartyManager>.instance.SendLeaveParty();
		}
	}

	private static void OnMessageServerDisconnected()
	{
		GlobalAutoBehaviour<ChattingManager>.instance.OnChattingServerDisonnected();
		if (GlobalAutoBehaviour<PartyManager>.isValidInstance && GlobalAutoBehaviour<PartyManager>.instance.IsValidParty())
		{
			GlobalAutoBehaviour<PartyManager>.instance.SendLeaveParty();
		}
	}

	private static void OnEnterChannel(int channelNo)
	{
		GlobalAutoBehaviour<ChattingManager>.instance.OnChannelEnter(channelNo);
	}

	public StatData GetHeroStat(int creatureIndex)
	{
		CachedHeroInfo cachedHeroInfo = HeroManager.Get(creatureIndex);
		if (cachedHeroInfo == null)
		{
			return null;
		}
		return SceneAutoBehaviour<StatDataCache>.instance.GetHeroStat(cachedHeroInfo);
	}

	public StatData GetEquippedHeroStat(int creatureIndex)
	{
		CachedHeroInfo heroInfo = GetHeroInfo(creatureIndex);
		if (heroInfo == null)
		{
			return null;
		}
		return SceneAutoBehaviour<StatDataCache>.instance.GetCachedHeroStat(heroInfo);
	}

	public CachedHeroInfo GetHeroInfo(int heroIndex)
	{
		return HeroManager.Get(heroIndex);
	}

	public CachedHeroInfo[] GetHeroInfos()
	{
		List<CachedHeroInfo> list = new List<CachedHeroInfo>();
		foreach (CachedHeroInfo item in HeroManager)
		{
			list.Add(item);
		}
		return list.ToArray();
	}

	public List<CachedHeroInfo> GetHeroInfoList(bool sort)
	{
		List<CachedHeroInfo> list = ReservePoolUtil.CreateList<CachedHeroInfo>();
		foreach (CachedHeroInfo item in HeroManager)
		{
			list.Add(item);
		}
		if (sort)
		{
			list.Sort(HeroComparer.Default);
		}
		return list;
	}

	public void HandleFirstLobby(NShared.FirstLobby.Response response)
	{
		if (response != null)
		{
			InitBookMarkHero(response.PlayerBookMarkHeroInfo);
			InitHeroPresetStorage(response.HeroPresetStorages);
			InitEquipPresetStorageInfos(response.EquipPresetInfos);
			InitEquipItemIdentifiedInfos(response.EquipItemIdentifiedInfos);
			InitPetInfos(response.PetInfos);
			InitPetMiscInfos(response.PetMiscInfos);
			InitPetGachaCountInfos(response.PetGachaCountInfos);
			InitGachaCeilingCountInfos(response.GachaCeilingCountInfos);
			InitEventCraftManager(response.EventCraftItemInfos);
			InitKingsPassManager(response.KingsPassMissionInfos);
			InitTeamLevelBuffInfos(response.TeamLevelBuffInfos);
			SetKingsPassSeaonInfo(response.KingsPassSeaonInfo);
			InitPetExploreManager(response.PetExploreInfos);
			PayShopManagement.SetPayShopUserInfo(response.PayShopUserGroup, response.UserTypeEndTime);
			GlobalAutoBehaviour<NGame2.NPet.PetManager>.instance.Init();
			GlobalAutoBehaviour<NGame2.TeamLevelBuffManager>.instance.Init();
			InitRightAwayProductManager();
			InitGachaSelectShopManager();
			InitLegendCostumeManager();
			InitRaidManager(null);
			OpenPunishmentRaidManager.Set(response.PunishmentRaidInfos);
			OpenPunishmentRaidManager.Set(response.OpenPunishmentRaidInfos);
		}
	}

	public void InitBookMarkHero(PlayerBookMarkHeroInfo playerBookMarkHeroInfo)
	{
		if (playerBookMarkHeroInfo == null || String_Extension_Method.IsNullOrEmpty(playerBookMarkHeroInfo.BookMarkHeroIndices))
		{
			return;
		}
		string[] array = playerBookMarkHeroInfo.BookMarkHeroIndices.Split(',');
		for (int i = 0; i < array.Length; i++)
		{
			if (String_Extension_Method.TryToInt(array[i], out var output))
			{
				CachedHeroInfo heroInfo = GetHeroInfo(output);
				if (heroInfo != null)
				{
					heroInfo.BookmarkPriority = array.Length - i;
				}
			}
			HeroList.BeginNewSession();
		}
	}

	public void InitHeroPresetStorage(HeroPresetStorageInfo[] heroPresetStorageInfos)
	{
		InitHeroPresetStorageInfos(heroPresetStorageInfos);
	}

	public List<HeroEventBonusData> GetActiveNpcBuffList(EventBonusType eventBonusType, bool filterBoosterIcon)
	{
		List<HeroEventBonusData> list = new List<HeroEventBonusData>();
		foreach (int buffedNpcIndex in GameRoot.Instance.HeroBonusDataContainer.GetBuffedNpcIndices())
		{
			CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(buffedNpcIndex);
			if (cachedHeroInfo == null)
			{
				continue;
			}
			HeroBonusData data = GameRoot.Instance.HeroBonusDataContainer.GetData(cachedHeroInfo.HeroIndex, cachedHeroInfo.Star, cachedHeroInfo.Transcended);
			if (data == null || (filterBoosterIcon && !data.ShowBoosterIconTooltip))
			{
				continue;
			}
			HeroEventBonusData[] heroBonusTypeDatas = data.GetHeroBonusTypeDatas(eventBonusType);
			foreach (HeroEventBonusData heroEventBonusData in heroBonusTypeDatas)
			{
				if (!__Extension_Method_ICollection__.IsNullOrEmpty(heroEventBonusData.BonusValue))
				{
					list.Add(heroEventBonusData);
				}
			}
		}
		return list;
	}

	public int GetPetBonusBuffValue(EventBonusType eventBonusType)
	{
		if (!GlobalAutoBehaviour<NGame2.NPet.PetManager>.isValidInstance)
		{
			return 0;
		}
		return GlobalAutoBehaviour<NGame2.NPet.PetManager>.instance.GetPetBonusValue(eventBonusType);
	}

	public List<AccountBuffData> GetActiveAccountBuffList(bool filterBoosterIcon)
	{
		List<AccountBuffData> list = new List<AccountBuffData>();
		foreach (HeroEventBonusData activeNpcBuff in GetActiveNpcBuffList(EventBonusType.AccountBuff, filterBoosterIcon))
		{
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(activeNpcBuff.BonusValue))
			{
				int key = Vespa_String_Extension_Method.TOINT(activeNpcBuff.BonusValue[0], 0);
				AccountBuffData data = GameRoot.Instance.AccountBuffDataContainer.GetData(key);
				if (data != null)
				{
					list.Add(data);
				}
			}
		}
		return list;
	}

	public List<AccountBuffDataBase> GetActiveAccountBuffDataBaseList(bool filterBoosterIcon = false)
	{
		List<AccountBuffDataBase> list = new List<AccountBuffDataBase>();
		foreach (HeroEventBonusData activeNpcBuff in GetActiveNpcBuffList(EventBonusType.AccountBuff, filterBoosterIcon))
		{
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(activeNpcBuff.BonusValue))
			{
				int key = Vespa_String_Extension_Method.TOINT(activeNpcBuff.BonusValue[0], 0);
				AccountBuffData data = GameRoot.Instance.AccountBuffDataContainer.GetData(key);
				if (data != null)
				{
					AccountBuffDataBase accountBuffDataBase = new AccountBuffDataBase();
					accountBuffDataBase.AccountBuffIndex = data.Index;
					accountBuffDataBase.SkillIndex = data.SkillIndex;
					accountBuffDataBase.SkillLevel = data.SkillLevel;
					list.Add(accountBuffDataBase);
				}
			}
		}
		return list;
	}

	public Dictionary<int, int> GetActiveAccountBuffDictionary(bool filterBoosterIcon = false)
	{
		List<AccountBuffData> activeAccountBuffList = GetActiveAccountBuffList(filterBoosterIcon);
		Dictionary<int, int> dictionary = new Dictionary<int, int>();
		foreach (AccountBuffData item in activeAccountBuffList)
		{
			if (dictionary.TryGetValue(item.Index, out var value))
			{
				int value2 = value + item.SkillValue;
				dictionary.Remove(item.Index);
				dictionary.Add(item.Index, value2);
			}
			else
			{
				dictionary.Add(item.Index, item.SkillValue);
			}
		}
		return dictionary;
	}

	public HeroBonusData GetActiveAddRewardBuff(bool filterBoosterIcon = false)
	{
		List<HeroBonusData> heroBonusWithBonusType = GameRoot.Instance.HeroBonusDataContainer.GetHeroBonusWithBonusType1(EventBonusType.AddReward);
		if (heroBonusWithBonusType != null && heroBonusWithBonusType.Count > 0)
		{
			int heroIndex = heroBonusWithBonusType[0].HeroIndex;
			CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(heroIndex);
			if (heroInfo != null)
			{
				return GameRoot.Instance.HeroBonusDataContainer.GetData(heroInfo.HeroIndex, heroInfo.Star, heroInfo.Transcended);
			}
		}
		return null;
	}

	public Dictionary<StaminaType, int> GetActiveStaminaBuffDictionary(bool filterBoosterIcon = false)
	{
		Dictionary<StaminaType, int> dictionary = new Dictionary<StaminaType, int>();
		foreach (HeroEventBonusData activeNpcBuff in GetActiveNpcBuffList(EventBonusType.AddStamina, filterBoosterIcon))
		{
			if (__Extension_Method_ICollection__.IsNullOrEmpty(activeNpcBuff.BonusValue) || activeNpcBuff.BonusValue.Count() <= 1)
			{
				continue;
			}
			StaminaType o_value = StaminaType.None;
			int num = Vespa_String_Extension_Method.TOINT(activeNpcBuff.BonusValue[1].ToString(), 0);
			if (EnumTools.TryParse<StaminaType>(activeNpcBuff.BonusValue[0].ToString(), out o_value))
			{
				if (dictionary.TryGetValue(o_value, out var value))
				{
					int value2 = value + num;
					dictionary.Remove(o_value);
					dictionary.Add(o_value, value2);
				}
				else
				{
					dictionary.Add(o_value, num);
				}
			}
		}
		return dictionary;
	}

	public bool IsEquipSlotLocked(int heroIndex, int partIndex)
	{
		return HeroManager.Get(heroIndex)?.IsEquipSlotLocked(partIndex) ?? false;
	}

	public void SetEquipSlotOpened(int heroIndex, int partIndex)
	{
		HeroManager.SetEquipSlotOpened(heroIndex, partIndex);
	}

	public void SetExtendTranscendPoint(HeroTranscendResultInfo resultInfo)
	{
		if (resultInfo != null)
		{
			HeroManager.SetExtendTranscendPoint(resultInfo.HeroIndex, resultInfo.NewPointValue);
		}
	}

	public static int GetMaxHeroLevelOfEquipItem<T>(int itemIndex, IEnumerable<T> heroInfos) where T : HeroInfo
	{
		ItemData<EquipItemData> equipItemData = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(itemIndex);
		if (equipItemData == null)
		{
			return 0;
		}
		List<T> list = null;
		if (__Extension_Method_ICollection__.COUNT(equipItemData.SubData.CreatureIndex) > 0)
		{
			list = new List<T>();
			int i;
			for (i = 0; i < __Extension_Method_ICollection__.COUNT(equipItemData.SubData.CreatureIndex); i++)
			{
				T val = heroInfos.FirstOrDefault((T e) => e.HeroIndex == equipItemData.SubData.CreatureIndex[i]);
				if (val != null)
				{
					list.Add(val);
				}
			}
		}
		else if (__Extension_Method_ICollection__.COUNT(equipItemData.SubData.TagType) > 0)
		{
			list = new List<T>();
			for (int num = 0; num < __Extension_Method_ICollection__.COUNT(equipItemData.SubData.TagType); num++)
			{
				CreatureTagType tagType = equipItemData.SubData.TagType[num];
				foreach (T heroInfo in heroInfos)
				{
					if (heroInfo.CreatureData.HasTag(tagType))
					{
						list.Add(heroInfo);
					}
				}
			}
		}
		if (__Extension_Method_ICollection__.COUNT(list) == 0)
		{
			return 0;
		}
		int num2 = 0;
		for (int num3 = 0; num3 < __Extension_Method_ICollection__.COUNT(list); num3++)
		{
			T val2 = list[num3];
			if (val2 != null)
			{
				num2 = Mathf.Max(num2, val2.Level);
			}
		}
		return num2;
	}

	public int GetHeroLevelOfEquipItem(int itemIndex)
	{
		return GetMaxHeroLevelOfEquipItem(itemIndex, HeroManager);
	}

	public int GetHeroLevelOfEquipItem(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo == null)
		{
			return 0;
		}
		return GetHeroLevelOfEquipItem(equipItemInfo.ItemIndex);
	}

	public CachedHeroInfo GetHeroOfEquipItem(int equipItemSlotIndex)
	{
		foreach (CachedHeroInfo item in HeroManager)
		{
			if (item.GetEquipItemSlotIndexList().Contains(equipItemSlotIndex))
			{
				return item;
			}
		}
		return null;
	}

	public List<EquipItemInfo> GetUnequippedEquipItemInfoList(bool includeChest = true)
	{
		List<int> list = new List<int>();
		foreach (CachedHeroInfo item in HeroManager)
		{
			list.AddRange(item.GetEquipItemSlotIndexList());
		}
		List<EquipItemInfo> list2 = new List<EquipItemInfo>();
		foreach (EquipItemInfo item2 in EquipItemManager)
		{
			if (!list.Contains(item2.SlotIndex) && (includeChest || item2.InventoryType != 1) && item2.InventoryType != 3 && (item2 == null || item2.ItemData == null || item2.ItemData.Type != ItemType.EventEnchantWeapon))
			{
				list2.Add(item2);
			}
		}
		return list2;
	}

	public List<EquipItemInfo> GetOwnedAllEquipItemList()
	{
		List<EquipItemInfo> list = new List<EquipItemInfo>();
		foreach (EquipItemInfo item in EquipItemManager)
		{
			list.Add(item);
		}
		return list;
	}

	public List<EquipItemInfo> GetOwnedEquipItemList(Func<EquipItemInfo, bool> selector)
	{
		List<EquipItemInfo> list = new List<EquipItemInfo>();
		foreach (EquipItemInfo item in EquipItemManager)
		{
			if (selector(item))
			{
				list.Add(item);
			}
		}
		return list;
	}

	public bool CheckExistEquipItem(Func<EquipItemInfo, bool> selector)
	{
		bool result = false;
		foreach (EquipItemInfo item in EquipItemManager)
		{
			if (selector(item))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public List<EquipItemInfo> GetInventoryItemInfoList(InventoryType inventoryType)
	{
		List<EquipItemInfo> list = new List<EquipItemInfo>();
		foreach (EquipItemInfo item in EquipItemManager)
		{
			if (item.GetInventoryType() == inventoryType)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public List<ItemInfo> GetPublicAwakenItemList()
	{
		List<ItemInfo> publicUniqueEquipAwakenItemList = GetPublicUniqueEquipAwakenItemList();
		publicUniqueEquipAwakenItemList.AddRange(GetPublicTreasureAwakenItemList());
		return publicUniqueEquipAwakenItemList;
	}

	public List<ItemInfo> GetPublicUniqueEquipAwakenItemList()
	{
		return ItemManager.GetItemsByType(ItemType.PublicUniqueEquipAwaken);
	}

	public bool CheckHavePublicUniqueEquipAwakenItem()
	{
		return __Extension_Method_ICollection__.COUNT(GetPublicUniqueEquipAwakenItemList()) > 0;
	}

	public List<ItemInfo> GetPublicTreasureAwakenItemList()
	{
		return ItemManager.GetItemsByType(ItemType.PublicTreasureAwaken);
	}

	public bool CheckHavePublicTreasureAwakenItem()
	{
		return __Extension_Method_ICollection__.COUNT(GetPublicTreasureAwakenItemList()) > 0;
	}

	public Dictionary<int, List<EquipItemInfo>> GetCanUseAwakeEquipItemList(bool noTechnomagic = false)
	{
		List<int> list = new List<int>();
		foreach (CachedHeroInfo item in HeroManager)
		{
			list.AddRange(item.GetEquipItemSlotIndexList());
		}
		Dictionary<int, List<EquipItemInfo>> dictionary = new Dictionary<int, List<EquipItemInfo>>();
		Dictionary<int, List<EquipItemInfo>> dictionary2 = new Dictionary<int, List<EquipItemInfo>>();
		foreach (EquipItemInfo item2 in EquipItemManager)
		{
			bool flag = item2.CLI_CanAwaken();
			bool flag2 = item2.CLI_CanUse_as_AwakenMaterial();
			bool flag3 = list.Contains(item2.SlotIndex);
			ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(item2.ItemIndex);
			if ((data != null && data.GetSubData<EquipItemData>().PartType == EquipPartType.EnhanceStone) || item2.GetInventoryType() == InventoryType.Chest || (noTechnomagic && item2.IsTechnoMagic))
			{
				continue;
			}
			if (flag3)
			{
				if (flag)
				{
					if (!dictionary2.ContainsKey(item2.ItemIndex))
					{
						dictionary2.Add(item2.ItemIndex, new List<EquipItemInfo>());
					}
					dictionary2[item2.ItemIndex].Add(item2);
				}
			}
			else if (flag || flag2)
			{
				if (!dictionary.ContainsKey(item2.ItemIndex))
				{
					dictionary.Add(item2.ItemIndex, new List<EquipItemInfo>());
				}
				dictionary[item2.ItemIndex].Add(item2);
			}
		}
		Dictionary<int, List<EquipItemInfo>>.KeyCollection.Enumerator enumerator3 = dictionary2.Keys.GetEnumerator();
		List<int> list2 = new List<int>();
		while (enumerator3.MoveNext())
		{
			bool flag4 = false;
			if (dictionary.ContainsKey(enumerator3.Current))
			{
				foreach (EquipItemInfo item3 in dictionary[enumerator3.Current])
				{
					if (item3.CLI_CanUse_as_AwakenMaterial())
					{
						flag4 = true;
						break;
					}
				}
			}
			if (!flag4)
			{
				list2.Add(enumerator3.Current);
			}
		}
		for (int i = 0; i < list2.Count; i++)
		{
			int key = list2[i];
			dictionary2.Remove(key);
		}
		enumerator3 = dictionary.Keys.GetEnumerator();
		list2.Clear();
		bool flag5 = CheckHavePublicUniqueEquipAwakenItem();
		bool flag6 = CheckHavePublicTreasureAwakenItem();
		while (enumerator3.MoveNext())
		{
			if (dictionary2.ContainsKey(enumerator3.Current))
			{
				continue;
			}
			List<EquipItemInfo> list3 = dictionary[enumerator3.Current];
			if (list3.Count > 0 && ((list3[0].IsUniqueWeaponWithoutClassWeapon && flag5) || (list3[0].IsUniqueTreasure && flag6)))
			{
				continue;
			}
			if (list3.Count == 1)
			{
				list2.Add(enumerator3.Current);
				continue;
			}
			int num = 0;
			int num2 = 0;
			int num3 = 0;
			bool flag7 = false;
			for (int j = 0; j < list3.Count; j++)
			{
				bool flag8 = list3[j].CLI_CanAwaken();
				bool flag9 = list3[j].CLI_CanUse_as_AwakenMaterial();
				if (flag8 && flag9)
				{
					num3++;
				}
				else if (!flag8 && flag9)
				{
					num++;
				}
				else if (flag8 && !flag9)
				{
					num2++;
				}
				if (num3 == 0)
				{
					if (num >= 1 && num2 >= 1)
					{
						flag7 = true;
						break;
					}
				}
				else if (num3 == 1)
				{
					if (num >= 1 || num2 >= 1)
					{
						flag7 = true;
						break;
					}
				}
				else if (num3 >= 2)
				{
					flag7 = true;
					break;
				}
			}
			if (!flag7)
			{
				list2.Add(enumerator3.Current);
			}
		}
		for (int k = 0; k < list2.Count; k++)
		{
			int key2 = list2[k];
			dictionary.Remove(key2);
		}
		return dictionary;
	}

	public int GetMaxChestItemCount()
	{
		return Helper.GetChestExtendSize(UserManager.MiscInfo.ChestExtend);
	}

	public int GetMaxEquipItemCount()
	{
		return Helper.GetInventoryExtendSize(UserManager.MiscInfo.InventoryExtend);
	}

	public int GetMaxPunishmentRuneItemCount()
	{
		return Helper.GetPunishmentExtendSize(UserManager.MiscInfo.PunishmentRuneStorageSlotExtend);
	}

	public bool IsMaxEquipItemCount()
	{
		int maxEquipItemCount = GetMaxEquipItemCount();
		return __Extension_Method_ICollection__.COUNT(GetUnequippedEquipItemInfoList()) - __Extension_Method_ICollection__.COUNT(GetInventoryItemInfoList(InventoryType.Chest)) >= maxEquipItemCount;
	}

	public bool IsMaxPunishmentRuneItemCount()
	{
		int maxPunishmentRuneItemCount = GetMaxPunishmentRuneItemCount();
		List<EquipItemInfo> inventoryItemInfoList = GetInventoryItemInfoList(InventoryType.PunishmentRune);
		inventoryItemInfoList.RemoveAll((EquipItemInfo e) => e.IsEquipped);
		return inventoryItemInfoList.Count >= maxPunishmentRuneItemCount;
	}

	public List<EquipItemInfo> GetCanEquipAwakenTransitionItemList(EquipTransitionTicketData equipTransData, EquipItemInfo giveItemInfo, EquipItemInfo getItemInfo, bool isSelectExceptItemGroup)
	{
		bool flag = giveItemInfo != null;
		bool flag2 = getItemInfo != null;
		bool flag3 = !__Extension_Method_ICollection__.IsNullOrEmpty(equipTransData.FixedGiveItem);
		bool flag4 = !__Extension_Method_ICollection__.IsNullOrEmpty(equipTransData.FixedGetItem);
		List<EquipItemInfo> list = new List<EquipItemInfo>();
		foreach (EquipItemInfo item in EquipItemManager)
		{
			ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(item.ItemIndex);
			if (data == null)
			{
				continue;
			}
			int detailIndex = data.GetSubData<EquipItemData>().DetailIndex;
			if (__Extension_Method_ICollection__.IsNullOrEmpty(equipTransData.SelectAbleDetailIndex) || !TypeIterator.Contains(equipTransData.SelectAbleDetailIndex, detailIndex))
			{
				continue;
			}
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(item.EquipItemData.CreatureIndex))
			{
				int heroIndex = item.EquipItemData.CreatureIndex[0];
				if (GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(heroIndex) == null)
				{
					continue;
				}
			}
			if (flag)
			{
				if (flag2 && (item.SlotIndex == getItemInfo.SlotIndex || item.ItemIndex == getItemInfo.ItemIndex))
				{
					continue;
				}
				ItemData<EquipItemData> data2 = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(giveItemInfo.ItemIndex);
				if (data2 == null)
				{
					continue;
				}
				int detailIndex2 = data2.GetSubData<EquipItemData>().DetailIndex;
				if (detailIndex != detailIndex2 || item.SlotIndex == giveItemInfo.SlotIndex || item.ItemIndex == giveItemInfo.ItemIndex || item.Star >= giveItemInfo.Star || (flag4 && !TypeIterator.Contains(equipTransData.FixedGetItem, item.ItemIndex)))
				{
					continue;
				}
				if (!__Extension_Method_ICollection__.IsNullOrEmpty(equipTransData.ExceptItemGroup1))
				{
					if (isSelectExceptItemGroup)
					{
						if (!TypeIterator.Contains(equipTransData.ExceptItemGroup1, item.ItemIndex))
						{
							continue;
						}
					}
					else if (TypeIterator.Contains(equipTransData.ExceptItemGroup1, item.ItemIndex))
					{
						continue;
					}
				}
			}
			else if (item.Star < 1 || item.Star < equipTransData.LeastStar || (flag3 && !TypeIterator.Contains(equipTransData.FixedGiveItem, item.ItemIndex)))
			{
				continue;
			}
			list.Add(item);
		}
		return list;
	}

	public List<EquipItemInfo> GetCanEquipSoulWeaponTransitionItemList(EquipTransitionTicketData equipTransData, EquipItemInfo giveItemInfo, EquipItemInfo getItemInfo, bool isSelectExceptItemGroup)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(equipTransData.SelectAbleDetailIndex))
		{
			return null;
		}
		bool flag = giveItemInfo != null;
		bool flag2 = getItemInfo != null;
		bool flag3 = !__Extension_Method_ICollection__.IsNullOrEmpty(equipTransData.FixedGiveItem);
		bool flag4 = !__Extension_Method_ICollection__.IsNullOrEmpty(equipTransData.FixedGetItem);
		List<EquipItemInfo> list = new List<EquipItemInfo>();
		IEnumerable<int> source = from i in GetOwnedEquipItemList((EquipItemInfo e) => e.IsSoulWeapon() ? true : false)
			select i.ItemIndex;
		foreach (EquipItemInfo item in EquipItemManager)
		{
			ItemData<EquipItemData> data = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(item.ItemIndex);
			if (data == null)
			{
				continue;
			}
			int detailIndex = data.GetSubData<EquipItemData>().DetailIndex;
			if (!TypeIterator.Contains(equipTransData.SelectAbleDetailIndex, detailIndex))
			{
				continue;
			}
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(item.EquipItemData.CreatureIndex))
			{
				int heroIndex = item.EquipItemData.CreatureIndex[0];
				if (GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(heroIndex) == null)
				{
					continue;
				}
			}
			if (!flag)
			{
				if (!item.IsSoulWeapon() || (flag3 && !TypeIterator.Contains(equipTransData.FixedGiveItem, item.ItemIndex)))
				{
					continue;
				}
			}
			else
			{
				if ((flag2 && item.ItemIndex == getItemInfo.ItemIndex) || item.ItemIndex == giveItemInfo.ItemIndex || (item.IsSoulWeapon() && item.SoulWeaponInfo.Grade >= giveItemInfo.SoulWeaponInfo.Grade))
				{
					continue;
				}
				ItemData<EquipItemData> data2 = GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(giveItemInfo.ItemIndex);
				if (data2 == null)
				{
					continue;
				}
				int detailIndex2 = data2.GetSubData<EquipItemData>().DetailIndex;
				if (detailIndex != detailIndex2 || (!item.IsSoulWeapon() && source.Contains(item.ItemIndex)) || (flag4 && !TypeIterator.Contains(equipTransData.FixedGetItem, item.ItemIndex)))
				{
					continue;
				}
				if (!__Extension_Method_ICollection__.IsNullOrEmpty(equipTransData.ExceptItemGroup1))
				{
					if (isSelectExceptItemGroup)
					{
						if (!TypeIterator.Contains(equipTransData.ExceptItemGroup1, item.ItemIndex))
						{
							continue;
						}
					}
					else if (TypeIterator.Contains(equipTransData.ExceptItemGroup1, item.ItemIndex))
					{
						continue;
					}
				}
			}
			list.Add(item);
		}
		return list;
	}

	public List<EquipItemInfo> GetSoulWeaponAbilityItemList(SoulWeaponAbilityTicketData soulWeaponAbilityTicketData, EquipItemInfo selectItemInfo)
	{
		bool flag = selectItemInfo != null;
		bool flag2 = !__Extension_Method_ICollection__.IsNullOrEmpty(soulWeaponAbilityTicketData.FixedEquipItemIndex);
		ESoulWeaponAbilityTicketType itemType = soulWeaponAbilityTicketData.ItemType;
		List<EquipItemInfo> list = new List<EquipItemInfo>();
		IEnumerable<int> source = from i in GetOwnedEquipItemList((EquipItemInfo e) => e.IsSoulWeapon() ? true : false)
			select i.ItemIndex;
		foreach (EquipItemInfo item in EquipItemManager)
		{
			if (GameRoot.Instance.ItemDataContainer.GetData<EquipItemData>(item.ItemIndex) == null || !item.IsUniqueWeaponWithoutClassWeapon)
			{
				continue;
			}
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(item.EquipItemData.CreatureIndex))
			{
				int heroIndex = item.EquipItemData.CreatureIndex[0];
				if (GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(heroIndex) == null)
				{
					continue;
				}
			}
			if (flag2)
			{
				bool flag3 = false;
				for (int num = 0; num < __Extension_Method_ICollection__.COUNT(soulWeaponAbilityTicketData.FixedEquipItemIndex); num++)
				{
					int num2 = soulWeaponAbilityTicketData.FixedEquipItemIndex[num];
					if (item.ItemIndex == num2)
					{
						flag3 = true;
						break;
					}
				}
				if (!flag3)
				{
					continue;
				}
			}
			bool flag4 = itemType == ESoulWeaponAbilityTicketType.Grade || itemType == ESoulWeaponAbilityTicketType.Jumping;
			if ((flag4 && !item.IsSoulWeapon() && source.Contains(item.ItemIndex)) || (!flag4 && !item.IsSoulWeapon()))
			{
				continue;
			}
			switch (itemType)
			{
			case ESoulWeaponAbilityTicketType.Grade:
				if (item.IsSoulWeapon() && soulWeaponAbilityTicketData.SoulWeaponGrade > 0 && soulWeaponAbilityTicketData.SoulWeaponGrade <= item.SoulWeaponInfo.Grade)
				{
					continue;
				}
				break;
			case ESoulWeaponAbilityTicketType.Ether:
				if (item.IsSoulWeapon() && soulWeaponAbilityTicketData.EtherReinforceLevel > 0 && soulWeaponAbilityTicketData.EtherReinforceLevel <= item.SoulWeaponInfo.ReinforceLevel)
				{
					continue;
				}
				break;
			case ESoulWeaponAbilityTicketType.Jumping:
				if (item.IsSoulWeapon() && soulWeaponAbilityTicketData.SoulWeaponGrade > 0 && soulWeaponAbilityTicketData.SoulWeaponGrade <= item.SoulWeaponInfo.Grade && soulWeaponAbilityTicketData.EtherReinforceLevel > 0 && soulWeaponAbilityTicketData.EtherReinforceLevel <= item.SoulWeaponInfo.ReinforceLevel)
				{
					continue;
				}
				break;
			}
			if (!flag || item.SlotIndex != selectItemInfo.SlotIndex)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public bool IsPossibleLiberationWeapon(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo == null)
		{
			return false;
		}
		if (equipItemInfo.EquipItemData == null)
		{
			return false;
		}
		if (!equipItemInfo.EquipItemData.IsOpenSoulWeapon)
		{
			return false;
		}
		if (equipItemInfo.EquipItemData.SubType != EquipItemSubType.SpecialWeapon)
		{
			return false;
		}
		if (equipItemInfo.IsSoulWeapon())
		{
			return false;
		}
		if (equipItemInfo.GetInventoryType() == InventoryType.Chest)
		{
			return false;
		}
		if (SoulWeaponManager.CheckAlreadySoulWeapon(equipItemInfo))
		{
			return false;
		}
		if (!SoulWeaponManager.HaveSoulWeaponHero(equipItemInfo))
		{
			return false;
		}
		return true;
	}

	public void ApplyBeginContentResponse(NShared.BeginContent.Response response)
	{
		ItemManager.Apply(response.ItemResults);
	}

	public void ApplyEndContentResponse(NShared.EndContent.Response response)
	{
		CampaignManager.Apply(response.CampaignResults);
		UserManager.Apply(response.StaminaResult);
		UserManager.Apply(response.CurrencyResults);
		ItemManager.Apply(response.ItemResults);
		EquipItemManager.Apply(response.EquipItemInfos);
		PlayerCurrencyManager.Apply(response.CurrencyResults);
		OpenPunishmentRaidManager.Apply(response.OpenPunishmentRaidInfos);
		OpenPunishmentRaidManager.Set(response.PunishmentRaidInfos);
		QuestManager.Apply(response.ReservedMainQuestInfo);
	}

	public void ApplyBeginCampaignResponse(NShared.BeginCampaign.Response response)
	{
		UserManager.Apply(null, null, response.CurrencyResults);
		UserManager.Apply(response.StaminaResult);
		ItemTimeDurationManager.Apply(response.ItemTimeDurationInfo);
	}

	public void ApplyEndCampaignResponse(NShared.EndCampaign.Response response)
	{
		CampaignManager.Apply(response.CampaignResults);
		HideoutDungeonManager.Apply(response.HideoutDungeonResult);
		KeyTypeDungeonManager.Apply(response.ConquestDungeonResult);
		UserManager.Apply(response.StaminaResult);
		UserManager.Apply(response.CurrencyResults);
		UserManager.Apply(response.ExpResult);
		UserManager.Apply(response.UndergroundPrisonKeyResult);
		if (response.ExpResultsByGetHero != null)
		{
			for (int i = 0; i < response.ExpResultsByGetHero.Length; i++)
			{
				UserManager.Apply(response.ExpResultsByGetHero[i]);
			}
		}
		ItemManager.Apply(response.ItemResults);
		EquipItemManager.Apply(response.EquipItemInfos);
		HeroManager.ApplyHeroExp(response.HeroExpResults);
		ApplyAddHero(response.HeroInfos, null, reserveWindow: false);
		ItemManager.Apply(response.FlaskItemResults);
		HeroManager.Apply(response.FlaskResults);
		WorldMapEventTimeManager.Set(response.WorldMapEventTimeInfos);
		WorldMapEventManager.Unset(response.RemovedWorldMapEventInfos);
		WorldMapEventManager.Set(response.WorldMapEventInfos);
		TowerManager.Apply(response.TowerInfos);
		RaidManager.Apply(response.CompletedRaidInfo);
		ApplyRaidResult(response.RaidResult);
		ApplyGuildResult(response.GuildPointResult, response.GuildActivityPointResult, response.GuildMemberAccActivityPointResult);
		ApplyGuildRaidResult(response);
		ApplyChallengeRankerResult(response.ChallengeRaidRankerInfo);
		ApplyGuildSuppressResult(response.GuildSuppressInfo, response.GuildSuppressScoreInfo, response.GuildSuppressPlayInfo);
		SingletonUpdater<WorldBoss>.instance.ApplyWorldBossRankerInfo(response.WorldBossRankInfo);
		PlayerCurrencyManager.Apply(response.CurrencyResults);
		QuestManager.Apply(response.ReservedMainQuestInfo);
		GodkingTrialDungeonInfoManager.Apply(response.GodkingTrialDungeonInfo);
		GodkingTrialDungeonInfoManager.SetClearedDungeonChapterIndex(response.GodkingTrialDungeonInfo);
		UserManager.Apply(response.EclipseStaminaResult);
		if (response.UnderPrisonInfos != null)
		{
			SetUnderPrisonInfos(response.UnderPrisonInfos);
		}
		SprkTeamExpRefresh.ApplyFinal(this, response);
	}

	public void ApplyCompleteScenarioDungeonResponse(NShared.CompleteScenarioDungeon.Response response)
	{
		CampaignManager.Apply(response.DungeonInfo);
	}

	public void ApplyRaidResult(RaidResultInfo info)
	{
		if (info != null)
		{
			UserManager.Apply(info);
			RaidManager.Apply(info.RaidInfos);
		}
	}

	public void ApplyGuildResult(GuildPointResultInfo pointInfo, GuildActivityPointResultInfo activityPointInfo, GuildMemberAccActivityPointResultInfo memberActivityInfo)
	{
		if (GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager != null && GlobalAutoBehaviour<GuildManager>.instance.MemberManager != null && GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.GetGuildId() != 0L)
		{
			if (pointInfo != null)
			{
				UserManager.Apply(pointInfo);
			}
			if (activityPointInfo != null)
			{
				GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.SetActivityPoint(activityPointInfo.NewValue);
			}
			GuildMemberInfo myMemberInfo = GlobalAutoBehaviour<GuildManager>.instance.MemberManager.GetMyMemberInfo();
			if (myMemberInfo != null && memberActivityInfo != null)
			{
				myMemberInfo.AccActivityPoint = memberActivityInfo.NewValue;
			}
		}
	}

	public void ApplyChallengeRankerResult(ChallengeRaidRankerInfo challengeRaidRankerInfo)
	{
		if (challengeRaidRankerInfo != null && challengeRaidRankerInfo.AccountId == UserInfo.AccountId)
		{
			ChallengeRaidRankerInfo myRankInfo = ChallengeRaidWrapper.MyRankInfo;
			ChallengeRaidWrapper.MyRankInfo = challengeRaidRankerInfo;
			if (myRankInfo == null || myRankInfo.Score == 0L || myRankInfo.BattleTime == 0)
			{
				LastChallengeRaidRank = challengeRaidRankerInfo.Rank;
				LastChallengeRaidBestScore = 0L;
			}
			else
			{
				LastChallengeRaidRank = myRankInfo.Rank;
				LastChallengeRaidBestScore = myRankInfo.Score;
			}
		}
	}

	public void ApplyGuildSuppressResult(GuildSuppressInfo guildSuppressInfo, GuildSuppressScoreInfo guildSuppressScoreInfo, GuildSuppressPlayInfo guildSuppressPlayInfo)
	{
		if (!GlobalAutoBehaviour<GuildManager>.isValidInstance)
		{
			return;
		}
		UserGuildManager myGuildManager = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager;
		if (myGuildManager == null)
		{
			return;
		}
		GuildSuppressManager guildSuppressManager = myGuildManager.GuildSuppressManager;
		if (guildSuppressManager == null)
		{
			return;
		}
		GuildSuppressInfo guildSuppressInfo2 = guildSuppressManager.GetGuildSuppressInfo();
		if (guildSuppressInfo2 != null)
		{
			LastGuildSuppressLevel = guildSuppressInfo2.Level;
			if (guildSuppressScoreInfo != null && guildSuppressScoreInfo.AccountId == UserInfo.AccountId)
			{
				GuildSuppressScoreInfo guildSuppressScoreInfo2 = guildSuppressManager.GetGuildSuppressScoreInfo();
				LastGuildSuppressRank = guildSuppressScoreInfo2?.Rank ?? 0;
				LastGuildSuppressScore = guildSuppressScoreInfo2?.TotalDamage ?? guildSuppressScoreInfo.TotalDamage;
			}
			guildSuppressManager.Apply(guildSuppressInfo);
			guildSuppressManager.Apply(guildSuppressScoreInfo);
			guildSuppressManager.Apply(guildSuppressPlayInfo);
		}
	}

	private void ApplyGuildRaidResult(NShared.EndCampaign.Response response)
	{
		RestoredPortal.SeedGuildRaidScore(response);
		if (response == null)
		{
			return;
		}
		NShared.GuildRaidDungeonInfo[] guildRaidInfos = response.GuildRaidInfos;
		GuildRaidMemberTotalScoreInfo guildRaidMemberTotalScoreInfo = response.GuildRaidMemberTotalScoreInfo;
		if (!GlobalAutoBehaviour<GuildManager>.isValidInstance)
		{
			return;
		}
		UserGuildManager myGuildManager = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager;
		if (myGuildManager == null)
		{
			return;
		}
		GuildRaidManager guildRaidManager = myGuildManager.GuildRaidManager;
		if (guildRaidManager != null)
		{
			if (guildRaidMemberTotalScoreInfo != null && guildRaidMemberTotalScoreInfo.AccountId == UserInfo.AccountId)
			{
				LastGuildRaidRank = guildRaidManager.GetGuildRaidScoreRank(UserInfo.AccountId);
				LastGuildRaidScore = guildRaidManager.GetGuildRaidScoreInfo(UserInfo.AccountId)?.Score ?? guildRaidMemberTotalScoreInfo.Score;
			}
			guildRaidManager.Apply(guildRaidInfos);
			if (LastGuildRaidScore >= 0 && guildRaidMemberTotalScoreInfo != null && guildRaidMemberTotalScoreInfo.Score > LastGuildRaidScore)
			{
				guildRaidManager.Apply(guildRaidMemberTotalScoreInfo);
			}
			SendGuildRaidDungeonOpen();
		}
	}

	private void SendGuildRaidDungeonOpen()
	{
		SendOpenGuildRaidBonusDungeon();
		SendOpenGuildRaidDummyDungeon();
	}

	private void SendOpenGuildRaidBonusDungeon()
	{
		UserGuildManager myGuildManager = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager;
		if (myGuildManager == null)
		{
			return;
		}
		GuildRaidManager guildRaidManager = myGuildManager.GuildRaidManager;
		if (guildRaidManager != null)
		{
			NShared.GuildRaidDungeonInfo bonusDungeonInfo = null;
			if (guildRaidManager.IsBonusDungeon(out bonusDungeonInfo) && guildRaidManager.CheckOpenGuildRaidDungeon(bonusDungeonInfo) && UserInfo != null && bonusDungeonInfo.ClearMemberId == UserInfo.AccountId)
			{
				SingletonAuto<MessageLogHandler>.instance.SendToGuild(new GuildRaidBonusDungeonOpen
				{
					Nick = UserInfo.Nick
				});
			}
		}
	}

	private void SendOpenGuildRaidDummyDungeon()
	{
		UserGuildManager myGuildManager = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager;
		if (myGuildManager == null)
		{
			return;
		}
		GuildRaidManager guildRaidManager = myGuildManager.GuildRaidManager;
		if (guildRaidManager != null)
		{
			NShared.GuildRaidDungeonInfo dummyDungeonInfo = null;
			if (guildRaidManager.IsDummyDungeon(out dummyDungeonInfo) && guildRaidManager.CheckOpenGuildRaidDungeon(dummyDungeonInfo) && UserInfo != null && dummyDungeonInfo.ClearMemberId == UserInfo.AccountId)
			{
				SingletonAuto<MessageLogHandler>.instance.SendToGuild(new GuildRaidDummyDungeonOpen
				{
					Nick = UserInfo.Nick
				});
			}
		}
	}

	public void ClearLastGuildRaidInfo()
	{
		LastGuildRaidRank = 0;
		LastGuildRaidScore = -1L;
	}

	public void ApplyMatchResult(MatchResultInfo matchResultInfo)
	{
		if (matchResultInfo != null)
		{
			UserManager.Apply(matchResultInfo.SwordResult);
			HeroManager.ApplyHeroExp(matchResultInfo.HeroExpResults);
			UserManager.Apply(matchResultInfo.CurrencyResults);
			ItemManager.Apply(matchResultInfo.ItemResults);
		}
	}

	public void ApplyDbRewardResult(DbRewardResult result)
	{
		UserManager.Apply(result.CurrencyResults);
		UserManager.Apply(result.StaminaResultInfos);
		ItemManager.Apply(result.ItemResults);
		EquipItemManager.Apply(result.EquipItemResults);
		PetManager.Apply(result.PetInfos);
	}

	public void ApplyRewardResult(RewardResultInfo result)
	{
		if (result != null)
		{
			UserManager.Apply(result.CurrencyResults);
			UserManager.Apply(result.StaminaResultInfos);
			ItemManager.Apply(result.ItemResults);
			EquipItemManager.Apply(result.EquipItemResults);
			PetManager.Apply(result.PetInfos);
			ItemTimeDurationManager.Apply(result.ItemTimeDurationInfos);
			HeroFriendlyManager.Apply(result.HeroFriendlyInfos);
			UserAnyMiscManager.Set(result.PlayerAnyMiscs);
		}
	}

	public void ApplyRewardResult(IList<RewardResultInfo> results)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(results); i++)
		{
			ApplyRewardResult(results[i]);
		}
	}

	public void ApplyAddHero(HeroAddResultInfo[] infos)
	{
		if (infos != null)
		{
			for (int i = 0; i < infos.Length; i++)
			{
				ApplyAddHero(infos[i]);
			}
		}
	}

	public void ApplyAddHero(HeroAddResultInfo resultInfo)
	{
		if (resultInfo != null && GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			HeroManager.AddHeroAndAnnounce(resultInfo.HeroInfo, reserveWindow: true);
			UserManager.Apply(resultInfo.TeamExpResult);
			UserManager.Apply(resultInfo.StaminaResult);
			HeroFriendlyManager.Apply(resultInfo.HeroFriendlyInfo);
		}
	}

	public void ApplyAddHero(HeroInfo[] heroInfos, ExpResultInfo[] expResultInfos, bool reserveWindow = true, Action onCloseWindow = null)
	{
		if (GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(heroInfos); i++)
			{
				HeroManager.AddHeroAndAnnounce(heroInfos[i], reserveWindow, (i == __Extension_Method_ICollection__.COUNT(heroInfos) - 1) ? onCloseWindow : null);
			}
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(expResultInfos); j++)
			{
				UserManager.Apply(expResultInfos[j]);
			}
		}
	}

	public void ApplyAddHeroForTutorial(HeroInfo[] heroInfos, ExpResultInfo[] expResultInfos, bool reserveWindow = true)
	{
		if (GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(heroInfos))
			{
				HeroManager.AddHerosAndAnnounceRecreateChangeHeroInfo(heroInfos, reserveWindow);
			}
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(expResultInfos); i++)
			{
				UserManager.Apply(expResultInfos[i]);
			}
		}
	}

	public void ApplyHeroGrowthReward(List<HeroInfo> heroInfos, Action<bool> onCloseWindow = null)
	{
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			Vespa_System_Action_Extension_Method.fire(onCloseWindow, i_argA: false);
		}
		else
		{
			HeroManager.ApplyAddOrGrowthHeroInfos(heroInfos, onCloseWindow);
		}
	}

	public void ApplyReceiveMail(MailReceiveResult mailReceiveResult)
	{
		if (mailReceiveResult != null)
		{
			UserManager.Apply(mailReceiveResult.CurrencyResults);
			UserManager.Apply(mailReceiveResult.GuildPointResult);
			UserManager.Apply(mailReceiveResult.StaminaResults);
			ItemManager.Apply(mailReceiveResult.ItemResults);
			EquipItemManager.Apply(mailReceiveResult.EquipItemResults);
			UserAnyMiscManager.Set(mailReceiveResult.PlayerAnyMiscs);
			PlayerCurrencyManager.Apply(mailReceiveResult.CurrencyResults);
			PetManager.Apply(mailReceiveResult.PetResult);
			if (mailReceiveResult.HeroAddResult != null)
			{
				SceneAutoBehaviour<MailManagement>.instance.CloseAllActiveWindow();
				ApplyAddHero(mailReceiveResult.HeroAddResult);
			}
		}
	}

	public void ApplyReceiveMail(IList<MailReceiveResult> mailReceiveResults)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(mailReceiveResults); i++)
		{
			ApplyReceiveMail(mailReceiveResults[i]);
		}
	}

	public void ApplyItemGachaResult(GachaItemResultInfo[] gachaItemResultInfos)
	{
		if (gachaItemResultInfos != null)
		{
			for (int i = 0; i < gachaItemResultInfos.Length; i++)
			{
				EquipItemManager.Apply(gachaItemResultInfos[i].EquipItemResult);
				ItemManager.Apply(gachaItemResultInfos[i].ItemResult);
				PetManager.Apply(gachaItemResultInfos[i].PetItemResult);
				UserManager.Apply(gachaItemResultInfos[i].StaminaResult);
			}
		}
	}

	IEnumerable<CachedHeroInfo> IAccountInfo.GetHeroInfos()
	{
		return HeroManager;
	}

	public List<PetInfo> GetPetInfoList(bool sort, PetManager.EPetCompareType petCompareType = NGame2.NPet.PetManager.EPetCompareType.Defualt)
	{
		List<PetInfo> list = ReservePoolUtil.CreateList<PetInfo>();
		foreach (PetInfo item in PetManager)
		{
			list.Add(item);
		}
		if (sort)
		{
			if (petCompareType == NGame2.NPet.PetManager.EPetCompareType.PetManagement)
			{
				list.Sort(PetManagementComparer.Default);
			}
			else
			{
				list.Sort(PetComparer.Default);
			}
		}
		return list;
	}

	public List<PetInfo> GetPetInfoLisExceptLinePet()
	{
		List<PetInfo> list = ReservePoolUtil.CreateList<PetInfo>();
		foreach (PetInfo item in PetManager)
		{
			if (!item.IsLinePet)
			{
				list.Add(item);
			}
		}
		return list;
	}

	public List<PetInfo> GetPetAwakenInfoList()
	{
		List<PetInfo> list = ReservePoolUtil.CreateList<PetInfo>();
		foreach (PetInfo item in PetManager)
		{
			if (item.CanAwaken())
			{
				list.Add(item);
			}
		}
		list.Sort(PetComparer.Default);
		return list;
	}

	public bool CanOpenPetAwakenTutorial()
	{
		foreach (PetInfo item in PetManager)
		{
			if (item.CreatureData.CreatureGrade == CreatureGrade.Legend)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsConnected(LoginMethodType type)
	{
		string text = "";
		switch (type)
		{
		case LoginMethodType.Steam:
			text = FaceBookId;
			break;
		case LoginMethodType.GooglePlus:
			text = GooglePlusId;
			break;
		case LoginMethodType.GameCenter:
			text = GameCenterId.id;
			break;
		case LoginMethodType.Apple:
			text = AppleId;
			break;
		case LoginMethodType.Twitter:
			text = TwitterId;
			break;
		case LoginMethodType.Line:
			text = LineId;
			break;
		}
		return !String_Extension_Method.IsNullOrEmpty(text);
	}

	public void SetConnected(LoginMethodType type, string id)
	{
		switch (type)
		{
		case LoginMethodType.Steam:
			FaceBookId = id;
			break;
		case LoginMethodType.GooglePlus:
			GooglePlusId = id;
			break;
		case LoginMethodType.GameCenter:
			GameCenterId = (id: id, legacyId: string.Empty);
			break;
		case LoginMethodType.Apple:
			AppleId = id;
			break;
		case LoginMethodType.Twitter:
			TwitterId = id;
			break;
		case LoginMethodType.Line:
			LineId = id;
			break;
		}
	}

	public UserInfo GetUserInfo()
	{
		if (loginResponse == null)
		{
			return null;
		}
		return loginResponse.UserInfo;
	}
}
