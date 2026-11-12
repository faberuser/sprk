using System;
using System.Linq;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NEventTrigger;
using NGame2.NScene;
using NGame2.NScene.NLobby;
using NGame2.NUI.NHelper.NTower;
using NGame2.NUI.NManager.NTower;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWindow.NTower;
using NGame2.NUtil;
using NGame2.NUtil.NDataHelper;
using NShared;
using NShared.BeginCampaign;
using NShared.GetMazeTowerInfo;
using NShared.GetTowerNpcInfo;
using NShared.NEvent;
using NShared.NLocalization;
using NShared.ReceiveMazeTowerReward;
using NShared.ReceiveTowerReward;
using NShared.ResetTower;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NTask;
using NVespa.NTask.NRuntime;
using UnityEngine;

namespace NGame2.NUI.NManager;

public class TowerManagement : BaseManagement<TowerManagement>, BattlePartySetting.Listener, BattlePartySetting.PreviewAnimationListener, BattleDungeonInfo.Listener, ContentUIGatewayComponent.IListener
{
	public interface IImplement
	{
		void Open(TowerData towerData, TowerInfo towerInfo, int showFloor, bool showClearEffect, bool returnFromBattle, Action onOpen);
	}

	private int MazeTowerRewardStep = 1;

	private bool _isTowerBattle = true;

	private int _gatewayTowerIndex;

	private int _gatewayTowerFloor;

	private IImplement _impl;

	public int ChapterIndex => GetChapterIndex();

	public int DungeonIndex => GetDungeonIndex();

	public TowerData TowerData { get; private set; }

	public TowerFloorData FloorData { get; private set; }

	public TowerInfo TowerInfo { get; private set; }

	public TowerNpcCache NpcCache { get; private set; }

	public TowerHeroCache HeroCache { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		UseBackgroundBlack(use: true);
		AddInstance<TowerWindow>("Resources/Prefab2/Window/Tower/TowerWindow");
		AddInstance<LabyrinthWindow>("Resources/Prefab2/Window/Tower/LabyrinthWindow");
		AddInstance<PitDungeonInfo>("Resources/Prefab2/Window/Tower/PitDungeonInfo");
		AddInstance<DungeonRewardInfo>("Resources/Prefab2/Window/Dungeon/DungeonRewardInfo");
		AddInstance<BossMonsterInfo>("Resources/Prefab2/Window/Dungeon/BossMonsterInfo");
		AddInstance<TowerDungeonInfo>("Resources/Prefab2/Window/Tower/TowerDungeonInfo");
		AddInstance<BattleDungeonInfo>("Resources/Prefab2/Window/Dungeon/BattleDungeonInfo");
		AddInstance<PartyBattleDungeonInfo>("Resources/Prefab2/Window/Dungeon/PartyBattleDungeonInfo");
		AddInstance<BattlePartySetting>("Resources/Prefab2/Window/BattlePartySetting/BattlePartySetting");
		AddInstance<OrdealTowerIntro>("Resources/Prefab2/Window/Tower/OrdealTowerIntro");
		AddInstance<OrdealTowerWindow>("Resources/Prefab2/Window/Tower/OrdealTowerWindow");
		AddInstance<ChallengeTowerWindow>("Resources/Prefab2/Window/Tower/ChallengeTowerWindow");
		AddInstance<MazeTowerList>("Resources/Prefab2/Window/Tower/MazeTowerList");
		AddInstance<MazeTowerWindow>("Resources/Prefab2/Window/Tower/MazeTowerWindow");
		AddInstance<ContentUIGatewayMain>("Resources/Prefab2/Window/Tower/ContentUIGatewayMain");
	}

	private IImplement CreateImpl(TowerType towerType)
	{
		return towerType switch
		{
			TowerType.Tower => new TowerImpl(this), 
			TowerType.Labyrinth => new LabyrinthImpl(this), 
			TowerType.Ordeal => new OrdealTowerImpl(this), 
			TowerType.Challenge => new ChallengeTowerImpl(this), 
			TowerType.Maze => new MazeTowerImpl(this), 
			TowerType.GatewaytoTrial => new GatewayImpl(this), 
			_ => null, 
		};
	}

