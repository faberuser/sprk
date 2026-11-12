using System;
using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NChatting;
using NGame2.NContents;
using NGame2.NContentsDefine;
using NGame2.NDropBonusEvent;
using NGame2.NGuild;
using NGame2.NUI.NComponent;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow.NTopMost;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public sealed class LobbyTopmostMenu : BaseWindow, IUserObserver<UserManager.UserStatInfo>, IUserObserver, IUserObserver<UserGuildManager.ConstructionResourceInfo>, IUserObserver<NGame2.NAccount.UserPlayerCurrencyManager.ChangedCurrencyInfo>, IUserObserver<ContentsStatusInfo>, BoosterInfoMenuComponent.Listener, ContentUIDispatchBattleMenuComponent.IListener, IInspectorGuiDrawer
{
	public interface ICustomStaminaObserver
	{
		StaminaType StaminaType { get; }
	}

	public interface ISubStaminaObserver : ICustomStaminaObserver
	{
		StaminaType SubStaminaType { get; }
	}

	public interface ICustomChapterObserver
	{
		int ChapterIndex { get; }
	}

	public interface IUserStatObserver
	{
		UserManager.UserStatType StatType { get; }
	}

	public interface IMiniGuideButtonObserver
	{
		int WindowGuideIndex { get; }
	}

	public enum PublicObserverType
	{
		None,
		DisableBuyShopUserStat,
		ShowGUildResource
	}

	public interface IPublicObserver
	{
	}

	public interface ILobbyTopmostObserver<T> : IPublicObserver
	{
		PublicObserverType Type { get; }

		T BaseValue { get; }
	}

	public class EventDungeonAssetInfo
	{
		public UserManager.UserStatType StatType { get; set; }

		public UserManager.UserStatType StatTypeSub { get; set; }

		public string SpriteName { get; set; }

		public string SpriteNameSub { get; set; }
	}

	public class SubStatAssetInfo
	{
		public UserManager.UserStatType StatTypeSub { get; set; }
	}

	public interface Listener
	{
		void OnClickBuyStamina();

		void OnClickBuyGold();

		void OnClickBuyGem();

		void OnClickRechargeUndergroundPrisonKey();

		void OnClickChat();

		void OnClickAvatarHero();

		void OnClickMail();

		void OnClickEtcMenu();

		void OnClickBackButton();

		void OnClickMiniGuidePopup(GameMiniGuideData miniGuideData);

		void OnClickBoosterInfoMenu();

		void RequestInitBoosterInfoList();

		void OnClickDispatchBattleMenu();
	}

	[Serializable]
	public class StaminaInfo
	{
		public GameObject Root;

		public UILabel CountLabel;

		public GameObject Button_Buy;

		public UIWidget IconRoot;

		public IconProxy IconProxy;

		public string StaminaName { get; private set; }

		public void SetupStaminaInfo(StaminaType staminaType, long curCount, long maxCount)
		{
			SetupStaminaInfo(staminaType.ToString(), curCount, maxCount);
		}

		public void SetupStaminaInfo(string staminaName, long curCount, long maxCount)
		{
			StaminaName = staminaName;
			if (IconProxy == null)
			{
				IconProxy = IconProxy.Create(IconRoot);
			}
			if (IconProxy != null)
			{
				IconProxy.SetupStamina(staminaName);
			}
			int num = 0;
			if (maxCount > num)
			{
				UIHelper.SetLabelText(CountLabel, $"{curCount}/{maxCount}");
			}
			else
			{
				UIHelper.SetLabelText(CountLabel, curCount);
			}
			ShowBuyButton(isShow: true);
			CountLabel.text = SprkWorldBossEntryText(staminaName, CountLabel.text);
		}

		public void ShowBuyButton(bool isShow)
		{
			_ = StaminaName == StaminaType.Chicken.ToString();
			UIHelper.EnableGameObject(Button_Buy, enable: false);
		}

		public void ReleaseIconProxy()
		{
			if (IconProxy != null)
			{
				FactoryBase.SafeReleaseComponent(ref IconProxy);
			}
			IconProxy = null;
		}

		private static string SprkWorldBossEntryText(string name, string original)
		{
			if (!(name == "WorldBossTicket") && !(name == "WorldBossGlobalTicket"))
			{
				return original;
			}
			return "∞";
		}
	}

	[Serializable]
	public class PlayerAssetInfo
	{
		public GameObject Root;

		public UILabel CountLabel;

		public GameObject Button_Buy;

		public GameObject MaxCapExceedWarning;

		public CurrencyTooltipComponent CurrencyTooltipComponent;

		public UIWidget IconRoot;

		public IconProxy IconProxy;

		public UserManager.UserStatType UserStateType { get; private set; }

		public void SetupStateType(UserManager.UserStatType userStateType, long value)
		{
			UserStateType = userStateType;
			if (IconProxy == null)
			{
				IconProxy = IconProxy.Create(IconRoot);
			}
			if (IconProxy != null)
			{
				IconProxy.SetupCurrencyNoBg(userStateType.ToString());
			}
			ResourceUiData.GetDisplayAssetValue(userStateType, value, out var valueStr, out var exceedMaxCap);
			UIHelper.SetLabelText(CountLabel, valueStr);
			UIHelper.EnableGameObject(MaxCapExceedWarning, exceedMaxCap);
			ShowBuyButton(isShow: true);
			UserManager.UserStatTypeAttribute userStatTypeAttribute = EnumAttributeContainer<UserManager.UserStatType, UserManager.UserStatTypeAttribute>.Get(userStateType);
			CurrencyTooltipComponent.CurrencyType = userStatTypeAttribute?.CurrencyType ?? CurrencyType.None;
		}

		public void ShowBuyButton(bool isShow)
		{
			if (UserStateType == UserManager.UserStatType.Gold)
			{
				_ = 1;
			}
			else
				_ = UserStateType == UserManager.UserStatType.Gem;
			UIHelper.EnableGameObject(Button_Buy, enable: false);
		}

		public void ReleaseIconProxy()
		{
			if (IconProxy != null)
			{
				FactoryBase.SafeReleaseComponent(ref IconProxy);
			}
			IconProxy = null;
		}
	}

	[Serializable]
	public class GuildConstructionResourceInfo
	{
		public GuildConstructionResourceTopmostMenuType MenuType;

		public GameObject Root;
	}

	[Serializable]
	public class ProfileUiData
	{
		public UIWidget Root;

		public UILabel LabelTeamLevel;

		public UILabel LabelNick;

		public UIBundleLoader BundleTexMyHero;

		public UISprite SpriteTeamExp;

		public UILabel LabelTeamExp;

		public UISprite SpriteNickNameModify;

		public UISimpleGrid PartyGrid;
	}

	[Serializable]
	public class BackButtonTitleUiData
	{
		public UIWidget Root;

		public UILabel LabelTitle;

		public GameObject MiniGuideButton;
	}

	[Serializable]
	public class ResourceUiData
	{
		private const int MaxOpenStaminaInfo = 2;

		public UIWidget Root;

		public UITable ResourceUIDataTable;

		public List<GuildConstructionResourceInfo> GuildResourceInfoList = new List<GuildConstructionResourceInfo>();

		public StaminaInfo StaminaInfo;

		public StaminaInfo SubStaminaInfo;

		public PlayerAssetInfo MainPlayerAssetInfo;

		public PlayerAssetInfo SubPlayerAssetInfo;

		public PlayerAssetInfo MainSmallPlayerAssetInfo;

		public PlayerAssetInfo SubSamllPlayerAssetInfo;

		public PlayerAssetInfo GemAssetInfo;

		public UIWidget ChickenStaminaTooltip;

		public UIWidget StaminaTooltip;

		public UIWidget StaminaTooltipWithoutTime;

		public UIWidget GemTooltip;

		public UILabel Label_StaminaTeamLevel;

		public UILabel Label_StaminaBonus;

		public UILabel Label_StaminaTotal;

		public UILabel Label_FullChickenStaminaRecoverTime;

		public UILabel Label_ChickenStaminaRecoverTime;

		public UILabel Label_FullStaminaRecoverTime;

		public UILabel Label_StaminaRecoverTime;

		public UILabel Label_PayGem;

		public UILabel Label_FreeGem;

		public int GetOpenedStaminaInfoCount
		{
			get
			{
				int num = 0;
				if (StaminaInfo != null && StaminaInfo.Root.activeSelf)
				{
					num++;
				}
				if (SubStaminaInfo != null && SubStaminaInfo.Root.activeSelf)
				{
					num++;
				}
				return num;
			}
		}

		public void SetMainStamina(StaminaType type, long value, long maxValue)
		{
			SetStamina(StaminaInfo, type, value, maxValue);
		}

		public void SetSubStamina(StaminaType type, long value, long maxValue)
		{
			SetStamina(SubStaminaInfo, type, value, maxValue);
		}

		private void SetStamina(StaminaInfo info, StaminaType type, long value, long maxValue)
		{
			bool flag = type != StaminaType.Empty;
			bool activeSelf = info.Root.activeSelf;
			if (activeSelf != flag)
			{
				UIHelper.EnableGameObject(info.Root, flag);
			}
			if (flag)
			{
				if (type == StaminaType.TreasureHouseKey || type == StaminaType.GuildArenaKey || type == StaminaType.EventDungeonGoldKey)
				{
					int num = 0;
					info.SetupStaminaInfo(type, value, num);
				}
				else
				{
					info.SetupStaminaInfo(type, value, maxValue);
				}
			}
			if (activeSelf != flag)
			{
				ResourceUIDataTable.Reposition();
			}
		}

		public void SetEventStamina(int value, int maxValue)
		{
			UIHelper.EnableGameObject(StaminaInfo.Root, enable: true);
			StaminaInfo.SetupStaminaInfo("EliteBattleIcon", value, maxValue);
		}

		public static void GetDisplayAssetValue(UserManager.UserStatType type, long value, out string valueStr, out bool exceedMaxCap)
		{
			exceedMaxCap = false;
			switch (type)
			{
			case UserManager.UserStatType.FriendshipPoint:
			{
				int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("MaxFriendshipPoint");
				valueStr = $"{value}/{valueInt}";
				return;
			}
			case UserManager.UserStatType.PunishmentRaidKey:
			{
				StaminaData data = GameRoot.Instance.StaminaDataContainer.GetData(StaminaType.PunishmentRaidKey);
				int num = ((data == null) ? 1 : int.Parse(data.MaxCountValue));
				valueStr = $"{value}/{num}";
				return;
			}
			}
			valueStr = Vespa_Long_Extension_Method.ToCommaString(value);
			if (UserManager.UserStatTypeAttribute.TryToCurrency(type, out var currencyType) && CurrencyMaxCap.ExceedMaxCap(currencyType, value))
			{
				exceedMaxCap = true;
				valueStr = Vespa_String_Extension_Method.NguiColor(valueStr, Color.red);
			}
		}

		public void SetPlayerAsset(UserManager.UserStatType type, long value, bool hidePlayerAsset)
		{
			UIHelper.EnableGameObject(MainPlayerAssetInfo.Root, enable: false);
			UIHelper.EnableGameObject(SubPlayerAssetInfo.Root, enable: false);
			UIHelper.EnableGameObject(MainSmallPlayerAssetInfo.Root, enable: false);
			UIHelper.EnableGameObject(SubSamllPlayerAssetInfo.Root, enable: false);
			if (2 <= GetOpenedStaminaInfoCount || hidePlayerAsset)
			{
				ResourceUIDataTable.Reposition();
				return;
			}
			EventDungeonAssetInfo eventDungeonAssetInfo = SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<LobbyTopmostMenu>().GetEventDungeonAssetInfo();
			if (eventDungeonAssetInfo != null && ((type != UserManager.UserStatType.None && type == eventDungeonAssetInfo.StatType) || type == eventDungeonAssetInfo.StatTypeSub) && eventDungeonAssetInfo.StatTypeSub != UserManager.UserStatType.None)
			{
				long userStat = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetUserStat(eventDungeonAssetInfo.StatType);
				long userStat2 = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetUserStat(eventDungeonAssetInfo.StatTypeSub);
				UIHelper.EnableGameObject(MainSmallPlayerAssetInfo.Root, enable: true);
				UIHelper.EnableGameObject(SubSamllPlayerAssetInfo.Root, enable: true);
				MainSmallPlayerAssetInfo.SetupStateType(eventDungeonAssetInfo.StatType, userStat);
				SubSamllPlayerAssetInfo.SetupStateType(eventDungeonAssetInfo.StatTypeSub, userStat2);
				ResourceUIDataTable.Reposition();
				return;
			}
			UIHelper.EnableGameObject(MainPlayerAssetInfo.Root, enable: true);
			MainPlayerAssetInfo.SetupStateType(type, value);
			SubStatAssetInfo subStatAssetInfo = SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<LobbyTopmostMenu>().GetSubStatAssetInfo();
			if (subStatAssetInfo != null && subStatAssetInfo.StatTypeSub != UserManager.UserStatType.None)
			{
				long currencyValue = GlobalAutoBehaviour<AccountManager>.instance.PlayerCurrencyManager.GetCurrencyValue(subStatAssetInfo.StatTypeSub);
				UIHelper.EnableGameObject(SubPlayerAssetInfo.Root, enable: true);
				SubPlayerAssetInfo.SetupStateType(subStatAssetInfo.StatTypeSub, currencyValue);
			}
			ResourceUIDataTable.Reposition();
		}

		public void SetGuildResource(GuildConstructionResourceTopmostMenuType menuType, int wood, int stone, int metal)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(GuildResourceInfoList); i++)
			{
				bool flag = GuildResourceInfoList[i].MenuType == menuType;
				UIHelper.EnableGameObject(GuildResourceInfoList[i].Root, flag);
				if (!flag || menuType != GuildConstructionResourceTopmostMenuType.ResourceComboBox)
				{
					continue;
				}
				UIComboBox component = GuildResourceInfoList[i].Root.GetComponent<UIComboBox>();
				if (!(component == null))
				{
					int currentKey = component.CurrentKey;
					component.Clear();
					component.AddItem(1, Vespa_Int_Extension_Method.ToCommaString(wood), GuildConstructionResource.Wood.ToString());
					component.AddItem(2, Vespa_Int_Extension_Method.ToCommaString(stone), GuildConstructionResource.Stone.ToString());
					component.AddItem(3, Vespa_Int_Extension_Method.ToCommaString(metal), GuildConstructionResource.Metal.ToString());
					if (currentKey == -1)
					{
						component.SelectFirstItem(isNotify: false);
					}
					else
					{
						component.SelectItem(currentKey);
					}
				}
			}
			ResourceUIDataTable.Reposition();
		}

		public void ShowBuyButton(bool isShow)
		{
			if (StaminaInfo != null)
			{
				StaminaInfo.ShowBuyButton(isShow);
			}
			if (SubStaminaInfo != null)
			{
				SubStaminaInfo.ShowBuyButton(isShow);
			}
			if (MainPlayerAssetInfo != null)
			{
				MainPlayerAssetInfo.ShowBuyButton(isShow);
			}
			if (SubPlayerAssetInfo != null)
			{
				SubPlayerAssetInfo.ShowBuyButton(isShow);
			}
			if (MainSmallPlayerAssetInfo != null)
			{
				MainSmallPlayerAssetInfo.ShowBuyButton(isShow);
			}
			if (SubSamllPlayerAssetInfo != null)
			{
				SubSamllPlayerAssetInfo.ShowBuyButton(isShow);
			}
			if (GemAssetInfo != null)
			{
				GemAssetInfo.ShowBuyButton(isShow);
			}
		}
	}

	[Serializable]
	public class ButtonUiData
	{
		public UIWidget Root;

		public GameObject ChatAlertObject;

		public UILabel ChatAlertCountLabel;

		public GameObject MailButton;

		public GameObject MailCountObject;

		public UILabel MailCountLabel;

		public GameObject EtcMenuButton;

		public GameObject EtcMenuAlertObject;
	}

	[Serializable]
	public class BoosterInfoListUiData
	{
		public UIWidget Root;

		public UISimpleGrid BoosterInfoMenuGrid;

		public BoosterInfoMenuComponent BoosterInfoMenuComp;

		public void ReleaseComponent()
		{
			if (BoosterInfoMenuComp != null)
			{
				FactoryBase.SafeReleaseComponent(ref BoosterInfoMenuComp);
			}
			BoosterInfoMenuComp = null;
		}
	}

	[Serializable]
	public class TopBarUiData
	{
		public ETopmostMenuType[] TypeList;

		public UISprite SpriteTopBar;
	}

	public interface INoticeIconUpdater
	{
		void Initialize(GameObject root);

		void UpdateShow(bool canShow);
	}

	public class GuildSuppressNoticeIconUpdater : INoticeIconUpdater
	{
		private GameRoot root;

		void INoticeIconUpdater.Initialize(GameObject root)
		{
		}

		void INoticeIconUpdater.UpdateShow(bool canShow)
		{
		}
	}

	public class TreasureHouseNoticeIconUpdater : INoticeIconUpdater
	{
		void INoticeIconUpdater.Initialize(GameObject root)
		{
		}

		void INoticeIconUpdater.UpdateShow(bool canShow)
		{
		}
	}

	[Flags]
	public enum TopmostMenuVisibleFlag
	{
		None = 0,
		Profile = 1,
		BackButtonTitle = 2,
		Resource = 4,
		Bar = 8,
		Etc = 0x10,
		ButtonUi = 0x20,
		HidePlayerAsset = 0x40,
		__MAX__ = 0x41
	}

	public const string PrefabPath = "Resources/Prefab2/Lobby/TopmostMenu";

	public ProfileUiData ProfileUi;

	public BackButtonTitleUiData BackButtonTitleUi;

	public ResourceUiData ResourceUi;

	public ButtonUiData ButtonUi;

	public BoosterInfoListUiData BoosterInfoListUi;

	public TopBarUiData[] TopBarUi;

	public PlaySoundComponent SoundComp;

	public UIWidget DispatchMenuRoot;

	private ContentUIDispatchBattleMenuComponent _dispatchMenuComp;

	private bool _isDispatchShow;

	public UISimpleGrid NoticeGrid;

	private List<ICustomStaminaObserver> _customStaminaObserverList = new List<ICustomStaminaObserver>();

	private List<IUserStatObserver> _userStatObserverList = new List<IUserStatObserver>();

	private List<IMiniGuideButtonObserver> _miniGuideButtonObserverList = new List<IMiniGuideButtonObserver>();

	private Dictionary<PublicObserverType, List<IPublicObserver>> _topMostMenuObserverList = new Dictionary<PublicObserverType, List<IPublicObserver>>();

	private readonly Dictionary<PublicObserverType, Action<List<IPublicObserver>>> _observerCallbackList = new Dictionary<PublicObserverType, Action<List<IPublicObserver>>>();

	private EventDungeonAssetInfo _eventDungeonAssetInfo;

	private SubStatAssetInfo _subStatAssetInfo;

	private Listener _listener;

	private StaminaType _currentStaminaType;

	private long _currentStamina;

	private long _currentMaxStamina;

	private StaminaType _currentSubStaminaType;

	private long _currentSubStamina;

	private long _currentSubMaxStamina;

	private int _currentOpenStaminaInfoCount;

	private int _currentRecStaminaInfoCount;

	private StaminaType _selectTooltipStamina;

	private int _currentEventStamina;

	private int _currentEventMaxStamina;

	private UserManager.UserStatType _currentStatType;

	private long _currentAsset;

	private bool _hidePlayerAsset;

	private GuildConstructionResourceTopmostMenuType _currentConstructionResoucreTopMenuType;

	private DateTime _lastBoosterCheckTime = DateTime.UtcNow;

	private DateTime _lastAlertCheckTime = DateTime.UtcNow;

	private bool _HideMail;

	private ETopmostMenuFlag _currentTopmostMenuFlag;

	private IWindow _currentPopup;

	public static float LastUpdateStaminaTooltipTime;

	private int _currentMiniGuideIndex;

	private GameMiniGuideData _currentMiniGuideData;

	private bool _isShowLobbyMenu;

	private bool isEvent;

	private List<INoticeIconChcker> _listNoticeIconChecker = new List<INoticeIconChcker>();

	[HideInInspector]
	public UILabel LabelTeamLevel;

	[HideInInspector]
	public UILabel Label_Nick;

	[HideInInspector]
	public UITexture TextureMyHero;

	[HideInInspector]
	public UISprite SpriteTeamExp;

	[HideInInspector]
	public UILabel LabelTeamExp;

	[HideInInspector]
	public GameObject ChatAlertObject;

	[HideInInspector]
	public UILabel ChatAlertCountLabel;

	[HideInInspector]
	public GameObject MailAlertObject;

	[HideInInspector]
	public GameObject MailButton;

	[HideInInspector]
	public UIWidget WidgetTitleBack;

	[HideInInspector]
	public UILabel Label_OpenedWindowTitle;

	[HideInInspector]
	public UIWidget WidgetProfile;

	[HideInInspector]
	public UISimpleGrid PartyMemberGrid;

	[HideInInspector]
	public UILabel LabelGem;

	[HideInInspector]
	public UIWidget WidgetStaminaTooltip;

	[HideInInspector]
	public UIWidget WidgetGoldTooltip;

	[HideInInspector]
	public UIWidget WidgetGemTooltip;

	private const string PATH_PREFAB_DISPATCH_MENU_COMPONENT = "Resources/Prefab2/Window/DispatchBattle/ContentUIDispatchBattleMenuComponent";

	public GameMiniGuideData CurrentMiniGuideData
	{
		get
		{
			if (_currentMiniGuideIndex == 0)
			{
				return null;
			}
			return GameRoot.Instance.GameMiniGuideDataContainer.GetData(_currentMiniGuideIndex);
		}
	}

	public EventDungeonAssetInfo GetEventDungeonAssetInfo()
	{
		return _eventDungeonAssetInfo;
	}

	public SubStatAssetInfo GetSubStatAssetInfo()
	{
		return _subStatAssetInfo;
	}

	private LobbyTopmostMenu()
	{
		_observerCallbackList.Add(PublicObserverType.DisableBuyShopUserStat, UpdateDisableBuyShopUserStat);
		_observerCallbackList.Add(PublicObserverType.ShowGUildResource, UpdateGuildResource);
	}

	protected override void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			base.OnDestroy();
			CloseNoticeIcon();
			_customStaminaObserverList.Clear();
			_userStatObserverList.Clear();
			_miniGuideButtonObserverList.Clear();
			_topMostMenuObserverList.Clear();
			_listener = null;
			_currentPopup = null;
		}
	}

	public void Open(ICustomStaminaObserver observer, IUserStatObserver statObserver, Listener listener)
	{
		base.Open();
		_listener = listener;
		OpenStaminaType(observer);
		OpenPlayerAssetType(statObserver);
		InitPlayerInfo();
		SetDisplayMode(ETopmostMenuFlag.NoWindowExist);
		GlobalAutoBehaviour<DropBonusEventManager>.instance.ObserverManager.AddObserver(this);
		GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.ResourceObserverManager.AddObserver(this);
		ResourceUi.ResourceUIDataTable.repositionNow = true;
		OpenBoosterInfoMenu();
		InitNoticeIcon();
	}

	public override void Reset()
	{
		base.Reset();
		if (ResourceUi != null)
		{
			if (ResourceUi.StaminaInfo != null)
			{
				ResourceUi.StaminaInfo.ReleaseIconProxy();
			}
			if (ResourceUi.MainPlayerAssetInfo != null)
			{
				ResourceUi.MainPlayerAssetInfo.ReleaseIconProxy();
			}
			if (ResourceUi.SubPlayerAssetInfo != null)
			{
				ResourceUi.SubPlayerAssetInfo.ReleaseIconProxy();
			}
			if (ResourceUi.MainSmallPlayerAssetInfo != null)
			{
				ResourceUi.MainSmallPlayerAssetInfo.ReleaseIconProxy();
			}
			if (ResourceUi.SubSamllPlayerAssetInfo != null)
			{
				ResourceUi.SubSamllPlayerAssetInfo.ReleaseIconProxy();
			}
			if (ResourceUi.GemAssetInfo != null)
			{
				ResourceUi.GemAssetInfo.ReleaseIconProxy();
			}
		}
		if (BoosterInfoListUi != null)
		{
			BoosterInfoListUi.ReleaseComponent();
		}
		SingletonAuto<ContentsStatusManager>.instance.ObserverManager.RemoveObserver(this);
		GlobalAutoBehaviour<DropBonusEventManager>.instance.ObserverManager.RemoveObserver(this);
		GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.ResourceObserverManager.RemoveObserver(this);
	}

	public override void Close()
	{
		base.Close();
	}

	public override void OnClosing()
	{
		base.OnClosing();
	}

	private void InitPlayerInfo()
	{
		UserManager userManager = GlobalAutoBehaviour<AccountManager>.instance.UserManager;
		if (userManager != null)
		{
			SetAvatarHero(userManager.AvatarHeroIndex);
			SetTeamLevel(userManager.TeamLevel);
			SetTeamLevelExp(userManager.TeamLevel, userManager.TeamExp);
			SetNickName(userManager.Nick);
			SetGem(userManager.TotalGem);
			UpdateStamina();
			UpdatePlayerAsset();
			UpdateMiniGuideButton();
			UpdatePublicObserver();
			UIHelper.EnableGameObject(ResourceUi.StaminaTooltip, enable: false);
		}
	}

	public void SetTeamLevel(int Level)
	{
		UIHelper.SetLabelText(ProfileUi.LabelTeamLevel, Level);
	}

	public void SetNickName(string nickname)
	{
		UIHelper.SetLabelText(ProfileUi.LabelNick, nickname);
		UIHelper.EnableGameObject(ProfileUi.SpriteNickNameModify, GlobalAutoBehaviour<AccountManager>.instance.UserManager.MiscInfo.NickChange >= 1);
	}

	public void SetTeamLevelExp(int team_level, int team_exp)
	{
		Helper.LevelInfo teamLevelInfo = Helper.GetTeamLevelInfo(team_level, team_exp);
		if (teamLevelInfo.IsMaxLevel)
		{
			UIHelper.SetSpriteFillAmount(ProfileUi.SpriteTeamExp, 1f);
			UIHelper.SetLabelText(ProfileUi.LabelTeamExp, "MAX");
		}
		else
		{
			UIHelper.SetSpriteFillAmount(ProfileUi.SpriteTeamExp, teamLevelInfo.ExpPercent);
			UIHelper.SetLabelText(ProfileUi.LabelTeamExp, Vespa_Float_Extension_Method.ToExpString(teamLevelInfo.ExpPercent * 100f));
		}
	}

	public void SetGold(long Gold)
	{
	}

	public void SetGem(long Gem)
	{
		if (ResourceUi.GemAssetInfo != null)
		{
			ResourceUi.GemAssetInfo.SetupStateType(UserManager.UserStatType.Gem, Gem);
		}
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		if (AccountManager.IsLogined())
		{
			UpdateStamina();
			UpdateStaminaTooltip(_selectTooltipStamina);
			string backButtonTitle;
			ETopmostMenuType topmostMenuType = GlobalAutoBehaviour<GlobalManagement>.instance.GetTopmostMenuType(out backButtonTitle);
			TopmostMenuVisibleFlag topmostMenuVisibleFlag = GetTopmostMenuVisibleFlag(topmostMenuType);
			UpdatePlayerAsset(topmostMenuVisibleFlag);
			UpdateAlert();
			UpdateTopmostMenuType(backButtonTitle, topmostMenuType, topmostMenuVisibleFlag);
			UpdateMiniGuideButton();
			UpdatePublicObserver();
			UpdateEventStamina();
		}
	}

	private void UpdateTopmostMenuType(string title, ETopmostMenuType topmostMenuType, TopmostMenuVisibleFlag topmostMenuVisibleFlag)
	{
		bool flag = (topmostMenuVisibleFlag & TopmostMenuVisibleFlag.Profile) != 0;
		bool flag2 = (topmostMenuVisibleFlag & TopmostMenuVisibleFlag.BackButtonTitle) != 0;
		bool flag3 = (topmostMenuVisibleFlag & TopmostMenuVisibleFlag.Resource) != 0;
		bool isBarVisible = (topmostMenuVisibleFlag & TopmostMenuVisibleFlag.Bar) != 0;
		bool enable = (topmostMenuVisibleFlag & TopmostMenuVisibleFlag.Etc) != 0;
		bool enable2 = (topmostMenuVisibleFlag & TopmostMenuVisibleFlag.ButtonUi) != 0;
		UIHelper.EnableGameObject(ProfileUi.Root, flag);
		SetOpenedWindowTitle(flag2 ? title : string.Empty);
		bool num = UIHelper.IsEnabledGameObjet(ResourceUi.Root);
		UIHelper.EnableGameObject(ResourceUi.Root, flag3);
		if (!num && flag3)
		{
			ResourceUi.ResourceUIDataTable.Reposition();
		}
		UIHelper.EnableGameObject(ButtonUi.Root, enable2);
		UpdateTopBar(topmostMenuType, isBarVisible);
		UIHelper.EnableGameObject(ButtonUi.EtcMenuButton, enable);
		UpdateBoosterInfoMenu(flag && !flag2);
		UpdateDispatchMenu(flag && !flag2);
		UpdateNoticeIconChecker(flag && !flag2);
	}

	public void SetDisplayMode(ETopmostMenuFlag topmostMenuFlag)
	{
		if (topmostMenuFlag != _currentTopmostMenuFlag)
		{
			_currentTopmostMenuFlag = topmostMenuFlag;
			UIHelper.EnableGameObject(ProfileUi.Root, (topmostMenuFlag & ETopmostMenuFlag.ShowProfile) != 0);
			UIHelper.EnableGameObject(ProfileUi.PartyGrid, (topmostMenuFlag & ETopmostMenuFlag.ShowParty) != 0);
		}
	}

	public void OpenStaminaType(ICustomStaminaObserver observer)
	{
		isEvent = false;
		if (!_customStaminaObserverList.Contains(observer))
		{
			_customStaminaObserverList.Add(observer);
		}
	}

	public void CloseStaminaType(ICustomStaminaObserver observer)
	{
		_customStaminaObserverList.Remove(observer);
	}

	public void UpdateEventStamina()
	{
		if (isEvent)
		{
			SetEventStamina(_currentEventStamina, _currentEventMaxStamina);
		}
	}

	public void UpdateStamina()
	{
		StaminaType staminaType = StaminaType.None;
		StaminaType subStamina = StaminaType.Empty;
		int reqChapterIndex = 0;
		if (_customStaminaObserverList.Count > 0)
		{
			staminaType = _customStaminaObserverList[_customStaminaObserverList.Count - 1].StaminaType;
			staminaType = ((staminaType == StaminaType.None) ? StaminaType.Chicken : staminaType);
			if (_customStaminaObserverList[_customStaminaObserverList.Count - 1] is ICustomChapterObserver customChapterObserver)
			{
				reqChapterIndex = customChapterObserver.ChapterIndex;
			}
			if (_customStaminaObserverList[_customStaminaObserverList.Count - 1] is ISubStaminaObserver subStaminaObserver)
			{
				subStamina = subStaminaObserver.SubStaminaType;
			}
		}
		if (staminaType == StaminaType.None || staminaType == StaminaType.Gold || staminaType == StaminaType.Gem)
		{
			staminaType = StaminaType.Chicken;
		}
		SetMainStamina(staminaType, reqChapterIndex);
		SetSubStamina(subStamina);
	}

	public static void UpdateChickenStaminaTimeInfo(UIWidget widgetTooltip, UILabel labelTeamLevel, UILabel labelBonus, UILabel labelTotal, UILabel Label_FullChickenStaminaRecoverTime, UILabel Label_ChickenStaminaRecoverTime, StaminaType staminaType)
	{
		if (UIHelper.IsEnabledGameObjet(widgetTooltip) && !(Time.time - LastUpdateStaminaTooltipTime < 1f))
		{
			UIHelper.EnableGameObject(labelTeamLevel, enable: true);
			UIHelper.EnableGameObject(labelBonus, enable: true);
			UIHelper.EnableGameObject(labelTotal, enable: true);
			int num = 0;
			int bonusStamina = 0;
			int num2 = 0;
			StaminaData data = GameRoot.Instance.StaminaDataContainer.GetData(staminaType);
			int teamLevel = GlobalAutoBehaviour<AccountManager>.instance.UserManager.UserInfo.TeamLevel;
			Dictionary<string, object> dictionary = JsonMarshaler.EncodeJsonObject(GameRoot.Instance.TeamLevelDataContainer.GetData(teamLevel));
			if (dictionary != null && dictionary.ContainsKey(data.MaxCountValue))
			{
				num = BaseJsonMarshaler.ToInt(dictionary[data.MaxCountValue].ToString());
			}
			Array.ForEach(GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfos(), delegate(CachedHeroInfo e)
			{
				bonusStamina += e.CreatureData.AddStamina;
			});
			Dictionary<StaminaType, int> activeStaminaBuffDictionary = GlobalAutoBehaviour<AccountManager>.instance.GetActiveStaminaBuffDictionary();
			int value = 0;
			if (activeStaminaBuffDictionary.TryGetValue(StaminaType.Chicken, out value))
			{
				bonusStamina += value;
			}
			int teamLevelBuffBonusValue = GlobalAutoBehaviour<TeamLevelBuffManager>.instance.GetTeamLevelBuffBonusValue(EventBonusType.AddStamina);
			bonusStamina += teamLevelBuffBonusValue;
			num2 = num + bonusStamina;
			string stringDirect = LocalizationManager.GetStringDirect("TOOLTIP_INFO_CURRENT_LEVEL_STAMINA", num);
			UIHelper.SetLabelText(labelTeamLevel, stringDirect);
			stringDirect = LocalizationManager.GetStringDirect("TOOLTIP_INFO_BONUS_STAMINA", bonusStamina);
			UIHelper.SetLabelText(labelBonus, stringDirect);
			stringDirect = LocalizationManager.GetStringDirect("TOOLTIP_INFO_TOTAL_STAMINA", num2);
			UIHelper.SetLabelText(labelTotal, stringDirect);
			LastUpdateStaminaTooltipTime = Time.time;
			string FullStaminaRechargeRemainTime = string.Empty;
			string OneStaminaRechargeRemainTime = string.Empty;
			GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetCurStaminaTimeInfo(staminaType, out FullStaminaRechargeRemainTime, out OneStaminaRechargeRemainTime);
			UIHelper.SetLabelText(Label_FullChickenStaminaRecoverTime, FullStaminaRechargeRemainTime);
			UIHelper.SetLabelText(Label_ChickenStaminaRecoverTime, OneStaminaRechargeRemainTime);
		}
	}

	public static void UpdateStaminaTimeInfo(UIWidget widgetTooltip, UILabel Label_FullStaminaRecoverTime, UILabel Label_StaminaRecoverTime, StaminaType staminaType)
	{
		if (UIHelper.IsEnabledGameObjet(widgetTooltip) && !(Time.time - LastUpdateStaminaTooltipTime < 1f))
		{
			LastUpdateStaminaTooltipTime = Time.time;
			string FullStaminaRechargeRemainTime = string.Empty;
			string OneStaminaRechargeRemainTime = string.Empty;
			if (GlobalAutoBehaviour<AccountManager>.instance.KeyTypeDungeonManager.IsKeyTypeDungeon(staminaType))
			{
				GlobalAutoBehaviour<AccountManager>.instance.KeyTypeDungeonManager.GetKeyTooltipInfo(staminaType, out FullStaminaRechargeRemainTime, out OneStaminaRechargeRemainTime);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetCurStaminaTimeInfo(staminaType, out FullStaminaRechargeRemainTime, out OneStaminaRechargeRemainTime);
			}
			UIHelper.SetLabelText(Label_FullStaminaRecoverTime, FullStaminaRechargeRemainTime);
			UIHelper.SetLabelText(Label_StaminaRecoverTime, OneStaminaRechargeRemainTime);
		}
	}

	private void SetMainStamina(StaminaType staminaType, int reqChapterIndex)
	{
		int num = 0;
		long currentValue = num;
		int num2 = 0;
		long maxValue = num2;
		GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetCurrentStamina(staminaType, out currentValue, out maxValue, reqChapterIndex);
		if (staminaType != _currentStaminaType || currentValue != _currentStamina || maxValue != _currentMaxStamina)
		{
			SetMainStaminaUI(staminaType, currentValue, maxValue);
		}
	}

	private void SetMainStaminaUI(StaminaType type, long stamina, long maxStamina)
	{
		_currentStaminaType = type;
		_currentStamina = stamina;
		_currentMaxStamina = maxStamina;
		ResourceUi.SetMainStamina(type, stamina, maxStamina);
		_currentOpenStaminaInfoCount = ResourceUi.GetOpenedStaminaInfoCount;
	}

	private void SetSubStamina(StaminaType staminaType)
	{
		int num = 0;
		long currentValue = num;
		int num2 = 0;
		long maxValue = num2;
		GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetCurrentStamina(staminaType, out currentValue, out maxValue);
		if (staminaType != _currentSubStaminaType || currentValue != _currentSubStamina || maxValue != _currentSubMaxStamina)
		{
			SetSubStaminaUI(staminaType, currentValue, maxValue);
			_currentOpenStaminaInfoCount = ResourceUi.GetOpenedStaminaInfoCount;
		}
	}

	private void SetSubStaminaUI(StaminaType type, long stamina, long maxStamina)
	{
		_currentSubStaminaType = type;
		_currentSubStamina = stamina;
		_currentSubMaxStamina = maxStamina;
		ResourceUi.SetSubStamina(type, stamina, maxStamina);
	}

	private void SetPayGem()
	{
		UIHelper.SetLabelText(ResourceUi.Label_PayGem, GlobalAutoBehaviour<AccountManager>.instance.UserManager.PayGem);
		UIHelper.SetLabelText(ResourceUi.Label_FreeGem, GlobalAutoBehaviour<AccountManager>.instance.UserManager.Gem);
	}

	public void SetEventStamina(int stamina, int maxStamina)
	{
		isEvent = true;
		_currentStaminaType = StaminaType.None;
		_currentStamina = stamina;
		_currentEventStamina = stamina;
		_currentMaxStamina = maxStamina;
		_currentEventMaxStamina = maxStamina;
		ResourceUi.SetEventStamina(stamina, maxStamina);
	}

	public void OnShowStaminaTooltip()
	{
		_selectTooltipStamina = _currentStaminaType;
		UIWidget tooltipWidget = ShowStaminaTooltip(_selectTooltipStamina);
		RepositionTooltip(tooltipWidget, ResourceUi.StaminaInfo.Root.transform);
	}

	public void OnShowSubStaminaTooltip()
	{
		_selectTooltipStamina = _currentSubStaminaType;
		UIWidget tooltipWidget = ShowStaminaTooltip(_selectTooltipStamina);
		RepositionTooltip(tooltipWidget, ResourceUi.SubStaminaInfo.Root.transform);
	}

	private void RepositionTooltip(UIWidget tooltipWidget, Transform staminaTm)
	{
		if (!(tooltipWidget == null) && !(staminaTm == null))
		{
			tooltipWidget.transform.position = Vespa_Unity_Vector3_Extension_Method.ReplaceX(tooltipWidget.transform.position, staminaTm.position.x);
		}
	}

	private void UpdateStaminaTooltip(StaminaType staminaType)
	{
		switch (staminaType)
		{
		case StaminaType.Empty:
		case StaminaType.None:
		case StaminaType.GuildSuppressKey:
			break;
		case StaminaType.Chicken:
			UpdateChickenStaminaTimeInfo(ResourceUi.ChickenStaminaTooltip, ResourceUi.Label_StaminaTeamLevel, ResourceUi.Label_StaminaBonus, ResourceUi.Label_StaminaTotal, ResourceUi.Label_FullChickenStaminaRecoverTime, ResourceUi.Label_ChickenStaminaRecoverTime, staminaType);
			break;
		default:
			UpdateStaminaTimeInfo(ResourceUi.StaminaTooltip, ResourceUi.Label_FullStaminaRecoverTime, ResourceUi.Label_StaminaRecoverTime, staminaType);
			break;
		}
	}

	private UIWidget ShowStaminaTooltip(StaminaType staminaType)
	{
		UIWidget uIWidget = null;
		switch (staminaType)
		{
		case StaminaType.Chicken:
			UIHelper.EnableGameObject(ResourceUi.ChickenStaminaTooltip, enable: true);
			UpdateChickenStaminaTimeInfo(ResourceUi.ChickenStaminaTooltip, ResourceUi.Label_StaminaTeamLevel, ResourceUi.Label_StaminaBonus, ResourceUi.Label_StaminaTotal, ResourceUi.Label_FullChickenStaminaRecoverTime, ResourceUi.Label_ChickenStaminaRecoverTime, staminaType);
			return ResourceUi.ChickenStaminaTooltip;
		case StaminaType.GuildSuppressKey:
		case StaminaType.GuildArenaKey:
			UIHelper.EnableGameObject(ResourceUi.StaminaTooltipWithoutTime, enable: true);
			return ResourceUi.StaminaTooltipWithoutTime;
		default:
			UIHelper.EnableGameObject(ResourceUi.StaminaTooltip, enable: true);
			UpdateStaminaTimeInfo(ResourceUi.StaminaTooltip, ResourceUi.Label_FullStaminaRecoverTime, ResourceUi.Label_StaminaRecoverTime, staminaType);
			return ResourceUi.StaminaTooltip;
		}
	}

	public void OnHideStaminaTooltip()
	{
		_selectTooltipStamina = StaminaType.None;
		UIHelper.EnableGameObject(ResourceUi.ChickenStaminaTooltip, enable: false);
		UIHelper.EnableGameObject(ResourceUi.StaminaTooltipWithoutTime, enable: false);
		UIHelper.EnableGameObject(ResourceUi.StaminaTooltip, enable: false);
	}

	private bool IsPaidRubyPrint()
	{
		return LobbyManagement.GetCountryServerData()?.IsPaidRubyPrint ?? false;
	}

	public void OnShowGemTooltip()
	{
		if (IsPaidRubyPrint())
		{
			SetPayGem();
			UIHelper.EnableGameObject(ResourceUi.GemTooltip, enable: true);
		}
		else if (ResourceUi.GemAssetInfo.CurrencyTooltipComponent != null)
		{
			ResourceUi.GemAssetInfo.CurrencyTooltipComponent.OnShowTooltip();
		}
	}

	public void OnHideGemTooltip()
	{
		if (IsPaidRubyPrint())
		{
			UIHelper.EnableGameObject(ResourceUi.GemTooltip, enable: false);
		}
		else if (ResourceUi.GemAssetInfo.CurrencyTooltipComponent != null)
		{
			ResourceUi.GemAssetInfo.CurrencyTooltipComponent.OnHideTooltip();
		}
	}

	public void OpenPlayerAssetType(IUserStatObserver observer, EventDungeonAssetInfo eventDungeonAssetInfo = null, SubStatAssetInfo subStatAssetInfo = null)
	{
		if (!_userStatObserverList.Contains(observer))
		{
			_userStatObserverList.Add(observer);
		}
		if (eventDungeonAssetInfo != null)
		{
			_eventDungeonAssetInfo = eventDungeonAssetInfo;
		}
		if (subStatAssetInfo != null)
		{
			_subStatAssetInfo = subStatAssetInfo;
		}
	}

	public void ClosePlayerAssetType(IUserStatObserver observer)
	{
		_userStatObserverList.Remove(observer);
		_subStatAssetInfo = null;
		UserManager.UserStatType statType = observer.StatType;
		if (statType == UserManager.UserStatType.EventDungeonDailyPoint || statType == UserManager.UserStatType.EventDungeonPoint || statType == UserManager.UserStatType.EventDungeonPoint2 || statType == UserManager.UserStatType.EventDungeonPoint3 || statType == UserManager.UserStatType.EventGiftPoint)
		{
			_eventDungeonAssetInfo = null;
		}
	}

	public void UpdatePlayerAsset(TopmostMenuVisibleFlag topmostMenuVisibleFlag = TopmostMenuVisibleFlag.None)
	{
		bool flag = (topmostMenuVisibleFlag & TopmostMenuVisibleFlag.HidePlayerAsset) != 0;
		UserManager.UserStatType userStatType = UserManager.UserStatType.None;
		if (_userStatObserverList.Count > 0)
		{
			userStatType = _userStatObserverList[_userStatObserverList.Count - 1].StatType;
			userStatType = ((userStatType == UserManager.UserStatType.None) ? UserManager.UserStatType.Gold : userStatType);
		}
		if (userStatType == UserManager.UserStatType.None || userStatType == UserManager.UserStatType.Gem)
		{
			userStatType = UserManager.UserStatType.Gold;
		}
		long userStat = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetUserStat(userStatType);
		if (userStatType != _currentStatType || userStat != _currentAsset || _currentOpenStaminaInfoCount != _currentRecStaminaInfoCount || _hidePlayerAsset != flag)
		{
			SetPlayerAsset(userStatType, userStat, flag);
		}
	}

	private void SetPlayerAsset(UserManager.UserStatType type, long assetValue, bool hidePlayerAsset = false)
	{
		_currentStatType = type;
		_currentAsset = assetValue;
		_currentRecStaminaInfoCount = _currentOpenStaminaInfoCount;
		_hidePlayerAsset = hidePlayerAsset;
		ResourceUi.SetPlayerAsset(type, assetValue, hidePlayerAsset);
	}

	private void SetGuildResource(GuildConstructionResourceTopmostMenuType menuType, int wood, int stone, int metal)
	{
		_currentConstructionResoucreTopMenuType = menuType;
		ResourceUi.SetGuildResource(menuType, wood, stone, metal);
	}

	public void OpenMiniGuideButton(IMiniGuideButtonObserver observer)
	{
		if (!_miniGuideButtonObserverList.Contains(observer))
		{
			_miniGuideButtonObserverList.Add(observer);
		}
	}

	public void CloseMiniGuideButton(IMiniGuideButtonObserver observer)
	{
		_miniGuideButtonObserverList.Remove(observer);
	}

	public void UpdateMiniGuideButton()
	{
		int num = 0;
		if (_miniGuideButtonObserverList.Count > 0)
		{
			num = _miniGuideButtonObserverList[_miniGuideButtonObserverList.Count - 1].WindowGuideIndex;
		}
		if (num != _currentMiniGuideIndex)
		{
			SetMiniGuideButton(num);
		}
	}

	private void SetMiniGuideButton(int miniGuideIndex)
	{
		_currentMiniGuideIndex = miniGuideIndex;
		UIHelper.EnableGameObject(BackButtonTitleUi.MiniGuideButton, miniGuideIndex != 0);
	}

	public void SetTopMostMenuObserver<T>(ILobbyTopmostObserver<T> observer)
	{
		if (!_topMostMenuObserverList.ContainsKey(observer.Type))
		{
			_topMostMenuObserverList.Add(observer.Type, new List<IPublicObserver>());
		}
		List<IPublicObserver> list = _topMostMenuObserverList[observer.Type];
		if (!list.Contains(observer))
		{
			list.Add(observer);
		}
	}

	public void RemoveTopMostMenuObserver<T>(ILobbyTopmostObserver<T> observer)
	{
		if (_topMostMenuObserverList.ContainsKey(observer.Type))
		{
			List<IPublicObserver> list = _topMostMenuObserverList[observer.Type];
			if (list.Contains(observer))
			{
				list.Remove(observer);
			}
		}
	}

	private void UpdatePublicObserver()
	{
		foreach (KeyValuePair<PublicObserverType, Action<List<IPublicObserver>>> observerCallback in _observerCallbackList)
		{
			PublicObserverType key = observerCallback.Key;
			Action<List<IPublicObserver>> value = observerCallback.Value;
			if (!_topMostMenuObserverList.ContainsKey(key))
			{
				Vespa_System_Action_Extension_Method.fire(value, null);
				continue;
			}
			List<IPublicObserver> i_argA = _topMostMenuObserverList[key];
			Vespa_System_Action_Extension_Method.fire(value, i_argA);
		}
	}

	private void UpdateDisableBuyShopUserStat(List<IPublicObserver> observerList = null)
	{
		bool flag = false;
		if (__Extension_Method_ICollection__.COUNT(observerList) > 0)
		{
			flag = (Vespa_Generic_IList_Extension_Method.GetLast(observerList) as ILobbyTopmostObserver<DisableBuyShopUserStatObserverValue>).BaseValue.Value;
		}
		ResourceUi.ShowBuyButton(!flag);
	}

	private void UpdateGuildResource(List<IPublicObserver> observerList = null)
	{
		GuildConstructionResourceTopmostMenuType guildConstructionResourceTopmostMenuType = GuildConstructionResourceTopmostMenuType.Hide;
		if (__Extension_Method_ICollection__.COUNT(observerList) > 0)
		{
			guildConstructionResourceTopmostMenuType = (Vespa_Generic_IList_Extension_Method.GetLast(observerList) as ILobbyTopmostObserver<GuildResourceObserverValue>).BaseValue.Value;
		}
		if (_currentStaminaType != StaminaType.Empty)
		{
			guildConstructionResourceTopmostMenuType = GuildConstructionResourceTopmostMenuType.Hide;
		}
		if (_currentConstructionResoucreTopMenuType == guildConstructionResourceTopmostMenuType)
		{
			return;
		}
		int num;
		int num2;
		if (GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager != null)
		{
			num = ((GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info != null) ? 1 : 0);
			if (num != 0)
			{
				num2 = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Wood;
				goto IL_0071;
			}
		}
		else
		{
			num = 0;
		}
		num2 = 0;
		goto IL_0071;
		IL_0071:
		int wood = num2;
		int stone = ((num != 0) ? GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Stone : 0);
		int metal = ((num != 0) ? GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info.Metal : 0);
		SetGuildResource(guildConstructionResourceTopmostMenuType, wood, stone, metal);
	}

	private void SetAvatarHero(int heroIndex)
	{
		string profileIconPath = SprkLobbyAvatar.GetProfileIconPath(heroIndex);
		UIHelper.SetBundleTexture(ProfileUi.BundleTexMyHero, profileIconPath);
		SprkLobbyAvatar.WatchProfileIcon(this);
	}

	private void UpdateTopBar(ETopmostMenuType topmostMenuType, bool isBarVisible)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(TopBarUi); i++)
		{
			TopBarUiData topBarUiData = TopBarUi[i];
			if (topBarUiData != null)
			{
				bool flag = __Extension_Method_ICollection__.COUNT(topBarUiData.TypeList) > 0 && topBarUiData.TypeList.Contains(topmostMenuType);
				UIHelper.EnableGameObject(topBarUiData.SpriteTopBar, isBarVisible && flag);
			}
		}
	}

	private void OpenBoosterInfoMenu()
	{
		BoosterInfoListUi.BoosterInfoMenuComp = UIHelper.AddGridItem(BoosterInfoListUi.BoosterInfoMenuGrid, BoosterInfoMenuComponent.PrefabAuto);
		BoosterInfoListUi.BoosterInfoMenuComp.Open(this);
	}

	public void SetShowLobbyMenu(bool showLobbyMenu)
	{
		_isShowLobbyMenu = showLobbyMenu;
	}

	public void SetBoosterInfoMenu(int totalBoosterInfoCount, int newBoosterInfoCount, bool isOpenBoosterInfoList)
	{
		if (BoosterInfoListUi.BoosterInfoMenuComp == null)
		{
			UIHelper.EnableGameObject(BoosterInfoListUi.Root, enable: false);
		}
		else if (totalBoosterInfoCount > 0)
		{
			UIHelper.EnableGameObject(BoosterInfoListUi.Root, enable: true);
			if (isOpenBoosterInfoList)
			{
				BoosterInfoListUi.BoosterInfoMenuComp.SetEffectPlay(play: true);
			}
			bool newSprite = !isOpenBoosterInfoList && newBoosterInfoCount > 0;
			BoosterInfoListUi.BoosterInfoMenuComp.SetNewSprite(newSprite);
		}
		else
		{
			UIHelper.EnableGameObject(BoosterInfoListUi.Root, enable: false);
			BoosterInfoListUi.BoosterInfoMenuComp.SetEffectPlay(play: false);
		}
	}

	public void SetNewBoosterInfoNotify(bool visible)
	{
		if (BoosterInfoListUi.BoosterInfoMenuComp != null)
		{
			BoosterInfoListUi.BoosterInfoMenuComp.SetNewSprite(visible);
		}
	}

	private void UpdateBoosterInfoMenu(bool visible)
	{
		if (_isShowLobbyMenu && visible && !SceneAutoBehaviour<SubStoryManagement>.instance.OnSubStoryMap)
		{
			UIHelper.EnableGameObject(BoosterInfoListUi.Root, enable: true);
		}
		else
		{
			UIHelper.EnableGameObject(BoosterInfoListUi.Root, enable: false);
		}
		if (UIHelper.IsEnabledGameObjet(BoosterInfoListUi.Root) && BoosterInfoListUi.BoosterInfoMenuComp != null)
		{
			BoosterInfoListUi.BoosterInfoMenuComp.SetEffectPlay(play: true);
		}
	}

	private void UpdateDispatchMenu(bool visible)
	{
		if (_isDispatchShow)
		{
			if (_isShowLobbyMenu && visible && !SceneAutoBehaviour<SubStoryManagement>.instance.OnSubStoryMap)
			{
				UIHelper.EnableGameObject(DispatchMenuRoot, enable: true);
			}
			else
			{
				UIHelper.EnableGameObject(DispatchMenuRoot, enable: false);
			}
			if (_dispatchMenuComp != null)
			{
				_dispatchMenuComp.SetEffectPlay(play: true);
			}
		}
	}

	void BoosterInfoMenuComponent.Listener.OnClickBoosterInfoMenu()
	{
		if (_listener != null)
		{
			_listener.OnClickBoosterInfoMenu();
		}
	}

	void BoosterInfoMenuComponent.Listener.RequestInitBoosterInfoList()
	{
		if (_listener != null)
		{
			_listener.RequestInitBoosterInfoList();
		}
	}

	public void OnClickButton_BuyStamina()
	{
		if (!ShowTutorialCenterMessage() && _listener != null)
		{
			_listener.OnClickBuyStamina();
		}
	}

	public void OnClickButton_BuyGold()
	{
		if (!ShowTutorialCenterMessage() && _listener != null)
		{
			_listener.OnClickBuyGold();
		}
	}

	public void OnClickButton_BuyGem()
	{
		if (!ShowTutorialCenterMessage() && _listener != null)
		{
			_listener.OnClickBuyGem();
		}
	}

	[UIEvent(AllowDuringPatchDownloading = true)]
	public void OnClickButton_Chat()
	{
		if (!ShowTutorialCenterMessage() && _listener != null)
		{
			_listener.OnClickChat();
		}
	}

	public void OnClickAvatarHero()
	{
		if (_listener != null)
		{
			_listener.OnClickAvatarHero();
		}
	}

	[UIEvent(AllowDuringPatchDownloading = true)]
	public void OnClickButton_Mail()
	{
		if (!ShowTutorialCenterMessage() && _listener != null)
		{
			_listener.OnClickMail();
		}
	}

	public void OnClickButton_EtcMenu()
	{
		if (_listener != null)
		{
			_listener.OnClickEtcMenu();
		}
	}

	public void OnClickButton_MiniGuideMenu()
	{
		if (CurrentMiniGuideData != null && _listener != null)
		{
			_listener.OnClickMiniGuidePopup(CurrentMiniGuideData);
		}
	}

	public void OnClickPartyMember()
	{
	}

	private void SetChatAlert(int count)
	{
		UIHelper.EnableGameObject(ButtonUi.ChatAlertObject, count > 0);
		UIHelper.SetLabelText(ButtonUi.ChatAlertCountLabel, count);
	}

	private void UpdateAlert()
	{
		DateTime utcNow = DateTime.UtcNow;
		if (!((utcNow - _lastAlertCheckTime).TotalSeconds < 0.5))
		{
			_lastAlertCheckTime = utcNow;
			int totalAlertCount = GlobalAutoBehaviour<ChattingManager>.instance.GetTotalAlertCount(refresh: true);
			SetChatAlert(totalAlertCount);
			SetMailCount(MailManagement.MailCount);
		}
	}

	public void UpdateEtcMenuAlert(bool enable)
	{
		ButtonUi.EtcMenuAlertObject.SetActive(enable);
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	public void OnChanged(UserManager.UserStatInfo userStatInfo)
	{
		OnChangePlayerAsset(userStatInfo.UserStatType, userStatInfo.AfterValue);
		checked
		{
			switch (userStatInfo.UserStatType)
			{
			case UserManager.UserStatType.Nick:
				SetNickName(userStatInfo.AfterValue3);
				break;
			case UserManager.UserStatType.Gem:
			case UserManager.UserStatType.PayGem:
			{
				long totalGem = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TotalGem;
				SetGem(totalGem);
				break;
			}
			case UserManager.UserStatType.AvatarHeroIndex:
			{
				long num3 = userStatInfo.AfterValue;
				SetAvatarHero((int)num3);
				break;
			}
			case UserManager.UserStatType.TeamLevel:
			{
				long num2 = userStatInfo.AfterValue;
				SetTeamLevel((int)num2);
				break;
			}
			case UserManager.UserStatType.TeamExp:
			{
				int team_level = unchecked((int)userStatInfo.AfterValue2);
				long num = userStatInfo.AfterValue;
				SetTeamLevelExp(team_level, (int)num);
				break;
			}
			}
		}
	}

	private void OnChangePlayerAsset(UserManager.UserStatType statType, long AfterValue)
	{
		switch (statType)
		{
		case UserManager.UserStatType.Gold:
		case UserManager.UserStatType.PvpCoin:
		case UserManager.UserStatType.RoyalPoint:
		case UserManager.UserStatType.GuildPoint:
		case UserManager.UserStatType.EventDungeonPoint:
		case UserManager.UserStatType.EventDungeonPoint2:
		case UserManager.UserStatType.GloryPoint:
			SetPlayerAsset(statType, AfterValue);
			break;
		}
	}

	void IUserObserver<UserGuildManager.ConstructionResourceInfo>.OnChanged(UserGuildManager.ConstructionResourceInfo changedInfo)
	{
		OnChangeGuildConstructionResource(changedInfo);
	}

	private void OnChangeGuildConstructionResource(UserGuildManager.ConstructionResourceInfo info)
	{
		SetGuildResource(GuildConstructionResourceTopmostMenuType.ResourceComboBox, info.Wood, info.Stone, info.Metal);
	}

	void IUserObserver<NGame2.NAccount.UserPlayerCurrencyManager.ChangedCurrencyInfo>.OnChanged(NGame2.NAccount.UserPlayerCurrencyManager.ChangedCurrencyInfo changedInfo)
	{
		OnChangePlayerAsset(changedInfo.currencyType, changedInfo.Amount);
	}

	private void OnChangePlayerAsset(CurrencyType currencyType, long AfterValue)
	{
		UserManager.UserStatType userStatType = UserManager.UserStatTypeAttribute.ToUserStatType(currencyType);
		if ((uint)(userStatType - 42) <= 1u)
		{
			SetPlayerAsset(userStatType, AfterValue);
		}
	}

	private void SetMailCount(int count)
	{
		bool enable = count > 0;
		if (_HideMail)
		{
			enable = false;
		}
		UIHelper.EnableGameObject(ButtonUi.MailCountObject, enable);
		UIHelper.SetLabelText(ButtonUi.MailCountLabel, count);
	}

	public void SetOpenedWindowTitle(string title)
	{
		if (String_Extension_Method.IsNullOrEmpty(title))
		{
			UIHelper.EnableGameObject(BackButtonTitleUi.Root, enable: false);
			return;
		}
		UIHelper.EnableGameObject(BackButtonTitleUi.Root, enable: true);
		UIHelper.SetLabelText(BackButtonTitleUi.LabelTitle, title);
	}

	[UIEvent(AllowDuringPatchDownloading = true)]
	public void OnClickBackButton()
	{
		if (_listener != null)
		{
			_listener.OnClickBackButton();
		}
	}

	public void PlayTurnBackSound()
	{
		if (SoundComp != null)
		{
			SoundComp.Play();
		}
	}

	public void OnClickRechargeUndergroundPrisonKey()
	{
		if (_listener != null)
		{
			_listener.OnClickRechargeUndergroundPrisonKey();
		}
	}

	public bool ShowTutorialCenterMessage()
	{
		string backButtonTitle;
		ETopmostMenuType topmostMenuType = GlobalAutoBehaviour<GlobalManagement>.instance.GetTopmostMenuType(out backButtonTitle);
		if (topmostMenuType == ETopmostMenuType.Tutorial || topmostMenuType == ETopmostMenuType.Tutorial_Guide)
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("TUTORIAL_RAIDPARTYSETTING_3"));
			return true;
		}
		return false;
	}

	private void InitNoticeIcon()
	{
		SingletonAuto<ContentsStatusManager>.instance.ObserverManager.AddObserver(this);
		_listNoticeIconChecker.Add(new GuidSuppressNoticeIconChecker());
		_listNoticeIconChecker.Add(new TreasureHouseNoticeIconChecker());
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listNoticeIconChecker); i++)
		{
			_listNoticeIconChecker[i]?.Init(NoticeGrid);
		}
	}

	private void CloseNoticeIcon()
	{
		SingletonAuto<ContentsStatusManager>.instance.ObserverManager.RemoveObserver(this);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listNoticeIconChecker); i++)
		{
			_listNoticeIconChecker[i]?.Close();
		}
	}

	private void UpdateNoticeIconChecker(bool show)
	{
		show = _isShowLobbyMenu && show && !SceneAutoBehaviour<SubStoryManagement>.instance.OnSubStoryMap;
		bool flag = false;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listNoticeIconChecker); i++)
		{
			INoticeIconChcker noticeIconChcker = _listNoticeIconChecker[i];
			if (noticeIconChcker != null)
			{
				flag |= noticeIconChcker.Check(show);
			}
		}
		if (flag)
		{
			UIHelper.RepositionGrid(NoticeGrid);
		}
	}

	private void UpdateNoticeIconContentsStatus()
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listNoticeIconChecker); i++)
		{
			_listNoticeIconChecker[i]?.CheckContentsStatus();
		}
	}

	void IUserObserver<ContentsStatusInfo>.OnChanged(ContentsStatusInfo changedInfo)
	{
		if (changedInfo != null)
		{
			UpdateNoticeIconContentsStatus();
		}
	}

	public void ShowDispatchBattleMenu(bool isShow)
	{
		isShow = RestoredPortal.DispatchShortcutVisible(isShow);
		if (DispatchMenuRoot == null || _isDispatchShow == isShow)
		{
			return;
		}
		_isDispatchShow = isShow;
		UIHelper.EnableGameObject(DispatchMenuRoot, _isDispatchShow);
		if (_isDispatchShow && _dispatchMenuComp == null)
		{
			ContentUIDispatchBattleMenuComponent contentUIDispatchBattleMenuComponent = new PrefabAuto<ContentUIDispatchBattleMenuComponent>("Resources/Prefab2/Window/DispatchBattle/ContentUIDispatchBattleMenuComponent").Factory.Create(DispatchMenuRoot.transform);
			if (!(contentUIDispatchBattleMenuComponent == null))
			{
				_dispatchMenuComp = contentUIDispatchBattleMenuComponent;
				_dispatchMenuComp.Open(this);
			}
		}
	}

	public void EnableEffectFinishDispatchBattle(bool enable)
	{
		if (!(_dispatchMenuComp == null))
		{
			_dispatchMenuComp.EnableEffectFinish(enable);
		}
	}

	public void OnClickDispatchBattleMenu()
	{
		if (RestoredPortal.DispatchListHasEntries() && _listener != null)
		{
			_listener.OnClickDispatchBattleMenu();
		}
	}

	void ContentUIDispatchBattleMenuComponent.IListener.OnClick(ContentUIDispatchBattleMenuComponent comp)
	{
		OnClickDispatchBattleMenu();
	}

	private TopmostMenuVisibleFlag GetTopmostMenuVisibleFlag(ETopmostMenuType menuType)
	{
		TopmostMenuVisibleFlag topmostMenuVisibleFlag = TopmostMenuVisibleFlag.None;
		switch (menuType)
		{
		case ETopmostMenuType.HideAll:
			return TopmostMenuVisibleFlag.None;
		case ETopmostMenuType.ResourceOnly:
			return TopmostMenuVisibleFlag.Resource | TopmostMenuVisibleFlag.ButtonUi;
		case ETopmostMenuType.BackButtonTitle:
		case ETopmostMenuType.BackButtonTitleShop:
			return TopmostMenuVisibleFlag.BackButtonTitle | TopmostMenuVisibleFlag.Resource | TopmostMenuVisibleFlag.Bar | TopmostMenuVisibleFlag.ButtonUi;
		case ETopmostMenuType.NakedBackButtonTitle:
			return TopmostMenuVisibleFlag.BackButtonTitle | TopmostMenuVisibleFlag.Resource | TopmostMenuVisibleFlag.ButtonUi;
		case ETopmostMenuType.Tutorial:
			return TopmostMenuVisibleFlag.Resource;
		case ETopmostMenuType.Tutorial_Guide:
			return TopmostMenuVisibleFlag.BackButtonTitle | TopmostMenuVisibleFlag.Resource | TopmostMenuVisibleFlag.Bar | TopmostMenuVisibleFlag.ButtonUi;
		case ETopmostMenuType.BackButtonOnly:
			return TopmostMenuVisibleFlag.BackButtonTitle;
		case ETopmostMenuType.SoulWeapon:
			return TopmostMenuVisibleFlag.BackButtonTitle | TopmostMenuVisibleFlag.Resource | TopmostMenuVisibleFlag.Bar | TopmostMenuVisibleFlag.ButtonUi;
		case ETopmostMenuType.MissionMain:
			return TopmostMenuVisibleFlag.BackButtonTitle | TopmostMenuVisibleFlag.Resource | TopmostMenuVisibleFlag.ButtonUi;
		case ETopmostMenuType.Punishment:
			return TopmostMenuVisibleFlag.BackButtonTitle | TopmostMenuVisibleFlag.Resource | TopmostMenuVisibleFlag.ButtonUi | TopmostMenuVisibleFlag.HidePlayerAsset;
		default:
			return TopmostMenuVisibleFlag.Profile | TopmostMenuVisibleFlag.Resource | TopmostMenuVisibleFlag.Etc | TopmostMenuVisibleFlag.ButtonUi;
		}
	}
}
