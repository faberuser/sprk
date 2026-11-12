using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NAchievement;
using NGame2.NChatting.NUserBlock;
using NGame2.NContents;
using NGame2.NContentsInfo;
using NGame2.NCutScene;
using NGame2.NDevice;
using NGame2.NDispatchBattle;
using NGame2.NDropBonusEvent;
using NGame2.NEventTrigger;
using NGame2.NGuild;
using NGame2.NLobby;
using NGame2.NLobby.NActivity;
using NGame2.NLogin;
using NGame2.NMessageLog;
using NGame2.NNotification;
using NGame2.NParty;
using NGame2.NPatch;
using NGame2.NScene;
using NGame2.NScene.NLobby;
using NGame2.NTutorial;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NComponent2.NPayShop;
using NGame2.NUI.NHelper.NPayShop;
using NGame2.NUI.NManager.ClassBuff;
using NGame2.NUI.NManager.Mission;
using NGame2.NUI.NManager.NBooster;
using NGame2.NUI.NManager.NPet;
using NGame2.NUI.NPet;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.LobbyChapterReward;
using NGame2.NUI.NWindow.NAttendance;
using NGame2.NUI.NWindow.NAttendance.NIntegration;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWindow.NChatting;
using NGame2.NUI.NWindow.NEntryMenu;
using NGame2.NUI.NWindow.NEventCalendar;
using NGame2.NUI.NWindow.NNotice;
using NGame2.NUI.NWindow.NSoulWeapon;
using NGame2.NUI.NWindow.NTutorial;
using NGame2.NUI.NWorldMap;
using NGame2.NUtil;
using NGame2.NUtil.NCostume;
using NGame2.NUtil.NWrappedInfo;
using NGameAsset;
using NGameAsset.NPatch;
using NShared;
using NShared.ChangeAvatarHero;
using NShared.ChangeCountryCode;
using NShared.ChangePetAvatar;
using NShared.CheckNewMail;
using NShared.CraftPunishmentRune;
using NShared.EventCalendar;
using NShared.GetAdeventureNotice;
using NShared.GetAttendanceReward;
using NShared.GetRightAwayProductInfo;
using NShared.NEvent;
using NShared.NLocalization;
using NShared.NMessage.NProtocol;
using NShared.RefreshRightAwayProductInfo;
using NShared.RewardClearChapter;
using NShared.UseCoupon;
using NVespa;
using NVespa.NGlobal;
using NVespa.NGui;
using NVespa.NScene;
using NVespa.NSingleton;
using NVespa.NWeb;
using UnityEngine;

namespace NGame2.NUI.NManager.NLobby;

public class LobbyManagement : BaseManagement<LobbyManagement>, LobbyTopmostMenu.ICustomStaminaObserver, LobbyTopmostMenu.IUserStatObserver, LobbyTopmostMenu.ILobbyTopmostObserver<GuildResourceObserverValue>, LobbyTopmostMenu.IPublicObserver, LobbyTopmostMenu.Listener, LobbyRightMenu.Listener, LobbyNodeFunctionMenu.Listener, LobbyRightMenu.IDataProvider, GameOptionWindow.Listener, GameOptionWindow.IDataProvider, MyInfoWindow.Listener, MyInfoWindow.IDataProvider, SelectAvatarHero.Listener, SelectAvatarPet.Listener, SelectCountryFlag.Listener, SelectLanguage.Listener, ManageBlockedUser.Listener, SkipCurrencyConfirmCountPopup.Listener, MoveToLastBattlePopup.Listener, EtcMenuWindow.Listener, LeftMenu.Listener, GuideSelect.Listener, GuideMenu.Listener, WorldBossKillRewardPopup.Listener, PlayRecord.Listener, BoosterInfoList.Listener, HeroDetailView.Listener, HeroFullSceneViewBase.Listener, LobbyPlayGuideMenu.Listener, PlayGuidePopup.Listener, HonoraryChamber.IListener, LobbyChapterClearReward.IListener, MainQuestGuideMenu.IListener, ContentUIDispatchBattleOnGoingList.IListener, NGame2.NUI.NWindow.RightAwayProductMenu.Listener, GachaSelectShopMenu.IListener, PunishmentRuneCraftWindow.IListener, PunishmentRuneCraftResult.IListener, IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>, IUserObserver, IUserObserver<UserManager.UserStatInfo>, IUserObserver<DropBonusEventManager.DropBonusEventChangeInfo>, ITutorialListener, IUserObserver<MainQuestInfo>, IUserObserver<SubQuestInfo>, IUserObserver<UserDispatchBattleInfoManager.Observer.BroadCastInfo>
{
	public static bool TestGoatWarrior = false;

	public static List<int> DisplayedChapterIndices = new List<int>();

	private DateTime _lastRefreshRightAwayProductInfoTime = DateTime.UtcNow;

	private static float _rightAwayProductInfoRefreshTime = 60f;

	public static Dictionary<int, AssetTexture> RightAwayBannerDic = new Dictionary<int, AssetTexture>();

	private DateTime _lastCheckNewMailArrivedTime = DateTime.UtcNow;

	private float _lastCheckNewRewardableMonthlyHero;

	private DateTime _lastCheckShopRestockTime = DateTime.MinValue;

	private float _lastCheckClearMainQuest;

	private float _lastCheckEventDungeonCurrency;

	private bool ForceShopRestockUpdate = true;

	private bool _InPoll;

	private LocalStorageDateTime LastLobbyNoticeTime = new LocalStorageDateTime(DateTime.MinValue, typeof(LobbyManagement).FullName + "_LastLobbyNoticeTime");

	private Action _onMoveEnd;

	private List<NGame2.NUI.NManager.NLobby.IPassiveWindow> _passiveWindowList = new List<NGame2.NUI.NManager.NLobby.IPassiveWindow>();

	private StaminaType _chapterStaminaType = StaminaType.Chicken;

	private StaminaType _chapterSubStaminaType = StaminaType.Empty;

	private static int _eventCalendarVersion = 0;

	private static EventCalendarData[] _eventCalendarDatas;

	private Coroutine _coProcessingUpdateDispatchMenuVisible;

	private LocalAccountString _lastQuestCheckDate = new LocalAccountString(string.Empty, typeof(LobbyManagement).FullName + "_lastQuestCheckDate");

	private LocalAccountBool _openQuestDescWindow = new LocalAccountBool(i_default: true, typeof(LobbyManagement).FullName + "_openQuestDescWindow");

	private LocalStorageDateTime LastRequestAdvTime = new LocalStorageDateTime(DateTime.Now, typeof(LobbyManagement).FullName + "_LastRequestAdvTime");

	private bool ForceRefreshAdvNotice;

	private HashSet<LobbyFullWindow> _openedFullWindows = new HashSet<LobbyFullWindow>();

	private WindowOpenQueue<GetNewHero2> _getNewHeroOpenQueue = new WindowOpenQueue<GetNewHero2>();

	private int _prevCheckedChatperIndex = -1;

	private GuildResourceObserverValue _tempGuildResourceObserverValue = new GuildResourceObserverValue();

	public string LastQuestCheckDate
	{
		get
		{
			return _lastQuestCheckDate.Value;
		}
		set
		{
			_lastQuestCheckDate.Value = value;
		}
	}

	public bool OpenQuestDescWindow
	{
		get
		{
			return _openQuestDescWindow.Value;
		}
		set
		{
			_openQuestDescWindow.Value = value;
		}
	}

	StaminaType LobbyTopmostMenu.ICustomStaminaObserver.StaminaType => GetChapterStaminaType();

	UserManager.UserStatType LobbyTopmostMenu.IUserStatObserver.StatType
	{
		get
		{
			int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
			CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(chapterIndex);
			if (data == null)
			{
				return UserManager.UserStatType.Gold;
			}
			UserManager.UserStatType o_value = UserManager.UserStatType.None;
			if (!EnumTools.TryParse<UserManager.UserStatType>(data.UserStatType[0], out o_value))
			{
				o_value = UserManager.UserStatType.Gold;
			}
			if (o_value == UserManager.UserStatType.None)
			{
				o_value = UserManager.UserStatType.Gold;
			}
			return o_value;
		}
	}

	GuildResourceObserverValue LobbyTopmostMenu.ILobbyTopmostObserver<GuildResourceObserverValue>.BaseValue
	{
		get
		{
			int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
			CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(chapterIndex);
			if (data == null)
			{
				_tempGuildResourceObserverValue.Value = GuildConstructionResourceTopmostMenuType.None;
			}
			else
			{
				_tempGuildResourceObserverValue.Value = data.GuildResourceMenuType;
			}
			return _tempGuildResourceObserverValue;
		}
	}

	LobbyTopmostMenu.PublicObserverType LobbyTopmostMenu.ILobbyTopmostObserver<GuildResourceObserverValue>.Type => LobbyTopmostMenu.PublicObserverType.ShowGUildResource;

	public List<PlayGuideInfo> ListPlayGuideInfo
	{
		get
		{
			if (GlobalAutoBehaviour<AccountManager>.instance.ListPlayGuideInfo == null)
			{
				GlobalAutoBehaviour<AccountManager>.instance.ListPlayGuideInfo = new List<PlayGuideInfo>();
				GlobalAutoBehaviour<AccountManager>.instance.ListPlayGuideInfo.Clear();
			}
			return GlobalAutoBehaviour<AccountManager>.instance.ListPlayGuideInfo;
		}
	}

	public List<PlayGuideInfo> ListSortedPlayGuideInfo
	{
		get
		{
			if (GlobalAutoBehaviour<AccountManager>.instance.ListSortedPlayGuideInfo == null)
			{
				GlobalAutoBehaviour<AccountManager>.instance.ListSortedPlayGuideInfo = new List<PlayGuideInfo>();
				GlobalAutoBehaviour<AccountManager>.instance.ListSortedPlayGuideInfo.Clear();
			}
			return GlobalAutoBehaviour<AccountManager>.instance.ListSortedPlayGuideInfo;
		}
	}

	public bool CheckUpdatePlayGuide { get; set; }

	public bool CheckPlayGuideStartCourtineStatus { get; set; }

	protected override void Awake()
	{
		base.Awake();
		_showLobbyMenu = true;
		SingletonUpdater<TutorialManager>.instance.AddListener(this);
		AddInstance<LobbyTopmostMenu>("Resources/Prefab2/Lobby/TopmostMenu", isPassiveWindow: true, preload: true);
		AddInstance<LobbyLeftMenuAligner>("Resources/Prefab2/Lobby/LeftMenuAligner", isPassiveWindow: true, preload: true);
		AddInstance<LobbyRightMenu>("Resources/Prefab2/Lobby/RightMenu", isPassiveWindow: true, preload: true);
		AddInstance<LobbyNodeFunctionMenu>("Resources/Prefab2/Lobby/NodeFunctionMenu", isPassiveWindow: true, preload: true);
		AddInstance<SubStoryNodeInfo>("Resources/Prefab2/Window/SubStory/SubStoryNodeInfo", isPassiveWindow: true, preload: true);
		AddInstance<GameOptionWindow>("Resources/Prefab2/Window/GameOption/GameOptionWindow");
		AddInstance<MyInfoWindow>("Resources/Prefab2/Window/GameOption/MyInfoWindow");
		AddInstance<SelectAvatarHero>("Resources/Prefab2/Window/GameOption/SelectAvatarHero");
		AddInstance<SelectAvatarPet>("Resources/Prefab2/Window/GameOption/SelectAvatarPet");
		AddInstance<SelectCountryFlag>("Prefab2/Window/GameOption/SelectCountryFlag");
		AddInstance<SelectLanguage>("Resources/Prefab2/Window/GameOption/SelectLanguage");
		AddInstance<ManageBlockedUser>("Resources/Prefab2/Window/GameOption/ManageBlockedUser");
		AddInstance<SkipCurrencyConfirmCountPopup>("Resources/Prefab2/Window/GameOption/SkipCurrencyConfirmCountPopup");
		AddInstance<GetNewHero2>("Resources/Prefab2/Window/FullScene/GetNewHero2");
		AddInstance<LobbyNoticePopup>("Resources/Prefab2/Window/Notice/LobbyNoticePopup");
		AddInstance<ContentsOpenPopup>("Resources/Prefab2/Window/Achievement/ContentsOpenPopup");
		AddInstance<ButtonAnimateWindow>("Resources/Prefab2/Window/Tutorial/ButtonAnimateWindow");
		AddInstance<MenuOpenWindow>("Resources/Prefab2/Window/Tutorial/MenuOpenWindow");
		AddInstance<EventCalendarWindow>("Resources/Prefab2/Window/EventCalendar/EventCalendarWindow");
		AddInstance<LoginPatchNotePopup>("Resources/Prefab2/Window/Login/LoginPatchNotePopup");
		AddInstance<WorldBossKillRewardPopup>("Resources/Prefab2/Window/WorldBoss/WorldBossKillRewardPopup");
		AddInstance<MonthlyHeroWindow>("Resources/Prefab2/Window/MonthlyHero/MonthlyHeroWindow");
		AddInstance<EtcMenuWindow>("Resources/Prefab2/Window/Lobby/EtcMenuWindow");
		AddInstance<OfficialChannelWindow>("Resources/Prefab2/Window/Lobby/OfficialChannelWindow");
		AddInstance<LeftMenu>("Resources/Prefab2/Lobby/LeftMenu", isPassiveWindow: true, preload: true);
		AddInstance<GuideSelect>("Resources/Prefab2/Window/Tutorial/GuideSelect");
		AddInstance<GuideMenu>("Resources/Prefab2/Window/Tutorial/GuideMenu");
		AddInstance<MiniGuidePopup>("Resources/Prefab2/Window/Tutorial/MiniGuidePopup");
		AddInstance<PlayRecord>("Resources/Prefab2/Window/PlayRecord/PlayRecord");
		AddInstance<Roulette>("Resources/Prefab2/Window/MiniGame/Roulette");
		AddInstance<BoosterInfoList>("Resources/Prefab2/Lobby/BoosterInfoList");
		AddInstance<CostumeOpenPopup>("Resources/Prefab2/Window/SubStory/CostumeOpenPopup");
		AddInstance<HeroDetailView>("Resources/Prefab2/Window/FullScene/HeroDetailView");
		AddInstance<EquipmentItemFilter>("Resources/Prefab2/Component/Inventory/EquipmentItemFilter");
		AddInstance<PortalAdventureNoticeList>("Resources/Prefab2/Window/Portal/PortalAdventureNoticeList");
		AddInstance<LobbyPlayGuideMenu>("Resources/Prefab2/Lobby/LobbyPlayGuideMenu", isPassiveWindow: true, preload: true);
		AddInstance<PlayGuidePopup>("Resources/Prefab2/Window/PlayGuide/PlayGuidePopup");
		AddInstance<HonoraryChamber>("Resources/Prefab2/Window/Lobby/HonoraryChamber");
		AddInstance<LoginCreateNick>("Resources/Prefab2/Window/Login/LoginCreateNick");
		AddInstance<ContentUIDispatchBattleOnGoingList>("Resources/Prefab2/Window/DispatchBattle/ContentUIDispatchBattleOnGoingList");
		AddInstance<ContentUIDispatchBattle>("Resources/Prefab2/Window/DispatchBattle/ContentUIDispatchBattle");
		AddInstance<LobbyChapterClearReward>("Resources/Prefab2/Window/LobbyChapterClearReward/LobbyChapterClearReward");
		AddInstance<MainQuestRewardPopup>("Resources/Prefab2/Lobby/MainQuestRewardPopup");
		AddInstance<CreatureIntro>("Resources/Prefab2/Window/FullScene/CreatureIntro");
		AddInstance<LobbyShakmehBossGauge>("Resources/Prefab2/Window/ShakmehDungeon/LobbyShakmehBossGauge");
		AddInstance<MissionMilestonePopup>("Resources/Prefab2/Window/MissionMilestone/MissionMilestonePopup");
		AddInstance<AttendanceIntegrationWindow>("Resources/Prefab2/Window/Attendance/Integration/AttendanceIntegrationWindow");
		AddInstance<LobbyRightGridWindow>("Resources/Prefab2/Lobby/LobbyRightGridWindow");
		AddInstance<PunishmentRuneCraftWindow>("Resources/Prefab2/Window/PunishmentRuneCraft/PunishmentRuneCraftWindow");
		AddInstance<PunishmentRuneCraftResult>("Resources/Prefab2/Window/PunishmentRuneCraft/PunishmentRuneCraftResultWindow");
		AddInstance<PunishmentRuneCraftHelpPopup>("Resources/Prefab2/Window/PunishmentRuneCraft/PunishmentRuneCraftHelpPopup");
		AddInstance<AccumulateLogin>("Resources/Prefab2/Window/AccumulateLogin/AccumulateLogin");
		RegisterPassiveWindow();
		UpdateChapterStamina();
		PayShopManagement.LoadNewBannerImageUrlInfo();
	}