	public void OpenTower(int towerIndex, int showFloor = 0, bool showClearEffect = false, bool returnFromBattle = false, Action onOpen = null)
	{
		_isTowerBattle = true;
		TowerData data = GameRoot.Instance.TowerDataContainer.GetData(towerIndex);
		TowerInfo towerInfo = GlobalAutoBehaviour<AccountManager>.instance.TowerManager.Get(towerIndex);
		OpenTower(data, towerInfo, showFloor, showClearEffect, returnFromBattle, onOpen);
	}

	public void OpenGatewayUI(Action onOpen)
	{
		_isTowerBattle = false;
		ContentUIGatewayMain contentUIGatewayMain = GetInstance<ContentUIGatewayMain>();
		if (contentUIGatewayMain != null)
		{
			contentUIGatewayMain.Open(this, onOpen);
		}
	}

	public void OpenGatewayDungeonInfoUI(int chapterIndex, int dungeonIndex, int rewardIndex, GatewayToTrialData data)
	{
		BattleDungeonInfo battleDungeonInfo = GetInstance<BattleDungeonInfo>();
		if (!(battleDungeonInfo == null))
		{
			EnterDungeonData enterDungeonData = DungeonHelper.GetEnterDungeonData(chapterIndex, dungeonIndex);
			RewardData data2 = GameRoot.Instance.RewardDataContainer.GetData(rewardIndex);
			battleDungeonInfo.PresetCustomRewardData(data2);
			battleDungeonInfo.PresetShowHeroReward(show: true);
			battleDungeonInfo.Open(enterDungeonData, this);
			battleDungeonInfo.SetupDungeonName(LocalizationManager.GetStringDirect(data.DungeonName, data.Step));
		}
	}

	private bool OpenAutoNextOnGateway()
	{
		GetInstance<BattlePartySetting>().TryStartBattle(null);
		return true;
	}

	private void StartGatewayBattle(EnterDungeonData enterDungeon, BattlePartySetting.BattlePartyInfo battlePartyInfo, BattlePartySetting.ExtraOptionData extraOption)
	{
		NShared.BeginCampaign.Request request = new NShared.BeginCampaign.Request
		{
			ChapterIndex = enterDungeon.Chapter,
			DungeonIndex = enterDungeon.OriginalDungeon,
			LinkedDungeonIndex = enterDungeon.LinkedDungeon,
			HeroIndices = battlePartyInfo.heroIndices,
			LeaderHeroIndex = battlePartyInfo.leaderHeroIndex,
			AccountIds = battlePartyInfo.accountIds,
			Repeat = false,
			TowerIndex = _gatewayTowerIndex,
			TowerFloor = _gatewayTowerFloor,
			AffixIndices = battlePartyInfo.AffixIndices
		};
		SingletonAuto<BattleContext>.instance.BeginCampaign(request, delegate
		{
			LobbyScene2.ReturnState.DisabledFlag |= LobbyScene2.ReturnStateOpenFlag.OpenHeroDeck;
			LobbyScene2.ListReturnState.Add(new TowerReturnState(_gatewayTowerIndex, _gatewayTowerFloor, TowerType.GatewaytoTrial));
		});
	}

	private void OpenTower(TowerData towerData, TowerInfo towerInfo, int showFloor, bool showClearEffect, bool returnFromBattle, Action onOpen)
	{
		if (towerData == null)
		{
			return;
		}
		TowerType type = towerData.Type;
		IImplement impl = CreateImpl(type);
		if (impl == null)
		{
			VespaDebug.LogError("Failed to create tower impl : " + type);
			return;
		}
		TowerData = towerData;
		TowerInfo = towerInfo;
		GroupTask groupTask = new GroupTask();
		if (towerData.UseNpc)
		{
			groupTask.AddAsyncTask(delegate(Action callback)
			{
				RequestTowerNpcInfo(towerData.TowerIndex, callback);
			});
		}
		groupTask.AddTask(delegate
		{
			impl.Open(towerData, towerInfo, showFloor, showClearEffect, returnFromBattle, onOpen);
			_impl = impl;
		});
		AddTask(groupTask);
		if (showFloor != 0)
		{
			InitFloorData(showFloor);
		}
		else
		{
			FloorData = null;
		}
	}

