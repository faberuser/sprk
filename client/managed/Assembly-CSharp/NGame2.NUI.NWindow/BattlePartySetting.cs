using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NContents;
using NGame2.NDispatchBattle;
using NGame2.NEventTrigger;
using NGame2.NScene;
using NGame2.NScene.NPreloader;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NBooster;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWindow.NSelectRewardPopup;
using NGameAsset;
using NShared;
using NShared.BeginCampaign;
using NShared.NEvent;
using NShared.NLocalization;
using NShared.NMessage.NProtocol;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using NVespa.NTask;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI.NWindow;

[UIEvent(AllowDuringPatchDownloading = true)]
public class BattlePartySetting : BaseWindow, IBattlePartySetting, HeroPortraitComponent.Listener, ClassTagIconComponent.Listener, ItemIconComponent.Listener, PreSelectRewardPopup.Listener, LobbyTopmostMenu.ICustomStaminaObserver, LobbyTopmostMenu.IMiniGuideButtonObserver, IBoosterListObserver, IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>, IUserObserver, IUserObserver<NGame2.NAccount.UserEquipItemManager.EquipStatInfo>, UserHeroListPanel.IListener, SkillBookSelectRewardPopup.IListener, HeroPresetStorageWebManager.ILoadPresetListener
{
	public interface Listener
	{
		void StartBattle(EnterDungeonData enterDungeon, BattlePartyInfo battlePartyInfo, ExtraOptionData extraOption);

		bool OnCloseRequest(BattlePartySetting closeThis);

		void OpenHeroDetailView(CachedHeroInfo heroInfo);

		bool OpenAutoNextBattleSelect();
	}

	public interface PreviewAnimationListener
	{
		void ActivatePreview(bool bActivate);
	}

	public interface IDataProvider
	{
		EnterDungeonData GetEnterDungeon();

		int[] GetForcedPartyMemberHeroIndices();

		int[] GetPartyMemberHeroIndices(bool isMainParty = true, int slotCount = 0);

		int GetPartyMemberHeroIndex(int slotIndex, bool isMainParty = true);

		void SetPartyMemberHeroIndex(int slotIndex, int heroIndex, bool isMainParty = true);

		EHeroState GetPartyHeroState();

		string GetStartButtonLabelText();

		StaminaType GetStaminaType();

		int GetStaminaConsumeCount();

		bool EnableOnetimeBooster();

		List<CreatureTagType> GetSelectCreatureTagTypeList();

		List<CreatureTagType> GetAcceptedCreatureTagTypeList();

		List<CreatureData> GetBannedHeroDataList();

		void ResetPartyMember();

		int[] GetAffixIndices();

		void SetAffixIndices(int[] affixIndices);

		RaidPartySetting.ClassEnterPermissionBanTypeData GetClassEnterPermissionBanTypeData();

		List<BanRuleData> GetBanRuleDataList();
	}

	public class ExtraOptionData
	{
		public bool AutoRepeat;

		public bool AutoNext;

		public bool IsEndCampaign;

		public int OnlineGameSpeedRatio = 1;

		public int EnterTicketCount;

		public bool IsContinousBattle;
	}

	public class BattlePartyInfo
	{
		public long[] accountIds;

		public int[] heroIndices;

		public int[] subHeroIndices;

		public int leaderHeroIndex;

		public StaminaType StaminaType;

		public int StaminaCount;

		public int OnetimeBoosterCount;

		public List<int> DeltaHeroIndexList;

		public List<int> ZetaHeroIndexList;

		public int[] AffixIndices;
	}

	[Serializable]
	public class StaminaInfo
	{
		public StaminaType Type;

		public GameObject Root;
	}

	[Serializable]
	public class SquardUi
	{
		public enum ESquardType
		{
			OnlyMain,
			MainSub,
			MainSubShowAll
		}

		public ESquardType Type;

		public Squard Squard;
	}

	[Serializable]
	public class OnetimeBoosterUi
	{
		public enum EOnetimeBoosterType
		{
			ShowItemIcon,
			NoShowItemICon
		}

		public EOnetimeBoosterType Type;

		public UseOnetimeBoosterPanel OnetimeBoosterPanel;
	}

	[Serializable]
	public class MiniPortraitUi
	{
		public enum EMiniPortraitType
		{
			Large,
			Small
		}

		public EMiniPortraitType Type;

		public MiniPortraitSquardPanel MiniPortraitPanel;
	}

	[Serializable]
	public class StartBattleUiData
	{
		public enum ButtonGroupType
		{
			UseResource,
			NoResource,
			WorldBoss,
			Event,
			Match
		}

		[Serializable]
		public class ButtonGroup
		{
			[SerializeField]
			public ButtonGroupType _type;

			[SerializeField]
			public UIWidget _groupRoot;

			[SerializeField]
			public UIWidget _staminaRoot;
		}

		[SerializeField]
		private UIWidget _iconRoot;

		[SerializeField]
		private IconProxy _iconProxy;

		[SerializeField]
		private UILabel _staminaCountLabel;

		[SerializeField]
		private UILabel _eventStaminaCountLabel;

		[SerializeField]
		private UIWidget _groupUseResource;

		[SerializeField]
		private UIWidget _groupNoResource;

		[SerializeField]
		private UIWidget _groupWorldBoss;

		[SerializeField]
		private UIWidget _groupWorldBoss_SetSubParty;

		[SerializeField]
		private UIWidget _groupWorldBoss_StartBattle;

		[SerializeField]
		private UILabel _buttonLabelUseResource;

		[SerializeField]
		private UILabel _buttonLabelNoResource;

		[SerializeField]
		private UIWidget _staminaRoot;

		[SerializeField]
		private UIWidget _eventStaminaRoot;

		[SerializeField]
		private ButtonGroup[] _buttonGroups;

		[HideInInspector]
		public BaseWindow callerWindow;

		public void SetButtonLabel(string text)
		{
			UIHelper.SetLabelText(_buttonLabelUseResource, text);
			UIHelper.SetLabelText(_buttonLabelNoResource, text);
		}

		public void SetEventStamina(BattleType battleType, int staminaCount)
		{
			Array.ForEach(_buttonGroups, delegate(ButtonGroup e)
			{
				UIHelper.EnableGameObject(e._staminaRoot, enable: false);
			});
			UIHelper.SetLabelText(_staminaCountLabel, $"- {staminaCount}");
			ButtonGroup buttonGroup = Array.Find(_buttonGroups, (ButtonGroup e) => e._type == ButtonGroupType.UseResource);
			UIHelper.EnableGameObject(buttonGroup._groupRoot, enable: true);
			UIHelper.EnableGameObject(buttonGroup._staminaRoot, enable: true);
			SetupStaminaIcon(battleType, "EliteBattleIcon");
		}

		public void SetStamina(ButtonGroupType buttonGroupType, BattleType battleType, StaminaType staminaType, int staminaCount = 0, bool showForce = false)
		{
			ButtonGroup selectedButtonGroup = Array.Find(_buttonGroups, (ButtonGroup e) => e._type == buttonGroupType);
			Array.ForEach(_buttonGroups, delegate(ButtonGroup e)
			{
				if (selectedButtonGroup != e)
				{
					UIHelper.EnableGameObject(e._groupRoot, enable: false);
					UIHelper.EnableGameObject(e._staminaRoot, enable: false);
				}
			});
			if (selectedButtonGroup == null)
			{
				return;
			}
			if (!UIHelper.IsEnabledGameObjet(selectedButtonGroup._groupRoot))
			{
				UIHelper.EnableGameObject(selectedButtonGroup._groupRoot, enable: true);
			}
			bool flag = GlobalAutoBehaviour<BoosterManager>.instance.CheckSaveStamina().IsActiveBattleType(battleType);
			if (staminaCount == 0 && !showForce && !flag)
			{
				UIHelper.EnableGameObject(selectedButtonGroup._staminaRoot, enable: false);
				if (callerWindow != null && (callerWindow.TopmostMenuType == ETopmostMenuType.Tutorial || callerWindow.TopmostMenuType == ETopmostMenuType.Tutorial_Guide))
				{
					UIHelper.EnableGameObject(selectedButtonGroup._staminaRoot, enable: true);
					UIHelper.SetLabelText(_staminaCountLabel, $"- {staminaCount}");
				}
			}
			else
			{
				UIHelper.EnableGameObject(selectedButtonGroup._staminaRoot, enable: true);
				SetupStaminaIcon(battleType, staminaType);
				UIHelper.SetLabelText(_staminaCountLabel, $"- {staminaCount}");
			}
		}

		public void SetEventStamina(long remainCount, long maxTryCount)
		{
			Array.ForEach(_buttonGroups, delegate(ButtonGroup e)
			{
				UIHelper.EnableGameObject(e._groupRoot, enable: false);
				UIHelper.EnableGameObject(e._staminaRoot, enable: false);
			});
			ButtonGroup buttonGroup = Array.Find(_buttonGroups, (ButtonGroup e) => e._type == ButtonGroupType.Event);
			UIHelper.EnableGameObject(buttonGroup._groupRoot, enable: true);
			UIHelper.EnableGameObject(buttonGroup._staminaRoot, enable: true);
			UIHelper.SetLabelText(_eventStaminaCountLabel, $"{remainCount} / {maxTryCount}");
			_eventStaminaCountLabel.pivot = UIWidget.Pivot.Center;
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_eventStaminaCountLabel.transform);
		}

		public void SetWorldBossButtonStatus(BattleType battleType, bool isMainParty = true)
		{
			if (battleType != BattleType.ArcdimRaid && battleType != BattleType.ArcdimHardRaid)
			{
				UIHelper.EnableGameObject(_groupNoResource, enable: false);
				UIHelper.EnableGameObject(_groupUseResource, enable: false);
				UIHelper.EnableGameObject(_groupWorldBoss, enable: true);
				if (isMainParty)
				{
					UIHelper.EnableGameObject(_groupWorldBoss_SetSubParty, enable: true);
					UIHelper.EnableGameObject(_groupWorldBoss_StartBattle, enable: false);
				}
				else
				{
					UIHelper.EnableGameObject(_groupWorldBoss_SetSubParty, enable: false);
					UIHelper.EnableGameObject(_groupWorldBoss_StartBattle, enable: true);
				}
			}
		}

		public void SetEventDungeonOverDefaultDeckCount(bool isMainParty = true)
		{
			UIHelper.EnableGameObject(_groupNoResource, enable: false);
			UIHelper.EnableGameObject(_groupUseResource, !isMainParty);
			UIHelper.EnableGameObject(_groupWorldBoss, isMainParty);
			if (isMainParty)
			{
				UIHelper.EnableGameObject(_groupWorldBoss_SetSubParty, enable: true);
				UIHelper.EnableGameObject(_groupWorldBoss_StartBattle, enable: false);
			}
		}

		public void CreateIconProxy()
		{
			if (!(_iconRoot == null) && _iconProxy == null)
			{
				_iconProxy = IconProxy.Create(_iconRoot);
			}
		}

		public void ReleaseIconProxy()
		{
			if (_iconProxy != null)
			{
				FactoryBase.SafeReleaseComponent(ref _iconProxy);
			}
			_iconProxy = null;
		}