	private void Start()
	{
		SceneAutoBehaviour<NVespa.NScene.GuiManager>.instance.Add("LobbyManagement", this, delegate
		{
			GachaSelctShopHelper.OnDrawGUI();
			if (GUILayout.Button("OpenPunish"))
			{
				SceneAutoBehaviour<PunishManagment>.instance.OpenPunishMainWindow();
			}
			if (GUILayout.Button("Open PunishmentRuneCraft"))
			{
				GetInstance<PunishmentRuneCraftWindow>().Open(this);
			}
			if (GUILayout.Button("Open PunishmentRuneCraft Help Popup"))
			{
				GetInstance<PunishmentRuneCraftHelpPopup>().Open();
			}
			LocalStorageString localStorageString = new LocalStorageString(string.Empty, typeof(LobbyManagement).FullName + "HeroPresetStorageKey");
			using (new BlockManager().TitleBox("TextHeroStorage").Vertical())
			{
				using (new BlockManager().Horizontal())
				{
					GUILayout.Label("heroPresetStorageKey", GUILayout.Width(100f));
					localStorageString.Value = GUILayout.TextField(localStorageString.Value, GUILayout.Width(100f));
				}
				using (new BlockManager().Horizontal())
				{
					if (GUILayout.Button("Save HeroPreset"))
					{
						SingletonAuto<HeroPresetStorageWebManager>.instance.RequestAddHeroStorageSlot(localStorageString.Value);
					}
					if (GUILayout.Button("Load HeroPreset"))
					{
						SingletonAuto<HeroPresetStorageWebManager>.instance.RequestSetHeroStorageSlot(localStorageString.Value);
					}
				}
			}
			LocalStorageString localStorageString2 = new LocalStorageString(string.Empty, typeof(LobbyManagement).FullName + "HeroIndex");
			LocalStorageString localStorageString3 = new LocalStorageString("1", typeof(LobbyManagement).FullName + "HeroStar");
			LocalStorageString localStorageString4 = new LocalStorageString("0", typeof(LobbyManagement).FullName + "HeroTranscended");
			using (new BlockManager().Horizontal())
			{
				GUILayout.Label("Hero Index", GUILayout.Width(100f));
				localStorageString2.Value = GUILayout.TextField(localStorageString2.Value, GUILayout.Width(100f));
			}
			using (new BlockManager().Horizontal())
			{
				GUILayout.Label("Hero Star", GUILayout.Width(100f));
				localStorageString3.Value = GUILayout.TextField(localStorageString3.Value);
			}
			using (new BlockManager().Horizontal())
			{
				GUILayout.Label("Hero Transcended", GUILayout.Width(100f));
				localStorageString4.Value = GUILayout.TextField(localStorageString4.Value);
			}
			int num = Vespa_String_Extension_Method.TOINT(localStorageString2.Value, 0);
			int num2 = Vespa_String_Extension_Method.TOINT(localStorageString3.Value, 1);
			int num3 = Vespa_String_Extension_Method.TOINT(localStorageString4.Value, 0);
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(num);
			IList<EquipItemData> allData = GameRoot.Instance.EquipItemDataContainer.GetAllData();
			if (data == null)
			{
				GUILayout.Label("(invalid)");
			}
			else
			{
				GUILayout.Label(data.Name);
			}
			using (new BlockManager().Enable(data != null).Horizontal())
			{
				GUILayout.Label("GetNewHero2");
				if (GUILayout.Button("NewHero"))
				{
					if (SceneAutoBehaviour<SoulWeaponManagement>.instance.GetInstance<SoulWeaponLiberationResultWindow>().IsOpen())
					{
						return;
					}
					HeroInfo heroInfo = new HeroInfo
					{
						HeroIndex = num,
						Star = (sbyte)num2,
						Level = 1
					};
					GetInstance<GetNewHero2>().Open(GetNewHero2.OpenData.CreateSingleFromHeroInfo(heroInfo, GetNewHero2.EOpenMode.NewHero, hideInfo: false), null, null);
				}
				if (GUILayout.Button("Awaken"))
				{
					if (SceneAutoBehaviour<SoulWeaponManagement>.instance.GetInstance<SoulWeaponLiberationResultWindow>().IsOpen())
					{
						return;
					}
					HeroInfo heroInfo2 = new HeroInfo
					{
						HeroIndex = num,
						Star = (sbyte)num2,
						Transcended = (sbyte)num3,
						Level = 1
					};
					GetInstance<GetNewHero2>().Open(GetNewHero2.OpenData.CreateSingleFromHeroInfo(heroInfo2, GetNewHero2.EOpenMode.Awaken, hideInfo: false), null, null);
				}
				if (GUILayout.Button("Transcend"))
				{
					if (SceneAutoBehaviour<SoulWeaponManagement>.instance.GetInstance<SoulWeaponLiberationResultWindow>().IsOpen())
					{
						return;
					}
					HeroInfo heroInfo3 = new HeroInfo
					{
						HeroIndex = num,
						Star = (sbyte)num2,
						Transcended = (sbyte)num3,
						Level = 1
					};
					GetInstance<GetNewHero2>().Open(GetNewHero2.OpenData.CreateSingleFromHeroInfo(heroInfo3, GetNewHero2.EOpenMode.Transcend, hideInfo: false), null, null);
				}
				if (GUILayout.Button("SoulWeapon"))
				{
					if (GetInstance<GetNewHero2>().IsOpen() || SceneAutoBehaviour<SoulWeaponManagement>.instance.GetInstance<SoulWeaponLiberationResultWindow>().IsOpen())
					{
						return;
					}
					HashString hashString = "";
					for (int i = 0; i < __Extension_Method_ICollection__.COUNT(allData); i++)
					{
						if (allData[i].IsOpenSoulWeapon && allData[i].CreatureIndex[0] == num)
						{
							hashString = allData[i].EquipCode;
							break;
						}
					}
					if (hashString.IsNullOrEmpty())
					{
						return;
					}
					ItemData dataByCode = GameRoot.Instance.ItemDataContainer.GetDataByCode(hashString);
					EquipItemInfo equipItemInfo = new EquipItemInfo
					{
						ItemIndex = dataByCode.Index
					};
					equipItemInfo = UIHelper.GetFakeSoulWeaponInfo(equipItemInfo);
					equipItemInfo.SoulWeaponInfo.EquipItemSlotIndex = -1;
					SceneAutoBehaviour<SoulWeaponManagement>.instance.GetInstance<SoulWeaponLiberationResultWindow>().Open(equipItemInfo.SoulWeaponInfo, equipItemInfo);
				}
				if (GUILayout.Button("Close"))
				{
					SceneAutoBehaviour<SoulWeaponManagement>.instance.GetInstance<SoulWeaponLiberationResultWindow>().CloseWindow();
					GetInstance<GetNewHero2>().Close();
				}
			}
			using (new BlockManager().Enable(data != null).Horizontal())
			{
				if (GUILayout.Button("ModelView"))
				{
					CachedHeroInfo heroInfo4 = new CachedHeroInfo
					{
						HeroIndex = num,
						Star = data.StartHeroStar,
						Transcended = (sbyte)num3,
						Level = (short)data.StartHeroLevel
					};
					GetInstance<HeroModelView>().Open(heroInfo4, this);
				}
			}
			using (new BlockManager().Horizontal().TitleBox("New PayShop"))
			{
				if (GUILayout.Button("Open NewPayShop"))
				{
					OpenNewPayShop();
				}
				if (GUILayout.Button("Init New Flag"))
				{
					PayShopCategoryComponent.ClearNewFlagValue();
				}
			}
			LocalStorageString localStorageString5 = new LocalStorageString(string.Empty, typeof(LobbyManagement).FullName + "itemIndexText");
			LocalStorageString localStorageString6 = new LocalStorageString("0", typeof(LobbyManagement).FullName + "itemStarText");
			using (new BlockManager().Horizontal())
			{
				GUILayout.Label("Item Index", GUILayout.Width(100f));
				localStorageString5.Value = GUILayout.TextField(localStorageString5.Value, GUILayout.Width(100f));
			}
			using (new BlockManager().Horizontal())
			{
				GUILayout.Label("Item Star", GUILayout.Width(100f));
				localStorageString6.Value = GUILayout.TextField(localStorageString6.Value);
			}
			int num4 = Vespa_String_Extension_Method.TOINT(localStorageString5.Value, 0);
			int star = Vespa_String_Extension_Method.TOINT(localStorageString6.Value, 1);
			ItemData data2 = GameRoot.Instance.ItemDataContainer.GetData(num4);
			if (data2 == null)
			{
				GUILayout.Label("(invalid item)");
			}
			else
			{
				GUILayout.Label(data2.Name);
			}
			using (new BlockManager().Enable(data2 != null).Horizontal())
			{
				if (GUILayout.Button("Get Animation"))
				{
					BaseRewardPopup.PopupOpenData popupOpenData = new BaseRewardPopup.PopupOpenData().SetTitle("Test");
					if (data2.Type == ItemType.Equip)
					{
						EquipItemInfo equipItemInfo2 = new EquipItemInfo
						{
							ItemIndex = num4,
							Star = star
						};
						popupOpenData.SetEquipResultInfos(new EquipItemInfo[1] { equipItemInfo2 });
					}
					else
					{
						ItemResultInfo itemResultInfo = new ItemResultInfo
						{
							ItemIndex = num4,
							AddCount = 1,
							NewCount = 1
						};
						popupOpenData.SetItemResultInfos(new ItemResultInfo[1] { itemResultInfo });
					}
					BaseRewardPopup.OpenPopup(popupOpenData, GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpenedUI3DUI());
				}
			}
			TestGoatWarrior = GUILayout.Toggle(TestGoatWarrior, "TestGoatWarrior");
			if (GUILayout.Button("Clear Disabled Lobby Popup"))
			{
				LobbyNoticePopup.ClearDisabledLobbyNoticeIndices();
				EventWebViewPopup.ClearLastEnterLobbyOpen();
			}
			IList<LoadingData> allData2 = GameRoot.Instance.LoadingDataContainer.GetAllData();
			LocalStorageInt localStorageInt = new LocalStorageInt(0, typeof(LobbyManagement).FullName + "LoadingTableIndex");
			int num5 = (Vespa_Generic_IList_Extension_Method.IsValidIndex(allData2, localStorageInt.Value) ? allData2[localStorageInt.Value].Index : 75);
			using (new BlockManager().Horizontal())
			{
				GUILayout.Label("Loading Index : " + num5);
				if (!GlobalAutoBehaviour<LoadingManager>.instance.IsSceneLoading)
				{
					if (GUILayout.Button("LoadingTestScene On"))
					{
						localStorageInt.Value = 0;
						GlobalAutoBehaviour<LoadingManager>.instance.LoadingTestSceneOn();
					}
				}
				else
				{
					if (GUILayout.Button("+"))
					{
						localStorageInt.Value++;
						int value = localStorageInt.Value;
						num5 = (Vespa_Generic_IList_Extension_Method.IsValidIndex(allData2, value) ? allData2[value].Index : num5);
						GlobalAutoBehaviour<LoadingManager>.instance.LoadingTest(num5);
					}
					if (GUILayout.Button("-"))
					{
						localStorageInt.Value--;
						int value2 = localStorageInt.Value;
						num5 = (Vespa_Generic_IList_Extension_Method.IsValidIndex(allData2, value2) ? allData2[value2].Index : num5);
						GlobalAutoBehaviour<LoadingManager>.instance.LoadingTest(num5);
					}
					if (GUILayout.Button("LoadingTestScene Off"))
					{
						localStorageInt.Value = 0;
						GlobalAutoBehaviour<LoadingManager>.instance.LoadingTestSceneOff();
					}
				}
			}
			LocalStorageString localStorageString7 = new LocalStorageString("0", typeof(LobbyManagement).FullName + "ForcedLoadingIndex");
			using (new BlockManager().Horizontal())
			{
				GUILayout.Label("Forced Loading Index : ");
				localStorageString7.Value = GUILayout.TextField(localStorageString7.Value, GUILayout.Width(100f));
				if (GlobalAutoBehaviour<LoadingManager>.instance.IsSceneLoading && GUILayout.Button("Test"))
				{
					int num6 = Vespa_String_Extension_Method.TOINT(localStorageString7.Value, 0);
					for (int j = 0; j < __Extension_Method_ICollection__.COUNT(allData2); j++)
					{
						if (allData2[j].Index == num6)
						{
							localStorageInt.Value = j;
							num5 = num6;
							break;
						}
					}
					GlobalAutoBehaviour<LoadingManager>.instance.LoadingTest(num5);
				}
			}
			LocalStorageString localStorageString8 = new LocalStorageString("0", typeof(LobbyManagement).FullName + "FocusWidgetPath");
			using (new BlockManager().Horizontal())
			{
				GUILayout.Label("Path : ");
				localStorageString8.Value = GUILayout.TextField(localStorageString8.Value, GUILayout.Width(300f));
				if (GUILayout.Button("FocusWidget"))
				{
					Transform transform = Vespa_Unity_Transform_Extension_Method.FindFlexible(GameUIRoot.instance.transform, localStorageString8.Value);
					if (transform == null)
					{
						VespaDebug.LogError(" Failed to find widget : " + localStorageString8.Value);
					}
					else
					{
						UIWidget component = transform.GetComponent<UIWidget>();
						if (component == null)
						{
							VespaDebug.LogError(" Failed to find UIWidget : " + localStorageString8.Value);
						}
						else
						{
							LayerInfo layerInfo = GameUIRoot.instance.GetLayerInfo(component.gameObject.layer);
							Camera camera = ((layerInfo != null) ? layerInfo.Camera : GameUIRoot.instance.UI.Camera);
							Vector2 staticUiSize = TutorialWindow.GetStaticUiSize();
							Vector3[] worldCorners = component.worldCorners;
							Vector3[] array = new Vector3[4];
							for (int k = 0; k < __Extension_Method_ICollection__.COUNT(worldCorners); k++)
							{
								Vector3 vector = camera.WorldToViewportPoint(worldCorners[k]);
								array[k] = new Vector3((vector.x - 0.5f) * staticUiSize.x, (vector.y - 0.5f) * staticUiSize.y);
							}
							Vector3 vector2 = Vector3.Min(Vector3.Min(array[0], array[1]), Vector3.Min(array[2], array[3]));
							Vector3.Max(Vector3.Max(array[0], array[1]), Vector3.Max(array[2], array[3]));
							if (transform.GetComponent<BoxCollider>() == null)
							{
								VespaDebug.LogError(" Failed to find BoxCollider : " + localStorageString8.Value);
							}
						}
					}
				}
			}
			using (new BlockManager().Horizontal())
			{
				if (GUILayout.Button("MissionMain Wnd Open"))
				{
					SceneAutoBehaviour<MissionManagement>.instance.MissionWindowManager.OpenMissionMain();
				}
				if (GUILayout.Button("InspectUser Info Open"))
				{
					InspectUserManagement.InspectUserData inspectUser = new InspectUserManagement.InspectUserData(6028834L, 11511, 185, "U-6028834", InspectUserCategory.General);
					SceneAutoBehaviour<InspectUserManagement>.instance.RequestInspectUser(inspectUser);
				}
			}
			ForceRefreshAdvNotice = GUILayout.Toggle(ForceRefreshAdvNotice, "Forced Adventure Notice Refresh");
			if (GUILayout.Button("PurchasePolicy"))
			{
				GetCountryServerData();
				PurchasePolicyPopup.OpenPopup();
			}
			if (GUILayout.Button("SoulStoneRestoreWindow") && SceneAutoBehaviour<SoulWeaponManagement>.isValidInstance)
			{
				SoulStoneRestoreWindow soulStoneRestoreWindow = SceneAutoBehaviour<SoulWeaponManagement>.instance.GetInstance<SoulStoneRestoreWindow>();
				if (soulStoneRestoreWindow != null && soulStoneRestoreWindow.IsSoulStoneSelect())
				{
					CenterMessage.Open(LocalizationManager.GetStringDirect("SOULWEAPON_RESTORE_BUFF_POPUP"));
				}
			}
			using (new BlockManager().TitleBox("MessagePopup").Horizontal())
			{
				if (GUILayout.Button("OpenTempPopup"))
				{
					MessagePopup.OpenError(BaseResultType.AccountError);
				}
			}
			LocalStorageString localStorageString9 = new LocalStorageString(string.Empty, typeof(LobbyManagement).FullName + "ForcePurchaseMarketingIndex");
			using (new BlockManager().Horizontal())
			{
				GUILayout.Label("   PurchaseMarketing Index");
				localStorageString9.Value = GUILayout.TextField(localStorageString9.Value);
				if (GUILayout.Button("Open", GUILayout.Width(50f)))
				{
					PurchaseMarketingManager.OpenForcePurchaseMarketingPopupByProduct(int.Parse(localStorageString9.Value));
				}
			}
			using (new BlockManager().TitleBox("GuildSuppressContentsStatus").Horizontal())
			{
				if (GUILayout.Button("GuildSuppressStatusOn"))
				{
					SingletonAuto<ContentsStatusManager>.instance.Add(new ContentsStatusInfo
					{
						ContentsType = ContentsStatusType.GuildSuppress.ToString(),
						ContentsSubType = ContentsStatusSubType.None.ToString(),
						Status = ContentsStatus.On.ToString()
					});
				}
				if (GUILayout.Button("GuildSuppressStatusHide"))
				{
					SingletonAuto<ContentsStatusManager>.instance.Add(new ContentsStatusInfo
					{
						ContentsType = ContentsStatusType.GuildSuppress.ToString(),
						ContentsSubType = ContentsStatusSubType.None.ToString(),
						Status = ContentsStatus.Hide.ToString()
					});
				}
				if (GUILayout.Button("GuildSuppressStatusDisable"))
				{
					SingletonAuto<ContentsStatusManager>.instance.Add(new ContentsStatusInfo
					{
						ContentsType = ContentsStatusType.GuildSuppress.ToString(),
						ContentsSubType = ContentsStatusSubType.None.ToString(),
						Status = ContentsStatus.Disable.ToString()
					});
				}
			}
		});
		GlobalAutoBehaviour<AccountManager>.instance.AddEditorViewer();
	}

	private void OnEnable()
	{
		if (_coProcessingUpdateDispatchMenuVisible != null)
		{
			VespaCoroutine.Stop(_coProcessingUpdateDispatchMenuVisible);
			_coProcessingUpdateDispatchMenuVisible = null;
		}
		_coProcessingUpdateDispatchMenuVisible = VespaCoroutine.Start(CoProcessingUpdateDispatchBattleInfoMenuVisible(), null, null);
	}

