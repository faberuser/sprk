using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NBattleField;
using NGame2.NContentsDefine;
using NGame2.NContentsInfo;
using NGame2.NEventTrigger;
using NGame2.NMatch;
using NGame2.NOrdeal;
using NGame2.NScene;
using NGame2.NTutorial;
using NGame2.NUI.NComponent2.NOrdeal;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWindow.NGlobalTopmost;
using NGame2.NUI.NWorldMap;
using NShared;
using NShared.EndOrdealArena;
using NShared.NEvent;
using NShared.NLocalization;
using NShared.OrdealInfo;
using NShared.RefreshNode;
using NShared.ResurrectionHero;
using NShared.SelectBuff;
using NShared.SelectTier;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager;

public class OrdealArenaManagement : BaseManagement<OrdealArenaManagement>, OrdealArenaSelectWindow.IListener, OrdealArenaTierSelectComponent.IListener, OrdealArenaMapWindow.IListener, IEventListener, ITouchDisabler, OrdealArenaManager.IListener, BattlePartySetting.Listener, OrdealArenaPvpResult.IListener, PvpStatus.Listener, IUserObserver<OrdealArenaInfoChanged>, IUserObserver, OrdealArenaBuffSelectPopup.IListener
{
	public interface IUpdater
	{
		bool Update();
	}

	public class Updater : IUpdater
	{
		private Action _updateAction;

		private Func<bool> _checker;

		public bool Set(Func<bool> checker, Action updateAction)
		{
			if (updateAction == null || checker == null)
			{
				return false;
			}
			_updateAction = updateAction;
			_checker = checker;
			return true;
		}

		public bool Update()
		{
			if (_updateAction == null || _checker == null || !_checker())
			{
				return false;
			}
			_updateAction();
			return true;
		}
	}

	public class WindowUpdater<T> : IUpdater where T : BaseWindow
	{
		private T _window;

		private Action _updateAction;

		public bool Set(T window, Action updateAction)
		{
			if (window == null || updateAction == null)
			{
				return false;
			}
			_window = window;
			_updateAction = updateAction;
			return true;
		}

		public bool Update()
		{
			if (_window == null || !_window.IsOpen() || _updateAction == null)
			{
				return false;
			}
			_updateAction();
			return true;
		}
	}

	private Dictionary<Type, Action<IEvent>> _dicEvent = new Dictionary<Type, Action<IEvent>>();

	private bool _isSeasonExperiod;

	private bool _isFirstEnter;

	private List<IUpdater> _listUpdater = new List<IUpdater>();

	private DateTime _checkTime;

	private string _heroRemainResetTime;

	public bool IsOrdealArenaMap { get; set; }

	bool ITouchDisabler.IsValid => true;

	protected override void Awake()
	{
		base.Awake();
		AddObserver();
		AddListener();
		AddWindows();
		RegistEventHandlers();
	}