		public void SetupStaminaIcon(BattleType battleType, string staminaName)
		{
			CreateIconProxy();
			SaveStaminaBoosterInfo saveStaminaBoosterInfo = GlobalAutoBehaviour<BoosterManager>.instance.CheckSaveStamina();
			if (_iconProxy != null)
			{
				if (staminaName.Contains("Chicken") && saveStaminaBoosterInfo.IsActiveBattleType(battleType))
				{
					_iconProxy.SetupStamina("SaveStamina");
				}
				else
				{
					_iconProxy.SetupStamina(staminaName);
				}
			}
		}

		public void SetupStaminaIcon(BattleType battleType, StaminaType staminaType)
		{
			CreateIconProxy();
			SaveStaminaBoosterInfo saveStaminaBoosterInfo = GlobalAutoBehaviour<BoosterManager>.instance.CheckSaveStamina();
			if (_iconProxy != null)
			{
				if (staminaType == StaminaType.Chicken && saveStaminaBoosterInfo.IsActiveBattleType(battleType))
				{
					_iconProxy.SetupStamina("SaveStamina");
				}
				else
				{
					_iconProxy.SetupStamina(staminaType);
				}
			}
		}
	}

	public const int DefaultPartySlotCount = 4;

	public const int DefaultSubPartySlotCount = 4;

	public const string PrefabPath = "Resources/Prefab2/Window/BattlePartySetting/BattlePartySetting";

	public UserHeroListPanel UserHeroListPanel;

	public SquardUi[] SquardUis;

	public MiniPortraitUi[] MiniPortraitUIs;

	public OnetimeBoosterUi[] OnetimeBoosterUis;

	public StartBattleUiData StartBattleUi;

	public UIButton Button_ClearParty;

	public UILabel Label_DebugText;

	protected Squard _currentSquard;

	protected UseOnetimeBoosterPanel _onetimeBoosterPanel;

	protected Listener _listener;

	protected PreviewAnimationListener _previewAnimationListener;

	protected IDataProvider _dataProvider;

	private StaminaType _staminaType;

	protected int _staminaCount;

	protected bool _autoRepeatable;

	private bool _isAutoCampaign;

	private float _autoCampaignElapsedTime;

	private bool _isDungeonHeroSet;

	private bool _isScenario;

	private bool _isSkipAnimatePosition;

	protected int _useOnetimeBoosterItemCount;

	protected Dictionary<SkillAttrType, int> _heroCountByAttrDic;

	protected Action _switchSquardAction;

	private HashSet<string> _voiceReservedHeroSet = new HashSet<string>();

	private List<BattleCreatureCreateInfo> creatureCreateInfoList = new List<BattleCreatureCreateInfo>();

	protected List<CreatureData> _bannedHeroDataList;

	protected List<CreatureTagType> _selectCreatureTagTypeList;

	protected List<CreatureTagType> _acceptedCreatureTagTypeList;

	protected StringBuilder _acceptedClassStringBuilder = new StringBuilder();

	public virtual StaminaType StaminaType
	{
		get
		{
			if (_dataProvider == null)
			{
				return StaminaType.Chicken;
			}
			return _dataProvider.GetStaminaType();
		}
	}

	public virtual BattleType BattleType
	{
		get
		{
			if (_dataProvider == null || _dataProvider.GetEnterDungeon() == null || _dataProvider.GetEnterDungeon().LinkedDungeonData == null)
			{
				return BattleType.None;
			}
			return _dataProvider.GetEnterDungeon().LinkedDungeonData.BattleType;
		}
	}

	public virtual int StaminaCount
	{
		get
		{
			if (_dataProvider == null)
			{
				return 0;
			}
			return _dataProvider.GetStaminaConsumeCount();
		}
	}

	public new virtual int WindowGuideIndex => 0;

	public List<BattleCreatureCreateInfo> CreatureCreateInfoList => creatureCreateInfoList;

	private int SubSquardCount
	{
		get
		{
			EnterDungeonData enterDungeon = _dataProvider.GetEnterDungeon();
			if (enterDungeon == null)
			{
				return 0;
			}
			CampaignDungeonData linkedDungeonData = enterDungeon.LinkedDungeonData;
			if (linkedDungeonData == null)
			{
				return 0;
			}
			if (linkedDungeonData.SubSquardCount <= 0)
			{
				return 0;
			}
			int subSquardCount = linkedDungeonData.SubSquardCount;
			if (!Helper.CheckFixedSetHero(enterDungeon.Chapter, enterDungeon.LinkedDungeon, enterDungeon.Difficulty, enterDungeon.IsScenario))
			{
				return subSquardCount;
			}
			DungeonHeroSetData data = GameRoot.Instance.DungeonHeroSetDataContainer.GetData(enterDungeon.Chapter, enterDungeon.LinkedDungeon, enterDungeon.Difficulty);
			if (data == null)
			{
				return subSquardCount;
			}
			IList<DungeonHeroListSetData> datasByGroupIndex = GameRoot.Instance.DungeonHeroListSetDataContainer.GetDatasByGroupIndex(data.HeroListSetDataIndex);
			if (__Extension_Method_ICollection__.IsNullOrEmpty(datasByGroupIndex))
			{
				return subSquardCount;
			}
			subSquardCount = datasByGroupIndex.Count % 4;
			if (subSquardCount <= 4)
			{
				return subSquardCount;
			}
			return 4;
		}
	}

	protected bool CheckPossibleHeroAttributeType
	{
		get
		{
			if (BattleType == BattleType.ArcdimHardRaid)
			{
				return true;
			}
			return false;
		}
	}

	private bool CheckTechnoEnchantRaid
	{
		get
		{
			if (BattleType != BattleType.ArcdimRaid)
			{
				return BattleType == BattleType.ArcdimHardRaid;
			}
			return true;
		}
	}

	public void Open(Listener listener, IDataProvider dataProvider)
	{
		Open(delegate
		{
			Init(listener, dataProvider);
		}, null);
	}

	public void Open(Listener listener, IDataProvider dataProvider, Action openCompleted)
	{
		Open(delegate
		{
			Init(listener, dataProvider);
		}, openCompleted);
	}

	public void Open(Listener listener, PreviewAnimationListener previewListener, IDataProvider dataProvider)
	{
		_previewAnimationListener = previewListener;
		Open(delegate
		{
			Init(listener, dataProvider);
		}, delegate
		{
			if (_previewAnimationListener != null)
			{
				_previewAnimationListener.ActivatePreview(bActivate: false);
			}
		});
	}

	protected void BaseInit()
	{
		SetClearPartyButton();
		SingletonAuto<HeroPresetStorageWebManager>.instance.AddLoadPresetListener(this);
		_isSkipAnimatePosition = false;
		SetDebugLabel();
	}

	public void Init(Listener listener, IDataProvider dataProvider)
	{
		GlobalAutoBehaviour<BoosterManager>.instance.AddObserver(this);
		StartBattleUi.callerWindow = this;
		_listener = listener;
		_dataProvider = dataProvider;
		_autoCampaignElapsedTime = 0f;
		_isAutoCampaign = false;
		EnterDungeonData enterDungeon = _dataProvider.GetEnterDungeon();
		_autoRepeatable = AutoRepeatable(enterDungeon);
		if (enterDungeon != null)
		{
			_isScenario = enterDungeon.IsScenario;
			_isDungeonHeroSet = Helper.CheckFixedSetHero(enterDungeon.Chapter, enterDungeon.LinkedDungeon, enterDungeon.Difficulty, _isScenario);
		}
		BaseInit();
		SetPickClassList();
		SetAcceptedClassList();
		SetBannedHeroList();
		SetSquardEmptySlot();
		SetSquardHeroList(out var mainHeroList, out var subHeroList);
		SetSquardMiniPortrait();
		SetUserHeroList();
		RefreshMainSquardClassTag();
		SetupUseOnetimeBoosterPanel();
		SceneAutoBehaviour<LobbyManagement>.instance.OpenLobbyTopmostMenu(this);
		RefreshStaminaCount();
		SetEventTriggerSquardInit(enterDungeon.Chapter, enterDungeon.LinkedDungeon, mainHeroList?.ToArray(), subHeroList?.ToArray());
		CampaignDungeonData linkedDungeonData = enterDungeon.LinkedDungeonData;
		if (linkedDungeonData != null)
		{
			if (SubSquardCount > 0)
			{
				if (linkedDungeonData.BattleType == BattleType.EventDungeon)
				{
					StartBattleUi.SetEventDungeonOverDefaultDeckCount();
				}
				else
				{
					StartBattleUi.SetWorldBossButtonStatus(BattleType);
				}
			}
			if (DispatchHelper.IsDispatchable(linkedDungeonData))
			{
				UserHeroListPanel.SetDispatch();
			}
		}
		if (GameOptionData.EditorCanAutoPlay)
		{
			SingletonUpdater<Executer>.instance.DelayRun(3f, this, delegate
			{
				RequestStartBattle(null);
			});
		}
	}

	public static bool AutoRepeatable(EnterDungeonData enterDungeon)
	{
		if (enterDungeon.IsScenario)
		{
			return false;
		}
		CampaignDungeonData linkedDungeonData = enterDungeon.LinkedDungeonData;
		if (linkedDungeonData == null)
		{
			return false;
		}
		if (linkedDungeonData.BattleType == BattleType.UnderPrison)
		{
			if (GlobalAutoBehaviour<AccountManager>.instance.UnderPrisonManager.Get(linkedDungeonData.ChapterIndex) == null)
			{
				return false;
			}
			UnderPrisonDungeonData underPrisonDungeonData = GameRoot.Instance.UnderPrisonDungeonDataContainer.GetUnderPrisonDungeonData(linkedDungeonData.ChapterIndex, linkedDungeonData.DungeonIndex);
			if (underPrisonDungeonData == null)
			{
				return false;
			}
			if (GlobalAutoBehaviour<AccountManager>.instance.UnderPrisonManager.GetOpenDiff(linkedDungeonData.ChapterIndex) <= underPrisonDungeonData.Diff)
			{
				return false;
			}
			return true;
		}
		BattleDefineData battleDefine = linkedDungeonData.GetBattleDefine(enterDungeon.IsScenario);
		if (battleDefine == null)
		{
			return false;
		}
		if (linkedDungeonData.Value != null && linkedDungeonData.Value.Length != 0 && linkedDungeonData.Value[0] == 1)
		{
			return false;
		}
		if (linkedDungeonData.BattleType == BattleType.SubStoryBoss || linkedDungeonData.BattleType == BattleType.PunishmentRaid || linkedDungeonData.BattleType == BattleType.KarmaRaid)
		{
			return false;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.CampaignManager == null)
		{
			return false;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.GetCompleteStar(enterDungeon.Chapter, enterDungeon.OriginalDungeon, enterDungeon.Difficulty) >= battleDefine.AutoRepeatStarCount;
	}

	public override void ReserveCache(UiPreloader preloader)
	{
		base.ReserveCache(preloader);
	}

	public override void Reset()
	{
		base.Reset();
		GlobalAutoBehaviour<BoosterManager>.instance.RemoveObserver(this);
		_listener = null;
		_staminaType = StaminaType.None;
		_staminaCount = -1;
		_isDungeonHeroSet = false;
		if (StartBattleUi != null)
		{
			StartBattleUi.ReleaseIconProxy();
		}
		if (_currentSquard != null)
		{
			_currentSquard.Release();
		}
		if (UserHeroListPanel != null)
		{
			UserHeroListPanel.Release();
		}
		if (_onetimeBoosterPanel != null)
		{
			_onetimeBoosterPanel.Release();
			_onetimeBoosterPanel = null;
		}
	}

	public override void OnSystemBackButton()
	{
		OnClickExitButton();
	}

	public void SetupUseOnetimeBoosterPanel()
	{
		if (_onetimeBoosterPanel != null)
		{
			CampaignDungeonData linkedDungeonData = _dataProvider.GetEnterDungeon().LinkedDungeonData;
			bool isUseOnetimeBooster = _dataProvider.EnableOnetimeBooster();
			bool isScenario = _dataProvider.GetEnterDungeon().IsScenario;
			_onetimeBoosterPanel.Init(linkedDungeonData, isUseOnetimeBooster, isScenario, this);
		}
	}

	public bool IsUseOnetimeBoosterItem()
	{
		if (_onetimeBoosterPanel == null)
		{
			return false;
		}
		return _onetimeBoosterPanel.IsUseOnetimeBoosterItem();
	}

	public int GetOnetimeBoosterCount()
	{
		if (_onetimeBoosterPanel != null && _onetimeBoosterPanel.UseOnetimeBoosterItem)
		{
			return _useOnetimeBoosterItemCount;
		}
		return 0;
	}

	public void SetEventTriggerSquardInit(int chapterIndex, int dungeonIndex, CachedHeroInfo[] mainHeroInfos, CachedHeroInfo[] subHeroInfos)
	{
		IEvent triggerEvent = new BattleInitDeck
		{
			ChapterIndex = chapterIndex,
			DungeonIndex = dungeonIndex,
			MainHeroInfos = mainHeroInfos,
			SubHeroInfos = subHeroInfos
		};
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent);
	}

	public void SetEventTriggerSquardInsert(CachedHeroInfo insertHeroInfo, bool isMain)
	{
		IEvent triggerEvent = new BattleInsertDeck
		{
			IsMain = isMain,
			InsertHeroInfo = insertHeroInfo
		};
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent);
	}

	public void SetEventTriggerSquardRemove(int removeHeroIndex)
	{
		IEvent triggerEvent = new BattleRemoveDeck
		{
			RemoveHeroIndex = removeHeroIndex
		};
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent);
	}

	public virtual void ForceRefreshStaminaCount()
	{
		if (_dataProvider != null && !IsWindowAnimating())
		{
			_staminaType = StaminaType;
			_staminaCount = StaminaCount;
			if (_dataProvider.GetEnterDungeon().IsScenarioReplay)
			{
				_staminaCount = 0;
			}
			UpdateStartBattleUI();
		}
	}

	protected void RefreshStaminaCount()
	{
		if ((StaminaType != _staminaType || StaminaCount != _staminaCount) && !IsWindowAnimating())
		{
			_staminaType = StaminaType;
			_staminaCount = StaminaCount;
			if (_dataProvider != null && _dataProvider.GetEnterDungeon().IsScenarioReplay)
			{
				_staminaCount = 0;
			}
			UpdateStartBattleUI();
		}
	}

	protected virtual void UpdateStartBattleUI()
	{
		if (_dataProvider == null)
		{
			return;
		}
		EnterDungeonData enterDungeon = _dataProvider.GetEnterDungeon();
		CampaignDungeonData linkedDungeonData = _dataProvider.GetEnterDungeon().LinkedDungeonData;
		BattleType battleType = BattleType.None;
		StaminaType staminaType = StaminaType.None;
		if (linkedDungeonData != null)
		{
			battleType = linkedDungeonData.BattleType;
			staminaType = linkedDungeonData.ReqStaminaType;
		}
		bool flag = GlobalAutoBehaviour<BoosterManager>.instance.CheckSaveStamina().IsActiveBattleType(battleType);
		StartBattleUi.SetButtonLabel(_dataProvider.GetStartButtonLabelText());
		if (battleType == BattleType.UnderPrison)
		{
			int chapter = enterDungeon.Chapter;
			UnderPrisonInfo underPrisonInfo = GlobalAutoBehaviour<AccountManager>.instance.UnderPrisonManager.Get(chapter);
			if (underPrisonInfo != null)
			{
				int maxTryCount = underPrisonInfo.MaxTryCount;
				long num = maxTryCount;
				int completedCount = underPrisonInfo.CompletedCount;
				long remainCount = checked(num - completedCount);
				StartBattleUi.SetEventStamina(remainCount, num);
			}
		}
		else if (staminaType == StaminaType.None)
		{
			StartBattleUi.SetStamina(StartBattleUiData.ButtonGroupType.NoResource, battleType, staminaType, _staminaCount);
			int num2 = 0;
			long maxTryCount2 = num2;
			int num3 = 0;
			long remainCount2 = num3;
			if (GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CheckEventDungeon(enterDungeon.Chapter, enterDungeon.LinkedDungeon, out maxTryCount2, out remainCount2))
			{
				StartBattleUi.SetEventStamina(remainCount2, maxTryCount2);
			}
		}
		else if (_staminaCount == 0 && !_autoRepeatable && !flag)
		{
			bool flag2 = _currentSquard.IsSelectedMainSquard();
			if (linkedDungeonData.SubSquardCount > 0 && flag2)
			{
				StartBattleUi.SetStamina(StartBattleUiData.ButtonGroupType.UseResource, battleType, staminaType);
				StartBattleUi.SetWorldBossButtonStatus(battleType);
			}
			else
			{
				StartBattleUi.SetStamina(StartBattleUiData.ButtonGroupType.NoResource, battleType, staminaType);
			}
		}
		else
		{
			switch (staminaType)
			{
			case StaminaType.Sword:
			case StaminaType.GuildRaidTicket:
				StartBattleUi.SetStamina(StartBattleUiData.ButtonGroupType.Match, battleType, staminaType, _staminaCount);
				break;
			case StaminaType.WorldBossTicket:
			case StaminaType.WorldBossGlobalTicket:
				StartBattleUi.SetStamina(StartBattleUiData.ButtonGroupType.WorldBoss, battleType, staminaType, _staminaCount);
				break;
			default:
				StartBattleUi.SetStamina(StartBattleUiData.ButtonGroupType.UseResource, battleType, staminaType, _staminaCount);
				break;
			}
		}
	}

	protected virtual void SetUserHeroList()
	{
		if (UserHeroListPanel != null)
		{
			UserHeroListPanel.InitHeroListPanel(_dataProvider.GetEnterDungeon(), this, SetUserHeroListSelectionMark);
		}
	}

	protected void SetUserHeroListSelectionMark(HeroPortraitComponent heroPortraitComponent)
	{
		if (!(heroPortraitComponent == null) && _currentSquard != null)
		{
			bool num = heroPortraitComponent.HeroInfo != null && _currentSquard.IsContainSquardHero(heroPortraitComponent.HeroInfo.HeroIndex, 0L);
			EHeroState portraitState = EHeroState.Free;
			if (num)
			{
				portraitState = _dataProvider.GetPartyHeroState();
			}
			heroPortraitComponent.SetPortraitState(portraitState);
		}
	}

	private void SetupMiniPortrait(bool isMain)
	{
		if (_currentSquard != null)
		{
			_currentSquard.SwichingSquard(isMain);
		}
	}

	void HeroPortraitComponent.Listener.OnClick(HeroPortraitComponent heroPortrait)
	{
		if (!heroPortrait.IsControllerable || _currentSquard == null)
		{
			return;
		}
		if (heroPortrait.IsBanned)
		{
			ShowEnterClassBanMessage(heroPortrait);
		}
		else
		{
			if (heroPortrait.IsDispatched)
			{
				return;
			}
			if (heroPortrait.HeroInfo == null)
			{
				VespaDebug.LogError("HeroInfo is null!!!");
				return;
			}
			int heroIndex = heroPortrait.HeroInfo.HeroIndex;
			long ownerId = heroPortrait.OwnerId;
			Common.HeroPortraitAnim squardHeroProtraitAnim = _currentSquard.GetSquardHeroProtraitAnim(heroIndex, 0L);
			if (squardHeroProtraitAnim != null && Vespa_Common_Class_Extension_Method.EQUALS(squardHeroProtraitAnim.Component, heroPortrait))
			{
				RemoveSquardHero(heroPortrait.HeroInfo.HeroIndex, heroPortrait.OwnerId, heroPortrait.CreatureData.TagType);
				return;
			}
			squardHeroProtraitAnim = _currentSquard.GetSquardHeroProtraitAnim(heroIndex, ownerId);
			if (squardHeroProtraitAnim != null)
			{
				RemoveSquardHero(heroPortrait.HeroInfo.HeroIndex, heroPortrait.OwnerId, heroPortrait.CreatureData.TagType);
				return;
			}
			if (!CanBanClassEnter(heroPortrait))
			{
				ShowEnterClassBanMessage(heroPortrait);
				return;
			}
			squardHeroProtraitAnim = _currentSquard.GetSquardHeroProtraitAnim(heroIndex, 0L);
			if (squardHeroProtraitAnim != null)
			{
				CenterMessage.Open(LocalizationManager.GetString(PartyBattle.HERO_DUPLICATED_NOT_ALLOWED));
			}
			else if (CheckMainSquardSelectableClassTag(heroPortrait))
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("NOT_REQ_HEROES"));
			}
			else if (!IsAcceptedClassTagType(heroPortrait))
			{
				string text = _acceptedClassStringBuilder.ToString();
				CenterMessage.Open(LocalizationManager.GetString(GodkingTrial.NOTICE_ACCEPTED_CLASS, text));
			}
			else if (IsBannedHero(heroPortrait))
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("NOT_REQ_HEROES"));
			}
			else
			{
				bool isMainSquard = _currentSquard.IsSelectedMainSquard();
				InsertSquardHero(heroPortrait, isMainSquard);
				RefreshMainSquardClassTag();
				UpdateBanHeroPortrait();
			}
		}
	}

	private bool CheckSwapToSubParty()
	{
		if (!CheckPossibleSwapToSubParty())
		{
			return false;
		}
		if (IsMainSquardFull())
		{
			return _currentSquard.GetSubSquardSlotCount() != 0;
		}
		return false;
	}

	private bool IsMainSquardFull()
	{
		if (_currentSquard.IsSelectedMainSquard())
		{
			int mainSquardSlotCount = _currentSquard.GetMainSquardSlotCount();
			List<int> mainSquardTotalHeroIndexList = _currentSquard.GetMainSquardTotalHeroIndexList();
			if (mainSquardSlotCount <= __Extension_Method_ICollection__.COUNT(mainSquardTotalHeroIndexList))
			{
				return true;
			}
		}
		return false;
	}

	private void DelayRunSwitchToSubParty()
	{
		float delay = GameSetting.data.Ui.PartySetting.EnterTime * 2f;
		SingletonUpdater<Executer>.instance.DelayRun(delay, this, delegate
		{
			if (CheckSwapToSubParty())
			{
				SwitchToSubParty();
				if (_switchSquardAction != null)
				{
					Vespa_System_Action_Extension_Method.fire(_switchSquardAction);
				}
			}
		});
	}

	protected virtual bool CheckPossibleSwapToSubParty()
	{
		return true;
	}

	public bool CheckMainSquardSelectableClassTag(HeroPortraitComponent heroPortrait)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_selectCreatureTagTypeList))
		{
			return false;
		}
		if (_currentSquard == null)
		{
			return false;
		}
		List<CreatureTagType> leftClassTagList = _currentSquard.GetLeftClassTagList(isMain: true, _selectCreatureTagTypeList);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(leftClassTagList))
		{
			return false;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(leftClassTagList); i++)
		{
			CreatureTagType creatureTagType = leftClassTagList[i];
			if (heroPortrait.CreatureData.TagType == creatureTagType)
			{
				return false;
			}
		}
		int mainSquardSlotCount = _currentSquard.GetMainSquardSlotCount();
		List<int> mainSquardTotalHeroIndexList = _currentSquard.GetMainSquardTotalHeroIndexList();
		if (mainSquardSlotCount - __Extension_Method_ICollection__.COUNT(mainSquardTotalHeroIndexList) > __Extension_Method_ICollection__.COUNT(leftClassTagList))
		{
			return false;
		}
		return true;
	}

	public bool IsAcceptedClassTagType(HeroPortraitComponent heroPortrait)
	{
		if (heroPortrait == null)
		{
			return true;
		}
		if (heroPortrait.CreatureData == null)
		{
			return true;
		}
		if (_acceptedCreatureTagTypeList == null)
		{
			return true;
		}
		int i = 0;
		for (int num = __Extension_Method_ICollection__.COUNT(_acceptedCreatureTagTypeList); i < num; i++)
		{
			CreatureTagType creatureTagType = _acceptedCreatureTagTypeList[i];
			if (heroPortrait.CreatureData.TagType == creatureTagType)
			{
				return true;
			}
		}
		return false;
	}

	public bool IsBannedHero(HeroPortraitComponent heroPortrait)
	{
		if (heroPortrait == null)
		{
			return false;
		}
		if (heroPortrait.CreatureData == null)
		{
			return false;
		}
		if (_bannedHeroDataList == null)
		{
			return false;
		}
		int i = 0;
		for (int num = __Extension_Method_ICollection__.COUNT(_bannedHeroDataList); i < num; i++)
		{
			CreatureData creatureData = _bannedHeroDataList[i];
			if (creatureData != null && heroPortrait.CreatureData.Index == creatureData.Index)
			{
				return true;
			}
		}
		return false;
	}

	void HeroPortraitComponent.Listener.OnViewInfo(HeroPortraitComponent heroPortrait, bool isMine)
	{
		if (heroPortrait == null || AutoNextBattle.IsEnabled())
		{
			return;
		}
		if (TopmostMenuType == ETopmostMenuType.Tutorial)
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("TUTORIAL_RAIDPARTYSETTING_3"));
		}
		else if (!_isDungeonHeroSet)
		{
			if (isMine)
			{
				SceneAutoBehaviour<MyHeroManagement>.instance.OpenMyHero(heroPortrait.HeroInfo);
			}
			else if (_listener != null)
			{
				_listener.OpenHeroDetailView(heroPortrait.HeroInfo);
			}
		}
	}

	void HeroPortraitComponent.Listener.OnClickHeroNotify(HeroNotifyComponent.ENotifyInfo notifyInfo, CachedHeroInfo cachedHeroInfo)
	{
	}

	void HeroPortraitComponent.Listener.OnLongClick(HeroPortraitComponent heroPortrait, CachedHeroInfo heroInfo)
	{
	}

	protected virtual void UpdateBanHeroPortrait()
	{
	}

	protected virtual bool CanBanClassEnter(HeroPortraitComponent heroPortrait)
	{
		return true;
	}

	protected virtual void ShowEnterClassBanMessage(HeroPortraitComponent heroPortrait = null)
	{
	}

	protected virtual int GetPartyMemberCount()
	{
		return 4;
	}

	protected virtual int GetMaxPartyMemberPerPlayer()
	{
		return 4;
	}

	protected int GetPartyMemberSlotCount()
	{
		return GetPartyMemberCount();
	}

	protected SquardUi GetSquardUi(SquardUi.ESquardType sqaurdType)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(SquardUis); i++)
		{
			SquardUi squardUi = SquardUis[i];
			if (squardUi != null && squardUi.Type == sqaurdType)
			{
				return squardUi;
			}
		}
		return null;
	}

	protected virtual void SetSquardEmptySlot()
	{
		if (_dataProvider == null || _dataProvider.GetEnterDungeon() == null || _dataProvider.GetEnterDungeon().LinkedDungeonData == null)
		{
			_currentSquard = GetSquardUi(SquardUi.ESquardType.OnlyMain).Squard;
			if (_currentSquard != null)
			{
				_currentSquard.InitSquard(4, 0, this);
				SetOnetimeBoosterUIs(0);
			}
			_currentSquard.InitAnimationSquardPostionAction(AnimateSquardPositionAction);
			return;
		}
		EnterDungeonData enterDungeon = _dataProvider.GetEnterDungeon();
		if (enterDungeon != null)
		{
			CampaignDungeonData linkedDungeonData = enterDungeon.LinkedDungeonData;
			_ = linkedDungeonData.BattleType;
			int mainSquardCount = ((linkedDungeonData.MainSquardCount == 0) ? 4 : linkedDungeonData.MainSquardCount);
			int subSquardCount = SubSquardCount;
			if (subSquardCount <= 0)
			{
				_currentSquard = GetSquardUi(SquardUi.ESquardType.OnlyMain).Squard;
			}
			else
			{
				_currentSquard = GetSquardUi(SquardUi.ESquardType.MainSub).Squard;
			}
			SetMiniportraitUis(subSquardCount);
			SetOnetimeBoosterUIs(subSquardCount);
			if (_currentSquard != null)
			{
				_currentSquard.InitSquard(mainSquardCount, subSquardCount, this);
				_currentSquard.InitAnimationSquardPostionAction(AnimateSquardPositionAction);
			}
			else
			{
				VespaDebug.LogError("Current Squard Is Null");
			}
		}
	}

	private MiniPortraitSquardPanel GetMiniPortraitPanel(MiniPortraitUi.EMiniPortraitType eMiniPortraitType)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(MiniPortraitUIs))
		{
			return null;
		}
		if (__Extension_Method_ICollection__.COUNT(MiniPortraitUIs) == 1)
		{
			return MiniPortraitUIs[0].MiniPortraitPanel;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(MiniPortraitUIs); i++)
		{
			MiniPortraitUi miniPortraitUi = MiniPortraitUIs[i];
			if (miniPortraitUi.Type == eMiniPortraitType)
			{
				return miniPortraitUi.MiniPortraitPanel;
			}
		}
		return null;
	}

	protected void SetMiniportraitUis(int subSquardCount)
	{
		if (_currentSquard != null && subSquardCount != 0)
		{
			MiniPortraitUi.EMiniPortraitType miniPortraitType = GetMiniPortraitType();
			MiniPortraitSquardPanel miniPortraitPanel = GetMiniPortraitPanel(miniPortraitType);
			_currentSquard.SetMiniPortraitPanel(miniPortraitPanel);
		}
	}

	private MiniPortraitUi.EMiniPortraitType GetMiniPortraitType()
	{
		MiniPortraitUi.EMiniPortraitType result = MiniPortraitUi.EMiniPortraitType.Large;
		CampaignDungeonData linkedDungeonData = _dataProvider.GetEnterDungeon().LinkedDungeonData;
		bool isUseOnetimeBooster = _dataProvider.EnableOnetimeBooster();
		bool isScenario = _dataProvider.GetEnterDungeon().IsScenario;
		if (UIHelper.IsUsableOneTimeBoosterItem(linkedDungeonData, isUseOnetimeBooster, isScenario) || isScenario || linkedDungeonData.BattleType == BattleType.EventDungeon)
		{
			result = MiniPortraitUi.EMiniPortraitType.Small;
		}
		return result;
	}

	private UseOnetimeBoosterPanel GetOnetimeBoosterPanel(OnetimeBoosterUi.EOnetimeBoosterType eOnetimeBoosterType)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(OnetimeBoosterUis))
		{
			return null;
		}
		if (__Extension_Method_ICollection__.COUNT(OnetimeBoosterUis) == 1)
		{
			return OnetimeBoosterUis[0].OnetimeBoosterPanel;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(OnetimeBoosterUis); i++)
		{
			OnetimeBoosterUi onetimeBoosterUi = OnetimeBoosterUis[i];
			if (onetimeBoosterUi.Type == eOnetimeBoosterType)
			{
				return onetimeBoosterUi.OnetimeBoosterPanel;
			}
		}
		return null;
	}

	protected void SetOnetimeBoosterUIs(int subSquardCount)
	{
		OnetimeBoosterUi.EOnetimeBoosterType eOnetimeBoosterType = OnetimeBoosterUi.EOnetimeBoosterType.ShowItemIcon;
		CampaignDungeonData linkedDungeonData = _dataProvider.GetEnterDungeon().LinkedDungeonData;
		bool isUseOnetimeBooster = _dataProvider.EnableOnetimeBooster();
		bool isScenario = _dataProvider.GetEnterDungeon().IsScenario;
		UIHelper.IsUsableOneTimeBoosterItem(linkedDungeonData, isUseOnetimeBooster, isScenario);
		if (subSquardCount > 0)
		{
			eOnetimeBoosterType = OnetimeBoosterUi.EOnetimeBoosterType.NoShowItemICon;
		}
		_onetimeBoosterPanel = GetOnetimeBoosterPanel(eOnetimeBoosterType);
	}

	protected void RefreshMainSquardClassTag()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_selectCreatureTagTypeList))
		{
			return;
		}
		List<CreatureTagType> leftClassTagList = _currentSquard.GetLeftClassTagList(isMain: true, _selectCreatureTagTypeList);
		List<int> mainSquardTotalHeroIndexList = _currentSquard.GetMainSquardTotalHeroIndexList();
		int mainSquardSlotCount = _currentSquard.GetMainSquardSlotCount();
		for (int i = 0; i < mainSquardSlotCount; i++)
		{
			if (__Extension_Method_ICollection__.COUNT(mainSquardTotalHeroIndexList) > i)
			{
				_currentSquard.SetMainEmptySlotClassTag(i, CreatureTagType.None);
				continue;
			}
			if (__Extension_Method_ICollection__.COUNT(leftClassTagList) <= 0)
			{
				_currentSquard.SetMainEmptySlotClassTag(i, CreatureTagType.None);
				continue;
			}
			_currentSquard.SetMainEmptySlotClassTag(i, leftClassTagList[0]);
			leftClassTagList.RemoveAt(0);
		}
	}

	protected virtual bool IsLocalSaveEnabled()
	{
		return true;
	}

	protected virtual bool IsSubPartyEnabled()
	{
		CampaignDungeonData linkedDungeonData = _dataProvider.GetEnterDungeon().LinkedDungeonData;
		if (linkedDungeonData == null)
		{
			return false;
		}
		return linkedDungeonData.SubSquardCount > 0;
	}

	protected bool IsForcedHero(int heroIndex)
	{
		if (_dataProvider == null)
		{
			return false;
		}
		int[] forcedPartyMemberHeroIndices = _dataProvider.GetForcedPartyMemberHeroIndices();
		if (forcedPartyMemberHeroIndices == null)
		{
			return false;
		}
		return TypeIterator.Contains(forcedPartyMemberHeroIndices, heroIndex);
	}

	protected List<CachedHeroInfo> GetHeroInfoList(bool isMainSquard)
	{
		List<Tuple<CachedHeroInfo, long>> list = new List<Tuple<CachedHeroInfo, long>>();
		EnterDungeonData enterDungeon = _dataProvider.GetEnterDungeon();
		int chapterIndex = 0;
		int dungeonIndex = 0;
		NShared.DungeonDifficultyType difficulty = NShared.DungeonDifficultyType.Easy;
		if (enterDungeon != null)
		{
			chapterIndex = enterDungeon.Chapter;
			dungeonIndex = enterDungeon.LinkedDungeon;
			difficulty = enterDungeon.Difficulty;
		}
		List<int> setHeroList = new List<int>();
		setHeroList.Clear();
		int squardSlotCount = _currentSquard.GetSquardSlotCount(isMainSquard);
		int remainCount = squardSlotCount;
		int startIndex = 0;
		int[] forcedPartyMemberHeroIndices = _dataProvider.GetForcedPartyMemberHeroIndices();
		if (!isMainSquard && !__Extension_Method_ICollection__.IsNullOrEmpty(forcedPartyMemberHeroIndices))
		{
			int squardSlotCount2 = _currentSquard.GetSquardSlotCount();
			startIndex = squardSlotCount2;
			remainCount = forcedPartyMemberHeroIndices.Length - squardSlotCount2;
		}
		SetHeroList(ref remainCount, ref setHeroList, forcedPartyMemberHeroIndices, startIndex);
		int[] partyMemberHeroIndices = _dataProvider.GetPartyMemberHeroIndices(isMainSquard, squardSlotCount);
		SetHeroList(ref remainCount, ref setHeroList, partyMemberHeroIndices, 0);
		foreach (int index in setHeroList)
		{
			if (list.Find((Tuple<CachedHeroInfo, long> item) => item.Item1.HeroIndex == index) != null)
			{
				continue;
			}
			CachedHeroInfo cachedHeroInfo = null;
			if (_isDungeonHeroSet)
			{
				HeroInfo dungeonHeroSetHeroInfo = Helper.GetDungeonHeroSetHeroInfo(chapterIndex, dungeonIndex, difficulty, index, _isScenario);
				if (dungeonHeroSetHeroInfo != null)
				{
					cachedHeroInfo = dungeonHeroSetHeroInfo as CachedHeroInfo;
				}
			}
			else
			{
				cachedHeroInfo = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfo(index);
			}
			if (cachedHeroInfo != null)
			{
				long creatureWavePositionScore = StatCalculator.GetCreatureWavePositionScore(cachedHeroInfo);
				if (1 == 0 || !DispatchHelper.IsDispatchedHero(cachedHeroInfo) || IsForcedHero(cachedHeroInfo.HeroIndex))
				{
					list.Add(Tuple.Create(cachedHeroInfo, creatureWavePositionScore));
				}
			}
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			list.Sort((Tuple<CachedHeroInfo, long> l, Tuple<CachedHeroInfo, long> r) => r.Item2.CompareTo(l.Item2));
		}
		List<CachedHeroInfo> list2 = new List<CachedHeroInfo>();
		for (int num = 0; num < list.Count; num++)
		{
			list2.Add(list[num].Item1);
		}
		return RestoredPortal.EligibleRaidSavedHeroes(list2, this);
	}

	private void SetHeroList(ref int remainCount, ref List<int> setHeroList, int[] heroIndices, int startIndex)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(heroIndices) || remainCount <= 0)
		{
			return;
		}
		for (int i = startIndex; i < heroIndices.Length; i++)
		{
			if (remainCount == 0)
			{
				break;
			}
			setHeroList.Add(heroIndices[i]);
			remainCount--;
		}
	}

	protected HeroPortraitComponent.OwnerInfo GetMyHeroPortraitComponentOwnerInfo()
	{
		return new HeroPortraitComponent.OwnerInfo
		{
			AccountId = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id,
			Nick = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Nick,
			Level = GlobalAutoBehaviour<AccountManager>.instance.UserManager.TeamLevel
		};
	}

	protected virtual void SetSquardHeroList(out List<CachedHeroInfo> mainHeroList, out List<CachedHeroInfo> subHeroList)
	{
		mainHeroList = new List<CachedHeroInfo>();
		subHeroList = new List<CachedHeroInfo>();
		if (IsLocalSaveEnabled())
		{
			SetMainSquardHeroList(ref mainHeroList);
			SetSubSquardHeroList(ref subHeroList);
		}
	}

	protected void SetMainSquardHeroList(ref List<CachedHeroInfo> list)
	{
		List<CachedHeroInfo> heroInfoList = GetHeroInfoList(isMainSquard: true);
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(heroInfoList))
		{
			HeroPortraitComponent.OwnerInfo myHeroPortraitComponentOwnerInfo = GetMyHeroPortraitComponentOwnerInfo();
			for (int i = 0; i < heroInfoList.Count; i++)
			{
				CachedHeroInfo cachedHeroInfo = heroInfoList[i];
				Vector3 mainEmptySlotPositionByIndex = _currentSquard.GetMainEmptySlotPositionByIndex(i);
				CreateSquardHeroPortraitAnim(isMainSquard: true, cachedHeroInfo, mainEmptySlotPositionByIndex, myHeroPortraitComponentOwnerInfo);
				list.Add(cachedHeroInfo);
			}
		}
	}

	protected void SetSubSquardHeroList(ref List<CachedHeroInfo> list)
	{
		if (!IsSubPartyEnabled())
		{
			return;
		}
		List<CachedHeroInfo> heroInfoList = GetHeroInfoList(isMainSquard: false);
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(heroInfoList))
		{
			HeroPortraitComponent.OwnerInfo myHeroPortraitComponentOwnerInfo = GetMyHeroPortraitComponentOwnerInfo();
			for (int i = 0; i < heroInfoList.Count; i++)
			{
				CachedHeroInfo cachedHeroInfo = heroInfoList[i];
				Vector3 subEmptySlotPositionByIndex = _currentSquard.GetSubEmptySlotPositionByIndex(i);
				CreateSquardHeroPortraitAnim(isMainSquard: false, cachedHeroInfo, subEmptySlotPositionByIndex, myHeroPortraitComponentOwnerInfo);
				list.Add(cachedHeroInfo);
			}
		}
	}

	protected void SetSquardMiniPortrait()
	{
		if (_currentSquard != null)
		{
			_currentSquard.InitMiniPortrait(_dataProvider.GetEnterDungeon());
		}
	}

	protected virtual void SaveSquardHeroListToLocal()
	{
		if (IsLocalSaveEnabled() && _currentSquard != null)
		{
			List<int> mainSquardTotalHeroIndexList = _currentSquard.GetMainSquardTotalHeroIndexList();
			int mainSquardSlotCount = _currentSquard.GetMainSquardSlotCount();
			SaveSquardHeroListToLocal(mainSquardTotalHeroIndexList, mainSquardSlotCount);
			List<int> subSquardTotalHeroIndexList = _currentSquard.GetSubSquardTotalHeroIndexList();
			int subSquardSlotCount = _currentSquard.GetSubSquardSlotCount();
			SaveSquardHeroListToLocal(subSquardTotalHeroIndexList, subSquardSlotCount, isMainParty: false);
		}
	}

	private void SaveSquardHeroListToLocal(List<int> squardHeroIndexList, int squardSlotCount, bool isMainParty = true)
	{
		for (int i = 0; i < squardSlotCount; i++)
		{
			int heroIndex = 0;
			if (__Extension_Method_ICollection__.COUNT(squardHeroIndexList) > i)
			{
				heroIndex = squardHeroIndexList[i];
			}
			if (IsForcedHero(heroIndex))
			{
				heroIndex = 0;
			}
			_dataProvider.SetPartyMemberHeroIndex(i, heroIndex, isMainParty);
		}
	}

	public void DeselectAllPartyMember()
	{
		if (_currentSquard != null)
		{
			_currentSquard.Release();
		}
	}

	public HeroPortraitComponent GetDeckHeroPortrait(int heroIndex, long accountId = 0L)
	{
		if (UserHeroListPanel != null)
		{
			if (accountId == 0L)
			{
				accountId = GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId;
			}
			return UserHeroListPanel.GetUserHeroPortraitComponent(accountId, heroIndex);
		}
		return null;
	}

	protected virtual Common.HeroPortraitAnim CreateSquardHeroPortraitAnim(bool isMainSquard, CachedHeroInfo heroInfo, Vector3 position, HeroPortraitComponent.OwnerInfo ownerInfo)
	{
		bool isControllerale = ownerInfo.AccountId == GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId;
		return _currentSquard.CreateHeroPortraitAnim(isMainSquard, heroInfo, position, ownerInfo, isControllerale, delegate(Common.HeroPortraitAnim portraitAnim)
		{
			if (isControllerale)
			{
				portraitAnim.Component.EnableViewMyInfo(enable: true);
			}
			else
			{
				portraitAnim.Component.EnableViewOtherInfo(enable: true);
			}
			bool flag = IsForcedHero(heroInfo.HeroIndex);
			portraitAnim.Component.SetLocked(flag);
			if (!flag && _isDungeonHeroSet)
			{
				portraitAnim.Component.ShowCreatureName();
			}
		});
	}

	protected virtual bool EnableInsertSquard(HeroPortraitComponent deckPortrait)
	{
		if (!RestoredPortal.CheckRaidHeroLevel(this, deckPortrait))
		{
			return false;
		}
		if (deckPortrait == null || deckPortrait.HeroInfo == null)
		{
			return false;
		}
		int num = 0;
		List<int> list = null;
		List<int> list2 = null;
		if (_currentSquard.IsSelectedMainSquard())
		{
			num = _currentSquard.GetMainSquardSlotCount();
			list = _currentSquard.GetMainSquardTotalHeroIndexList();
			list2 = _currentSquard.GetMainSquardMyHeroIndexList();
		}
		else
		{
			num = _currentSquard.GetSubSquardSlotCount();
			list = _currentSquard.GetSubSquardTotalHeroIndexList();
			list2 = _currentSquard.GetSubSquardMyHeroIndexList();
		}
		if (num <= __Extension_Method_ICollection__.COUNT(list))
		{
			return false;
		}
		if (GetMaxPartyMemberPerPlayer() <= __Extension_Method_ICollection__.COUNT(list2))
		{
			CenterMessage.Open(LocalizationManager.GetString(Battle.CANNOT_INSERT_MORE));
			return false;
		}
		bool flag = _currentSquard.IsContainSquardHero(deckPortrait.HeroInfo.HeroIndex, 0L);
		CampaignDungeonData linkedDungeonData = _dataProvider.GetEnterDungeon().LinkedDungeonData;
		if (linkedDungeonData != null && linkedDungeonData.SubSquardCount > 0 && flag)
		{
			return false;
		}
		if (flag)
		{
			CenterMessage.Open(LocalizationManager.GetString(Battle.HERO_ALREADY_EXIST_MAIN));
			return false;
		}
		return true;
	}

	private static string GetVoiceCode(int heroIndex, int heroStar, int costumeIndex)
	{
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroIndex);
		if (data == null)
		{
			return null;
		}
		string text = DefineSound.HeroSelectSound(data.CodeName);
		CostumeData costumeData = null;
		costumeData = ((costumeIndex == 0) ? GameRoot.Instance.CostumeDataContainer.GetDataByHeroStar(heroIndex, heroStar) : GameRoot.Instance.CostumeDataContainer.GetData(costumeIndex));
		if (costumeData != null)
		{
			text += costumeData.VoicePostfix;
		}
		return text;
	}

	protected void PlayHeroSelectSound(HeroInfo heroInfo)
	{
		if (heroInfo != null)
		{
			SoundStock stock = new SoundStock
			{
				Code = GetVoiceCode(heroInfo.HeroIndex, heroInfo.Star, heroInfo.CostumeIndex),
				LocalTimer = GlobalTimer.DefaultLocal
			};
			SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 99f);
		}
	}

	protected virtual void AnimateSquardPositionAction(List<Common.HeroPortraitAnim> heroPortraitList, List<PartySlotComponent> emptySlotList, Common.HeroPortraitAnim heroPortraitAnim)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(heroPortraitList); i++)
		{
			Common.HeroPortraitAnim heroPortraitAnim2 = heroPortraitList[i];
			PartySlotComponent partySlotComponent = emptySlotList[i];
			Common.AnimateMemberPosition(heroPortraitAnim2, partySlotComponent.transform.position, Vespa_Common_Class_Extension_Method.EQUALS(heroPortraitAnim2, heroPortraitAnim));
		}
	}

	public virtual void InsertSquardHeroByRaid(bool isMainSquard, CachedHeroInfo heroInfo, Vector3 position, HeroPortraitComponent.OwnerInfo ownerInfo)
	{
		Common.HeroPortraitAnim heroPortraitAnim = CreateSquardHeroPortraitAnim(isMainSquard, heroInfo, position, ownerInfo);
		if (heroPortraitAnim != null)
		{
			_currentSquard.AnimateSqaurdPosition(heroPortraitAnim);
			SaveSquardHeroListToLocal();
			RefreshSelectionMark();
			UpdateBanHeroPortrait();
		}
	}

	public virtual void RemoveSquardHeroByRaid(int heroIndex, long ownerAccountId, bool ignoreSelection = false)
	{
		Common.HeroPortraitAnim squardHeroProtraitAnim = _currentSquard.GetSquardHeroProtraitAnim(heroIndex, ownerAccountId, ignoreSelection);
		if (squardHeroProtraitAnim != null)
		{
			_currentSquard.RemoveHeroPortraitAnim(null, heroIndex, ownerAccountId);
			_currentSquard.AnimateSqaurdPosition(squardHeroProtraitAnim);
			SaveSquardHeroListToLocal();
			RefreshSelectionMark();
			UpdateBanHeroPortrait();
		}
	}

	public virtual bool InsertSquardHero(HeroPortraitComponent deckPortrait, bool isMainSquard)
	{
		if (!EnableInsertSquard(deckPortrait))
		{
			return false;
		}
		PlayHeroSelectSound(deckPortrait.HeroInfo);
		Common.HeroPortraitAnim heroPortraitAnim = CreateSquardHeroPortraitAnim(isMainSquard, deckPortrait.HeroInfo, deckPortrait.transform.position, deckPortrait.Owner);
		_currentSquard.AnimateSqaurdPosition(heroPortraitAnim);
		RefreshSelectionMark();
		SaveSquardHeroListToLocal();
		SetEventTriggerSquardInsert(deckPortrait.HeroInfo, isMainSquard);
		UpdateBanHeroPortrait();
		if (CheckSwapToSubParty())
		{
			DelayRunSwitchToSubParty();
		}
		return true;
	}

	public virtual bool RemoveSquardHero(int heroIndex, long ownerId = 0L, CreatureTagType classType = CreatureTagType.None, bool ignoreSelection = false)
	{
		if (IsForcedHero(heroIndex))
		{
			CenterMessage.Open(LocalizationManager.GetString(Battle.CANNOT_REMOVE_FORCED_HERO));
			return false;
		}
		Common.HeroPortraitAnim squardHeroProtraitAnim = _currentSquard.GetSquardHeroProtraitAnim(heroIndex, ownerId, ignoreSelection);
		if (squardHeroProtraitAnim == null)
		{
			return false;
		}
		long ownerId2 = squardHeroProtraitAnim.Component.OwnerId;
		HeroPortraitComponent deckHeroPortrait = GetDeckHeroPortrait(squardHeroProtraitAnim.HeroIndex, ownerId2);
		_currentSquard.RemoveHeroPortraitAnim(deckHeroPortrait, heroIndex, ownerId);
		_currentSquard.AnimateSqaurdPosition(squardHeroProtraitAnim);
		SaveSquardHeroListToLocal();
		SetEventTriggerSquardRemove(heroIndex);
		RefreshSelectionMark();
		RefreshMainSquardClassTag();
		UpdateBanHeroPortrait();
		return true;
	}

	protected void RefreshSelectionMark()
	{
		if (UserHeroListPanel != null)
		{
			UserHeroListPanel.SelectionMark();
		}
	}

	protected virtual bool CheckStartMemberCount()
	{
		return true;
	}

	protected virtual bool CheckBattleReadyToMulti()
	{
		return false;
	}

	protected virtual bool CheckIsMasterToParty()
	{
		return false;
	}

	protected int GetSquardMainHeroIndexCount()
	{
		if (_currentSquard == null)
		{
			return 0;
		}
		return __Extension_Method_ICollection__.COUNT(_currentSquard.GetMainSquardMyHeroIndexList());
	}

	protected int GetSquardTotalHeroIndexCount()
	{
		List<int> mainSquardMyHeroIndexList = _currentSquard.GetMainSquardMyHeroIndexList();
		List<int> subSquardTotalHeroIndexList = _currentSquard.GetSubSquardTotalHeroIndexList();
		return __Extension_Method_ICollection__.COUNT(mainSquardMyHeroIndexList) + __Extension_Method_ICollection__.COUNT(subSquardTotalHeroIndexList);
	}

	protected int GetSquardTotalSlotCount()
	{
		return _currentSquard.GetTotalSquardSlotCount();
	}

	protected int GetSquardTotalMyHeroIndexCount()
	{
		List<int> mainSquardMyHeroIndexList = _currentSquard.GetMainSquardMyHeroIndexList();
		List<int> subSquardMyHeroIndexList = _currentSquard.GetSubSquardMyHeroIndexList();
		return __Extension_Method_ICollection__.COUNT(mainSquardMyHeroIndexList) + __Extension_Method_ICollection__.COUNT(subSquardMyHeroIndexList);
	}

	public void TryStartBattle(ExtraOptionData extraOption)
	{
		SprkStoryAuto.TrackEntryOption(extraOption);
		SprkStoryAuto.BeforeStart(this, _dataProvider);
		if (!CheckStartMemberCount())
		{
			RequestStartBattle(extraOption);
			return;
		}
		if (!CheckTechnoEnchantRaid && GetSquardMainHeroIndexCount() == 0)
		{
			CenterMessage.Open(LocalizationManager.GetString(PartyBattle.NO_PARTY_HEROES));
			if (AutoNextBattle.IsEnabled())
			{
				SprkStoryAuto.StopForEntryEvent(AutoNextBattle.instance, send: false, PartyRoomRepeat.ResultType.NONE);
			}
			return;
		}
		OnClickStartBattle triggerEvent = new OnClickStartBattle
		{
			ChapterIndex = _dataProvider.GetEnterDungeon().Chapter,
			DungeonIndex = _dataProvider.GetEnterDungeon().LinkedDungeon,
			DifficultyType = _dataProvider.GetEnterDungeon().Difficulty,
			IsScenario = _dataProvider.GetEnterDungeon().IsScenario
		};
		if (SprkStoryAuto.EventRaised(SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent)))
		{
			if (AutoNextBattle.IsEnabled())
			{
				SprkStoryAuto.StopForEntryEvent(AutoNextBattle.instance, send: false, PartyRoomRepeat.ResultType.NONE);
			}
		}
		else
		{
			if (!SprkStoryAuto.EntryChecked(_dataProvider.GetEnterDungeon().CheckBattleEnter()))
			{
				return;
			}
			if (!AutoNextBattle.IsEnabled())
			{
				if (OpenInsufficientPartyHeroesPopupByTechnoEnchantRaid(extraOption))
				{
					return;
				}
				if (!CheckTechnoEnchantRaid)
				{
					int num = _currentSquard.GetMainSquardTotalHeroIndexList().Count();
					int num2 = __Extension_Method_ICollection__.COUNT(_currentSquard.GetSubSquardTotalHeroIndexList());
					int num3 = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.Count();
					if (num + num2 < GetSquardTotalSlotCount() && num3 >= GetSquardTotalSlotCount())
					{
						OpenInsufficientPartyHeroesPopup(extraOption);
						return;
					}
				}
			}
			CheckPreSelectReward(extraOption);
		}
	}

	private bool OpenInsufficientPartyHeroesPopupByTechnoEnchantRaid(ExtraOptionData extraOption)
	{
		if (!CheckTechnoEnchantRaid)
		{
			return false;
		}
		if (!CheckIsMasterToParty() && CheckBattleReadyToMulti())
		{
			return false;
		}
		if (_currentSquard == null)
		{
			return true;
		}
		List<int> mainSquardMyHeroIndexList = _currentSquard.GetMainSquardMyHeroIndexList();
		List<int> subSquardMyHeroIndexList = _currentSquard.GetSubSquardMyHeroIndexList();
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(mainSquardMyHeroIndexList) && mainSquardMyHeroIndexList.Count() > 0 && mainSquardMyHeroIndexList.Count() < _currentSquard.GetMainSquardSlotCount())
		{
			OpenInsufficientPartyHeroesPopup(extraOption);
			return true;
		}
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(subSquardMyHeroIndexList) && subSquardMyHeroIndexList.Count() > 0 && subSquardMyHeroIndexList.Count() < _currentSquard.GetSubSquardSlotCount())
		{
			OpenInsufficientPartyHeroesPopup(extraOption);
			return true;
		}
		return false;
	}

	private void OpenInsufficientPartyHeroesPopup(ExtraOptionData extraOption)
	{
		string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string message = LocalizationManager.GetString(PartyBattle.INSUFFICIENT_PARTY_HEROES_CONFIRM);
		MessagePopup.Open(text, message, delegate
		{
			CheckPreSelectReward(extraOption);
		}, null);
	}

	protected virtual void CheckPreSelectReward(ExtraOptionData extraOption)
	{
		CampaignDungeonData linkedDungeonData = _dataProvider.GetEnterDungeon().LinkedDungeonData;
		if (CheckSelectRewardPopup(linkedDungeonData))
		{
			int chapterIndex = linkedDungeonData.ChapterIndex;
			int dungeonIndex = linkedDungeonData.DungeonIndex;
			SelectRewardData data = GameRoot.Instance.SelectRewardDataContainer.GetData(chapterIndex, dungeonIndex);
			if (data == null)
			{
				return;
			}
			UnderPrisonData data2 = GameRoot.Instance.UnderPrisonDataContainer.GetData(chapterIndex);
			if (data2 != null && data2.Type == UnderPrisonType.SkillBook)
			{
				SkillBookSelectRewardPopup.OpenGlobal(linkedDungeonData.BattleType, data, this, delegate
				{
					RequestStartBattle(extraOption);
				}, SelectRewardButtonType.OKCancel);
			}
			else
			{
				PreSelectRewardPopup.Open(linkedDungeonData.BattleType, data, this, delegate
				{
					RequestStartBattle(extraOption);
				});
			}
		}
		else
		{
			NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.ClearSelectedReward();
			RequestStartBattle(extraOption);
		}
	}

	private bool CheckSelectRewardPopup(CampaignDungeonData dungeonData)
	{
		if (dungeonData == null)
		{
			return false;
		}
		int chapterIndex = dungeonData.ChapterIndex;
		int dungeonIndex = dungeonData.DungeonIndex;
		SelectRewardData data = GameRoot.Instance.SelectRewardDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data == null)
		{
			return false;
		}
		if (!data.IsSelectable())
		{
			return false;
		}
		return true;
	}

	protected virtual void RequestStartBattle(ExtraOptionData extraOption)
	{
		if (!SprkDispatchSelection.CanUseParty(this))
		{
			return;
		}
		EnterDungeonData enterDungeon = new EnterDungeonData();
		BattlePartyInfo battlePartyInfo = _currentSquard.GetBattlePartyInfo();
		if (_dataProvider != null)
		{
			enterDungeon = _dataProvider.GetEnterDungeon();
			battlePartyInfo.StaminaType = _dataProvider.GetStaminaType();
			battlePartyInfo.StaminaCount = _dataProvider.GetStaminaConsumeCount();
			battlePartyInfo.AffixIndices = _dataProvider.GetAffixIndices();
		}
		if (_currentSquard != null && __Extension_Method_ICollection__.COUNT(_currentSquard.GetLeftClassTagList(isMain: true, _selectCreatureTagTypeList)) > 0)
		{
			AutoNextBattle.StopWithoutMessage();
			CenterMessage.Open(LocalizationManager.GetStringDirect("NO_REQ_HEROES"));
			return;
		}
		battlePartyInfo.OnetimeBoosterCount = GetOnetimeBoosterCount();
		if (_listener != null)
		{
			_listener.StartBattle(enterDungeon, battlePartyInfo, extraOption);
		}
	}

	protected virtual void ProcessExitButton()
	{
		if (_listener == null || _listener.OnCloseRequest(this))
		{
			Close();
		}
	}

	public void OnClickExitButton()
	{
		if (AutoNextBattle.IsEnabled())
		{
			AutoNextBattle.instance.OnClickStop();
		}
		ProcessExitButton();
	}

	public override void Close()
	{
		if (_previewAnimationListener != null)
		{
			_previewAnimationListener.ActivatePreview(bActivate: true);
		}
		SingletonAuto<HeroPresetStorageWebManager>.instance.RemoveLoadPresetListener(this);
		base.Close();
	}

	public override ITask CloseAsync()
	{
		if (_previewAnimationListener != null)
		{
			_previewAnimationListener.ActivatePreview(bActivate: true);
		}
		return base.CloseAsync();
	}

	public void OnClickOnetimeBoosterHelp()
	{
		string text = LocalizationManager.GetString(NShared.NLocalization.Common.GUIDE_MENU);
		string stringDirect = LocalizationManager.GetStringDirect("BOOSTER_ONETIME_REWARD_TEXT");
		MessagePopup.Open(text, stringDirect);
	}

	public void OnClickStartBattle()
	{
		EnterDungeonData enterDungeon = _dataProvider.GetEnterDungeon();
		if (!SceneAutoBehaviour<MultiPartyManagement>.instance.CheckEnterBattle(enterDungeon))
		{
			return;
		}
		if (enterDungeon != null && enterDungeon.LinkedDungeonData != null && !GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CheckEnterCount(enterDungeon))
		{
			if (!enterDungeon.CheckIsNotInitDailyCount())
			{
				MessagePopup.Open(LocalizationManager.GetString(NShared.NLocalization.Common.INFORM), LocalizationManager.GetString(ResultType.ExceededDailyEntryCount));
			}
			return;
		}
		if (IsUseOnetimeBoosterItem())
		{
			_useOnetimeBoosterItemCount = 1;
		}
		if (!SprkStoryAuto.ChooseForParty(_dataProvider, _listener))
		{
			if (BattleSelectPopup.IsMultipleMenu(_dataProvider) && !AutoNextBattle.IsEnabled() && !NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.IsEnabled())
			{
				BattleSelectPopup.Open(_dataProvider);
				return;
			}
			bool flag = false;
			flag = ((enterDungeon.LinkedDungeonData != null) ? enterDungeon.CheckBattleEnter(StaminaCount) : UIHelper.CheckStamina(StaminaType, StaminaCount, enterDungeon.Chapter, null));
			OnClickStartBattle_Common(flag);
		}
	}

	private void OnClickStartBattle_Common(bool isTryStartBattle)
	{
		if (isTryStartBattle)
		{
			if (CheckAutoNextBattle())
			{
				TryStartBattle(null);
			}
		}
		else if (AutoNextBattle.IsEnabled())
		{
			AutoNextBattle.instance.OnClickStop();
		}
	}

	protected virtual bool CheckAutoNextBattle()
	{
		if (AutoNextBattle.IsEnabled() || !_listener.OpenAutoNextBattleSelect())
		{
			return true;
		}
		return false;
	}

	private bool GetStamina(out long curStamina, out int reqStamina)
	{
		int num = 0;
		curStamina = num;
		int num2 = 0;
		long maxTryCount = num2;
		reqStamina = 0;
		EnterDungeonData enterDungeon = _dataProvider.GetEnterDungeon();
		CampaignDungeonData linkedDungeonData = enterDungeon.LinkedDungeonData;
		if (GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.CheckEventDungeon(enterDungeon.Chapter, enterDungeon.LinkedDungeon, out maxTryCount, out curStamina))
		{
			reqStamina = 1;
			return true;
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetCurrentStamina(linkedDungeonData.ReqStaminaType, out curStamina, out maxTryCount))
		{
			reqStamina = StaminaCount;
			return true;
		}
		return false;
	}

	public void OnClickRepeatBattle()
	{
		EnterDungeonData enterDungeon = _dataProvider.GetEnterDungeon();
		_ = enterDungeon.LinkedDungeonData;
		if (!enterDungeon.CheckBattleEnter())
		{
			return;
		}
		if (IsUseOnetimeBoosterItem())
		{
			int num = 0;
			long curStamina = num;
			int reqStamina = 0;
			if (!GetStamina(out curStamina, out reqStamina))
			{
				return;
			}
			long repeatableCount = _onetimeBoosterPanel.GetRepeatableCount(curStamina, reqStamina);
			if (repeatableCount != 0L)
			{
				ItemData itemData = _onetimeBoosterPanel.GetItemData();
				List<Tuple<ItemData, int>> list = new List<Tuple<ItemData, int>>();
				long num2 = repeatableCount;
				list.Add(new Tuple<ItemData, int>(itemData, checked((int)num2)));
				string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
				string stringDirect = LocalizationManager.GetStringDirect("REPEATBATTLE_INFORM_BY_BOOSTER");
				SelectItemMessagePopup.OpenWithGemPrice(text, stringDirect, list, itemData.BuyGem, delegate(SelectItemMessagePopup selectItemPopup, int idx, int count)
				{
					_useOnetimeBoosterItemCount = count;
					OpenRepeatBattleNoticePopup(this);
				}, null, closeOnYes: false);
			}
		}
		else
		{
			OpenRepeatBattleNoticePopup(this);
		}
	}

	public static void OpenRepeatBattleNoticePopup(PreSelectRewardPopup.Listener listener, bool isEndCampaign = false)
	{
		string text = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string stringDirect = LocalizationManager.GetStringDirect("REPEATBATTLE_INFORM2");
		MessagePopup.OpenEnabled(text, stringDirect, delegate
		{
			ExtraOptionData extraOption = new ExtraOptionData
			{
				AutoRepeat = true,
				IsEndCampaign = isEndCampaign
			};
			NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.ClearSelectedReward();
			listener.TryStartBattle(extraOption);
		}, delegate
		{
			NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.ClearSelectedReward();
		}, "COMMON_CONFIRM", bYesEnabled: true);
	}

	public virtual void OnHeroPositionToggleChange()
	{
		if (UserHeroListPanel != null)
		{
			UserHeroListPanel.OnHeroPositionToggleChange();
			EnterDungeonData enterDungeon = _dataProvider.GetEnterDungeon();
			if (enterDungeon != null && enterDungeon.LinkedDungeonData != null && DispatchHelper.IsDispatchable(enterDungeon.LinkedDungeonData))
			{
				UserHeroListPanel.SetDispatch();
			}
		}
	}

	private void SwitchToSubParty()
	{
		SwitchToSubPartyReal();
	}

	private void SwitchToSubPartyReal()
	{
		SetupMiniPortrait(isMain: false);
		bool flag = _currentSquard.IsSelectedMainSquard();
		BattleType battleType = BattleType;
		if (battleType == BattleType.EventDungeon)
		{
			StartBattleUi.SetEventDungeonOverDefaultDeckCount(flag);
		}
		else
		{
			StartBattleUi.SetWorldBossButtonStatus(battleType, flag);
		}
		CampaignDungeonData linkedDungeonData = _dataProvider.GetEnterDungeon().LinkedDungeonData;
		if (linkedDungeonData != null)
		{
			OpenSubPartySetting openSubPartySetting = new OpenSubPartySetting();
			openSubPartySetting.BattleType = linkedDungeonData.BattleType;
			SingletonAuto<EventTriggerManager>.instance.RaiseEvent(openSubPartySetting);
		}
	}

	public void OnClickSetSubParty()
	{
		SwitchToSubParty();
	}

	public void OnToggleMainParty()
	{
		if (_currentSquard.GetMiniPortraitMainToggleValue())
		{
			SetupMiniPortrait(isMain: true);
			bool flag = _currentSquard.IsSelectedMainSquard();
			BattleType battleType = BattleType;
			if (battleType == BattleType.EventDungeon)
			{
				StartBattleUi.SetEventDungeonOverDefaultDeckCount(flag);
			}
			else
			{
				StartBattleUi.SetWorldBossButtonStatus(battleType, flag);
			}
		}
	}

	public void OnToggleSubParty()
	{
		if (_currentSquard.GetMiniPortraitSubToggleValue())
		{
			SwitchToSubParty();
		}
	}

	private IEnumerator coMoveScene()
	{
		yield return new WaitForSeconds(0.5f);
		GlobalAutoBehaviour<LoadingManager>.instance.MoveScene(LoadingManager.ESceneType.Battle2);
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		RefreshStaminaCount();
		UpdateSquard();
		UpdateAutoStart();
	}

	private void UpdateAutoStart()
	{
		if (AutoNextBattle.IsEnabled() && !_isAutoCampaign)
		{
			_autoCampaignElapsedTime += Time.deltaTime;
			if (_autoCampaignElapsedTime > AutoNextBattle.AutoCampaignDelayTime)
			{
				_isAutoCampaign = true;
				OnClickStartBattle();
			}
		}
	}

	private void UpdateSquard()
	{
		if (_currentSquard != null)
		{
			_currentSquard.UpdateHeroPortraitAnim();
		}
	}

	void ClassTagIconComponent.Listener.OnClick(ClassTagIconComponent comp)
	{
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<NGame2.NAccount.UserHeroManager.ChangeHeroInfo>.OnChanged(NGame2.NAccount.UserHeroManager.ChangeHeroInfo changedInfo)
	{
		OnChanged(changedInfo);
	}

	protected virtual void OnChanged(NGame2.NAccount.UserHeroManager.ChangeHeroInfo changedInfo)
	{
		if (changedInfo.ChangeType == NGame2.NAccount.UserHeroManager.HeroChangeType.Costume)
		{
			HeroPortraitComponent deckHeroPortrait = GetDeckHeroPortrait(changedInfo.HeroIndex, 0L);
			if (deckHeroPortrait != null)
			{
				deckHeroPortrait.SetHeroImage();
			}
			_currentSquard.GetSquardHeroProtraitAnim(changedInfo.HeroIndex, 0L)?.Component.SetHeroImage();
		}
		if (changedInfo.ChangeType != NGame2.NAccount.UserHeroManager.HeroChangeType.HeroLevelup)
		{
			return;
		}
		HeroPortraitComponent deckHeroPortrait2 = GetDeckHeroPortrait(changedInfo.HeroIndex, 0L);
		if (deckHeroPortrait2 != null)
		{
			deckHeroPortrait2.SetLevel(show: true);
		}
		List<Common.HeroPortraitAnim> allHeroPortraitAnimList = _currentSquard.GetAllHeroPortraitAnimList();
		if (__Extension_Method_ICollection__.COUNT(allHeroPortraitAnimList) > 0)
		{
			allHeroPortraitAnimList.Find((Common.HeroPortraitAnim portrait) => (portrait.HeroIndex == changedInfo.HeroIndex) ? true : false)?.Component.SetLevel(show: true);
		}
		HeroPortraitComponent miniSquardHeroProtrait = _currentSquard.GetMiniSquardHeroProtrait(changedInfo.HeroIndex);
		if (miniSquardHeroProtrait != null)
		{
			miniSquardHeroProtrait.SetLevel(show: true);
		}
	}

	public virtual void OnToggleOnetimeBootyBooster()
	{
		if (_onetimeBoosterPanel != null)
		{
			_onetimeBoosterPanel.OnToggleUseOnetimeBoosterItem();
		}
	}

	void ItemIconComponent.Listener.OnClick(ItemIconComponent itemIconComponent)
	{
		WindowPrefab<InvenItemDesc> windowPrefab = new WindowPrefab<InvenItemDesc>("Resources/Prefab2/Window/Inventory/InvenItemDesc2", GameUIRoot.ELayerType.UI);
		windowPrefab.instance.Open(itemIconComponent.ItemData, itemIconComponent.ItemCount, null);
		windowPrefab.instance.DisableAllButtons();
		GlobalAutoBehaviour<PopupManegement>.instance.Add(windowPrefab);
	}

	public int GetPartyMemberListCount()
	{
		int squardTotalHeroIndexCount = GetSquardTotalHeroIndexCount();
		if (squardTotalHeroIndexCount > 0)
		{
			return squardTotalHeroIndexCount;
		}
		return -1;
	}

	public List<Common.HeroPortraitAnim> GetAllSquardHeroPortraitAnimList()
	{
		return _currentSquard.GetAllHeroPortraitAnimList();
	}

	public void SetPickClassList()
	{
		_selectCreatureTagTypeList = _dataProvider.GetSelectCreatureTagTypeList();
	}

	public void SetAcceptedClassList()
	{
		_acceptedCreatureTagTypeList = _dataProvider.GetAcceptedCreatureTagTypeList();
		_acceptedClassStringBuilder.Clear();
		int i = 0;
		for (int num = __Extension_Method_ICollection__.COUNT(_acceptedCreatureTagTypeList); i < num; i++)
		{
			string stringDirect = LocalizationManager.GetStringDirect("CLASS_DESC_" + _acceptedCreatureTagTypeList[i].ToString().ToUpper());
			_acceptedClassStringBuilder.Append(stringDirect);
			if (i < num - 1)
			{
				_acceptedClassStringBuilder.Append(", ");
			}
		}
	}

	public void SetBannedHeroList()
	{
		_bannedHeroDataList = _dataProvider.GetBannedHeroDataList();
	}

	public void OnClickOpenClearParty()
	{
		OpenClearParty();
	}

	protected virtual void OpenClearParty()
	{
		if (_dataProvider != null)
		{
			EnterDungeonData enterDungeon = _dataProvider.GetEnterDungeon();
			int chapter = enterDungeon.Chapter;
			int linkedDungeon = enterDungeon.LinkedDungeon;
			NShared.DungeonDifficultyType difficulty = enterDungeon.Difficulty;
			SceneAutoBehaviour<InspectUserManagement>.instance.OpenRecommendClearPartyPopup(chapter, linkedDungeon, difficulty);
		}
	}

	private bool SetClearPartyButton()
	{
		ContentsStatus contentsStatus = SingletonAuto<ContentsStatusManager>.instance.GetContentsStatus(ContentsStatusType.ClearParty);
		if (contentsStatus == ContentsStatus.Disable || contentsStatus == ContentsStatus.Hide)
		{
			UIHelper.EnableGameObject(Button_ClearParty, enable: false);
			return false;
		}
		string[] valueArray = GameRoot.Instance.ConstantDataContainer.GetValueArray<string>("ClearPartyBattleTypes");
		if (__Extension_Method_ICollection__.COUNT(valueArray) == 0)
		{
			UIHelper.EnableGameObject(Button_ClearParty, enable: false);
			return false;
		}
		if (_dataProvider == null)
		{
			UIHelper.EnableGameObject(Button_ClearParty, enable: false);
			return false;
		}
		EnterDungeonData enterDungeon = _dataProvider.GetEnterDungeon();
		if (enterDungeon == null || enterDungeon.LinkedDungeonData == null)
		{
			UIHelper.EnableGameObject(Button_ClearParty, enable: false);
			return false;
		}
		if (enterDungeon.IsScenario)
		{
			UIHelper.EnableGameObject(Button_ClearParty, enable: false);
			return false;
		}
		if (!valueArray.Contains(enterDungeon.LinkedDungeonData.BattleType.ToString()))
		{
			UIHelper.EnableGameObject(Button_ClearParty, enable: false);
			return false;
		}
		UIHelper.EnableGameObject(Button_ClearParty, enable: true);
		return true;
	}

	protected virtual void SetDebugLabel()
	{
		if (!ClientValueManager.IsShowDungeonIndex)
		{
			UIHelper.EnableGameObject(Label_DebugText, enable: false);
			return;
		}
		EnterDungeonData enterDungeon = _dataProvider.GetEnterDungeon();
		if (enterDungeon == null)
		{
			UIHelper.EnableGameObject(Label_DebugText, enable: false);
			return;
		}
		CampaignDungeonData linkedDungeonData = enterDungeon.LinkedDungeonData;
		if (linkedDungeonData == null)
		{
			UIHelper.EnableGameObject(Label_DebugText, enable: false);
			return;
		}
		UIHelper.EnableGameObject(Label_DebugText, enable: true);
		UIHelper.SetLabelText(Label_DebugText, $"{linkedDungeonData.ChapterIndex},{linkedDungeonData.DungeonIndex}");
	}

	void IBoosterListObserver.OnChanged(BoosterInfo expiredBooster)
	{
		if (expiredBooster != null && expiredBooster.BonusType == EventBonusType.SaveStamina)
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("EQUIP_END_BOOSTER", expiredBooster.Name));
			ForceRefreshStaminaCount();
		}
	}

	void IUserObserver<NGame2.NAccount.UserEquipItemManager.EquipStatInfo>.OnChanged(NGame2.NAccount.UserEquipItemManager.EquipStatInfo changedInfo)
	{
		if (!_isSkipAnimatePosition)
		{
			_currentSquard.AnimateSqaurdPosition(null);
		}
	}

	void HeroPresetStorageWebManager.ILoadPresetListener.OnBeforeLoadPreset()
	{
		_isSkipAnimatePosition = true;
	}

	void HeroPresetStorageWebManager.ILoadPresetListener.OnAfterLoadPreset()
	{
		_isSkipAnimatePosition = false;
		_currentSquard.AnimateSqaurdPosition(null);
	}

	bool UserHeroListPanel.IListener.IsRestrictedHero(HeroInfo heroInfo)
	{
		if (heroInfo == null)
		{
			return false;
		}
		if (heroInfo.CreatureData == null)
		{
			return false;
		}
		int i = 0;
		for (int num = __Extension_Method_ICollection__.COUNT(_bannedHeroDataList); i < num; i++)
		{
			CreatureData creatureData = _bannedHeroDataList[i];
			if (creatureData != null && creatureData.Index == heroInfo.CreatureData.Index)
			{
				return true;
			}
		}
		if (_acceptedCreatureTagTypeList == null)
		{
			return false;
		}
		int j = 0;
		for (int num2 = __Extension_Method_ICollection__.COUNT(_acceptedCreatureTagTypeList); j < num2; j++)
		{
			if (heroInfo.CreatureData.TagType == _acceptedCreatureTagTypeList[j])
			{
				return false;
			}
		}
		return true;
	}

	bool UserHeroListPanel.IListener.hasAcceptedHeroes()
	{
		return __Extension_Method_ICollection__.COUNT(_acceptedCreatureTagTypeList) > 0;
	}

	bool UserHeroListPanel.IListener.hasBannedHeroes()
	{
		return __Extension_Method_ICollection__.COUNT(_bannedHeroDataList) > 0;
	}

	void SkillBookSelectRewardPopup.IListener.TryStartBattle(ExtraOptionData extraOption)
	{
	}

	void SkillBookSelectRewardPopup.IListener.GetReward(string[] itemCodes, Action enableButtonAction)
	{
	}

	StaminaType LobbyTopmostMenu.ICustomStaminaObserver.StaminaType
	{
		get
		{
			if (BattleType == BattleType.WorldBoss)
			{
				return StaminaType.WorldBossGlobalTicket;
			}
			return StaminaType;
		}
	}
}