	protected override void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			_getNewHeroOpenQueue.Release();
			ClearPassiveWindow();
			PayShopManagement.ReleaseAllBanner();
			ReleaseRightAwayBanner();
			base.OnDestroy();
			if (SingletonUpdater<TutorialManager>.isValidInstance)
			{
				SingletonUpdater<TutorialManager>.instance.RemoveListener(this);
			}
		}
	}

	private void OnApplicationPause(bool pauseStatus)
	{
	}

	public void CheckGooglePlayReward()
	{
	}

	private bool IsOrdealArena()
	{
		if (!SceneAutoBehaviour<OrdealArenaManagement>.isValidInstance)
		{
			return false;
		}
		return SceneAutoBehaviour<OrdealArenaManagement>.instance.IsOrdealArenaMap;
	}

	private bool IsSubStory()
	{
		if (!SceneAutoBehaviour<SubStoryManagement>.isValidInstance)
		{
			return false;
		}
		return SceneAutoBehaviour<SubStoryManagement>.instance.OnSubStoryMap;
	}

	private EPassiveWindowOpenFlag GetPassiveWindowOpenType()
	{
		if (IsOrdealArena())
		{
			return EPassiveWindowOpenFlag.OrdealArena;
		}
		if (IsSubStory())
		{
			return EPassiveWindowOpenFlag.SubStory;
		}
		return EPassiveWindowOpenFlag.MainStory;
	}

	private bool OnMainStoryMode()
	{
		return !OnSubStoryMode();
	}

	private bool OnSubStoryMode()
	{
		if (!SceneAutoBehaviour<SubStoryManagement>.isValidInstance)
		{
			return false;
		}
		return SceneAutoBehaviour<SubStoryManagement>.instance.OnSubStoryMap;
	}

	private bool CheckPassiveOpenCondtion(EPassiveWindowOpenFlag flag)
	{
		return (flag & GetPassiveWindowOpenType()) != 0;
	}

	private void RegisterPassiveWindow()
	{
		_passiveWindowList.Add(new NGame2.NUI.NManager.NLobby.PassiveWindow<LobbyRightMenu>(delegate(LobbyRightMenu window)
		{
			window.Open(this, this);
		}, CheckPassiveOpenCondtion, EPassiveWindowOpenFlag.MainStory));
		_passiveWindowList.Add(new NGame2.NUI.NManager.NLobby.PassiveWindow<LobbyNodeFunctionMenu>(delegate(LobbyNodeFunctionMenu window)
		{
			window.Open(this);
		}, CheckPassiveOpenCondtion, EPassiveWindowOpenFlag.MainStory | EPassiveWindowOpenFlag.SubStory));
		_passiveWindowList.Add(new NGame2.NUI.NManager.NLobby.PassiveWindow<LeftMenu>(delegate(LeftMenu window)
		{
			window.Open(this);
		}, CheckPassiveOpenCondtion, EPassiveWindowOpenFlag.MainStory));
		_passiveWindowList.Add(new NGame2.NUI.NManager.NLobby.PassiveWindow<LobbyRightGridWindow>(delegate(LobbyRightGridWindow window)
		{
			window.OpenInternal();
		}, CheckPassiveOpenCondtion, EPassiveWindowOpenFlag.MainStory));
		_passiveWindowList.Add(new NGame2.NUI.NManager.NLobby.PassiveWindow<SubStoryNodeInfo>(delegate(SubStoryNodeInfo window)
		{
			window.OpenWindow();
		}, CheckPassiveOpenCondtion, EPassiveWindowOpenFlag.SubStory));
		_passiveWindowList.Add(new NGame2.NUI.NManager.NLobby.PassiveWindow<OrdealArenaMapWindow>(delegate(OrdealArenaMapWindow window)
		{
			window.Open();
		}, CheckPassiveOpenCondtion, EPassiveWindowOpenFlag.OrdealArena));
	}

	private void ClearPassiveWindow()
	{
		_passiveWindowList.Clear();
	}

	private void ForEachPassiveWindow(Action<NGame2.NUI.NManager.NLobby.IPassiveWindow> action)
	{
		_passiveWindowList.ForEach(action);
	}

	private bool IsPassiveWindow(BaseWindow window)
	{
		if (window == null)
		{
			return false;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_passiveWindowList); i++)
		{
			if (_passiveWindowList[i].GetWindow(openedOnly: false) == window)
			{
				return true;
			}
		}
		return false;
	}

	private void RegisterLeftMenu()
	{
		LobbyLeftMenuAligner leftMenuAligner = GetInstance<LobbyLeftMenuAligner>();
		leftMenuAligner.Open();
		ForEachInstance(delegate(PrefabWrapper wrapper)
		{
			if (wrapper.Window is IAlignedMenuContainer container)
			{
				leftMenuAligner.Register(container);
			}
		});
	}

	public bool AnyTopmostDepthOrderWindowOpen()
	{
		return GlobalAutoBehaviour<GlobalManagement>.instance.FindWindow(delegate(IWindow window)
		{
			BaseWindow baseWindow = window as BaseWindow;
			if (baseWindow == null)
			{
				return false;
			}
			if (baseWindow.GetType() == typeof(LobbyTopmostMenu))
			{
				return false;
			}
			if (IsPassiveWindow(baseWindow))
			{
				return false;
			}
			return baseWindow.DepthOrder == EDepthOrder.Topmost;
		}) != null;
	}

	public void ReserveLobbyNoticePopup(LobbyNotice[] notices)
	{
		DateTime currentServerLocalTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime();
		DateTime value = LastLobbyNoticeTime.Value;
		if (currentServerLocalTime.Year != value.Year || currentServerLocalTime.Month != value.Month || currentServerLocalTime.Day != value.Day)
		{
			LastLobbyNoticeTime.Value = currentServerLocalTime;
			LobbyNoticePopup.ClearDisabledLobbyNoticeIndices();
		}
		int[] disabledLobbyNoticeIndices = LobbyNoticePopup.GetDisabledLobbyNoticeIndices();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(notices); i++)
		{
			LobbyNotice lobbyNotice = notices[i];
			if (!TypeIterator.Contains(disabledLobbyNoticeIndices, lobbyNotice.Index))
			{
				SceneAutoBehaviour<LobbyPopupQueue>.instance.ReserveLobbyNoticePopup(lobbyNotice);
			}
		}
	}

	private void ReserveNewlyOpenedContentsPopup(int tutorialIndex)
	{
		foreach (BottomButtonData data in GameRoot.Instance.BottomButtonDataContainer.GetAllData())
		{
			if (data.TutorialIndex == tutorialIndex && !String_Extension_Method.IsNullOrEmpty(data.ContentsOpenCode) && data.Type != ERightMenuButton.ClassBuff)
			{
				SceneAutoBehaviour<LobbyPopupQueue>.instance.AddPopup(GetInstance<ContentsOpenPopup>(), delegate(ContentsOpenPopup window)
				{
					window.Open(data.ContentsOpenCode);
				});
			}
		}
	}

	void ITutorialListener.OnTutorialBegin(TutorialData tutorialData)
	{
	}

	void ITutorialListener.OnTutorialEnd(int tutorialIndex, Action onEnd)
	{
		if (tutorialIndex != 0 && SceneAutoBehaviour<WorldMapManagement>.isValidInstance)
		{
			SceneAutoBehaviour<WorldMapManagement>.instance.ReloadDungeonOfReqTutorial(tutorialIndex);
		}
		if (!SingletonUpdater<TutorialManager>.instance.HasNextTurorial)
		{
			RequestCheckNewMailArrived();
		}
		ReserveNewlyOpenedContentsPopup(tutorialIndex);
		Vespa_System_Action_Extension_Method.fire(onEnd);
	}

	public void OpenLobbyWindows()
	{
		GlobalAutoBehaviour<BoosterManager>.instance.InitAllEffectiveBooster(BattleType.None, includeCash: true);
		GlobalAutoBehaviour<AccountManager>.instance.HeroManager.ObserverManager.AddObserver(this);
		GlobalAutoBehaviour<AccountManager>.instance.UserManager.ObserverManager.AddObserver(this);
		GlobalAutoBehaviour<DropBonusEventManager>.instance.ObserverManager.AddObserver(this);
		GlobalAutoBehaviour<AccountManager>.instance.QuestManager.MainQuestObserverManager.AddObserver(this);
		GlobalAutoBehaviour<AccountManager>.instance.QuestManager.SubQuestObserverManager.AddObserver(this);
		GlobalAutoBehaviour<AccountManager>.instance.AchievementManager.ObserverManager.AddObserver(this);
		GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager.ObserverManager.AddObserver(this);
		LobbyTopmostMenu lobbyTopmostMenu = GetInstance<LobbyTopmostMenu>();
		lobbyTopmostMenu.Open(this, this, this);
		lobbyTopmostMenu.SetTopMostMenuObserver(this);
		ForEachPassiveWindow(delegate(NGame2.NUI.NManager.NLobby.IPassiveWindow window)
		{
			window.Open();
		});
		RegisterLeftMenu();
		RequestCheckNewMailArrived();
		SceneAutoBehaviour<HeroNotifyContainer>.instance.Init();
		OpenBoosterInfoList();
	}

	public static bool OpenForChapterIndex(ERightMenuButton type)
	{
		CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(NGame2.NAccount.UserCampaignManager.ChapterIndex);
		if (data == null)
		{
			return false;
		}
		if (__Extension_Method_ICollection__.COUNT(data.ShowMenuButtons) == 0)
		{
			return false;
		}
		return data.ShowMenuButtons.Contains(type);
	}

	private void UpdateWorldBossInfo()
	{
		SingletonUpdater<NGame2.NContentsInfo.WorldBoss>.instance.RequestWorldBossInfo(showLoadingGauge: true, null);
	}

	public void OpenLobbyTopmostMenu(LobbyTopmostMenu.ICustomStaminaObserver observer)
	{
		GetInstance<LobbyTopmostMenu>().OpenStaminaType(observer);
	}

	public void CloseLobbyTopmostMenu(LobbyTopmostMenu.ICustomStaminaObserver observer)
	{
		GetInstance<LobbyTopmostMenu>().CloseStaminaType(observer);
	}

	public void MoveMap(int chapterIndex, int dungeonIndex, Action onEndMoveMap = null, Action onEndSceneChanger = null)
	{
		GetInstance<LobbyRightMenu>().OnMoveMap();
		SceneAutoBehaviour<WorldMapManagement>.instance.MoveMap(chapterIndex, dungeonIndex, delegate
		{
			Vespa_System_Action_Extension_Method.fire(onEndMoveMap);
			OnEndMoveMap();
		}, delegate
		{
			Vespa_System_Action_Extension_Method.fire(onEndSceneChanger);
			OpenGuildAttendancePopupInGuildPlace();
		});
	}

	public void OnEndMoveMap()
	{
		UpdateChapterStamina();
		EnableLobbyChapterReward();
		EnableLobbyShakmehBossGauge();
		SetLobbyChapterRewardInfo();
		EnableLobbyShakmehBossGauge();
	}

	public void OnEnterFullUi(LobbyFullWindow hider)
	{
		if (!SingletonUpdater<TutorialManager>.isValidInstance || !SingletonUpdater<TutorialManager>.instance.IsProgressing)
		{
			_openedFullWindows.Add(hider);
			MapLayout mapLayout = SceneAutoBehaviour<WorldMapManagement>.instance.GetInstance<MapLayout>();
			if (mapLayout != null)
			{
				mapLayout.gameObject.SetActive(value: false);
			}
		}
	}

	public void OnExitFullUi(LobbyFullWindow hider)
	{
		_openedFullWindows.Remove(hider);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_openedFullWindows))
		{
			MapLayout mapLayout = SceneAutoBehaviour<WorldMapManagement>.instance.GetInstance<MapLayout>();
			if (mapLayout != null)
			{
				mapLayout.gameObject.SetActive(value: true);
			}
		}
	}

	public void OpenPlayRecordWindow()
	{
		GetInstance<PlayRecord>().Open(this);
	}

	protected override void ChainUpdate()
	{
		base.ChainUpdate();
		CheckNewMailArrived();
		RefreshRightAwayProductInfo();
		GuildManager.RequestGuildInfo();
		UpdateLobbyMenu();
		UpdateShopRestockTime();
		UpdateNewRewardableMonthlyHero();
		UpdateEventDungeonCurrencyCheck();
		UpdateGetNewHeroQueue();
	}

	private void UpdateGetNewHeroQueue()
	{
		_getNewHeroOpenQueue.Update();
	}

	public static bool InLobbyIdle()
	{
		if (GlobalAutoBehaviour<LoadingManager>.instance.CurrentSceneType != LoadingManager.ESceneType.Lobby)
		{
			return false;
		}
		if ((SingletonUpdater<TutorialManager>.isValidInstance && SingletonUpdater<TutorialManager>.instance.IsProgressing) || (SceneAutoBehaviour<CutSceneManager>.isValidInstance && SceneAutoBehaviour<CutSceneManager>.instance.IsPlaying) || (GlobalAutoBehaviour<PopupManegement>.isValidInstance && GlobalAutoBehaviour<PopupManegement>.instance.AnyPopupExist()) || !SceneAutoBehaviour<LobbyPopupQueue>.instance.Empty() || GlobalAutoBehaviour<LoadingManager>.instance.IsSceneLoading)
		{
			return false;
		}
		return true;
	}

	private void InitRightAwayProductInfo(Action onSuccess, Action onFailed)
	{
		int contentsIntValue = SingletonAuto<ContentsValueManager>.instance.GetContentsIntValue("RightAwayProductInfoRefreshTimeSec", -1);
		if (contentsIntValue > 0)
		{
			_rightAwayProductInfoRefreshTime = (float)contentsIntValue * 1f;
		}
		else
		{
			_rightAwayProductInfoRefreshTime = float.Parse(GameRoot.Instance.ConstantDataContainer.GetValue("RightAwayProductInfoRefreshTime", "60"));
		}
		RequestRefreshRightAwayProductInfo(onSuccess, onFailed);
	}

	private void RefreshRightAwayProductInfo()
	{
		if (!((DateTime.UtcNow - _lastRefreshRightAwayProductInfoTime).TotalSeconds < (double)_rightAwayProductInfoRefreshTime))
		{
			RequestRefreshRightAwayProductInfo();
		}
	}

	private void RequestRefreshRightAwayProductInfo(Action onSuccess = null, Action onFailed = null)
	{
		_lastRefreshRightAwayProductInfoTime = DateTime.UtcNow;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(new NShared.RefreshRightAwayProductInfo.Request(), WebRequestFlag.None, delegate(NShared.RefreshRightAwayProductInfo.Response response)
		{
			if (response.BaseResult != BaseResultType.Success || response.Result != NShared.RefreshRightAwayProductInfo.ResultType.Success)
			{
				Vespa_System_Action_Extension_Method.fire(onFailed);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.SetRightAwayProductInfos(response.RightAwayProductInfos);
				GlobalAutoBehaviour<AccountManager>.instance.SetGachaSelectShopNoticeInfo(response.SelectShopNoticeInfos);
				Vespa_System_Action_Extension_Method.fire(onSuccess);
			}
		});
	}

	private void CheckNewMailArrived()
	{
		SprkMailUpdates.Tick(this);
	}

	private IEnumerator coShowNewMailMessage()
	{
		while (GlobalAutoBehaviour<LoadingManager>.instance.IsSceneLoading)
		{
			yield return null;
		}
		CenterMessage.Open(LocalizationManager.GetString(Mail.NEW_MAIL_ARRIVED));
	}

	public void RequestCheckNewMailArrived()
	{
		if (SprkNetworkPerformance.SkipMailCheck())
		{
			return;
		}
		long topmostMailIndex = MailManagement.TopmostMailIndex;
		NShared.CheckNewMail.Request request = new NShared.CheckNewMail.Request
		{
			TopmostMailIndex = topmostMailIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, WebRequestFlag.DisableRetry, delegate(NShared.CheckNewMail.Response response)
		{
			SprkMailUpdates.Finished(response);
			if (response != null && response.Result == NShared.CheckNewMail.ResultType.Success)
			{
				if (response.TotalMailCount != SingletonAuto<MailInfoContainer>.instance.GetTotalCount())
				{
					MailManagement.IsNewGlobalMailArrived = true;
				}
				MailManagement.MailCount = response.TotalMailCount;
				SingletonAuto<MailInfoContainer>.instance.AddOpenedMailIndices(response.OpenedMailIndices);
				if (response.NewMailArrived)
				{
					MailManagement.IsNewMailArrived = true;
					if (response.LastCheckedMailIndex != MailManagement.LastCheckedMailIndex)
					{
						MailManagement.LastCheckedMailIndex = response.LastCheckedMailIndex;
						VespaCoroutine.Start(coShowNewMailMessage());
					}
				}
			}
		});
	}

	private void UpdateNewRewardableMonthlyHero()
	{
		if (Time.time - _lastCheckNewRewardableMonthlyHero < 5f)
		{
			return;
		}
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(chapterIndex);
		if (data != null && data.Type == ChapterType.City)
		{
			_lastCheckNewRewardableMonthlyHero = Time.time;
			MapLayout mapLayout = SceneAutoBehaviour<WorldMapManagement>.instance.GetInstance<MapLayout>();
			if (mapLayout != null)
			{
				mapLayout.RefreshMonthlyHeroRewardableNew();
			}
		}
	}

	private void UpdateShopRestockTime()
	{
		if (!((DateTime.UtcNow - _lastCheckShopRestockTime).TotalSeconds < 5.0))
		{
			int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
			CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(chapterIndex);
			if (data != null && (data == null || data.Type == ChapterType.City))
			{
				_lastCheckShopRestockTime = DateTime.UtcNow;
				CheckShopRestockTime();
			}
		}
	}

	private void UpdateEventDungeonCurrencyCheck()
	{
		if (!(Time.time - _lastCheckEventDungeonCurrency < 5f))
		{
			_lastCheckEventDungeonCurrency = Time.time;
			MapLayout mapLayout = SceneAutoBehaviour<WorldMapManagement>.instance.GetInstance<MapLayout>();
			if (mapLayout != null)
			{
				mapLayout.RefreshEventDungeonCurrencyCheck();
			}
		}
	}

	private void CheckShopRestockTime()
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.ShopRestockTimeManager == null)
		{
			VespaDebug.LogError("NAccount.AccountManager.instance.ShopRestockTimeManager == null");
			return;
		}
		DateTime currentServerUTCTime = GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime();
		IList<ShopData> allData = GameRoot.Instance.ShopDataContainer.GetAllData();
		Dictionary<int, bool> dictionary = new Dictionary<int, bool>();
		foreach (ShopData item in allData)
		{
			ShopRestockTimeInfo shopRestockTimeInfo = GlobalAutoBehaviour<AccountManager>.instance.ShopRestockTimeManager.Get(item.Index);
			if (shopRestockTimeInfo != null)
			{
				DateTime o_dateTime = DateTime.MinValue;
				if (Vespa_String_Extension_Method2.TryParseNetworkTime2(shopRestockTimeInfo.RestockTime, out o_dateTime))
				{
					int num = ((item.RestockSecond > 0) ? item.RestockSecond : 3600);
					dictionary.Add(item.Index, currentServerUTCTime > o_dateTime.AddSeconds(num));
				}
			}
			else
			{
				dictionary.Add(item.Index, value: true);
			}
		}
		ShopRestockEffectPlay(dictionary);
	}

	private void ShopRestockEffectPlay(Dictionary<int, bool> playList)
	{
		if (__Extension_Method_ICollection__.COUNT(playList) > 0)
		{
			MapLayout mapLayout = SceneAutoBehaviour<WorldMapManagement>.instance.GetInstance<MapLayout>();
			if (mapLayout != null)
			{
				mapLayout.ShopRestockEffectPlay(playList);
			}
		}
	}

	private void ForceShopRestockCheck(bool use)
	{
		if (ForceShopRestockUpdate != use)
		{
			if (use)
			{
				ForceShopRestockUpdate = true;
				_lastCheckShopRestockTime = DateTime.MinValue;
				_lastCheckNewRewardableMonthlyHero = 0f;
				_lastCheckEventDungeonCurrency = 0f;
			}
			else
			{
				ForceShopRestockUpdate = false;
			}
		}
	}

	private void UpdateLobbyMenu()
	{
		bool flag = GlobalAutoBehaviour<GlobalManagement>.instance.ShowLobbyMenu();
		if (flag)
		{
			PortalSelect3 portalSelect = GetInstance<PortalSelect3>(openedOnly: true);
			if (portalSelect != null && portalSelect.IsOpen())
			{
				flag = false;
			}
		}
		if (flag)
		{
			ForEachPassiveWindow(delegate(NGame2.NUI.NManager.NLobby.IPassiveWindow window)
			{
				window.Open();
			});
		}
		else
		{
			ForEachPassiveWindow(delegate(NGame2.NUI.NManager.NLobby.IPassiveWindow window)
			{
				window.Close();
			});
		}
		ForceShopRestockCheck(flag);
		GetInstance<LobbyTopmostMenu>().SetShowLobbyMenu(flag);
	}

	private void ShowPlayGuideMenu(bool showLobbyMenu)
	{
		if (GetInstance<LobbyPlayGuideMenu>() != null)
		{
			if (!GlobalAutoBehaviour<LoadingManager>.instance.IsSceneLoading || !GetInstance<LobbyPlayGuideMenu>().IsOpen())
			{
				SetPlayGuideStatus();
			}
			else if (!CheckUpdatePlayGuide)
			{
				CheckUpdatePlayGuide = true;
			}
			bool show = (show = GameOptionData.Show_PlayGuide && showLobbyMenu && IsShowPlayGuideMenuByStatus());
			GetInstance<LobbyPlayGuideMenu>().ShowPlayGuideMenu(show);
		}
	}

	private void MoveDirect(PlayGuideInfo playGuideinfo)
	{
		if (playGuideinfo != null && playGuideinfo.PlayGuideData != null)
		{
			PlayGuideData playGuideData = playGuideinfo.PlayGuideData;
			if (!String_Extension_Method.IsNullOrEmpty(playGuideData.MoveTaskType))
			{
				CheckUpdatePlayGuide = false;
				CheckPlayGuideStartCourtineStatus = false;
				SetPlayGuideStatus(playGuideinfo, PlayGuideStatus.MOVED_DIRECT);
				SceneAutoBehaviour<ActivityManager>.instance.Run(playGuideData.MoveTaskType.ToString(), playGuideData.MoveTaskValue);
			}
		}
	}

	public void OpenSelectAvatarWindow()
	{
		AddTask(base.TaskFactory.Run(delegate(SelectAvatarHero window)
		{
			window.Open(this);
		}));
	}

	public void OpenSelectAvatarPetWindow()
	{
		AddTask(base.TaskFactory.Run(delegate(SelectAvatarPet window)
		{
			window.Open(this);
		}));
	}

	public void OpenSelectAvatarWindow(int avatarIndex)
	{
		AddTask(base.TaskFactory.Run(delegate(SelectAvatarHero window)
		{
			window.Open(this);
		}), base.TaskFactory.Run(delegate(SelectAvatarHero window)
		{
			window.SelectAvatar(avatarIndex);
		}), base.TaskFactory.Run(delegate(SelectAvatarHero window)
		{
			window.ScrollToSelectedComponent();
		}));
	}

	public void OpenSelectCountryFlagWindow(string countryCode)
	{
		AddTask(base.TaskFactory.Run(delegate(SelectCountryFlag window)
		{
			window.Open(countryCode, this);
		}), base.TaskFactory.Run(delegate(SelectCountryFlag window)
		{
			window.ScrollToSelectedComponent();
		}));
	}

	public void OpenMyInfoWindow()
	{
		GetInstance<MyInfoWindow>().Open(this, this);
	}

	public void OpenGameOptionWindow(GameOptionTab tab)
	{
		GetInstance<GameOptionWindow>().Open(this, this, tab);
	}

	public void OpenGameGuideWindow(Action OnClose = null)
	{
		GuideSelect guideSelect = GetInstance<GuideSelect>();
		guideSelect.Open(this);
		guideSelect.SetCloseAction(OnClose);
	}

	public void Tutorial_OpenGameGuideWindow()
	{
		GuideSelect guideSelect = GetInstance<GuideSelect>();
		guideSelect.Open(this);
		guideSelect.SetTutorialGuideWindow();
	}

	public CountryData GetCountryData()
	{
		string countryCode = SingletonUpdater<LoginManager>.instance.CountryCode;
		CountryData countryData = GameRoot.Instance.CountryDataContainer.GetData(countryCode);
		if (countryData == null)
		{
			countryData = GameRoot.Instance.CountryDataContainer.GetData("US");
			if (countryData == null)
			{
				countryData = GameRoot.Instance.CountryDataContainer.GetFirstData();
			}
		}
		return countryData;
	}

	public static CountryServerData GetCountryServerData()
	{
		string countryServerName = GlobalAutoBehaviour<AccountManager>.instance.CountryServerName;
		CountryServerData countryServerData = GameRoot.Instance.CountryServerDataContainer.GetData(countryServerName);
		if (countryServerData == null)
		{
			countryServerData = GameRoot.Instance.CountryServerDataContainer.GetData("None");
			if (countryServerData == null)
			{
				countryServerData = GameRoot.Instance.CountryServerDataContainer.GetFirstData();
			}
		}
		return countryServerData;
	}

	public static bool CanGoDungeon(int chapterindex, int dungeonIndex)
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanGoToChapter(chapterindex) && GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanVisit(chapterindex, dungeonIndex))
		{
			return GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanOpenChapterPortal(chapterindex);
		}
		return false;
	}

	private void OpenBoosterInfoList()
	{
		if (!GetInstance<BoosterInfoList>().IsOpen())
		{
			GetInstance<BoosterInfoList>().Open(this);
		}
	}

	public void OpenMiniGuidePopup(GameMiniGuideData miniGuideData)
	{
		int[] gameGuideIndex = miniGuideData.GameGuideIndex;
		GetInstance<MiniGuidePopup>().Open(gameGuideIndex);
	}

	public static void AddRightAwayBannerDic(int productIndex, string url)
	{
	}

	public static string GetBoardURL()
	{
		return "";
	}

	public static AssetTexture GetNewBannerAssetTexture(int productIndex)
	{
		return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(RightAwayBannerDic, productIndex, null);
	}

	public static void ReleaseRightAwayBanner()
	{
		foreach (KeyValuePair<int, AssetTexture> item in RightAwayBannerDic)
		{
			item.Value.CloseAndRelease();
			item.Value.Close();
		}
		RightAwayBannerDic.Clear();
	}

	private StaminaType GetChapterStaminaType()
	{
		return _chapterStaminaType;
	}

	private StaminaType GetChapterSubStaminaType()
	{
		return _chapterSubStaminaType;
	}

	private void UpdateChapterStamina()
	{
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(chapterIndex);
		if (data == null)
		{
			_chapterStaminaType = StaminaType.Chicken;
			_chapterSubStaminaType = StaminaType.Empty;
		}
		else
		{
			_chapterStaminaType = ((data.StaminaType == StaminaType.None) ? StaminaType.Chicken : data.StaminaType);
			_chapterSubStaminaType = ((data.SubStaminaType != StaminaType.None && data.SubStaminaType != StaminaType.Empty) ? data.StaminaType : StaminaType.Empty);
		}
	}

	void LobbyTopmostMenu.Listener.OnClickBuyGold()
	{
		SceneAutoBehaviour<MissionManagement>.instance.CloseAllActiveWindow(immediately: true);
		SceneAutoBehaviour<AchievementManagement>.instance.CloseAllActiveWindow(immediately: true);
		SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Gold.ToString());
	}

	void LobbyTopmostMenu.Listener.OnClickBuyGem()
	{
		SceneAutoBehaviour<MissionManagement>.instance.CloseAllActiveWindow(immediately: true);
		SceneAutoBehaviour<AchievementManagement>.instance.CloseAllActiveWindow(immediately: true);
		SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Ruby.ToString());
	}

	void LobbyTopmostMenu.Listener.OnClickBuyStamina()
	{
		SceneAutoBehaviour<MissionManagement>.instance.CloseAllActiveWindow(immediately: true);
		SceneAutoBehaviour<AchievementManagement>.instance.CloseAllActiveWindow(immediately: true);
		SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Stamina.ToString());
	}

	void LobbyTopmostMenu.Listener.OnClickChat()
	{
		SceneAutoBehaviour<MailManagement>.instance.CloseAllActiveWindow();
		SceneAutoBehaviour<PayShopManagement>.instance.CloseAllActiveWindow();
		if (!GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpened<ChattingWindow>())
		{
			SceneAutoBehaviour<ChattingManagement>.instance.OpenChatting(ChatType.None, 0L);
		}
		else
		{
			SceneAutoBehaviour<ChattingManagement>.instance.CloseAllActiveWindow();
		}
	}

	void LobbyTopmostMenu.Listener.OnClickAvatarHero()
	{
		OpenMyInfoWindow();
	}

	void LobbyTopmostMenu.Listener.OnClickMail()
	{
		SceneAutoBehaviour<ChattingManagement>.instance.CloseAllActiveWindow();
		SceneAutoBehaviour<PayShopManagement>.instance.CloseAllActiveWindow();
		if (!GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpened<MailListPopup>())
		{
			SceneAutoBehaviour<MailManagement>.instance.Open();
		}
		else
		{
			SceneAutoBehaviour<MailManagement>.instance.CloseAllActiveWindow();
		}
	}

	void LobbyTopmostMenu.Listener.OnClickEtcMenu()
	{
		_ = (object)null;
		_ = 56;
		_ = 1;
		_ = 0;
		Array.Empty<object>();
		GetInstance<EtcMenuWindow>().Open(this);
	}

	void LobbyTopmostMenu.Listener.OnClickBackButton()
	{
		GlobalAutoBehaviour<GlobalManagement>.instance.ExecuteBack();
	}

	void LobbyTopmostMenu.Listener.OnClickRechargeUndergroundPrisonKey()
	{
	}

	void LobbyTopmostMenu.Listener.OnClickMiniGuidePopup(GameMiniGuideData miniGuideData)
	{
		OpenMiniGuidePopup(miniGuideData);
	}

	void LobbyTopmostMenu.Listener.OnClickBoosterInfoMenu()
	{
		OpenBoosterInfoList();
		GetInstance<BoosterInfoList>().OpenBoosterInfoList();
		GetInstance<LobbyTopmostMenu>().SetNewBoosterInfoNotify(visible: false);
	}

	void LobbyTopmostMenu.Listener.RequestInitBoosterInfoList()
	{
		if (!SceneAutoBehaviour<CostumeManagement>.isValidInstance || !(SceneAutoBehaviour<CostumeManagement>.instance.GetInstance<HeroCostumeView>(openedOnly: true) != null))
		{
			OpenBoosterInfoList();
		}
	}

	void LobbyTopmostMenu.Listener.OnClickDispatchBattleMenu()
	{
		ContentUIDispatchBattleOnGoingList contentUIDispatchBattleOnGoingList = GetInstance<ContentUIDispatchBattleOnGoingList>();
		if (contentUIDispatchBattleOnGoingList != null)
		{
			if (!contentUIDispatchBattleOnGoingList.IsOpen())
			{
				contentUIDispatchBattleOnGoingList.Open(this);
			}
			else
			{
				contentUIDispatchBattleOnGoingList.Close();
			}
		}
	}

	void BoosterInfoList.Listener.RefreshBoosterInfoList(int totalBoosterInfoCount, int newBoosterInfoCount, bool isOpenBoosterInfoList)
	{
		GetInstance<LobbyTopmostMenu>().SetBoosterInfoMenu(totalBoosterInfoCount, newBoosterInfoCount, isOpenBoosterInfoList);
	}

	void BoosterInfoList.Listener.OnClickBoosterInfo(BoosterInfoListComponent comp)
	{
		BoosterInfo boosterInfo = comp.GetBoosterInfo();
		if (boosterInfo.MoveTaskType == ClientActivityType.None || String_Extension_Method.IsNullOrEmpty(boosterInfo.MoveTaskType.ToString()))
		{
			return;
		}
		if (boosterInfo.MinLevel > 0 && !GlobalAutoBehaviour<AccountManager>.instance.HasMinLevelHero(boosterInfo.MinLevel))
		{
			CenterMessage.Open(LocalizationManager.GetString(WorldMap.WORLDMAP_CANNOT_VISIT_NODE));
			return;
		}
		if (boosterInfo.MoveTaskType == ClientActivityType.OpenDungeonListPopup)
		{
			BottomButtonData data = GameRoot.Instance.BottomButtonDataContainer.GetData(ERightMenuButton.Portal);
			if (data == null)
			{
				return;
			}
			if (!LobbyRightMenu.CheckShowButtonByBottomButtonData(data))
			{
				CenterMessage.Open(LocalizationManager.GetString(WorldMap.WORLDMAP_CANNOT_VISIT_NODE));
				return;
			}
		}
		SceneAutoBehaviour<ActivityManager>.instance.Run(boosterInfo.MoveTaskType.ToString(), boosterInfo.MoveTaskValue);
	}

	void BoosterInfoList.Listener.EnableList()
	{
		EnableEffectFinishDispatchBattle();
	}

	void LobbyRightMenu.Listener.OnClickHeroButton()
	{
		if (SceneAutoBehaviour<MyHeroManagement>.instance.IsOpenedAny())
		{
			SceneAutoBehaviour<MyHeroManagement>.instance.CloseBut();
		}
		else
		{
			SceneAutoBehaviour<MyHeroManagement>.instance.OpenMyDeck();
		}
	}

	void LobbyRightMenu.Listener.OnClickInventoryButton()
	{
		SceneAutoBehaviour<InventoryManagement>.instance.OpenInventory();
	}

	void LobbyRightMenu.Listener.OnClickAchievementButton()
	{
		SceneAutoBehaviour<AchievementManagement>.instance.OpenAchievementList();
	}

	void LobbyRightMenu.Listener.OnClickQuestButton()
	{
		SceneAutoBehaviour<MissionManagement>.instance.MissionWindowManager.OpenMissionMain();
	}

	void LobbyRightMenu.Listener.OnClickFriendButton()
	{
		if (SceneAutoBehaviour<MyFriendManagement>.instance.IsOpenedAny())
		{
			SceneAutoBehaviour<MyFriendManagement>.instance.CloseBut();
		}
		else
		{
			SceneAutoBehaviour<MyFriendManagement>.instance.OpenFriendMenu(FriendMenu.EToggle.List);
		}
	}

	void LobbyRightMenu.Listener.OnClickPayShopButton()
	{
		OpenNewPayShop();
	}

	private void OpenNewPayShop()
	{
		SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop();
	}

	void LobbyRightMenu.Listener.OnClickArchive()
	{
		SceneAutoBehaviour<NGame2.NUI.NManager.IllustratedBookManagement>.instance.OpenIllustratedBookList();
	}

	void LobbyRightMenu.Listener.OnClickPortalRenewalButton()
	{
		SceneAutoBehaviour<PortalManagement>.instance.OpenPortalRenewalWindow();
	}

	void LobbyRightMenu.Listener.OnClickHeroStatButton()
	{
		OpenHeroStatWindow();
	}

	private void OpenHeroStatWindow()
	{
		HeroStatWebViewPopup.OpenGlobal();
	}

	public void OnPlayerMoveEnd()
	{
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		_ = NGame2.NAccount.UserCampaignManager.DungeonIndex;
		int linkedDungeonIndex = NGame2.NAccount.UserCampaignManager.LinkedDungeonIndex;
		CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(chapterIndex, linkedDungeonIndex);
		if (data != null && GetInstance<LobbyNodeFunctionMenu>().SelectMainButton(data) != null)
		{
			SceneAutoBehaviour<WorldMapManagement>.instance.RemoveTouchDisable();
			Vespa_System_Action_Extension_Method.fire(_onMoveEnd);
			_onMoveEnd = null;
		}
	}

	public void ExecuteCurrentNode(Action after)
	{
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		int linkedDungeonIndex = NGame2.NAccount.UserCampaignManager.LinkedDungeonIndex;
		NGame2.NAccount.Helper.FunctionStateData functionState = NGame2.NAccount.Helper.GetFunctionState(chapterIndex, linkedDungeonIndex, containsDisableText: true);
		if (functionState.State != NGame2.NAccount.Helper.FunctionStateData.EFunctionState.Enabled)
		{
			if (!String_Extension_Method.IsNullOrEmpty(functionState.DisableMsg))
			{
				CenterMessage.Open(functionState.DisableMsg);
			}
		}
		else
		{
			ExecuteNodeFunction(functionState.Type, after);
		}
	}

	public void ExecuteNodeFunction(NodeFunctionType functionType, Action after)
	{
		switch (functionType)
		{
		case NodeFunctionType.Battle:
		case NodeFunctionType.BattleOnce:
		case NodeFunctionType.MultiBattle:
			OpenCurrentNodeDungeonInfo(after);
			break;
		case NodeFunctionType.RunBattle:
			OpenNodeDungeonInfoInValues(after);
			break;
		case NodeFunctionType.EntryMenu:
		case NodeFunctionType.Temple:
		case NodeFunctionType.NPC:
		{
			int chapterIndex4 = NGame2.NAccount.UserCampaignManager.ChapterIndex;
			int dungeonIndex3 = NGame2.NAccount.UserCampaignManager.DungeonIndex;
			SceneAutoBehaviour<EntryMenuManagement>.instance.OpenEntryMenu(chapterIndex4, dungeonIndex3, skipAni: false, playNPCSound: true, isIgnoreTrigger: false, after);
			break;
		}
		case NodeFunctionType.Portal:
			SceneAutoBehaviour<PortalManagement>.instance.OpenPortalSelectWindow();
			Vespa_System_Action_Extension_Method.fire(after);
			break;
		case NodeFunctionType.City:
			MoveCurrentNodeChapter();
			Vespa_System_Action_Extension_Method.fire(after);
			break;
		case NodeFunctionType.MoveChapter:
		{
			OnClickMoveChapter triggerEvent4 = new OnClickMoveChapter
			{
				ChapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex,
				DungeonIndex = NGame2.NAccount.UserCampaignManager.LinkedDungeonIndex
			};
			if (!SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent4))
			{
				MoveCurrentNodeChapter();
				Vespa_System_Action_Extension_Method.fire(after);
			}
			break;
		}
		case NodeFunctionType.MoveSubStory:
		{
			OnClickMoveChapter triggerEvent3 = new OnClickMoveChapter
			{
				ChapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex,
				DungeonIndex = NGame2.NAccount.UserCampaignManager.LinkedDungeonIndex
			};
			if (!SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent3))
			{
				MoveCurrentNodeChapter(moveSubStory: true);
				Vespa_System_Action_Extension_Method.fire(after);
			}
			break;
		}
		case NodeFunctionType.Excavation:
			SceneAutoBehaviour<WorldMapManagement>.instance.StartExcavation();
			Vespa_System_Action_Extension_Method.fire(after);
			break;
		case NodeFunctionType.EventTrigger:
		{
			NodeFunctionMenu triggerEvent2 = new NodeFunctionMenu
			{
				ChapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex,
				DungeonIndex = NGame2.NAccount.UserCampaignManager.LinkedDungeonIndex
			};
			SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent2);
			Vespa_System_Action_Extension_Method.fire(after);
			break;
		}
		case NodeFunctionType.SubStoryEvent:
		case NodeFunctionType.SubStoryBattle:
			if (SceneAutoBehaviour<SubStoryManagement>.instance.CanStart(functionType))
			{
				int chapterIndex3 = NGame2.NAccount.UserCampaignManager.ChapterIndex;
				int linkedDungeonIndex = NGame2.NAccount.UserCampaignManager.LinkedDungeonIndex;
				EnterSubStoryNode triggerEvent = new EnterSubStoryNode
				{
					ChapterIndex = chapterIndex3,
					DungeonIndex = linkedDungeonIndex
				};
				SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent, delegate
				{
					SceneAutoBehaviour<SubStoryManagement>.instance.ExecuteNodeFunction(functionType);
				});
			}
			break;
		case NodeFunctionType.SubStoryBoss:
			if (SceneAutoBehaviour<SubStoryManagement>.instance.CanStart(functionType))
			{
				OpenCurrentNodeDungeonInfo(after);
			}
			break;
		case NodeFunctionType.HonoraryChamber:
			OpenHonoraryChamber();
			break;
		case NodeFunctionType.ShakmehDungeon:
		{
			int chapterIndex2 = NGame2.NAccount.UserCampaignManager.ChapterIndex;
			int dungeonIndex2 = NGame2.NAccount.UserCampaignManager.DungeonIndex;
			SceneAutoBehaviour<NGame2.NUI.NManager.ShakmehDungeonManagement>.instance.OpenShakmehDungeonWindow(chapterIndex2, dungeonIndex2);
			break;
		}
		case NodeFunctionType.OrdealArenaBattle:
		case NodeFunctionType.OrdealArenaEvent:
		case NodeFunctionType.OrdealArenaStart:
		case NodeFunctionType.OrdealArenaEnd:
			if (SceneAutoBehaviour<OrdealArenaManagement>.isValidInstance)
			{
				SceneAutoBehaviour<OrdealArenaManagement>.instance.ExecuteNodeFunction(functionType);
			}
			break;
		case NodeFunctionType.PetHouse:
			SceneAutoBehaviour<PetManagement>.instance.OpenPetHouse();
			break;
		case NodeFunctionType.WebView:
		{
			int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
			int dungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
			OpenWebViewNodeFunction(chapterIndex, dungeonIndex);
			break;
		}
		case NodeFunctionType.TreasureHouse:
			SceneAutoBehaviour<TreasureHouseManagement>.instance.OpenTreasureHouseWindow();
			break;
		case NodeFunctionType.PunishmentRaid:
			SceneAutoBehaviour<PunishManagment>.instance.OpenPunishMainWindow();
			break;
		case NodeFunctionType.PunishmentRuneCraft:
			OpenPunishmentRuneCraftWindow();
			break;
		case NodeFunctionType.Switching:
		case NodeFunctionType.MoveToNpc:
		case NodeFunctionType.NoEntry:
			break;
		}
	}

	public void OpenPunishmentRuneCraftWindow()
	{
		PunishmentRuneCraftWindow punishmentRuneCraftWindow = GetInstance<PunishmentRuneCraftWindow>();
		if (!(punishmentRuneCraftWindow == null) && !punishmentRuneCraftWindow.IsOpen())
		{
			punishmentRuneCraftWindow.Open(this);
		}
	}

	public void OpenHonoraryChamber(RecordsOfHonorContentType requestContentType = RecordsOfHonorContentType.None, HonoraryChamber.ServerType serverType = HonoraryChamber.ServerType.Local)
	{
		GetInstance<HonoraryChamber>().Open(this, requestContentType, serverType);
	}

	public void SaveToLocalOpenMultiSelect()
	{
		LobbyNodeFunctionMenu lobbyNodeFunctionMenu = GetInstance<LobbyNodeFunctionMenu>();
		if (!(lobbyNodeFunctionMenu == null) && !(lobbyNodeFunctionMenu.Button_Raid == null))
		{
			ContentUISeasonFirstViewFx component = lobbyNodeFunctionMenu.Button_Raid.GetComponent<ContentUISeasonFirstViewFx>();
			if (!(component == null))
			{
				component.SaveToLocal();
				component.Open();
			}
		}
	}

	public void OpenWebViewNodeFunction(int chapterIndex, int dungeonIndex)
	{
		CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data == null)
		{
			VespaDebug.LogError("NodeData is Null" + chapterIndex + "_" + dungeonIndex);
			return;
		}
		if (LoginData.WebEvent == null)
		{
			VespaDebug.LogError("WebEvent is Null");
			return;
		}
		int valueInt = data.GetValueInt(0);
		EventWebViewPopup.OpenGlobal($"{LoginData.WebEvent.LinkUrl}{valueInt}.html{LoginData.WebEvent.QueryString}&id={valueInt}");
	}

	void LobbyNodeFunctionMenu.Listener.OnClickMainButton(Action after)
	{
		ExecuteCurrentNode(after);
	}

	void LobbyNodeFunctionMenu.Listener.OnClickSubButton(NodeFunctionType functionType)
	{
		ExecuteNodeFunction(functionType, null);
	}

	void LobbyNodeFunctionMenu.Listener.OnClickSubButton(SubButtonFunctionType buttonType, NodeFunctionType functionType, string functionValue)
	{
		switch (buttonType)
		{
		case SubButtonFunctionType.Raid:
			SceneAutoBehaviour<RaidManagement>.instance.OpenRaidSelect();
			break;
		case SubButtonFunctionType.WorldBoss:
			SceneAutoBehaviour<WorldBossManagement>.instance.OpenWorldBossRanking(null);
			break;
		case SubButtonFunctionType.GuildArena:
			if (!String_Extension_Method.IsNullOrEmpty(functionValue))
			{
				MoveToNpc activity = new MoveToNpc(functionValue, executeNodeFunction: true, warp: false, delegate
				{
					SceneAutoBehaviour<GuildArenaManagement>.instance.OpenGuildArena();
				});
				SceneAutoBehaviour<ActivityManager>.instance.Run(activity);
			}
			break;
		case SubButtonFunctionType.SoulWeapon:
			SceneAutoBehaviour<SoulWeaponManagement>.instance.OpenSoulWeaponLobby(null);
			break;
		case SubButtonFunctionType.EventWorldBoss:
			break;
		}
	}

	void LobbyNodeFunctionMenu.Listener.OnClickLeaveCityButton()
	{
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		List<CampaignNodeData> allDataOfFunctionType = GameRoot.Instance.CampaignNodeDataContainer.GetAllDataOfFunctionType(NodeFunctionType.MoveChapter, chapterIndex);
		if (__Extension_Method_ICollection__.COUNT(allDataOfFunctionType) == 0)
		{
			VespaDebug.LogError("no move chapter node");
			return;
		}
		int dungeonIndex = allDataOfFunctionType[0].DungeonIndex;
		MoveNodeChapter(chapterIndex, dungeonIndex);
	}

	private void OpenCurrentNodeDungeonInfo(Action after)
	{
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		int dungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
		int linkedDungeonIndex = NGame2.NAccount.UserCampaignManager.LinkedDungeonIndex;
		SceneAutoBehaviour<LobbyCampaignDungeon>.instance.OpenDungeonInfo(chapterIndex, dungeonIndex, linkedDungeonIndex);
		Vespa_System_Action_Extension_Method.fire(after);
	}

	private void OpenNodeDungeonInfoInValues(Action after)
	{
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		int dungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
		string[] value = GameRoot.Instance.CampaignNodeDataContainer.GetData(chapterIndex, dungeonIndex).Value;
		if (value.Length >= 2)
		{
			int result = 0;
			int result2 = 0;
			if (int.TryParse(value[0], out result) && int.TryParse(value[1], out result2))
			{
				int linkedDungeonIndex = GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.GetLinkedDungeonIndex(result, result2);
				SceneAutoBehaviour<LobbyCampaignDungeon>.instance.OpenDungeonInfo(result, result2, linkedDungeonIndex);
				Vespa_System_Action_Extension_Method.fire(after);
			}
		}
	}

	private void MoveNodeChapter(int chapterIndex, int dungeonIndex, bool moveSubStory = false)
	{
		CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data == null)
		{
			VespaDebug.LogError("Dungeon data not found : chapterIndex=" + chapterIndex + " dungeonIndex=" + dungeonIndex);
			return;
		}
		int targetChapterIndex = data.GetValueInt(0);
		if (targetChapterIndex == 0)
		{
			VespaDebug.LogError("Target chapter not defined : chapterIndex=" + data.ChapterIndex + " dungeonIndex=" + data.DungeonIndex);
			return;
		}
		CampaignChapterData data2 = GameRoot.Instance.CampaignChapterDataContainer.GetData(targetChapterIndex);
		if (data2 == null)
		{
			MessagePopup.OpenError(LocalizationManager.GetString(WorldMap.MAP_CANNOT_MOVE_CHAPTER_NOT_EXIST));
			return;
		}
		if (data2.Type == ChapterType.WorldMap)
		{
			if (!GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CanGoToChapter(targetChapterIndex))
			{
				MessagePopup.OpenError(LocalizationManager.GetString(WorldMap.MAP_CANNOT_MOVE_CHAPTER_NOT_COMPLETED));
				return;
			}
			if (!data2.IsOpen)
			{
				MessagePopup.Open(LocalizationManager.GetString(NShared.NLocalization.Common.INFORM), LocalizationManager.GetString(NShared.NLocalization.System.NOT_IMPLEMENTED), null);
				return;
			}
		}
		int targetDungeonIndex = data.GetValueInt(1);
		if (targetDungeonIndex == 0)
		{
			targetDungeonIndex = 1;
		}
		if (data.GetValueBool(2))
		{
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string stringDirect = LocalizationManager.GetStringDirect(data2.Name);
			string message = LocalizationManager.GetString(WorldMap.MAP_MOVE_CHAPTER_CONFIRM, stringDirect);
			MessagePopup.Open(title, message, delegate
			{
				MoveMap(targetChapterIndex, targetDungeonIndex);
			}, null);
		}
		else
		{
			if (moveSubStory)
			{
				CampaignNodeData data3 = GameRoot.Instance.CampaignNodeDataContainer.GetData(NGame2.NAccount.UserCampaignManager.ChapterIndex, NGame2.NAccount.UserCampaignManager.DungeonIndex);
				SceneAutoBehaviour<SubStoryManagement>.instance.SetEntryChapterNode(data3);
			}
			MoveMap(targetChapterIndex, targetDungeonIndex);
		}
	}

	private void MoveCurrentNodeChapter(bool moveSubStory = false)
	{
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		int dungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
		MoveNodeChapter(chapterIndex, dungeonIndex, moveSubStory);
	}

	private IEnumerator CoRunAdventureNoticeAction(AdventureNoticeData adventureNoticeData, float delay)
	{
		yield return new WaitForSeconds(delay);
		MapLayout mayLayout = SceneAutoBehaviour<WorldMapManagement>.instance.GetInstance<MapLayout>();
		while (SceneAutoBehaviour<WorldMapManagement>.instance.IsMapLoading || mayLayout.IsPlayerMoving())
		{
			yield return null;
		}
		string valueString = ArgParser.GetValueString(adventureNoticeData.Action, 0);
		if (String_Extension_Method.IsNullOrEmpty(valueString))
		{
			yield break;
		}
		NGame2.NUI.NWindow.NEntryMenu.IAction action = SingletonAuto<ActionContainer>.instance.GetAction(valueString);
		if (action != null)
		{
			List<object> list = new List<object>();
			string[] action2 = adventureNoticeData.Action;
			foreach (string item in action2)
			{
				list.Add(item);
			}
			action.Run(null, null, list);
		}
	}

	private void RequestAdventureNotice()
	{
		NShared.GetAdeventureNotice.Request request = new NShared.GetAdeventureNotice.Request();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetAdeventureNotice.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.GetAdeventureNotice.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.AdventureNoticeManager.Apply(response);
			}
		});
	}

	void MoveToLastBattlePopup.Listener.MoveNode(int ChapterIndex, int DungeonIndex)
	{
		base.TaskFactory.Close<PortalSelect3>().Commit();
		MoveMap(ChapterIndex, DungeonIndex);
		UsePortal triggerEvent = new UsePortal
		{
			MoveToLast = true
		};
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent);
	}

	int LobbyRightMenu.IDataProvider.GetClearedAchievements()
	{
		return SingletonAuto<AchievementChecker>.instance.GetAllClearedAchievements(AchievementType.Daily, AchievementType.Weekly, AchievementType.Limited, AchievementType.PayShop, AchievementType.SoulWeapon);
	}

	bool LobbyRightMenu.IDataProvider.IsNewClearQuest()
	{
		if (SingletonAuto<AchievementChecker>.instance.GetAllClearedAchievements(AchievementType.General, AchievementType.Hero, AchievementType.EventOnce, AchievementType.PayShop, AchievementType.SoulWeapon) > 0)
		{
			return true;
		}
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			return false;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.QuestManager.IsGetRewardable;
	}

	int LobbyRightMenu.IDataProvider.GetNotClearedDailyQuests()
	{
		return SingletonAuto<AchievementChecker>.instance.GetAllNotClearedAchievements(AchievementType.General, AchievementType.Hero, AchievementType.DailyEvent, AchievementType.Weekly, AchievementType.Raid, AchievementType.EventOnce, AchievementType.PayShop, AchievementType.SoulWeapon);
	}

	void GameOptionWindow.Listener.OnChangeLanguage()
	{
		AddTask(base.TaskFactory.Run(delegate(SelectLanguage window)
		{
			window.Open(this);
		}));
	}

	void GameOptionWindow.Listener.OnLogout()
	{
		SingletonUpdater<LoginManager>.instance.Logout();
	}

	void GameOptionWindow.Listener.OnOqupieWebView()
	{
		OqupieWebViewPopup.OpenPopup();
	}

	void GameOptionWindow.Listener.OnReset()
	{
		SingletonUpdater<LoginManager>.instance.SignOut();
	}

	void GameOptionWindow.Listener.OnUseCoupon()
	{
		UseCouponPopup.Open(delegate(string couponCode)
		{
			if (!string.IsNullOrEmpty(couponCode))
			{
				SingletonUpdater<WebServiceManager>.instance.Requester.Request(new NShared.UseCoupon.Request
				{
					CouponCode = couponCode,
					CountryCode = SingletonUpdater<LoginManager>.instance.CountryCode
				}, delegate(NShared.UseCoupon.Response iResponse)
				{
					if (iResponse == null)
					{
						MessagePopup.OpenError(BaseResultType.Fail);
					}
					else if (iResponse.BaseResult != BaseResultType.Success)
					{
						MessagePopup.OpenError(iResponse.BaseResult);
					}
					else if (iResponse.Result != NShared.UseCoupon.ResultType.Success)
					{
						MessagePopup.OpenError(iResponse.Result);
					}
					else
					{
						CenterMessage.Open(LocalizationManager.GetString(Promotion.USE_COUPON_SUCCESS));
					}
				});
			}
		});
	}

	void GameOptionWindow.Listener.OnLeaveParty()
	{
		GlobalAutoBehaviour<PartyManager>.instance.SendLeaveParty();
	}

	void GameOptionWindow.Listener.OnCsMailPopup()
	{
		OpenCsMailPopup();
	}

	public void OpenCsMailPopup()
	{
		string stringDirect = LocalizationManager.GetStringDirect("CS_EMAIL_TITLE");
		string stringDirect2 = LocalizationManager.GetStringDirect("CS_POPUP_MESSAGE");
		Action<int> onYes = delegate(int index)
		{
			CSMailData data = GameRoot.Instance.CSMailDataContainer.GetData(index);
			if (data == null)
			{
				VespaDebug.LogError($"Failed to get CSMailData : Index = {index}");
			}
			else
			{
				SystemLanguage language = SingletonAuto<LocalizationManager>.instance.Language;
				string csEmail = SingletonAuto<LocalizationManager>.instance.GetLanguageValue(language).CsEmail;
				string mailTitle = $"[{LocalizationManager.GetStringDirect(data.CSMailTitle)}]";
				string stringDirect3 = LocalizationManager.GetStringDirect(data.CSMailDesc);
				string text = LoginData.LastLoginMethod.ToString();
				string countryServerName = GlobalAutoBehaviour<AccountManager>.instance.CountryServerName;
				long num = 0L;
				if (AccountManager.IsLogined())
				{
					UserManager userManager = GlobalAutoBehaviour<AccountManager>.instance.UserManager;
					if (userManager != null)
					{
						num = userManager.Id;
					}
				}
				string nick = GlobalAutoBehaviour<AccountManager>.instance.Nick;
				string operatingSystem = SystemInfo.operatingSystem;
				string text2 = $"{SystemInfo.deviceModel}/{SystemInfo.systemMemorySize}MB/{SystemInfo.processorType}";
				string arg = $"-----[Game Info]-----\n-Login : {text}\n-Server : {countryServerName}\n-Game ID: {num}\n-Nick Name: {nick}\n-OS : {operatingSystem}\n-Device info : {text2}\n";
				string message = $"{stringDirect3}\n\n\n\n\n{arg}".Replace("\n", "\r\n");
				DeviceHelper.SendCSEmail(csEmail, mailTitle, message);
				_ = (object)null;
				_ = 62;
				_ = 1;
				_ = 0;
				Array.Empty<object>();
			}
		};
		MessageOpenCS.OpenCS2(stringDirect, stringDirect2, onYes, null);
		_ = (object)null;
		_ = 61;
		_ = 1;
		_ = 0;
		Array.Empty<object>();
	}

	public void OpenSkipCurrencyConfirmCountPopup(object value, CurrencyType currencyType = CurrencyType.Gem)
	{
		GetInstance<SkipCurrencyConfirmCountPopup>().Open(this, value);
	}

	public void OpenManageBlockedUser()
	{
		GetInstance<ManageBlockedUser>().Open(this);
	}

	public void SetTextureQuality(ESpecType specType, int componentIndex)
	{
		if (SystemOptionData.Spec == specType || GlobalAutoBehaviour<PatchDownloader>.instance.IsPatchDownloading())
		{
			return;
		}
		PatchSpecType patchSpecType = PatchSpecType.UnDefine;
		switch (specType)
		{
		case ESpecType.Low:
			patchSpecType = PatchSpecType.Low;
			break;
		case ESpecType.Mid:
			patchSpecType = PatchSpecType.Mid;
			break;
		case ESpecType.Original:
			patchSpecType = PatchSpecType.High;
			break;
		}
		PatchDownloadSpecData recommandSpecData = PatchDownloadHelper.GetRecommandSpecData();
		bool flag = false;
		if (__Extension_Method_ICollection__.IsNullOrEmpty(recommandSpecData.SelectSpecInGameOption))
		{
			if (patchSpecType != recommandSpecData.Spec)
			{
				flag = true;
			}
		}
		else
		{
			flag = !recommandSpecData.SelectSpecInGameOption.Contains(patchSpecType);
		}
		if (flag)
		{
			string title = LocalizationManager.GetString(GameOption.OPTION_GAME_RESTARTGAME_TITLE);
			string stringDirect = LocalizationManager.GetStringDirect("OPTION_GAME_QUALITY_CANNOT_CHOOSE");
			MessagePopup.Open(title, stringDirect, delegate
			{
				AddTask(base.TaskFactory.Run(delegate(GameOptionWindow window)
				{
					window.RefreshComponent(componentIndex);
				}));
			}, delegate
			{
				AddTask(base.TaskFactory.Run(delegate(GameOptionWindow window)
				{
					window.RefreshComponent(componentIndex);
				}));
			});
			return;
		}
		string title2 = LocalizationManager.GetString(GameOption.OPTION_GAME_RESTARTGAME_TITLE);
		string message = LocalizationManager.GetString(GameOption.OPTION_GAME_RESTARTGAME_MESSAGE);
		MessagePopup.Open(title2, message, delegate
		{
			SystemOptionData.Spec = specType;
			AccountManager.Logout();
			SingletonUpdater<LoginManager>.instance.ReturnToLogin();
		}, delegate
		{
			AddTask(base.TaskFactory.Run(delegate(GameOptionWindow window)
			{
				window.RefreshComponent(componentIndex);
			}));
		});
	}

	void GameOptionWindow.Listener.ChangeLoginMethod(LoginMethodType methodType)
	{
		LoginMethodType lastLoginMethod = LoginData.LastLoginMethod;
		if (methodType == lastLoginMethod || methodType == LoginMethodType.None)
		{
			return;
		}
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string empty = string.Empty;
		Enum key = null;
		switch (methodType)
		{
		case LoginMethodType.GameCenter:
			key = GameOption.LINKACCOUNT_GAMECENTER;
			break;
		case LoginMethodType.Steam:
			key = GameOption.LINKACCOUNT_FACEBOOK;
			break;
		case LoginMethodType.GooglePlus:
			key = GameOption.LINKACCOUNT_GPGS;
			break;
		case LoginMethodType.Apple:
			key = GameOption.LINKACCOUNT_APPLE;
			break;
		case LoginMethodType.Twitter:
			key = GameOption.LINKACCOUNT_TWITTER;
			break;
		case LoginMethodType.Line:
			key = GameOption.LINKACCOUNT_LINE;
			break;
		}
		empty = LocalizationManager.GetString(key);
		if (lastLoginMethod != LoginMethodType.Guest)
		{
			empty = empty + "\n" + LocalizationManager.GetString(GameOption.LINKACCOUNT_WARNING);
		}
		MessagePopup.Open(title, empty, delegate
		{
			SingletonUpdater<LoginManager>.instance.ChangeLoginMethod(methodType, delegate
			{
				GetInstance<GameOptionWindow>().ReOpen();
			});
		}, null);
	}

	LoginMethodType GameOptionWindow.IDataProvider.GetLastLoginMethod()
	{
		return LoginData.LastLoginMethod;
	}

	CountryData GameOptionWindow.IDataProvider.GetCountryData()
	{
		return GetCountryData();
	}

	CountryServerData GameOptionWindow.IDataProvider.GetCountryServerData()
	{
		return GetCountryServerData();
	}

	void SelectAvatarHero.Listener.ChangeAvatarHero(UIAvatarHelper.AvatarInfo changeAvatarInfo)
	{
		if (PlayerAvatarInfo.IsEqualCurrentPlayerAvatarHero(changeAvatarInfo))
		{
			return;
		}
		AddTask(new WebServiceTask<NShared.ChangeAvatarHero.Request, NShared.ChangeAvatarHero.Response>(new NShared.ChangeAvatarHero.Request
		{
			AvatarHeroIndex = changeAvatarInfo.AvatarIndex,
			HeroIndex = changeAvatarInfo.HeroIndex
		}, (NShared.ChangeAvatarHero.Request request, NShared.ChangeAvatarHero.Response response) => (response.Result != NShared.ChangeAvatarHero.ResultType.Success) ? base.TaskFactory.Run(delegate
		{
			MessagePopup.OpenError(response.Result);
		}) : base.TaskFactory.Sequence(base.TaskFactory.Run(delegate
		{
			GlobalAutoBehaviour<AccountManager>.instance.UserManager.SetAvatarHeroIndex(request.AvatarHeroIndex, response.PlayerAvatarHeroInfo);
		}), base.TaskFactory.Close<SelectAvatarHero>(), base.TaskFactory.Run(delegate(MyInfoWindow window)
		{
			window.UpdateAvatarHero();
		}), base.TaskFactory.Run(delegate
		{
			SingletonAuto<MessageLogHandler>.instance.SendToFriend(new AvatarChangeNot
			{
				HeroIndex = request.AvatarHeroIndex
			});
		}))));
	}

	void SelectAvatarHero.Listener.OpenHeroCostumeView(int heroIndex)
	{
		SceneAutoBehaviour<CostumeManagement>.instance.OpenCostumeMySet(heroIndex);
	}

	void SelectAvatarPet.Listener.ChangeAvatarPet(int petIndex = 0)
	{
		AddTask(new WebServiceTask<NShared.ChangePetAvatar.Request, NShared.ChangePetAvatar.Response>(new NShared.ChangePetAvatar.Request
		{
			PetIndex = petIndex,
			AvatarPetIndex = petIndex
		}, delegate(NShared.ChangePetAvatar.Request request, NShared.ChangePetAvatar.Response response)
		{
			if (response == null)
			{
				return base.TaskFactory.Run(delegate
				{
					MessagePopup.OpenError(NShared.ChangePetAvatar.ResultType.Fail);
				});
			}
			if (response.BaseResult != BaseResultType.Success)
			{
				return base.TaskFactory.Run(delegate
				{
					MessagePopup.OpenError(response.BaseResult);
				});
			}
			return (response.Result != NShared.ChangePetAvatar.ResultType.Success) ? base.TaskFactory.Run(delegate
			{
				MessagePopup.OpenError(response.Result);
			}) : base.TaskFactory.Sequence(base.TaskFactory.Run(delegate
			{
				GlobalAutoBehaviour<AccountManager>.instance.PetManager.Apply(response.PetMiscInfos);
			}));
		}));
	}

	void HeroDetailView.Listener.OnTryBattle(HeroDetailView sender)
	{
		HeroDetailView.TryBattle(sender.HeroInfo);
		LobbyScene2.ListReturnState.Add(new SelectAvatarHeroTryBattleReturnState());
	}

	public void RestoreHeroDetailViewForSelectArvatarHero()
	{
		OpenMyInfoWindow();
		CostumeData data = GameRoot.Instance.CostumeDataContainer.GetData(HeroDetailView.CostumeIndex);
		if (data != null && GameRoot.Instance.CreatureDataContainer.GetData(data.HeroIndex) != null)
		{
			OpenSelectAvatarWindow(new UIAvatarHelper.AvatarInfo(data).AvatarIndex);
			CachedHeroInfo selectedHeroInfo = GetInstance<SelectAvatarHero>().GetSelectedHeroInfo();
			GetInstance<HeroDetailView>().Open(selectedHeroInfo, this);
		}
	}

	void HeroFullSceneViewBase.Listener.OnOpen()
	{
	}

	void HeroFullSceneViewBase.Listener.OnClose()
	{
	}

	void SelectCountryFlag.Listener.OnConfirmCountryFlag(string countryCode)
	{
		if (countryCode.Equals(GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetCountryFlagCode()))
		{
			return;
		}
		AddTask(new WebServiceTask<NShared.ChangeCountryCode.Request, NShared.ChangeCountryCode.Response>(new NShared.ChangeCountryCode.Request
		{
			CountryCode = countryCode
		}, (NShared.ChangeCountryCode.Request request, NShared.ChangeCountryCode.Response response) => (response.Result != NShared.ChangeCountryCode.ResultType.Success) ? base.TaskFactory.Run(delegate
		{
			MessagePopup.OpenError(response.Result);
		}) : base.TaskFactory.Sequence(base.TaskFactory.Run(delegate
		{
			GlobalAutoBehaviour<AccountManager>.instance.UserManager.SetCountryFlagIndex(countryCode);
		}), base.TaskFactory.Close<SelectCountryFlag>(), base.TaskFactory.Run(delegate(MyInfoWindow window)
		{
			window.UpdateCountryFlag();
		}))));
	}

	void SelectLanguage.Listener.OnChangeLanguage(SystemLanguage language)
	{
		if (SingletonAuto<LocalizationManager>.instance.IsValidLanguage(language))
		{
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string message = LocalizationManager.GetString(GameOption.LANGUAGE_CHANGE_CONFIRM, language);
			MessagePopup.Open(title, message, delegate
			{
				SingletonAuto<LocalizationManager>.instance.ChangeLanguage(language);
				SingletonUpdater<LoginManager>.instance.ReturnToLogin();
			}, null);
		}
	}

	void ManageBlockedUser.Listener.OnUnblock(ManageBlockedUser caller, long accountId, string userName)
	{
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string message = LocalizationManager.GetString(GameOption.UNBLOCK_USER_MESSAGE, userName);
		MessagePopup.Open(title, message, delegate
		{
			SingletonAuto<UserBlockManager>.instance.UnblockUser(accountId);
			CenterMessage.Open(LocalizationManager.GetString(GameOption.UNBLOCK_USER_MESSAGE_CENTER, userName));
			caller.RemoveBlockedUser(accountId);
		}, null);
	}

	void SkipCurrencyConfirmCountPopup.Listener.SetSkipCurrencyCount(int currencyCount)
	{
		SystemOptionData.RubyPopup_Skip_Count = currencyCount;
	}

	public void OnResponse(NGame2.NUI.NWindow.NAttendance.AttendanceReward caller, NShared.GetAttendanceReward.Response response)
	{
		if (response == null)
		{
			MessagePopup.OpenError(BaseResultType.Fail);
			return;
		}
		if (response.BaseResult != BaseResultType.Success)
		{
			MessagePopup.OpenError(response.BaseResult);
			return;
		}
		if (response.Result != NShared.GetAttendanceReward.ResultType.Success)
		{
			MessagePopup.OpenError(response.Result);
			return;
		}
		GlobalAutoBehaviour<AccountManager>.instance.AttendanceManager.Apply(response.AttendanceInfos);
		DbRewardResult rewardResult = response.AttendanceRewardResult;
		if (rewardResult != null)
		{
			GlobalAutoBehaviour<AccountManager>.instance.ApplyDbRewardResult(rewardResult);
			caller.StartReceiveEffect(delegate
			{
				BaseRewardPopup.PopupOpenData popupOpen = new BaseRewardPopup.PopupOpenData().SetTitle(caller.Title).SetItemResultInfos(rewardResult.ItemResults).SetEquipResultInfos(rewardResult.EquipItemResults)
					.SetCurrencyResultInfos3(rewardResult.CurrencyResults)
					.AddPetInfos(rewardResult.PetInfos);
				if (rewardResult.HeroInfos != null && rewardResult.HeroInfos.Length != 0)
				{
					GlobalAutoBehaviour<AccountManager>.instance.ApplyAddHero(rewardResult.HeroInfos, rewardResult.ExpResultInfos, reserveWindow: false, delegate
					{
						BaseRewardPopup.OpenPopup(popupOpen, GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpenedUI3DUI());
					});
				}
				else
				{
					BaseRewardPopup.OpenPopup(popupOpen, GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpenedUI3DUI());
				}
			});
		}
		SingletonAuto<NotificationManager>.instance.ResetLocalNotification();
	}

	public void MoveNode(int chapterIndex, int dungeonIndex)
	{
		PortalData data = GameRoot.Instance.PortalDataContainer.GetData(chapterIndex, dungeonIndex);
		string enumString = Vespa_String_Extension_Method.GetEnumString(ClientActivityType.MoveToNode);
		string[] taskValues = new string[4]
		{
			chapterIndex.ToString(),
			dungeonIndex.ToString(),
			(data?.ExecuteNodeFunction ?? false).ToString(),
			"True"
		};
		SceneAutoBehaviour<ActivityManager>.instance.Run(enumString, taskValues);
		UsePortal triggerEvent = new UsePortal
		{
			MoveToLast = false
		};
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent);
	}

	private void OpenChapterRewardPopup(NShared.RewardClearChapter.Request request, NShared.RewardClearChapter.Response response)
	{
		BaseRewardPopup.PopupOpenData popupOpenData = new BaseRewardPopup.PopupOpenData().SetTitle(LocalizationManager.GetStringDirect("CHAPTER_REWARD_POPUP_TITLE")).SetCurrencyResultInfos3(response.CurrencyResults);
		if (response.ItemResult != null)
		{
			popupOpenData.SetItemResultInfos(new ItemResultInfo[1] { response.ItemResult });
		}
		if (response.EquipItemResult != null)
		{
			popupOpenData.SetEquipResultInfos(new EquipItemInfo[1] { response.EquipItemResult });
		}
		BaseRewardPopup.OpenPopup(popupOpenData, GlobalAutoBehaviour<GlobalManagement>.instance.WindowOpenedUI3DUI());
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>.OnChanged(NGame2.NAccount.UserHeroManager.ChangeHeroInfo changedInfo)
	{
		if (changedInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.AddedHero)
		{
			CachedHeroInfo cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(changedInfo.NewValue);
			if (cachedHeroInfo == null)
			{
				return;
			}
			OpenGetNewHero(GetNewHero2.Stock.Create(cachedHeroInfo, GetNewHero2.EOpenMode.NewHero, hideInfo: false), changedInfo.ReserveWindow, null, changedInfo.OnCloseAction);
		}
		else if (changedInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.AddedHeros)
		{
			GetNewHeroSceneBundleData data = GameRoot.Instance.GetNewHeroSceneBundleDataContainer.GetData(changedInfo.NewValue);
			if (data == null)
			{
				VespaDebug.LogError("GetNewHeroBundleData is null");
				return;
			}
			GetNewHero2.OpenData openData = new GetNewHero2.OpenData();
			int[] sequenceIndices = data.GetSequenceIndices();
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(sequenceIndices); i++)
			{
				CachedHeroInfo cachedHeroInfo2 = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(sequenceIndices[i]);
				if (cachedHeroInfo2 != null)
				{
					string packCode = data.PackCode;
					string treeName = data.TreeName[i];
					bool showInfoOnHeroLeft = data.GetShowInfoOnHeroLeft(i);
					openData.Enqueue(GetNewHero2.Stock.CreateCustom(cachedHeroInfo2, GetNewHero2.EOpenMode.NewHero, hideInfo: false, packCode, treeName, showInfoOnHeroLeft));
				}
			}
			OpenGetNewHero(openData, changedInfo.ReserveWindow, null, changedInfo.OnCloseAction);
		}
		else if (changedInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.GrowthStar || changedInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.GrowthTranscended)
		{
			CachedHeroInfo heroInfo = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Get(changedInfo.NewValue);
			if (heroInfo == null)
			{
				return;
			}
			SceneAutoBehaviour<MissionManagement>.instance.MissionWindowManager.CloseMilestonePopup(immediately: true, delegate
			{
				SceneAutoBehaviour<LobbyPopupQueue>.instance.AddPopup(GetInstance<GetNewHero2>(), delegate(GetNewHero2 window)
				{
					GetNewHero2.EOpenMode openMode = ((changedInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.GrowthStar) ? GetNewHero2.EOpenMode.Awaken : GetNewHero2.EOpenMode.Transcend);
					GetNewHero2.OpenData openData2 = GetNewHero2.OpenData.CreateSingleFromHeroInfo(heroInfo, openMode, hideInfo: false);
					window.Open(openData2, null, changedInfo.OnCloseAction);
				});
			});
		}
		else if (changedInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.AlreadyGrowth)
		{
			if (SceneAutoBehaviour<LobbyPopupQueue>.instance.Empty())
			{
				SceneAutoBehaviour<MissionManagement>.instance.MissionWindowManager.CloseMilestonePopup(immediately: true, delegate
				{
					if (changedInfo.OnCloseAction != null)
					{
						string stringDirect = LocalizationManager.GetStringDirect("MISSION_ALREADY_SUBREWARD");
						Vespa_System_Action_Extension_Method.fire(changedInfo.OnCloseAction);
						BaseRewardPopup componentInChildren = GlobalUIRoot.instance.ContentsGroup.transform.GetComponentInChildren<BaseRewardPopup>();
						if (componentInChildren != null)
						{
							componentInChildren.ActiveLabel(stringDirect);
						}
						else
						{
							string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
							string stringDirect2 = LocalizationManager.GetStringDirect("MISSION_ALREADY_GROWTH");
							MessagePopup.Open(title, stringDirect2, changedInfo.OnCloseAction);
						}
					}
				});
			}
			else
			{
				SceneAutoBehaviour<LobbyPopupQueue>.instance.AddAction(delegate
				{
					SceneAutoBehaviour<MissionManagement>.instance.MissionWindowManager.CloseMilestonePopup(immediately: true, delegate
					{
						if (changedInfo.OnCloseAction != null)
						{
							string stringDirect = LocalizationManager.GetStringDirect("MISSION_ALREADY_SUBREWARD");
							Vespa_System_Action_Extension_Method.fire(changedInfo.OnCloseAction);
							BaseRewardPopup componentInChildren = GlobalUIRoot.instance.ContentsGroup.transform.GetComponentInChildren<BaseRewardPopup>();
							if (componentInChildren != null)
							{
								componentInChildren.ActiveLabel(stringDirect);
							}
							else
							{
								string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
								string stringDirect2 = LocalizationManager.GetStringDirect("MISSION_ALREADY_GROWTH");
								MessagePopup.Open(title, stringDirect2, changedInfo.OnCloseAction);
							}
						}
					});
				});
			}
		}
		if (changedInfo.ReserveWindow)
		{
			SceneAutoBehaviour<LobbyPopupQueue>.instance.ReserveMenuOpenPopup();
		}
	}

	public void OpenGetNewHero(GetNewHero2.Stock openStock, bool reserveWindow, Action onComplete, Action onClose)
	{
		if (openStock.HeroInfo == null)
		{
			VespaDebug.LogError("creature data or heroInfo is null");
		}
		else
		{
			OpenGetNewHero(GetNewHero2.OpenData.CreateSingle(openStock), reserveWindow, onComplete, onClose);
		}
	}

	private void OpenGetNewHero(GetNewHero2.OpenData openData, bool reserveWindow, Action onComplete, Action onClose)
	{
		if (openData == null || openData.IsEmpty())
		{
			VespaDebug.LogError("openData is null or empty");
		}
		else if (ClientValueManager.IsSkipGetNewHero)
		{
			Vespa_System_Action_Extension_Method.fire(onComplete);
			Vespa_System_Action_Extension_Method.fire(onClose);
		}
		else if (reserveWindow)
		{
			SceneAutoBehaviour<LobbyPopupQueue>.instance.AddPopup(GetInstance<GetNewHero2>(), delegate(GetNewHero2 window)
			{
				window.Open(openData, onComplete, onClose);
			});
		}
		else
		{
			_getNewHeroOpenQueue.Add(GetInstance<GetNewHero2>(), delegate(GetNewHero2 window)
			{
				window.Open(openData, onComplete, onClose);
			});
		}
	}

	public void PlayTurnBackSound()
	{
		LobbyTopmostMenu lobbyTopmostMenu = GetInstance<LobbyTopmostMenu>();
		if (lobbyTopmostMenu != null)
		{
			lobbyTopmostMenu.PlayTurnBackSound();
		}
	}

	public void OnChanged(UserManager.UserStatInfo userStatInfo)
	{
		checked
		{
			if (userStatInfo.UserStatType == UserManager.UserStatType.TeamLevel && userStatInfo.BeforeValue != userStatInfo.AfterValue)
			{
				long num = userStatInfo.BeforeValue;
				long num2 = userStatInfo.AfterValue;
				long num3 = num;
				OnTeamLevelUp((int)num3, (int)num2);
			}
		}
	}

	private void OnTeamLevelUp(int PrevTeamLevel, int CurTeamLevel)
	{
		SceneAutoBehaviour<LobbyPopupQueue>.instance.AddPopup(TeamLevelupPopup.instance, delegate
		{
			TeamLevelupPopup.Open(PrevTeamLevel, CurTeamLevel);
		});
	}

	public void SetOnPlayerMoveEnd(Action onMoveEnd)
	{
		if (onMoveEnd == null)
		{
			SceneAutoBehaviour<WorldMapManagement>.instance.RemoveTouchDisable();
		}
		_onMoveEnd = onMoveEnd;
	}

	public void OpenPlayerAssetMenu(LobbyTopmostMenu.IUserStatObserver observer, LobbyTopmostMenu.EventDungeonAssetInfo eventDungeonAssetInfo = null, LobbyTopmostMenu.SubStatAssetInfo subStatAssetInfo = null)
	{
		GetInstance<LobbyTopmostMenu>().OpenPlayerAssetType(observer, eventDungeonAssetInfo, subStatAssetInfo);
	}

	public void ClosePlayerAssetMenu(LobbyTopmostMenu.IUserStatObserver observer)
	{
		GetInstance<LobbyTopmostMenu>().ClosePlayerAssetType(observer);
	}

	public void OpenMiniGuideButton(LobbyTopmostMenu.IMiniGuideButtonObserver observer)
	{
		GetInstance<LobbyTopmostMenu>().OpenMiniGuideButton(observer);
	}

	public void OpenMonthlyHeroWindow(int creatureIndex = -1)
	{
		GetInstance<MonthlyHeroWindow>().Open(null, creatureIndex);
	}

	public void CloseMiniGuideButton(LobbyTopmostMenu.IMiniGuideButtonObserver observer)
	{
		GetInstance<LobbyTopmostMenu>().CloseMiniGuideButton(observer);
	}

	public void SetTopMostMenuObserver<T>(LobbyTopmostMenu.ILobbyTopmostObserver<T> observer)
	{
		GetInstance<LobbyTopmostMenu>().SetTopMostMenuObserver(observer);
	}

	public void RemoveTopMostMenuObserver<T>(LobbyTopmostMenu.ILobbyTopmostObserver<T> observer)
	{
		GetInstance<LobbyTopmostMenu>().RemoveTopMostMenuObserver(observer);
	}

	void WorldBossKillRewardPopup.Listener.OnClickViewList(WorldBossKillRewardPopup caller)
	{
		caller.Close();
	}

	void EtcMenuWindow.Listener.OnClickCalendar()
	{
		string language = SingletonAuto<LocalizationManager>.instance.Language.ToString();
		NShared.EventCalendar.Request request = new NShared.EventCalendar.Request
		{
			EventCalendarVersion = _eventCalendarVersion,
			Language = language
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, WebRequestFlag.DisableRetry, delegate(NShared.EventCalendar.Response response)
		{
			if (response != null && response.BaseResult == BaseResultType.Success && response.Result == NShared.EventCalendar.ResultType.Success)
			{
				_eventCalendarVersion = response.EventCalendarVersion;
				LoginData.EventCalendarVersion = response.NewEvent;
				if (response.EventCalendarDatas != null)
				{
					_eventCalendarDatas = response.EventCalendarDatas;
				}
				GetInstance<EventCalendarWindow>().Open(_eventCalendarDatas, null);
			}
		});
	}

	private IEnumerator coPatchNoteConfirm()
	{
		bool confirmPatchNote = false;
		GetInstance<LoginPatchNotePopup>().Open(accepted: false, delegate
		{
			confirmPatchNote = true;
		});
		while (!confirmPatchNote)
		{
			yield return null;
		}
		GetInstance<LoginPatchNotePopup>().Close();
	}

	void EtcMenuWindow.Listener.OnClickNotice()
	{
		VespaCoroutine.Start(coPatchNoteConfirm());
	}

	void EtcMenuWindow.Listener.OnClickGuide()
	{
		OpenGameGuideWindow();
	}

	void EtcMenuWindow.Listener.OnClickPlayRecord()
	{
		OpenPlayRecordWindow();
	}

	void EtcMenuWindow.Listener.OpenClassBuffMain()
	{
		SceneAutoBehaviour<ClassBuffManagement>.instance.OpenClassBuffMain(CreatureTagType.Knight);
	}

	public void OpenWebEvent(int webIndex)
	{
		if (SingletonAuto<ContentsStatusManager>.instance.GetContentsStatus(ContentsStatusType.ETCMenu, ContentsStatusSubType.EventWeb) == ContentsStatus.On)
		{
			EventWebViewPopup.OpenGlobal($"{LoginData.WebEvent.LinkUrl}{webIndex}.html{LoginData.WebEvent.QueryString}&id={webIndex}");
		}
	}

	void GuideSelect.Listener.OnSelectGuide(GameGuideType guideType)
	{
		OpenGuideMenu(guideType);
	}

	void GuideMenu.Listener.OnClickShortCut(GameGuideData data)
	{
		GuideSelect guideSelect = GetInstance<GuideSelect>();
		if (guideSelect != null && guideSelect.IsOpen())
		{
			guideSelect.Close();
		}
		SceneAutoBehaviour<ActivityManager>.instance.Run(data.MoveTaskType, data.MoveTaskValue);
	}

	void LobbyPlayGuideMenu.Listener.OnClickPlayGuide()
	{
		GetInstance<PlayGuidePopup>().Open(this, ListSortedPlayGuideInfo);
	}

	void LobbyPlayGuideMenu.Listener.MoveDirect(PlayGuideInfo playGuideinfo)
	{
		MoveDirect(playGuideinfo);
	}

	void PlayGuidePopup.Listener.MoveDirect(PlayGuideInfo playGuideinfo)
	{
		MoveDirect(playGuideinfo);
	}

	public void OpenGuideMenu(GameGuideType guideType)
	{
		GuideMenu guideMenu = GetInstance<GuideMenu>();
		guideMenu.Open(this, guideType);
		if (GetInstance<GuideSelect>().TopmostMenuType == BaseWindow.ETopmostMenuType.Tutorial_Guide)
		{
			guideMenu.SetTutorialGuideWindow();
		}
	}

	public void OpenGuidebyIndex(int idx)
	{
		GetInstance<GuideMenu>().Open(this, idx);
	}

	public void OpenHeroDetailView(CachedHeroInfo heroinfo)
	{
		GetInstance<HeroDetailView>().OpenOnlyViewMotion(heroinfo, this);
	}

	public void OpenClassBuffPopup()
	{
		if (NGame2.NAccount.UserClassBuffManager.ReservedFirstOpen || ClassBuffWrapper.IsFirstOpenStorage)
		{
			return;
		}
		BottomButtonData menu = GameRoot.Instance.BottomButtonDataContainer.GetData(ERightMenuButton.ClassBuff);
		if (menu == null || !SingletonAuto<MenuOpenCheckManager>.instance.IsOpen(ERightMenuButton.ClassBuff))
		{
			return;
		}
		NGame2.NAccount.UserClassBuffManager.ReservedFirstOpen = true;
		SceneAutoBehaviour<LobbyPopupQueue>.instance.AddPopup(GetInstance<ContentsOpenPopup>(), delegate(ContentsOpenPopup window)
		{
			ClassBuffManagement classBuffManagement = SceneAutoBehaviour<ClassBuffManagement>.instance;
			if (!(classBuffManagement == null))
			{
				ClassBuffWrapper.IsFirstOpenStorage = true;
				window.Open(menu.ContentsOpenCode, delegate
				{
					classBuffManagement.AddClassBuffPoint();
				});
			}
		}, PopupQueue.PopupFlag.DefaultPopupFlag);
	}

	public static string LocalizeContentsName(ContentsOpenConditionData data)
	{
		if (data == null)
		{
			return "";
		}
		if (String_Extension_Method.IsNullOrEmpty(data.OpenTextResorce))
		{
			VespaDebug.LogError($"ContentsOpenConditionTable에 OpenTextResorce를 추가 해주세요 : {data.Index} {data.ContentsType}");
			return "";
		}
		int num = data.OpenLocalKeyVariable.Length;
		string text = "";
		string[] array = new string[num];
		for (int i = 0; i < num; i++)
		{
			text = data.OpenLocalKeyVariable[i];
			if (!String_Extension_Method.IsNullOrEmpty(text))
			{
				array[i] = LocalizationManager.GetStringDirect(text);
			}
		}
		string openLocalKey = data.OpenLocalKey;
		object[] args = array;
		return LocalizationManager.GetStringDirect(openLocalKey, args);
	}

	private void ShowContentsOpenPopup(ContentsOpenConditionData data)
	{
		SceneAutoBehaviour<LobbyPopupQueue>.instance.AddPopup(GetInstance<ContentsOpenPopup>(), delegate(ContentsOpenPopup window)
		{
			window.Open(data.OpenTextResorce, LocalizeContentsName(data));
		}, PopupQueue.PopupFlag.DefaultPopupFlag);
	}

	void IUserObserver<DropBonusEventManager.DropBonusEventChangeInfo>.OnChanged(DropBonusEventManager.DropBonusEventChangeInfo changedInfo)
	{
	}

	public void ClearListPlayGuideInfo()
	{
		GlobalAutoBehaviour<AccountManager>.instance.ListPlayGuideInfo.Clear();
	}

	private void InitPlayGuideInfo()
	{
		if (ListPlayGuideInfo != null && ListPlayGuideInfo.Count <= 0)
		{
			ClearListPlayGuideInfo();
			foreach (PlayGuideData allDatum in GameRoot.Instance.PlayGuideDataContainer.GetAllData())
			{
				AddPlayGuideInfo(allDatum, PlayGuideStatus.NOT_ENOUGH);
			}
			SetPlayGuideStatus();
		}
		ShowPlayGuideMenu(showLobbyMenu: false);
	}

	public void SetPlayGuideStatus()
	{
		foreach (PlayGuideInfo item in ListPlayGuideInfo)
		{
			item.Status = CheckPlayGuideStatus(item.PlayGuideData, item.Status);
		}
	}

	public void AddPlayGuideInfo(PlayGuideData data, PlayGuideStatus status)
	{
		if (data != null)
		{
			ListPlayGuideInfo.Add(new PlayGuideInfo(data, status));
		}
	}

	public void SetPlayGuideStatus(PlayGuideInfo info, PlayGuideStatus status)
	{
		if (info == null)
		{
			return;
		}
		foreach (PlayGuideInfo item in ListPlayGuideInfo)
		{
			if (item != null && item.PlayGuideData != null && item.PlayGuideData.Index == info.PlayGuideData.Index)
			{
				item.Status = status;
				break;
			}
		}
	}

	public bool IsShowPlayGuideMenuByStatus()
	{
		foreach (PlayGuideInfo item in ListPlayGuideInfo)
		{
			if (item != null && (item.Status == PlayGuideStatus.ENOUGH || item.Status == PlayGuideStatus.SHOWN || item.Status == PlayGuideStatus.MOVED_DIRECT))
			{
				return true;
			}
		}
		return false;
	}

	public PlayGuideStatus CheckPlayGuideStatus(PlayGuideData playGuideData, PlayGuideStatus currentStatus)
	{
		if (playGuideData != null && (playGuideData.TutorialIndex == 0 || (playGuideData.TutorialIndex != 0 && GlobalAutoBehaviour<AccountManager>.instance.TutorialManager != null && GlobalAutoBehaviour<AccountManager>.instance.TutorialManager.IsComplete(playGuideData.TutorialIndex))))
		{
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(playGuideData.Condition1) && !SingletonAuto<NGame2.NUtil.ConditionChecker>.instance.Check(playGuideData.Condition1))
			{
				return PlayGuideStatus.NOT_ENOUGH;
			}
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(playGuideData.Condition2) && !SingletonAuto<NGame2.NUtil.ConditionChecker>.instance.Check(playGuideData.Condition2))
			{
				return PlayGuideStatus.NOT_ENOUGH;
			}
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(playGuideData.Condition3) && !SingletonAuto<NGame2.NUtil.ConditionChecker>.instance.Check(playGuideData.Condition3))
			{
				return PlayGuideStatus.NOT_ENOUGH;
			}
			if (currentStatus == PlayGuideStatus.SHOWN)
			{
				return currentStatus;
			}
			return PlayGuideStatus.ENOUGH;
		}
		return PlayGuideStatus.NOT_ENOUGH;
	}

	public void OpenLobbyChapterReward()
	{
		LobbyChapterClearReward lobbyChapterClearReward = GetInstance<LobbyChapterClearReward>();
		if (!(lobbyChapterClearReward == null))
		{
			lobbyChapterClearReward.Open(this);
			EnableLobbyChapterReward();
		}
	}

	public void SetLobbyChapterRewardInfo()
	{
		LobbyChapterClearReward lobbyChapterClearReward = GetInstance<LobbyChapterClearReward>();
		if (!(lobbyChapterClearReward == null))
		{
			lobbyChapterClearReward.SetChapterRewardInfo();
		}
	}

	public void EnableLobbyChapterReward()
	{
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(chapterIndex);
		if (data != null)
		{
			bool isEnable = data.MainField && GlobalAutoBehaviour<GlobalManagement>.instance.ShowLobbyMenu();
			EnableLobbyChapterReward(isEnable);
		}
	}

	private void EnableLobbyChapterReward(bool isEnable)
	{
		LobbyChapterClearReward lobbyChapterClearReward = GetInstance<LobbyChapterClearReward>();
		if (!(lobbyChapterClearReward == null))
		{
			UIHelper.EnableGameObject(lobbyChapterClearReward.transform, isEnable);
		}
	}

	private void RequestChapterClearReward(int chapterIndex, int step, Action responseComplete)
	{
		NShared.RewardClearChapter.Request request = new NShared.RewardClearChapter.Request
		{
			ChapterIndex = chapterIndex,
			Step = step
		};
		SingletonUpdater<WebServiceRequester>.instance.Request(request, delegate(NShared.RewardClearChapter.Response response)
		{
			if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else if (response.Result != NShared.RewardClearChapter.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.ChapterRewardManager.Set(response.RewardInfo);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResult);
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.EquipItemResult);
				GlobalAutoBehaviour<AccountManager>.instance.ApplyAddHero(response.HeroAddResult);
				OpenChapterRewardPopup(request, response);
				if (responseComplete != null)
				{
					Vespa_System_Action_Extension_Method.fire(responseComplete);
				}
			}
		});
	}

	void LobbyChapterClearReward.IListener.GetReward(int chapterIndex, int step, Action responseComplete)
	{
		if (GameRoot.Instance.ChapterClearRewardDataContainer.GetData(new ChapterClearRewardKey(chapterIndex, step)) != null)
		{
			RequestChapterClearReward(chapterIndex, step, responseComplete);
		}
	}

	public void OpenLobbyShakmehBossGauge()
	{
		NGame2.NUI.NManager.ShakmehDungeonManagement shakmehDungeonManagement = SceneAutoBehaviour<NGame2.NUI.NManager.ShakmehDungeonManagement>.instance;
		if (!(shakmehDungeonManagement == null) && shakmehDungeonManagement.IsOpenShakmehDungeon)
		{
			LobbyShakmehBossGauge lobbyShakmehBossGauge = GetInstance<LobbyShakmehBossGauge>();
			if (!(lobbyShakmehBossGauge == null))
			{
				long getCurrBossPoint = shakmehDungeonManagement.ShakmehDungeonEnterData.GetCurrBossPoint;
				int getMaxPoint = shakmehDungeonManagement.ShakmehDungeonEnterData.GetMaxPoint;
				lobbyShakmehBossGauge.Open(getCurrBossPoint, getCurrBossPoint, getMaxPoint);
				EnableLobbyShakmehBossGauge();
			}
		}
	}

	public void EnableLobbyShakmehBossGauge()
	{
		LobbyShakmehBossGauge lobbyShakmehBossGauge = GetInstance<LobbyShakmehBossGauge>();
		if (lobbyShakmehBossGauge == null)
		{
			return;
		}
		NGame2.NUI.NManager.ShakmehDungeonManagement shakmehDungeonManagement = SceneAutoBehaviour<NGame2.NUI.NManager.ShakmehDungeonManagement>.instance;
		if (shakmehDungeonManagement == null)
		{
			UIHelper.EnableGameObject(lobbyShakmehBossGauge, enable: false);
			return;
		}
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		if (chapterIndex != _prevCheckedChatperIndex)
		{
			_prevCheckedChatperIndex = chapterIndex;
			if (!shakmehDungeonManagement.IsShakmehChatper(chapterIndex))
			{
				UIHelper.EnableGameObject(lobbyShakmehBossGauge, enable: false);
			}
			else
			{
				UIHelper.EnableGameObject(lobbyShakmehBossGauge, shakmehDungeonManagement.IsOpenShakmehDungeon);
			}
		}
	}

	void MyInfoWindow.Listener.OnChangeAvatar()
	{
		OpenSelectAvatarWindow();
	}

	void MyInfoWindow.Listener.OnChangeAvatarPet()
	{
		OpenSelectAvatarPetWindow();
	}

	void MyInfoWindow.Listener.OpenPetHouseWindow()
	{
		SceneAutoBehaviour<PetManagement>.instance.OpenPetHouse();
	}

	void MyInfoWindow.Listener.OnChangeCountryFlag()
	{
		string countryFlagCode = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetCountryFlagCode();
		OpenSelectCountryFlagWindow(countryFlagCode);
	}

	void SelectAvatarPet.Listener.OpenPetHouseManagement()
	{
		AddTask(base.TaskFactory.Run(delegate(MyInfoWindow window)
		{
			window.Close();
		}), base.TaskFactory.Run(delegate(SelectAvatarPet window)
		{
			window.Close();
		}), base.TaskFactory.Run(delegate
		{
			SceneAutoBehaviour<PetManagement>.instance.OpenPetHouse(EPetHouseObjectType.Management);
		}));
	}

	public void OpenVideo(string videoName)
	{
		if (!String_Extension_Method.IsNullOrEmpty(GameRoot.Instance.VideoDataContainer.GetVideoPath(videoName)))
		{
			VideoManager.Open_VideoCode(videoName);
		}
		else
		{
			VideoManager.Open_Path(videoName);
		}
	}

	public void OpenURLVideo(string url)
	{
		VideoManager.OpenURL(url);
	}

	public void StopVideo()
	{
		if (SingletonUpdater<VideoManager>.isValidInstance)
		{
			VideoManager.SkipToClose(null);
		}
	}

	private void EnableMainQuestGuide()
	{
		if (!(Time.time - _lastCheckClearMainQuest < 1f))
		{
			_lastCheckClearMainQuest = Time.time;
			MainQuestGuideMenu mainQuestGuideMenu = GetInstance<MainQuestGuideMenu>(openedOnly: true);
			if (!(mainQuestGuideMenu == null) && mainQuestGuideMenu.IsOpen() && !EnableMainQuest())
			{
				mainQuestGuideMenu.Close();
			}
		}
	}

	private bool CheckIsClearMainQuestLastStep()
	{
		if (!AccountManager.IsLogined())
		{
			return false;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.QuestManager == null)
		{
			return false;
		}
		MainQuestInfo mainQuestInfo = GlobalAutoBehaviour<AccountManager>.instance.QuestManager.MainQuestInfo;
		if (mainQuestInfo == null)
		{
			return false;
		}
		return GameRoot.Instance.MainQuestDataContainer.IsClearLastStep(mainQuestInfo.Step);
	}

	private bool EnableMainQuest()
	{
		if (!OnMainStoryMode())
		{
			return false;
		}
		if (CheckIsClearMainQuestLastStep())
		{
			return false;
		}
		if (!NGame2.NAccount.Helper.CheckRestriction("MainQuestLabelOpenCondition"))
		{
			return false;
		}
		return true;
	}

	void MainQuestGuideMenu.IListener.Move(string moveTaskType, string[] moveTaskValue)
	{
		SceneAutoBehaviour<MissionManagement>.instance.Move(moveTaskType, moveTaskValue);
	}

	void IUserObserver<MainQuestInfo>.OnChanged(MainQuestInfo changedInfo)
	{
		SceneAutoBehaviour<LobbyPopupQueue>.instance.ReservedMainQuestClearPopup();
		SceneAutoBehaviour<LobbyPopupQueue>.instance.ReservedContentsOpenConditionPopup();
	}

	void IUserObserver<SubQuestInfo>.OnChanged(SubQuestInfo changedInfo)
	{
		SceneAutoBehaviour<LobbyPopupQueue>.instance.ReserveSubQuestClearPopup(clearQuestList: false);
		SceneAutoBehaviour<LobbyPopupQueue>.instance.ReserveMainSubQuestOpenPopup();
	}

	public void OpenCreatureIntro(CreatureData data, Action completeAction = null, Action closeAction = null)
	{
		if (data != null)
		{
			CreatureIntro creatureIntro = GetInstance<CreatureIntro>();
			if (!(creatureIntro == null))
			{
				creatureIntro.Open(data, completeAction, closeAction);
			}
		}
	}

	private IEnumerator CoProcessingUpdateDispatchBattleInfoMenuVisible()
	{
		LobbyTopmostMenu menuUI = GetInstance<LobbyTopmostMenu>();
		WaitForSeconds waitSec = new WaitForSeconds(0.5f);
		while (true)
		{
			yield return new WaitUntil(() => menuUI != null && GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager != null);
			menuUI.ShowDispatchBattleMenu(GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager.Count() > 0);
			yield return waitSec;
		}
	}

	public void EnableEffectFinishDispatchBattle(UserDispatchBattleInfoManager.Observer.BroadCastReason reason = UserDispatchBattleInfoManager.Observer.BroadCastReason.OnUpdate, WCL_DispatchBattleInfo info = null)
	{
		LobbyTopmostMenu lobbyTopmostMenu = GetInstance<LobbyTopmostMenu>();
		if (!(lobbyTopmostMenu == null))
		{
			bool flag = GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager.IsCompleteDispatchBattle();
			if (reason == UserDispatchBattleInfoManager.Observer.BroadCastReason.OnRemove)
			{
				flag = GlobalAutoBehaviour<AccountManager>.instance.DispatchBattleInfoManager.IsCompleteDispatchBattle(info);
			}
			flag = CheckEnableEffectFinishByOtehr() && flag;
			lobbyTopmostMenu.EnableEffectFinishDispatchBattle(flag);
		}
	}

	private bool CheckEnableEffectFinishByOtehr()
	{
		BoosterInfoList boosterInfoList = GetInstance<BoosterInfoList>();
		if (boosterInfoList == null || boosterInfoList.BoosterInfoListGrid == null)
		{
			return false;
		}
		ContentUIDispatchBattleOnGoingList contentUIDispatchBattleOnGoingList = GetInstance<ContentUIDispatchBattleOnGoingList>();
		if (contentUIDispatchBattleOnGoingList == null)
		{
			return false;
		}
		if (UIHelper.IsEnabledGameObjet(boosterInfoList.BoosterInfoListGrid))
		{
			return false;
		}
		if (contentUIDispatchBattleOnGoingList.IsShowing())
		{
			return false;
		}
		return true;
	}

	private void OpenGuildAttendancePopupInGuildPlace()
	{
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		CampaignChapterData data = GameRoot.Instance.CampaignChapterDataContainer.GetData(chapterIndex);
		if (data != null && data.Type == ChapterType.City)
		{
			IList<CampaignNodeData> allChapterData = GameRoot.Instance.CampaignNodeDataContainer.GetAllChapterData(chapterIndex);
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(allChapterData) && allChapterData.ToList().Find((CampaignNodeData campaignNodeData) => campaignNodeData.FunctionButton == (HashString)NodeFunctionType.City.ToString()) != null)
			{
				OpenGuildAttendancePopup();
			}
		}
	}

	public void OpenGuildAttendancePopup()
	{
		if (RestoredPortal.AllowAutoGuildAttendance() && GlobalAutoBehaviour<GuildManager>.instance.CheckGuildAttendanceRewardable)
		{
			SceneAutoBehaviour<LobbyPopupQueue>.instance.AddAction(delegate
			{
				SceneAutoBehaviour<GuildManagement2>.instance.OpenGuildAttendancePopup(isReserve: true);
			}, null, PopupQueue.PopupFlag.DefaultPopupFlag);
		}
	}

	void ContentUIDispatchBattleOnGoingList.IListener.EnableList()
	{
		EnableEffectFinishDispatchBattle();
	}

	void IUserObserver<UserDispatchBattleInfoManager.Observer.BroadCastInfo>.OnChanged(UserDispatchBattleInfoManager.Observer.BroadCastInfo changedInfo)
	{
		EnableEffectFinishDispatchBattle(changedInfo.Reason, changedInfo.Info);
	}

	private void OpenAttendancePopup()
	{
		AttendanceIntegrationWindow attendanceIntegrationWindow = GetInstance<AttendanceIntegrationWindow>();
		if (!(attendanceIntegrationWindow == null))
		{
			attendanceIntegrationWindow.OpenWindow();
		}
	}

	void LeftMenu.Listener.OnClickQuestButton(EMissionType missionType)
	{
		switch (missionType)
		{
		case EMissionType.Event:
		{
			List<MissionCategoryData> webEventMissionList = GameRoot.Instance.MissionCategoryDataContainer.GetWebEventMissionList();
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(webEventMissionList); j++)
			{
				MissionCategoryData missionCategoryData2 = webEventMissionList[j];
				SceneAutoBehaviour<MissionManagement>.instance.MissionWindowManager.OpenMissionMain(missionCategoryData2.MainCategoryIndex);
			}
			break;
		}
		case EMissionType.Special:
		case EMissionType.Special2:
		case EMissionType.Special3:
		{
			List<MissionCategoryData> specialMissionList = GameRoot.Instance.MissionCategoryDataContainer.GetSpecialMissionList();
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(specialMissionList); i++)
			{
				MissionCategoryData missionCategoryData = specialMissionList[i];
				if (missionCategoryData.EqualMissionType(missionType) && !SingletonAuto<MissionStatus>.instance.IsMissionTimeout(missionCategoryData))
				{
					SceneAutoBehaviour<MissionManagement>.instance.MissionWindowManager.OpenMissionMain(missionCategoryData.MainCategoryIndex);
				}
			}
			break;
		}
		default:
			SceneAutoBehaviour<MissionManagement>.instance.MissionWindowManager.OpenMissionMain();
			break;
		}
	}

	void LeftMenu.Listener.OnClickAttendanceButton()
	{
		if (GlobalAutoBehaviour<GuildManager>.isValidInstance && !GlobalAutoBehaviour<GuildManager>.instance.CheckIsGuild)
		{
			OpenAttendancePopup();
			return;
		}
		VespaCoroutine.Start(SceneAutoBehaviour<GuildManagement2>.instance.CoRequestGetGuildAttendance(delegate
		{
			OpenAttendancePopup();
		}, isReserve: false));
	}

	void LeftMenu.Listener.OnClickMissionButton(MissionComponentData data)
	{
		SceneAutoBehaviour<MissionManagement>.instance.MissionWindowManager.OpenMissionMain();
		SceneAutoBehaviour<MissionManagement>.instance.MissionWindowManager.OpenMissionPopup(data);
	}

	void LeftMenu.Listener.OnClickSpecialMissionButton(MissionCategoryData data)
	{
		if (data == null)
		{
			VespaDebug.LogError("data is null");
		}
		else if (!data.IsAnySpecialMission())
		{
			VespaDebug.LogError("data is Not Special Mission");
		}
		else if (SingletonAuto<MissionStatus>.instance.IsMissionTimeout(data))
		{
			SingletonAuto<MissionStatus>.instance.OpenTimeoutPopup(null);
		}
		else
		{
			SceneAutoBehaviour<MissionManagement>.instance.MissionWindowManager.OpenMissionMain(data.MainCategoryIndex);
		}
	}

	void NGame2.NUI.NWindow.RightAwayProductMenu.Listener.InitRightAwayProductInfo(Action onSuccess, Action onFail)
	{
		InitRightAwayProductInfo(onSuccess, onFail);
	}

	void NGame2.NUI.NWindow.RightAwayProductMenu.Listener.OnClickBanner(int productIndex, Action onSuccess)
	{
		NShared.GetRightAwayProductInfo.Request request = new NShared.GetRightAwayProductInfo.Request
		{
			ProductIndex = productIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetRightAwayProductInfo.Response response)
		{
			if (response.BaseResult == BaseResultType.Success && response.Result == NShared.GetRightAwayProductInfo.ResultType.Success)
			{
				SceneAutoBehaviour<PayShopManagement>.instance.OepnRightAwayProductInfoPopup(response.PayShopProductInfo);
				Vespa_System_Action_Extension_Method.fire(onSuccess);
			}
		});
	}

	void GachaSelectShopMenu.IListener.OnClick()
	{
		OpenGachaSelectShopPopup();
	}

	private void OpenGachaSelectShopPopup()
	{
		SceneAutoBehaviour<PayShopManagement>.instance.OpenGachaSelectShopPopup();
	}

	private void RequestGetGachaSelect()
	{
	}

	void PunishmentRuneCraftWindow.IListener.OnCraftPunishmentRune(int voidRuneIndex, int legendRuneIndex, int[] materialRuneIndices)
	{
		PunishmentRuneData data = GameRoot.Instance.PunishmentRuneDataContainer.GetData(voidRuneIndex, legendRuneIndex);
		NShared.CraftPunishmentRune.Request request = new NShared.CraftPunishmentRune.Request
		{
			PunishmentRuneIndex = data.PunishmentRuneIndex,
			IngredientRuneIndices = materialRuneIndices
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.CraftPunishmentRune.Response response)
		{
			if (response.BaseResult != BaseResultType.Success || response.Result != NShared.CraftPunishmentRune.ResultType.Success)
			{
				if (response.Result == NShared.CraftPunishmentRune.ResultType.ContentsOff)
				{
					MessagePopup.OpenError(response.Result);
				}
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.EquipItemResult);
				PunishmentRuneOptionInfo punishmentRuneOption = response.EquipItemResult.PunishmentRuneOption;
				GetInstance<PunishmentRuneCraftResult>().Open(this, response.EquipItemResult, punishmentRuneOption);
			}
		});
	}

	void PunishmentRuneCraftWindow.IListener.OnClickHelp()
	{
		GetInstance<PunishmentRuneCraftHelpPopup>().Open();
	}

	void PunishmentRuneCraftResult.IListener.OnClickClose()
	{
		PunishmentRuneCraftWindow punishmentRuneCraftWindow = GetInstance<PunishmentRuneCraftWindow>();
		if (!punishmentRuneCraftWindow.IsOpen())
		{
			punishmentRuneCraftWindow.Open(this);
		}
		else
		{
			punishmentRuneCraftWindow.Init();
		}
	}

	public PunishmentRuneCraftWindow GetPunishmentRuneCraftWindow()
	{
		return GetInstance<PunishmentRuneCraftWindow>();
	}
}