	private bool OpenAutoNextOnTower()
	{
		MessagePopup3Button.Open(LocalizationManager.GetStringDirect("TOWER_SELECT_TITLE"), LocalizationManager.GetStringDirect("TOWER_CONTINUOUS_BATTLE_DETAIL"), button1Label: LocalizationManager.GetStringDirect("START_BATTLE"), button2Label: LocalizationManager.GetStringDirect("TOWER_CONTINUOUS_BATTLE_BUTTON"), onButton1: delegate
		{
			GetInstance<BattlePartySetting>().TryStartBattle(null);
		}, onButton2: delegate
		{
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			string stringDirect = LocalizationManager.GetStringDirect("TOWER_CONTINUOUS_BATTLE_MESSAGE");
			MessagePopup.OpenEnabled(title, stringDirect, delegate
			{
				BattlePartySetting.ExtraOptionData extraOption = new BattlePartySetting.ExtraOptionData
				{
					AutoNext = true
				};
				GetInstance<BattlePartySetting>().TryStartBattle(extraOption);
			}, null, "COMMON_CONFIRM", bYesEnabled: true);
		});
		return true;
	}

	private void ReloadTowerData(int towerIndex)
	{
		GroupTask groupTask = new GroupTask();
		TowerData towerData = GameRoot.Instance.TowerDataContainer.GetData(towerIndex);
		if (towerData.UseNpc)
		{
			groupTask.AddAsyncTask(delegate(Action callback)
			{
				RequestTowerNpcInfo(towerData.TowerIndex, callback);
			});
		}
		AddTask(groupTask);
	}

	public void RefreshTowerClearGauage()
	{
		MazeTowerList mazeTowerList = GetInstance<MazeTowerList>();
		if (!(mazeTowerList == null) && mazeTowerList.IsOpen())
		{
			mazeTowerList.Refresh();
		}
	}

	public void InitFloorData(int floor)
	{
		TowerFloorData data = GameRoot.Instance.TowerFloorDataContainer.GetData(TowerData.TowerIndex, floor);
		if (data == null)
		{
			VespaDebug.LogError("Floor data not found : towerIndex=" + TowerData.TowerIndex + " floor=" + floor);
		}
		else
		{
			InitFloorData(data);
		}
	}

	public void InitFloorData(TowerFloorData floorData)
	{
		if (floorData != null)
		{
			FloorData = floorData;
		}
	}

	private void StartTowerBattle(EnterDungeonData enterDungeon, BattlePartySetting.BattlePartyInfo battlePartyInfo, BattlePartySetting.ExtraOptionData extraOption)
	{
		int towerIndex = FloorData.TowerIndex;
		int towerFloor = FloorData.Floor;
		NShared.BeginCampaign.Request obj = new NShared.BeginCampaign.Request
		{
			ChapterIndex = ChapterIndex,
			DungeonIndex = DungeonIndex,
			LinkedDungeonIndex = DungeonIndex,
			HeroIndices = battlePartyInfo.heroIndices,
			LeaderHeroIndex = battlePartyInfo.leaderHeroIndex,
			AccountIds = battlePartyInfo.accountIds,
			Repeat = false,
			TowerIndex = towerIndex,
			TowerFloor = towerFloor
		};
		int onetimeBoosterCount = battlePartyInfo.OnetimeBoosterCount;
		obj.ItemBoosterCount = onetimeBoosterCount;
		obj.AffixIndices = battlePartyInfo.AffixIndices;
		NShared.BeginCampaign.Request request = obj;
		SingletonAuto<BattleContext>.instance.BeginCampaign(request, delegate(NShared.BeginCampaign.Response successResponse)
		{
			if (extraOption != null)
			{
				if (extraOption.AutoRepeat)
				{
					NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.OpenGlobal();
				}
				else if (extraOption.AutoNext)
				{
					AutoNextBattle.OpenGlobal(enterDungeon.LinkedDungeonData.BattleType, enterDungeon.Difficulty, request.ItemBoosterCount);
				}
			}
			SingletonAuto<CampaignContext>.instance.SetInitialUserHeroInfo(successResponse.TowerIntialUserHeroes);
			SingletonAuto<CampaignContext>.instance.SetInitialEnemyHeroInfo(successResponse.TowerIntialEnemyHeroes);
			if (NpcCache != null && NpcCache.TowerIndex == towerIndex)
			{
				CachedAccountInfo[] cachedNpcInfos = NpcCache.CachedNpcInfos;
				int num = towerFloor - 1;
				if (num < __Extension_Method_ICollection__.COUNT(cachedNpcInfos))
				{
					CachedHeroInfo[] forcedEnemyHeroInfo = cachedNpcInfos[num].HeroInfos.Values.ToArray();
					SingletonAuto<CampaignContext>.instance.SetForcedEnemyHeroInfo(forcedEnemyHeroInfo);
				}
			}
			ClearCache();
			LobbyScene2.ReturnState.DisabledFlag |= LobbyScene2.ReturnStateOpenFlag.OpenHeroDeck;
			LobbyScene2.ListReturnState.Add(new TowerReturnState(towerIndex, towerFloor, TowerData.Type));
		});
	}