	protected override void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			base.OnDestroy();
			RemoveObserver();
			RemoveListener();
		}
	}

	private void AddListener()
	{
		GlobalAutoBehaviour<OrdealArenaManager>.instance.AddListener(this);
	}

	private void RemoveListener()
	{
		if (GlobalAutoBehaviour<OrdealArenaManager>.isValidInstance)
		{
			GlobalAutoBehaviour<OrdealArenaManager>.instance.RemoveListener(this);
		}
	}

	private void AddObserver()
	{
		GlobalAutoBehaviour<OrdealArenaManager>.instance.AddObserver(this);
	}

	private void RemoveObserver()
	{
		if (GlobalAutoBehaviour<OrdealArenaManager>.isValidInstance)
		{
			GlobalAutoBehaviour<OrdealArenaManager>.instance.RemoveObserver(this);
		}
	}

	public void AddUpdateWindow<T>(T window, Action updateAction) where T : BaseWindow
	{
		WindowUpdater<T> windowUpdater = new WindowUpdater<T>();
		if (windowUpdater.Set(window, updateAction))
		{
			_listUpdater.Add(windowUpdater);
		}
	}

	public void AddUpdater(Func<bool> checker, Action updateAction)
	{
		Updater updater = new Updater();
		if (updater.Set(checker, updateAction))
		{
			_listUpdater.Add(updater);
		}
	}

	private void AddWindows()
	{
		AddInstance<OrdealArenaMapWindow>("Resources/Prefab2/Window/OrdealArena/OrdealArenaMapWindow", isPassiveWindow: true, preload: true);
		AddInstance<OrdealArenaSelectWindow>("Resources/Prefab2/Window/OrdealArena/OrdealArenaSelectWindow");
		AddInstance<OrdealArenaBattlePartySetting>("Resources/Prefab2/Window/OrdealArena/OrdealArenaBattlePartySetting");
		AddInstance<OrdealArenaEventWindow>("Resources/Prefab2/Window/OrdealArena/OrdealArenaEventWindow");
		AddInstance<OrdealArenaRewardInfoPopup>("Resources/Prefab2/Window/OrdealArena/OrdealArenaRewardInfoPopup");
		AddInstance<OrdealArenaPvpResult>("Resources/Prefab2/Window/OrdealArena/OrdealArenaPvpResult");
		AddInstance<BattleStats>("Resources/Prefab2/Window/Battle/BattleStats");
		AddInstance<PvpStatus>("Resources/Prefab2/Window/Battle/PvpStatus");
	}

	public void OpenSelectWindowWindow()
	{
		OrdealArenaSelectWindow ordealArenaSelectWindow = GetInstance<OrdealArenaSelectWindow>();
		if (!(ordealArenaSelectWindow == null) && !ordealArenaSelectWindow.IsOpen())
		{
			ordealArenaSelectWindow.Open(this);
		}
	}

	public void OpenRewardInfoPopup()
	{
		OrdealArenaRewardInfoPopup ordealArenaRewardInfoPopup = GetInstance<OrdealArenaRewardInfoPopup>();
		if (!(ordealArenaRewardInfoPopup == null) && !ordealArenaRewardInfoPopup.IsOpen())
		{
			ordealArenaRewardInfoPopup.WindowOpen();
		}
	}

	public void OpenEventWindow(OrdealArenaNodeData nodeData, NShared.EndOrdealArena.Response res)
	{
		if (nodeData != null && res != null)
		{
			OrdealArenaEventWindow ordealArenaEventWindow = GetInstance<OrdealArenaEventWindow>();
			if (!(ordealArenaEventWindow == null) && !ordealArenaEventWindow.IsOpen())
			{
				OrdealNodeInfo ordealNodeInfo = GlobalAutoBehaviour<OrdealArenaManager>.instance.GetOrdealNodeInfo(nodeData.Index);
				ordealArenaEventWindow.Open(ordealNodeInfo, res, null);
			}
		}
	}

	public void OpenPvpResult(NShared.EndOrdealArena.Response response)
	{
		OrdealArenaPvpResult ordealArenaPvpResult = GetInstance<OrdealArenaPvpResult>();
		if (!(ordealArenaPvpResult == null) && !ordealArenaPvpResult.IsOpen())
		{
			ordealArenaPvpResult.Open(this, response);
		}
	}

	public void OpenStatus(PvpStatus.MatchUserInfo userInfo1, PvpStatus.MatchUserInfo userInfo2)
	{
		PvpStatus pvpStatus = GetInstance<PvpStatus>();
		if (!(pvpStatus == null) && !pvpStatus.IsOpen())
		{
			NGame2.NContentsInfo.MatchArena.MatchSeasonInfo matchSeasonInfo = SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.GetMatchSeasonInfo(MatchPvpType.Normal);
			pvpStatus.Open(userInfo1, userInfo2, this, MatchArenaType.Ordeal, matchSeasonInfo);
		}
	}

	public void OpenBuffSelectPopup(bool isLobbyType = false)
	{
		if (GlobalAutoBehaviour<PopupManegement>.isValidInstance && !(GlobalAutoBehaviour<PopupManegement>.instance.FindWindow<OrdealArenaBuffSelectPopup>() != null))
		{
			OrdealArenaBuffSelectPopup.Open(this, isLobbyType);
		}
	}

	int PvpStatus.Listener.GetRemainTime()
	{
		if (!SingletonAuto<BattleFacade>.isValidInstance)
		{
			return 0;
		}
		BattleFacade.BattleCacheData battleCache = SingletonAuto<BattleFacade>.instance.GetBattleCache();
		if (battleCache != null)
		{
			return (battleCache.RemainedBattleTimeMs + 999) / 1000;
		}
		return 0;
	}

	bool PvpStatus.Listener.IsBattleEnabled()
	{
		if (!SingletonAuto<BattleFacade>.isValidInstance)
		{
			return false;
		}
		return SingletonAuto<BattleFacade>.instance.IsBattleEnabled();
	}

	public void OpenBattlePartySetting()
	{
		OrdealArenaBattlePartySetting ordealArenaBattlePartySetting = GetInstance<OrdealArenaBattlePartySetting>();
		if (!(ordealArenaBattlePartySetting == null) && !ordealArenaBattlePartySetting.IsOpen())
		{
			int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
			int dungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
			EnterDungeonData enterDungeonData = new EnterDungeonData(chapterIndex, dungeonIndex, dungeonIndex);
			List<CachedHeroInfo> enemyList = GlobalAutoBehaviour<OrdealArenaManager>.instance.GetEnemyList(chapterIndex, dungeonIndex);
			OrdealArenaBattlePartySettingProvider dataProvider = new OrdealArenaBattlePartySettingProvider("ordealArena", enterDungeonData, enemyList);
			ordealArenaBattlePartySetting.Open(this, dataProvider);
		}
	}

	void OrdealArenaSelectWindow.IListener.OnClickRewardInfo()
	{
		OpenRewardInfoPopup();
	}

	void OrdealArenaTierSelectComponent.IListener.OnClickButton(MatchTierType selectTierType)
	{
		if (GameRoot.Instance.OrdealArenaTierDataContainer.GetData(selectTierType) != null)
		{
			CurrencyMaxCap.WarningOrExecute(CurrencyType.OrdealArenaPoint, delegate
			{
				TierSelectAndStart(selectTierType);
			});
		}
	}

	private void TierSelectAndStart(MatchTierType selectTierType)
	{
		if (GlobalAutoBehaviour<OrdealArenaManager>.instance.IsSelctedTier())
		{
			MoveMap(selectTierType, isFirst: false);
			return;
		}
		string stringDirect = LocalizationManager.GetStringDirect("ORDEALARENA_POPUP_ENTERCONFIRM_TITLE");
		string tierText = OrdealHelper.GetTierText(selectTierType);
		string stringDirect2 = LocalizationManager.GetStringDirect("ORDEALARENA_POPUP_ENTERCONFIRM_DESC", tierText);
		MessagePopup3Button.Open(stringDirect, stringDirect2, null, delegate
		{
			GlobalAutoBehaviour<OrdealArenaManager>.instance.RequestSelectTier(selectTierType);
		}, NShared.NLocalization.Common.COMMON_NO, NShared.NLocalization.Common.COMMON_YES);
	}

	private void MoveOrdealAfter()
	{
		if (IsFirstEnter())
		{
			GlobalTopmostWindow.instance.AddTouchDisabler(this);
			MapLayout mapLayout = SceneAutoBehaviour<WorldMapManagement>.instance.GetInstance<MapLayout>();
			if (mapLayout != null)
			{
				float enterCameraDuration = OrdealHelper.GetEnterCameraDuration();
				mapLayout.MapViewer.ShowMapVertical(enterCameraDuration, delegate
				{
					GlobalTopmostWindow.instance.RemoveTouchDisabler(this);
					SingletonAuto<EventTriggerManager>.instance.RaiseEvent(new OpenOrdealArenaTierUI());
					AddOrdealArenaSeasonChecker();
					_isFirstEnter = false;
				});
			}
		}
		else
		{
			AddOrdealArenaSeasonChecker();
		}
	}

	public void AddOrdealArenaSeasonChecker()
	{
		AddUpdater(() => IsOrdealArenaMap, delegate
		{
			if ((!SingletonUpdater<TutorialManager>.isValidInstance || !SingletonUpdater<TutorialManager>.instance.IsProgressing) && GlobalAutoBehaviour<OrdealArenaManager>.instance.GetSeasonRemainTime().TotalSeconds < 0.0 && !_isSeasonExperiod)
			{
				_isSeasonExperiod = true;
				string stringDirect = LocalizationManager.GetStringDirect("ORDEALARENA_POPUP_SEASONEND_TITLE");
				string stringDirect2 = LocalizationManager.GetStringDirect("ORDEALARENA_POPUP_SEASONEND_DESC");
				MessagePopup.Open(stringDirect, stringDirect2, delegate
				{
					GlobalAutoBehaviour<OrdealArenaManager>.instance.ResetAll();
					OutOrdealArena();
				});
			}
		});
	}

	private bool IsFirstEnter()
	{
		return _isFirstEnter;
	}

	public void Open()
	{
	}

	public void ExecuteNodeFunction(NodeFunctionType functionType)
	{
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		int dungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
		if (!CanExecuteNodeFunction(chapterIndex, dungeonIndex))
		{
			return;
		}
		if (GlobalAutoBehaviour<OrdealArenaManager>.instance.HasSelectableBuff())
		{
			OpenBuffSelectPopup(isLobbyType: true);
			return;
		}
		switch (functionType)
		{
		case NodeFunctionType.OrdealArenaEvent:
		case NodeFunctionType.OrdealArenaEnd:
			GlobalAutoBehaviour<OrdealArenaManager>.instance.RequestEndOrdeaArena(chapterIndex, dungeonIndex);
			break;
		case NodeFunctionType.OrdealArenaBattle:
			OpenBattlePartySetting();
			break;
		case NodeFunctionType.OrdealArenaStart:
			break;
		}
	}

	public bool CanExecuteNodeFunction(int chapterIndex, int dungeonIndex)
	{
		return !GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(chapterIndex, dungeonIndex);
	}

	public void NodeRecreate(int chapterIndex, int clearFloor)
	{
		MapLayout mapLayout = SceneAutoBehaviour<WorldMapManagement>.instance.GetInstance<MapLayout>(openedOnly: true);
		if (mapLayout == null || !mapLayout.IsOpen())
		{
			return;
		}
		GameRoot.Instance.OrdealArenaNodeDataContainer.ForEachData(delegate(OrdealArenaNodeData data)
		{
			if (data.ChapterIndex == chapterIndex && data.Floor == clearFloor)
			{
				mapLayout.RecreateNode(data.DungeonIndex);
			}
		});
	}

	private void SetMapWindow(int chapterIndex, int dungeonIndex)
	{
		if (!IsOrdealArenaMap)
		{
			if (IsOpened<OrdealArenaMapWindow>())
			{
				GetInstance<OrdealArenaMapWindow>().Close2(immediately: true);
			}
			return;
		}
		OrdealArenaMapWindow ordealArenaMapWindow = GetInstance<OrdealArenaMapWindow>();
		if (ordealArenaMapWindow != null && !ordealArenaMapWindow.IsOpen())
		{
			ordealArenaMapWindow.Open(this);
			ordealArenaMapWindow.SetDungeonInfo(chapterIndex, dungeonIndex);
			if (GlobalAutoBehaviour<OrdealArenaManager>.instance.GetSeasonRemainTime().TotalSeconds > 0.0)
			{
				OpenBuffSelectPopup(isLobbyType: true);
			}
		}
	}

	private void SetDungeonInfo(int chapterIndex, int dungeonIndex)
	{
		OrdealArenaMapWindow ordealArenaMapWindow = GetInstance<OrdealArenaMapWindow>(openedOnly: true);
		if (!(ordealArenaMapWindow == null) && ordealArenaMapWindow.IsOpen())
		{
			ordealArenaMapWindow.SetDungeonInfo(chapterIndex, dungeonIndex);
		}
	}

	private void OutOrdealArena()
	{
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		int dungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
		CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(chapterIndex, dungeonIndex);
		int chapterIndex2 = ((data == null) ? 99 : ArgParser.GetValueInt(data.ResetNodeIndex, 0, 99));
		int dungeonIndex2 = ((data == null) ? 4 : ArgParser.GetValueInt(data.ResetNodeIndex, 1, 4));
		if (GlobalAutoBehaviour<PopupManegement>.isValidInstance)
		{
			GlobalAutoBehaviour<PopupManegement>.instance.CloseAll();
		}
		if (GlobalAutoBehaviour<GlobalManagement>.isValidInstance)
		{
			GlobalAutoBehaviour<GlobalManagement>.instance.CloseAllWindows();
		}
		if (SceneAutoBehaviour<LobbyManagement>.isValidInstance)
		{
			_listUpdater.Clear();
			_isSeasonExperiod = false;
			_isFirstEnter = false;
			SceneAutoBehaviour<LobbyManagement>.instance.MoveMap(chapterIndex2, dungeonIndex2);
		}
	}

	void OrdealArenaMapWindow.IListener.OnClickBack()
	{
		OutOrdealArena();
	}

	void OrdealArenaMapWindow.IListener.OnClickBattlePartySetting()
	{
		if (GlobalAutoBehaviour<OrdealArenaManager>.instance.HasSelectableBuff())
		{
			OpenBuffSelectPopup(isLobbyType: true);
		}
		else
		{
			OpenBattlePartySetting();
		}
	}

	void OrdealArenaMapWindow.IListener.OnClickRefreshNode(int nodeIndex)
	{
		int remainRefreshCount = GlobalAutoBehaviour<OrdealArenaManager>.instance.GetRemainRefreshCount();
		int maxRefreshNodeCount = OrdealHelper.GetMaxRefreshNodeCount();
		if (remainRefreshCount > 0)
		{
			string stringDirect = LocalizationManager.GetStringDirect("ORDEALARENA_POPUP_CHANGEOPPONENT_TITLE");
			string stringDirect2 = LocalizationManager.GetStringDirect("ORDEALARENA_POPUP_CHANGEOPPONENT_DESC", remainRefreshCount, maxRefreshNodeCount);
			MessagePopup3Button.Open(stringDirect, stringDirect2, null, delegate
			{
				GlobalAutoBehaviour<OrdealArenaManager>.instance.RequestRefreshNode(nodeIndex, useGem: false);
			}, NShared.NLocalization.Common.COMMON_NO, NShared.NLocalization.Common.COMMON_YES);
			return;
		}
		int refreshNodeGemCount = OrdealHelper.GetRefreshNodeGemCount();
		long num = OrdealHelper.SprkTotalGem(GlobalAutoBehaviour<AccountManager>.instance.UserInfo);
		long num2 = num;
		int num3 = refreshNodeGemCount;
		bool isEnoughGem = num3 <= num2;
		string stringDirect3 = LocalizationManager.GetStringDirect("ORDEALARENA_POPUP_CHANGEOPPONENT_TITLE");
		string stringDirect4 = LocalizationManager.GetStringDirect("ORDEALARENA_POPUP_CHANGEOPPONENT_GEM");
		string stringDirect5 = LocalizationManager.GetStringDirect("ORDEALARENA_BUTTON_CHANGEOPPONENT_BUY");
		PreserveMessagePopup2.OpenPriceConfirm(stringDirect3, stringDirect4, stringDirect5, PreserveMessagePopup2.EPriceType.Gem, 100, delegate
		{
			if (!isEnoughGem)
			{
				string title = LocalizationManager.GetString(NShared.NLocalization.Common.ERROR);
				string message = LocalizationManager.GetString(NShared.NLocalization.System.MOVE_GEM_SHOP_CONFIRM);
				MessagePopup3Button.Open(title, message, null, delegate
				{
					SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Ruby.ToString());
				}, NShared.NLocalization.Common.COMMON_NO, NShared.NLocalization.Common.COMMON_YES);
			}
			else
			{
				GlobalAutoBehaviour<OrdealArenaManager>.instance.RequestRefreshNode(nodeIndex, useGem: true);
			}
		}, isEnoughGem);
	}

	void OrdealArenaMapWindow.IListener.OnClickRecievedReward()
	{
		if (GlobalAutoBehaviour<OrdealArenaManager>.instance.HasSelectableBuff())
		{
			OpenBuffSelectPopup(isLobbyType: true);
			return;
		}
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		int dungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
		GlobalAutoBehaviour<OrdealArenaManager>.instance.RequestEndOrdeaArena(chapterIndex, dungeonIndex);
	}

	void OrdealArenaMapWindow.IListener.OnClickStartEvent()
	{
		if (GlobalAutoBehaviour<OrdealArenaManager>.instance.HasSelectableBuff())
		{
			OpenBuffSelectPopup(isLobbyType: true);
			return;
		}
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		int dungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
		GlobalAutoBehaviour<OrdealArenaManager>.instance.RequestEndOrdeaArena(chapterIndex, dungeonIndex);
	}

	bool BattlePartySetting.Listener.OnCloseRequest(BattlePartySetting closeThis)
	{
		return true;
	}

	bool BattlePartySetting.Listener.OpenAutoNextBattleSelect()
	{
		return false;
	}

	void BattlePartySetting.Listener.StartBattle(EnterDungeonData enterDungeon, BattlePartySetting.BattlePartyInfo battlePartyInfo, BattlePartySetting.ExtraOptionData extraOption)
	{
		int chapter = enterDungeon.Chapter;
		int linkedDungeon = enterDungeon.LinkedDungeon;
		int[] heroIndices = battlePartyInfo.heroIndices;
		CloseAllActiveWindow();
		GameRoot.Instance.CampaignNodeDataContainer.GetData(chapter, linkedDungeon);
		GlobalAutoBehaviour<OrdealArenaManager>.instance.RequestBeginOrdealArena(chapter, linkedDungeon, heroIndices);
	}

	void BattlePartySetting.Listener.OpenHeroDetailView(CachedHeroInfo heroInfo)
	{
	}

	void OrdealArenaManager.IListener.OnBeginOrdealArena(int chapterIndex, int dungeonIndex, int[] heroIndices, OrdealNodeInfo nodeInfo)
	{
		MatchedAccountInfo matchedAccountInfo = OrdealHelper.NpcMatchAccountInfo(nodeInfo);
		MatchedAccountInfo matchedAccountInfo2 = OrdealHelper.MyMatchAccountInfo(heroIndices);
		SingletonAuto<BattleContext>.instance.SetOrdealArena(heroIndices, 0, matchedAccountInfo, chapterIndex, dungeonIndex, matchedAccountInfo2);
		InitVsLoadingPlayer(matchedAccountInfo2, isLeft: true);
		InitVsLoadingPlayer(matchedAccountInfo, isLeft: false);
		SingletonAuto<BattleContext>.instance.MoveToBattleScene(LoadingManager.ELoadingType.None);
	}

	public void InitVsLoadingPlayer(MatchedAccountInfo accountInfo, bool isLeft)
	{
		if (accountInfo == null)
		{
			return;
		}
		int avatarHeroIndex = accountInfo.UserInfo.AvatarHeroIndex;
		string nick = accountInfo.UserInfo.Nick;
		Dictionary<int, CachedHeroInfo>.ValueCollection values = accountInfo.HeroInfos.Values;
		_ = accountInfo.FightingPower;
		PlayerBattleInfo battleInfo = accountInfo.BattleInfo;
		VisualTierRank visualTierRank = VisualTierRank.CreateFromServer(accountInfo.IsNpc ? (-1) : accountInfo.Rank);
		int worldTierIndex = battleInfo.WorldTierIndex;
		_ = accountInfo.Mmr;
		int winPercentage = -1;
		int num = 0;
		int num2 = 0;
		int successiveWin = 0;
		if (battleInfo != null)
		{
			num = battleInfo.SeasonWin;
			num2 = battleInfo.SeasonLose;
			successiveWin = battleInfo.SuccessiveWin;
			int num3 = num + num2;
			winPercentage = ((!accountInfo.IsNpc && num3 > 0) ? (accountInfo.BattleInfo.SeasonWin * 100 / num3) : (-1));
			_ = battleInfo.MatchScore;
		}
		SingletonUpdater<MatchManager>.instance.SetMatchArenaType(MatchArenaType.Normal);
		PvpLoading pvpLoading = PvpLoading.instance;
		if (!(pvpLoading == null))
		{
			if (isLeft)
			{
				pvpLoading.SetOrdealArenaBg();
				pvpLoading.InitOrdealLeft(MatchDuelType.OrdealArena, winPercentage, num, num2, successiveWin, worldTierIndex, visualTierRank, nick, avatarHeroIndex, values);
			}
			else
			{
				pvpLoading.InitOrdealRight(MatchDuelType.OrdealArena, winPercentage, num, num2, successiveWin, worldTierIndex, visualTierRank, nick, avatarHeroIndex, values);
			}
		}
	}

	void OrdealArenaManager.IListener.OnEndOrdealArena(OrdealArenaNodeData ordealNodeData, NShared.EndOrdealArena.Response res)
	{
		if (ordealNodeData != null)
		{
			switch (ordealNodeData.NodeType)
			{
			case EOrdealArenaNodeType.Battle:
				OpenPvpResult(res);
				break;
			case EOrdealArenaNodeType.Event:
				OpenEventWindow(ordealNodeData, res);
				break;
			case EOrdealArenaNodeType.End:
				OrdealHelper.OpenAreaRewardPopup(res.AreaRewardInfo, null);
				break;
			}
		}
	}

	void OrdealArenaManager.IListener.OnOrdealInfo(NShared.OrdealInfo.Response res)
	{
		OpenSelectWindowWindow();
	}

	void OrdealArenaManager.IListener.OnRefreshNode(NShared.RefreshNode.Response res)
	{
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		int dungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
		SetDungeonInfo(chapterIndex, dungeonIndex);
	}

	void OrdealArenaManager.IListener.OnResurrectionHero(int resurrectionHeroIndex, NShared.ResurrectionHero.Response res)
	{
		OrdealArenaBattlePartySetting ordealArenaBattlePartySetting = GetInstance<OrdealArenaBattlePartySetting>(openedOnly: true);
		if (ordealArenaBattlePartySetting != null && ordealArenaBattlePartySetting.IsOpen())
		{
			ordealArenaBattlePartySetting.ResetDeadHero(resurrectionHeroIndex);
		}
	}

	void OrdealArenaManager.IListener.OnSelectBuff(NShared.SelectBuff.Response res)
	{
		OrdealArenaMapWindow ordealArenaMapWindow = GetInstance<OrdealArenaMapWindow>(openedOnly: true);
		if (!(ordealArenaMapWindow == null))
		{
			ordealArenaMapWindow.SetBuffList();
		}
	}

	void OrdealArenaManager.IListener.OnSelectTier(NShared.SelectTier.Response res)
	{
		MoveMap(res.SelectedTier, isFirst: true);
	}

	private void MoveMap(MatchTierType matchTierType, bool isFirst)
	{
		OrdealArenaTierData data = GameRoot.Instance.OrdealArenaTierDataContainer.GetData(matchTierType);
		if (data != null)
		{
			_isFirstEnter = isFirst;
			int moveChapterIndex = data.MoveChapterIndex;
			int lastClearDungeonIndex = GlobalAutoBehaviour<OrdealArenaManager>.instance.GetLastClearDungeonIndex(moveChapterIndex);
			SceneAutoBehaviour<EntryMenuManagement>.instance.CloseEntryMenu(null);
			SceneAutoBehaviour<LobbyManagement>.instance.MoveMap(moveChapterIndex, lastClearDungeonIndex, delegate
			{
				GlobalAutoBehaviour<GlobalManagement>.instance.CloseAllWindows();
			}, MoveOrdealAfter);
		}
	}

	private void RegistEventHandlers()
	{
		SingletonAuto<EventTriggerManager>.instance.AddEventListener(this);
		RegistEventHandler<ChapterMove>(ChapterMoveHandler);
		RegistEventHandler<PlayerMoveBegin>(PlayerMoveBeginHandler);
		RegistEventHandler<PlayerMoveEnd>(PlayerMoveEndHandler);
		RegistEventHandler<PlayerVisitNode>(PlayerVisitNodeHandler);
	}

	private void RegistEventHandler<T>(Action<IEvent> action) where T : IEvent
	{
		Type typeFromHandle = typeof(T);
		_dicEvent.Remove(typeFromHandle);
		_dicEvent.Add(typeFromHandle, action);
	}

	private void ChapterMoveHandler(IEvent triggerEvent)
	{
		int chapterIndex = (triggerEvent as ChapterMove).ChapterIndex;
		int dungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
		if (GameRoot.Instance.OrdealArenaNodeDataContainer.IsOrdealArenaChapter(chapterIndex))
		{
			IsOrdealArenaMap = true;
		}
		else
		{
			IsOrdealArenaMap = false;
		}
		SetMapWindow(chapterIndex, dungeonIndex);
	}

	private void PlayerMoveBeginHandler(IEvent triggerEvent)
	{
		_ = IsOrdealArenaMap;
	}

	private void PlayerMoveEndHandler(IEvent triggerEvent)
	{
		if (IsOrdealArenaMap)
		{
			PlayerMoveEnd obj = triggerEvent as PlayerMoveEnd;
			int chapterIndex = obj.ChapterIndex;
			int dungeonIndex = obj.DungeonIndex;
			SetDungeonInfo(chapterIndex, dungeonIndex);
		}
	}

	private void PlayerVisitNodeHandler(IEvent triggerEvent)
	{
		_ = IsOrdealArenaMap;
	}

	void IEventListener.OnEvent(IEvent triggerEvent)
	{
		if (triggerEvent != null)
		{
			Type type = triggerEvent.GetType();
			if (_dicEvent.ContainsKey(type))
			{
				Vespa_System_Action_Extension_Method.fire(_dicEvent[type], triggerEvent);
			}
		}
	}

	bool IUserObserver.IsGlobal()
	{
		return false;
	}

	void IUserObserver<OrdealArenaInfoChanged>.OnChanged(OrdealArenaInfoChanged changedInfo)
	{
		if (changedInfo == null || !IsOrdealArenaMap)
		{
			return;
		}
		switch (changedInfo.Type)
		{
		case EOrdealArenaInfoChangeType.CompleteFloor:
		{
			OrdealNodeInfo changedNodeInfo = changedInfo.ChangedNodeInfo;
			if (changedNodeInfo != null)
			{
				int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
				int floor = changedNodeInfo.Floor;
				NodeRecreate(chapterIndex, floor);
				OrdealArenaMapWindow ordealArenaMapWindow = GetInstance<OrdealArenaMapWindow>(openedOnly: true);
				if (ordealArenaMapWindow != null && ordealArenaMapWindow.IsOpen())
				{
					int dungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
					ordealArenaMapWindow.SetDungeonInfo(chapterIndex, dungeonIndex);
				}
			}
			break;
		}
		case EOrdealArenaInfoChangeType.ResetDeadHeros:
		{
			OrdealArenaBattlePartySetting ordealArenaBattlePartySetting = GetInstance<OrdealArenaBattlePartySetting>(openedOnly: true);
			if (!(ordealArenaBattlePartySetting == null) && ordealArenaBattlePartySetting.IsOpen())
			{
				ordealArenaBattlePartySetting.ResetDeadHeros();
			}
			break;
		}
		case EOrdealArenaInfoChangeType.SessionExpried:
			break;
		}
	}

	void OrdealArenaPvpResult.IListener.OnClickStatics()
	{
		BattleInstance stoppedBattleInstance = SingletonAuto<BattleFacade>.instance.GetStoppedBattleInstance();
		if (stoppedBattleInstance != null)
		{
			BattleStats battleStats = GetInstance<BattleStats>();
			if (!(battleStats == null) && !battleStats.IsOpen())
			{
				battleStats.Open(stoppedBattleInstance);
			}
		}
	}

	void OrdealArenaPvpResult.IListener.OnClickLobby()
	{
		GlobalAutoBehaviour<OrdealArenaManager>.instance.ReturnToLobby(LoadingManager.ELoadingType.Default);
	}

	void OrdealArenaBuffSelectPopup.IListener.OnClickConfimeBuffSelect(OrdealArenaBuffData data)
	{
		OrdealArenaBuffSelectPopup ordealArenaBuffSelectPopup = GlobalAutoBehaviour<PopupManegement>.instance.FindWindow<OrdealArenaBuffSelectPopup>();
		if (ordealArenaBuffSelectPopup != null && ordealArenaBuffSelectPopup.IsOpen())
		{
			ordealArenaBuffSelectPopup.Close();
		}
		int index = data.Index;
		GlobalAutoBehaviour<OrdealArenaManager>.instance.RequestSelectBuff(index);
		OrdealArenaEventWindow ordealArenaEventWindow = GetInstance<OrdealArenaEventWindow>(openedOnly: true);
		if (ordealArenaEventWindow != null && ordealArenaEventWindow.IsOpen())
		{
			ordealArenaEventWindow.Close();
		}
	}

	protected override void ChainUpdate()
	{
		base.ChainUpdate();
		if (_listUpdater.Count == 0)
		{
			return;
		}
		DateTime now = DateTime.Now;
		if ((now - _checkTime).TotalSeconds < 1.0)
		{
			return;
		}
		_checkTime = now;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listUpdater); i++)
		{
			IUpdater updater = _listUpdater[i];
			if (updater == null || !updater.Update())
			{
				_listUpdater.RemoveAt(i);
				i--;
			}
		}
	}

	public string GetHeroRemainResetTimeText()
	{
		TimeSpan nextMidnightRemainTime = GlobalAutoBehaviour<OrdealArenaManager>.instance.GetNextMidnightRemainTime();
		_heroRemainResetTime = OrdealHelper.GetRemainTimeText(nextMidnightRemainTime);
		return _heroRemainResetTime;
	}
}