	public int GetCompletedFloor()
	{
		if (TowerInfo == null)
		{
			return 0;
		}
		return TowerInfo.CompletedFloor;
	}

	public int GetMaxFloor()
	{
		int completedFloor = GetCompletedFloor();
		int num = completedFloor + 1;
		TowerFloorData data = GameRoot.Instance.TowerFloorDataContainer.GetData(TowerData.TowerIndex, num);
		if (data != null && IsOpenedFloor(data))
		{
			return num;
		}
		return Mathf.Min(BaseTower.GetMaxFloor(TowerInfo.TowerIndex), completedFloor);
	}

	public static bool IsOpenedFloor(TowerFloorData floorData)
	{
		if (!String_Extension_Method.IsNullOrEmpty(floorData.OpenTime))
		{
			if (!Vespa_String_Extension_Method2.TryParseNetworkTime2(floorData.OpenTime, out var o_dateTime))
			{
				return false;
			}
			if (GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerLocalTime() < o_dateTime)
			{
				return false;
			}
		}
		return true;
	}

	public int GetChapterIndex()
	{
		if (FloorData == null)
		{
			return 0;
		}
		return FloorData.ChapterIndex;
	}

	public int GetDungeonIndex()
	{
		return TowerHelper.GetFloorDungeonIndex(FloorData);
	}

	private void RequestTowerNpcInfo(int towerIndex, Action onSuccess)
	{
		if (NpcCache != null && towerIndex == NpcCache.TowerIndex)
		{
			Vespa_System_Action_Extension_Method.fire(onSuccess);
			return;
		}
		NShared.GetTowerNpcInfo.Request request = new NShared.GetTowerNpcInfo.Request
		{
			TowerIndex = towerIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetTowerNpcInfo.Response response)
		{
			if (response.Result != NShared.GetTowerNpcInfo.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				if (NpcCache == null)
				{
					NpcCache = new TowerNpcCache();
				}
				NpcCache.TowerIndex = towerIndex;
				NpcCache.CachedNpcInfos = ((response.TowerNpcInfos != null) ? response.TowerNpcInfos : null);
				Vespa_System_Action_Extension_Method.fire(onSuccess);
			}
		});
	}

	private void ClearCache()
	{
		NpcCache = null;
		HeroCache = null;
	}

	public void OpenBattlePartySetting()
	{
		if (FloorData == null)
		{
			int maxFloor = GetMaxFloor();
			InitFloorData(maxFloor);
		}
		if (FloorData == null || TowerData == null)
		{
			return;
		}
		if (FloorData.TowerIndex != TowerData.TowerIndex)
		{
			VespaDebug.LogError("Tower and floor mismatch!");
			return;
		}
		int chapterIndex = GetChapterIndex();
		int dungeonIndex = GetDungeonIndex();
		StaminaType staminaType = StaminaType.None;
		int num = 0;
		long staminaValue = num;
		FloorData.GetReqStamina(GlobalAutoBehaviour<BoosterManager>.instance.CheckSaveStamina(), out staminaType, out staminaValue);
		EnterDungeonData enterDungeonData = new EnterDungeonData(chapterIndex, dungeonIndex, dungeonIndex, NShared.DungeonDifficultyType.Easy);
		BanRuleData data = GameRoot.Instance.BanRuleDataContainer.GetData(FloorData.ReqClass);
		CampaignDungeonData linkedDungeonData = enterDungeonData.LinkedDungeonData;
		string text = "";
		if (linkedDungeonData == null)
		{
			text = $"TOWER_PARTYSETTING_STORAGE_{TowerData.Type.ToString().ToUpperInvariant()}";
			if (TowerData.Type == TowerType.Maze)
			{
				text = $"TOWER_PARTYSETTING_STORAGE_{TowerData.Type.ToString().ToUpperInvariant()}_{TowerData.TowerIndex}";
			}
		}
		else
		{
			text = linkedDungeonData.LocalStorageKey;
		}
		DefaultBattlePartySettingDataProdiver dataProvider = new DefaultBattlePartySettingDataProdiver(text, staminaType, staminaValue, Battle.START_BATTLE, enterDungeonData, data);
		AddTask(OpenBattlePartySettingAsync(dataProvider));
	}

	private ITask OpenBattlePartySettingAsync(BattlePartySetting.IDataProvider dataProvider)
	{
		return base.TaskFactory.Run(delegate(BattlePartySetting window)
		{
			window.Open(this, this, dataProvider);
		});
	}

	public void OpenMazeTowerList(Action onOpen = null)
	{
		AddTask(OpenMazeTowerListAsync(onOpen));
	}

	private ITask OpenMazeTowerListAsync(Action onOpen = null)
	{
		return new WebServiceTask<NShared.GetMazeTowerInfo.Request, NShared.GetMazeTowerInfo.Response>(new NShared.GetMazeTowerInfo.Request(), delegate(NShared.GetMazeTowerInfo.Request request, NShared.GetMazeTowerInfo.Response response)
		{
			if (response.Result != NShared.GetMazeTowerInfo.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
				Vespa_System_Action_Extension_Method.fire(onOpen);
				return (ITask)null;
			}
			MazeTowerRewardStep = response.MazeTowerRewardStep;
			OpenMaze triggerEvent = new OpenMaze();
			SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent);
			Vespa_System_Action_Extension_Method.fire(onOpen);
			return base.TaskFactory.Run(delegate(MazeTowerList window)
			{
				window.Open(null, MazeTowerRewardStep);
			});
		});
	}

	public void RequestMazeTowerReward(int rewardStep, Action onSuccess)
	{
		ReceiveMazeTowerReward(rewardStep, onSuccess);
	}

	public void ReceiveMazeTowerReward(int rewardStep, Action onSuccess)
	{
		NShared.ReceiveMazeTowerReward.Request request = new NShared.ReceiveMazeTowerReward.Request
		{
			MazeTowerRewardIndex = rewardStep
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.ReceiveMazeTowerReward.Response response)
		{
			if (response.Result != NShared.ReceiveMazeTowerReward.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.EquipItemResults);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
				string stringDirect = LocalizationManager.GetStringDirect("MAZE_NAME");
				string stringDirect2 = LocalizationManager.GetStringDirect("MAZE_GET_REWARD");
				BaseRewardPopup.PopupOpenData popupOpenData = new BaseRewardPopup.PopupOpenData();
				popupOpenData.SetTitle($"{stringDirect} {stringDirect2}").AddItemResultInfos(response.ItemResults).AddEquipResultInfos(response.EquipItemResults)
					.AddCurrencyResultInfos3(response.CurrencyResults);
				BaseRewardPopup.OpenPopup(popupOpenData, isGlobal: false);
				Vespa_System_Action_Extension_Method.fire(onSuccess);
			}
		});
	}

	public void ProcessResetTower(BaseTower window, string title, string message, string action)
	{
		PricedMessagePopup.EPriceType priceType = ((window.ResetCostType == RewardType.Gold) ? PricedMessagePopup.EPriceType.Gold : PricedMessagePopup.EPriceType.Gem);
		int resetCost = window.ResetCost;
		PricedMessagePopup.Open(title, message, action, priceType, resetCost, delegate
		{
			AddTask(new WebServiceTask<NShared.ResetTower.Request, NShared.ResetTower.Response>(new NShared.ResetTower.Request
			{
				TowerIndex = window.TowerIndex
			}, (NShared.ResetTower.Request request, NShared.ResetTower.Response response) => (response.Result != NShared.ResetTower.ResultType.Success) ? base.TaskFactory.Run(delegate
			{
				MessagePopup.OpenError(response.Result);
			}) : base.TaskFactory.Run(delegate
			{
				ClearCache();
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults2);
				GlobalAutoBehaviour<AccountManager>.instance.TowerManager.Apply(response.TowerInfo);
				ReloadTowerData(response.TowerInfo.TowerIndex);
				InitFloorData(1);
				window.OnResetTower(response.TowerInfo);
			})));
		});
	}

	public void ProcessReceiveReward(Action onReceive)
	{
		int towerIndex = TowerData.TowerIndex;
		NShared.ReceiveTowerReward.Request request = new NShared.ReceiveTowerReward.Request
		{
			TowerIndex = towerIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.ReceiveTowerReward.Response response)
		{
			if (response.Result != NShared.ReceiveTowerReward.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
				GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Apply(response.EquipItemInfos);
				GlobalAutoBehaviour<AccountManager>.instance.TowerManager.Apply(response.TowerInfo);
				BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetTitle(LocalizationManager.GetString(Tower.TOWER_RECEIVE_REWARD_TITLE)).SetItemResultInfos(response.ItemResults).SetEquipResultInfos(response.EquipItemInfos)
					.SetCurrencyResultInfos3(response.CurrencyResults), isGlobal: false);
				Vespa_System_Action_Extension_Method.fire(onReceive);
			}
		});
	}

	void BattlePartySetting.Listener.StartBattle(EnterDungeonData enterDungeon, BattlePartySetting.BattlePartyInfo battlePartyInfo, BattlePartySetting.ExtraOptionData extraOption)
	{
		if (_isTowerBattle)
		{
			StartTowerBattle(enterDungeon, battlePartyInfo, extraOption);
		}
		else
		{
			StartGatewayBattle(enterDungeon, battlePartyInfo, extraOption);
		}
	}

	bool BattlePartySetting.Listener.OnCloseRequest(BattlePartySetting closeThis)
	{
		return true;
	}

	void BattlePartySetting.Listener.OpenHeroDetailView(CachedHeroInfo heroInfo)
	{
	}

	void BattlePartySetting.PreviewAnimationListener.ActivatePreview(bool bActivate)
	{
		PitDungeonInfo pitDungeonInfo = GetInstance<PitDungeonInfo>();
		if (UIHelper.IsEnabledGameObjet(pitDungeonInfo))
		{
			pitDungeonInfo.Boss3dView.ActivatePreview(bActivate);
		}
	}

	bool BattlePartySetting.Listener.OpenAutoNextBattleSelect()
	{
		if (_isTowerBattle)
		{
			return OpenAutoNextOnTower();
		}
		return OpenAutoNextOnGateway();
	}

	void BattleDungeonInfo.Listener.OnReadyBattle(EnterDungeonData enterDungeon)
	{
		if (!(GetInstance<BattlePartySetting>() == null))
		{
			DefaultBattlePartySettingDataProdiver dataProvider = new DefaultBattlePartySettingDataProdiver(enterDungeon.LinkedDungeonData.LocalStorageKey, StaminaType.Chicken, 0, Battle.START_BATTLE, enterDungeon);
			GetInstance<BattlePartySetting>().Open(this, dataProvider);
		}
	}

	void BattleDungeonInfo.Listener.OnMoveEnter(EnterDungeonData enterDungeon)
	{
	}

	void BattleDungeonInfo.Listener.OnResetEventDungeon()
	{
	}

	void BattleDungeonInfo.Listener.OnClickPartyPlayPopup(EnterDungeonData enterDungeon)
	{
	}

	void BattleDungeonInfo.Listener.OnClickDispatch(EnterDungeonData enterDungeon)
	{
	}

	bool BattleDungeonInfo.Listener.OnCloseRequest(BattleDungeonInfo closeThis)
	{
		return true;
	}

	void ContentUIGatewayComponent.IListener.OnClickEnter(ContentUIGatewayComponent comp, GatewayToTrialData gatewayData)
	{
		if (gatewayData != null)
		{
			_gatewayTowerIndex = gatewayData.TowerIndex;
			_gatewayTowerFloor = gatewayData.Step;
			EnterDungeonData enterDungeonData = DungeonHelper.GetEnterDungeonData(gatewayData.ChapterIndex, gatewayData.DungeonIndex);
			OpenGatewayDungeonInfoUI(enterDungeonData.Chapter, enterDungeonData.OriginalDungeon, gatewayData.RewardIndex, gatewayData);
		}
	}

	int BattleDungeonInfo.Listener.GetDungeonRemainTime(EnterDungeonData enterDungeon)
	{
		return -1;
	}
}
