using System;
using System.Collections;
using MiniJSON;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NContents;
using NGame2.NEventTrigger;
using NGame2.NLobby;
using NGame2.NLobby.NActivity;
using NGame2.NScene;
using NGame2.NScene.NLobby;
using NGame2.NUI.NManager.NEclipse;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWindow.NEclipse;
using NGame2.NUI.NWindow.NSoulWeapon;
using NGame2.NUI.NWindow.NSoulWeapon.NSoulWeaponManage;
using NShared;
using NShared.BeginCampaign;
using NShared.GetEclipseInfo;
using NShared.GiveUpEclipseDungeon;
using NShared.NEvent;
using NShared.NLocalization;
using NShared.SetEclipseDeck;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NTask.NCustomYieldInstruction;
using UnityEngine;

namespace NGame2.NUI.NManager.NLobby;

public class SoulWeaponManagement : BaseManagement<SoulWeaponManagement>, SoulWeaponLobby.IListener, SoulWeaponManageWindow.IListener, SoulWeaponReinforceWindow.IListener, EclipseMain.Listener, EclipseDeckSetting.Listener, EclipseMain.ReturnStateListener, EclipseBattleReady.Listener, BattlePartySetting.Listener, SoulWeaponWebManager.IListener, SoulWeaponLiberationWindow.IListener, SoulWeaponUpgradeWindow.IListener, SoulStoneRestoreWindow.IListener, SoulWeaponContentsWindow.IListener, SoulWeaponReinforceResultWindow.IListener, SoulWeaponUpgradeResultWindow.IListener, TransferSoulWeaponPopup.IListener, BreakSoulWeaponPopup.IListener, RenewSoulWeaponStatPopup.IListener, ConfirmRenewSoulWeaponStatPopup.IListener, ResultRenewSoulWeaponStatPopup.IListener, SoulStoneRestoreRewardPopup.IListener, MoveGodKingTemplePopup.IListener, EclipseRewardPopup.IListener, EclipseBattleRecordPopup.IListener
{
	private bool _RequestStartBattle;

	private EclipseWebManager _eclipseWebManager;

	private int EclipseMaxDeckCount => GameRoot.Instance.ConstantDataContainer.GetValueInt("EclipseMaxTeam", 10);

	protected override void Awake()
	{
		base.Awake();
		_RequestStartBattle = false;
		AddInstance<ContentUIGodkingTrialDungeonList>("Resources/Prefab2/Window/GodkingTrial/ContentUIGodkingTrialDungeonList");
		AddInstance<ContentUIGodkingTrialDungeonInfo>("Resources/Prefab2/Window/GodkingTrial/ContentUIGodkingTrialDungeonInfo");
		AddInstance<GodkingTrialRewardPopup>("Resources/Prefab2/Window/GodkingTrial/GodkingTrialRewardPopup");
		AddInstance<GodkingTrialPartySetting>("Resources/Prefab2/Window/GodkingTrial/GodkingTrialPartySetting");
		AddInstance<EclipseMain>("Resources/Prefab2/Window/SoulWeapon/EclipseMain");
		AddInstance<EclipseRewardInfo>("Resources/Prefab2/Window/SoulWeapon/EclipseRewardInfo");
		AddInstance<EclipseDeckSetting>("Resources/Prefab2/Window/SoulWeapon/EclipseDeckSetting");
		AddInstance<EclipseBattleReady>("Resources/Prefab2/Window/SoulWeapon/EclipseBattleReady");
		AddInstance<EclipseDeckDetailPopup>("Resources/Prefab2/Window/SoulWeapon/EclipseDeckDetailPopup");
		AddInstance<EclipseDungeonRewardPopup>(EclipseDungeonRewardPopup.PrefabPath);
		AddInstance<EclipseStartPopup>("Resources/Prefab2/Window/SoulWeapon/Popup/EclipseStartPopup");
		AddPrefabSoulWeapon();
	}

	public static bool IsOpenSoulweapon()
	{
		return SingletonAuto<ContentsOpenChecker>.instance.CheckContentsOpen(ContentsType.SoulWeapon);
	}

	public static bool IsOpenGodKingTemple()
	{
		int defaultValue = 65;
		int defaultValue2 = 11;
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("GodKingTempleReqChapter", defaultValue);
		int valueInt2 = GameRoot.Instance.ConstantDataContainer.GetValueInt("GodKingTempleReqDungeon", defaultValue2);
		return GlobalAutoBehaviour<AccountManager>.instance.CampaignManager.IsCompleted(valueInt, valueInt2);
	}

	public static bool IsCompleteSoulWeaponLiberation()
	{
		return GlobalAutoBehaviour<AccountManager>.instance.TutorialManager.IsComplete(7400);
	}

	private void Start()
	{
		SingletonAuto<SoulWeaponWebManager>.instance.AddListener(this);
		InitEclipseWebManager();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		SingletonAuto<SoulWeaponWebManager>.instance.RemoveListener(this);
		_eclipseWebManager = null;
	}

	private void AddPrefabSoulWeapon()
	{
		AddInstance<SoulWeaponLobby>("Resources/Prefab2/Window/SoulWeapon/SoulWeaponLobby");
		AddInstance<SoulWeaponManageWindow>("Resources/Prefab2/Window/SoulWeapon/SoulWeaponManageWindow");
		AddInstance<SoulWeaponReinforceWindow>("Resources/Prefab2/Window/SoulWeapon/SoulWeaponReinforceWindow");
		AddInstance<SoulWeaponReinforceResultWindow>("Resources/Prefab2/Window/SoulWeapon/SoulWeaponReinforceResultWindow");
		AddInstance<SoulWeaponContentsWindow>("Resources/Prefab2/Window/SoulWeapon/SoulWeaponContentsWindow");
		AddInstance<SoulWeaponLiberationResultWindow>("Resources/Prefab2/Window/SoulWeapon/SoulWeaponLiberationResultWIndow");
		AddInstance<SoulWeaponLiberationWindow>("Resources/Prefab2/Window/SoulWeapon/SoulWeaponLiberationWindow");
		AddInstance<SoulWeaponUpgradeResultWindow>("Resources/Prefab2/Window/SoulWeapon/SoulWeaponUpgradeResultWindow");
		AddInstance<SoulWeaponUpgradeWindow>("Resources/Prefab2/Window/SoulWeapon/SoulWeaponUpgradeWindow");
		AddInstance<SoulStoneRestoreWindow>("Resources/Prefab2/Window/SoulWeapon/SoulStoneRestoreWindow");
		AddInstance<BreakSoulWeaponPopup>(BreakSoulWeaponPopup.PrefabPath);
		AddInstance<TransferSoulWeaponPopup>(TransferSoulWeaponPopup.PrefabPath);
		AddInstance<ResultTransferSoulWeaponPopup>(ResultTransferSoulWeaponPopup.PrefabPath);
		AddInstance<RenewSoulWeaponStatPopup>(RenewSoulWeaponStatPopup.PrefabPath);
		AddInstance<ConfirmRenewSoulWeaponStatPopup>(ConfirmRenewSoulWeaponStatPopup.PrefabPath);
		AddInstance<ResultRenewSoulWeaponStatPopup>(ResultRenewSoulWeaponStatPopup.PrefabPath);
		AddInstance<SoulStoneRestoreRewardPopup>(SoulStoneRestoreRewardPopup.PrefabPath);
		AddInstance<MoveGodKingTemplePopup>(MoveGodKingTemplePopup.PrefabPath);
	}

	private void InitEclipseWebManager()
	{
		_eclipseWebManager = new EclipseWebManager();
	}

	public void OpenGodkingTrialDungeonList()
	{
		if (RestoredPortal.GodkingAccess())
		{
			ContentUIGodkingTrialDungeonList contentUIGodkingTrialDungeonList = GetInstance<ContentUIGodkingTrialDungeonList>();
			if (contentUIGodkingTrialDungeonList != null && !contentUIGodkingTrialDungeonList.IsOpen())
			{
				contentUIGodkingTrialDungeonList.Open();
			}
		}
	}

	public void OpenGodkingTrialDungeonInfo(int chapterIndex)
	{
		if (RestoredPortal.GodkingAccess())
		{
			ContentUIGodkingTrialDungeonInfo contentUIGodkingTrialDungeonInfo = GetInstance<ContentUIGodkingTrialDungeonInfo>();
			if (contentUIGodkingTrialDungeonInfo != null && !contentUIGodkingTrialDungeonInfo.IsOpen())
			{
				contentUIGodkingTrialDungeonInfo.Open(chapterIndex);
			}
		}
	}

	public void OpenGodkingTrialRewardPopup()
	{
		GodkingTrialRewardPopup godkingTrialRewardPopup = GetInstance<GodkingTrialRewardPopup>();
		if (godkingTrialRewardPopup != null && !godkingTrialRewardPopup.IsOpen())
		{
			godkingTrialRewardPopup.Open();
		}
	}

	public void OpenBattlePartySetting(BattlePartySetting.IDataProvider dataProvider)
	{
		GodkingTrialPartySetting godkingTrialPartySetting = GetInstance<GodkingTrialPartySetting>();
		if (godkingTrialPartySetting == null)
		{
			VespaDebug.LogError("SoulWeaponManagement:OpenBattlePartySetting - Cant open battle party setting ui");
		}
		else if (!godkingTrialPartySetting.IsOpen())
		{
			godkingTrialPartySetting.Open(this, dataProvider);
		}
	}

	private void StartGodkingTrialDungeon(EnterDungeonData enterDungeon, BattlePartySetting.BattlePartyInfo battlePartyInfo, BattlePartySetting.ExtraOptionData extraOption)
	{
		if (_RequestStartBattle)
		{
			return;
		}
		_RequestStartBattle = true;
		NShared.BeginCampaign.Request request = new NShared.BeginCampaign.Request
		{
			ChapterIndex = enterDungeon.Chapter,
			DungeonIndex = enterDungeon.OriginalDungeon,
			LinkedDungeonIndex = enterDungeon.LinkedDungeon,
			HeroIndices = battlePartyInfo.heroIndices,
			LeaderHeroIndex = battlePartyInfo.leaderHeroIndex,
			AccountIds = battlePartyInfo.accountIds,
			AffixIndices = battlePartyInfo.AffixIndices,
			Repeat = false
		};
		if (GameRoot.Instance.SelectRewardDataContainer.GetData(request.ChapterIndex, request.DungeonIndex) != null)
		{
			request.SelectedRewardItemCodes = NGame2.NUI.NWindow.NBattle.AutoRepeatBattle.GetSelectedRewardToArray();
		}
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
			LobbyScene2.ListReturnState.Clear();
			LobbyScene2.ReturnState.DisabledFlag |= LobbyScene2.ReturnStateOpenFlag.OpenHeroDeck;
			LobbyScene2.ListReturnState.Add(new GodkingTrialDungeonReturnState(enterDungeon.Chapter, enterDungeon.OriginalDungeon));
		}, delegate
		{
			_RequestStartBattle = false;
		});
	}

	public void OpenSoulWeaponLobby(Action onSucess)
	{
		RequestGetEclipseInfo(showLoading: true, delegate
		{
			SoulWeaponLobby soulWeaponLobby = GetInstance<SoulWeaponLobby>();
			if (!(soulWeaponLobby == null))
			{
				soulWeaponLobby.Open(this);
				Vespa_System_Action_Extension_Method.fire(onSucess);
			}
		}, null);
	}

	public void OpenSoulWeaponEntryMenu(Action action)
	{
		MoveToNpc activity = new MoveToNpc("SoulWeapon", executeNodeFunction: true, warp: true, delegate
		{
			Vespa_System_Action_Extension_Method.fire(action);
		});
		SceneAutoBehaviour<ActivityManager>.instance.Run(activity);
	}

	public void OpenSoulWeaponContentsWindow()
	{
		SoulWeaponContentsWindow soulWeaponContentsWindow = GetInstance<SoulWeaponContentsWindow>();
		if (soulWeaponContentsWindow != null)
		{
			soulWeaponContentsWindow.Open(this);
		}
	}

	public void OpenSoulWeaponManageWindow(EquipItemInfo equipItemInfo, bool isSelectFirst = false)
	{
		SoulWeaponManageWindow soulWeaponManageWindow = GetInstance<SoulWeaponManageWindow>();
		if (soulWeaponManageWindow != null)
		{
			soulWeaponManageWindow.Open(this, equipItemInfo, isSelectFirst);
		}
	}

	private void OpenSoulWeaponManageWindowBySoulWeapon(SoulWeaponInfo soulWeaponInfo)
	{
		EquipItemInfo equipItemInfoBySoulWeaponInfo = GetEquipItemInfoBySoulWeaponInfo(soulWeaponInfo);
		if (equipItemInfoBySoulWeaponInfo != null)
		{
			OpenSoulWeaponManageWindow(equipItemInfoBySoulWeaponInfo);
		}
	}

	private void OpenSoulWeaponReinforceWindow(EquipItemInfo equipItemInfo, bool isSelectFirst)
	{
		SoulWeaponReinforceWindow soulWeaponReinforceWindow = GetInstance<SoulWeaponReinforceWindow>();
		if (soulWeaponReinforceWindow != null)
		{
			EnableTopMostMenu(enable: true);
			soulWeaponReinforceWindow.Open(this, equipItemInfo, isSelectFirst);
		}
	}

	private void OpenSoulWeaponReinforceWindowBySoulWeaponInfo(SoulWeaponInfo soulWeaponInfo, bool isSelectFirst)
	{
		EquipItemInfo equipItemInfoBySoulWeaponInfo = GetEquipItemInfoBySoulWeaponInfo(soulWeaponInfo);
		if (equipItemInfoBySoulWeaponInfo != null)
		{
			OpenSoulWeaponReinforceWindow(equipItemInfoBySoulWeaponInfo, isSelectFirst);
		}
	}

	private void OpenSoulWeaponReinforceResultWindow(SoulWeaponInfo soulWeaponInfo, bool isSuccess, Action onClose)
	{
		SoulWeaponReinforceResultWindow soulWeaponReinforceResultWindow = GetInstance<SoulWeaponReinforceResultWindow>();
		if (soulWeaponReinforceResultWindow != null)
		{
			soulWeaponReinforceResultWindow.Open(this, soulWeaponInfo, isSuccess, onClose);
		}
	}

	private void OpenSoulWeaponUpgradeWindow(EquipItemInfo equipItemInfo, bool isSelectFirst)
	{
		SoulWeaponUpgradeWindow soulWeaponUpgradeWindow = GetInstance<SoulWeaponUpgradeWindow>();
		if (soulWeaponUpgradeWindow != null)
		{
			soulWeaponUpgradeWindow.Open(equipItemInfo, this, isSelectFirst);
		}
	}

	private void OpenSoulWeaponUpgradeWindowBySoulWeaponInfo(SoulWeaponInfo soulWeaponInfo, bool isSelectFirst)
	{
		EquipItemInfo equipItemInfoBySoulWeaponInfo = GetEquipItemInfoBySoulWeaponInfo(soulWeaponInfo);
		if (equipItemInfoBySoulWeaponInfo != null)
		{
			OpenSoulWeaponUpgradeWindow(equipItemInfoBySoulWeaponInfo, isSelectFirst);
		}
	}

	private void OpenSoulWeaponUpgradeResultWindow(SoulWeaponInfo originSoulWeaponInfo, EquipItemInfo equipItemInfo, Action onClose)
	{
		SoulWeaponUpgradeResultWindow soulWeaponUpgradeResultWindow = GetInstance<SoulWeaponUpgradeResultWindow>();
		if (soulWeaponUpgradeResultWindow != null)
		{
			EnableTopMostMenu(enable: false);
			soulWeaponUpgradeResultWindow.Open(this, originSoulWeaponInfo, equipItemInfo, onClose);
		}
	}

	public void OpenSoulWeaponLiberationWindow(EquipItemInfo equipItemInfo, bool isSelectFirst)
	{
		SoulWeaponLiberationWindow soulWeaponLiberationWindow = GetInstance<SoulWeaponLiberationWindow>();
		if (soulWeaponLiberationWindow != null)
		{
			OpenSoulWeaponLiberation triggerEvent = new OpenSoulWeaponLiberation();
			if (SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent))
			{
				soulWeaponLiberationWindow.Open(null, this, isSelectFirst);
			}
			else
			{
				soulWeaponLiberationWindow.Open(equipItemInfo, this, isSelectFirst);
			}
		}
	}

	private void OpenSoulWeaponLiberationResultWindow(SoulWeaponInfo soulWeaponInfo)
	{
		if (soulWeaponInfo != null)
		{
			SoulWeaponLiberationResultWindow soulWeaponLiberationResultWindow = GetInstance<SoulWeaponLiberationResultWindow>();
			if (soulWeaponLiberationResultWindow != null)
			{
				soulWeaponLiberationResultWindow.Open(soulWeaponInfo);
			}
		}
	}

	public void OpenSoulStoneRestoreWindow()
	{
		if (!IsOpenSoulweapon() || !IsOpenGodKingTemple())
		{
			string stringDirect = LocalizationManager.GetStringDirect("COMMON_MOVE_TASK_FAIL_TEXT");
			MessagePopup.Open(LocalizationManager.GetString(NShared.NLocalization.Common.INFORM), stringDirect);
			return;
		}
		SoulStoneRestoreWindow soulStoneRestoreWindow = GetInstance<SoulStoneRestoreWindow>();
		if (soulStoneRestoreWindow != null)
		{
			soulStoneRestoreWindow.Open(this);
		}
	}

	private void EnableTopMostMenu(bool enable)
	{
		LobbyTopmostMenu lobbyTopmostMenu = SceneAutoBehaviour<LobbyManagement>.instance.GetInstance<LobbyTopmostMenu>();
		if (lobbyTopmostMenu != null)
		{
			UIHelper.EnableGameObject(lobbyTopmostMenu, enable);
		}
	}

	private void OpenTransferSoulWeaponPopup(EquipItemInfo equipItemInfo)
	{
		TransferSoulWeaponPopup transferSoulWeaponPopup = GetInstance<TransferSoulWeaponPopup>();
		if (transferSoulWeaponPopup != null)
		{
			transferSoulWeaponPopup.Open(this, equipItemInfo);
		}
	}

	private void OpenResultTransferSoulWeaponPopup(SoulWeaponInfo resultSoulWeaponInfo)
	{
		if (resultSoulWeaponInfo != null)
		{
			ResultTransferSoulWeaponPopup resultTransferSoulWeaponPopup = GetInstance<ResultTransferSoulWeaponPopup>();
			if (resultTransferSoulWeaponPopup != null)
			{
				resultTransferSoulWeaponPopup.Open(resultSoulWeaponInfo);
			}
		}
	}

	public void OpenBreakSoulWeaponPopup(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo != null)
		{
			BreakSoulWeaponPopup breakSoulWeaponPopup = GetInstance<BreakSoulWeaponPopup>();
			if (breakSoulWeaponPopup != null)
			{
				breakSoulWeaponPopup.Open(this, equipItemInfo);
			}
		}
	}

	private void OpenRenewSoulWeaponStatPopup(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo != null)
		{
			RenewSoulWeaponStatPopup renewSoulWeaponStatPopup = GetInstance<RenewSoulWeaponStatPopup>();
			if (renewSoulWeaponStatPopup != null)
			{
				renewSoulWeaponStatPopup.Open(this, equipItemInfo.SoulWeaponInfo);
			}
		}
	}

	private void OpenRenewSoulWeaponStatPopupBySoulWeaponInfo(SoulWeaponInfo soulWeaponInfo)
	{
		EquipItemInfo equipItemInfoBySoulWeaponInfo = GetEquipItemInfoBySoulWeaponInfo(soulWeaponInfo);
		if (equipItemInfoBySoulWeaponInfo != null)
		{
			OpenRenewSoulWeaponStatPopup(equipItemInfoBySoulWeaponInfo);
		}
	}

	private void OpenConfirmRenewSoulWeaponStatPopup(SoulWeaponInfo soulWeaponInfo)
	{
		if (soulWeaponInfo != null)
		{
			ConfirmRenewSoulWeaponStatPopup confirmRenewSoulWeaponStatPopup = GetInstance<ConfirmRenewSoulWeaponStatPopup>();
			if (confirmRenewSoulWeaponStatPopup != null)
			{
				confirmRenewSoulWeaponStatPopup.Open(this, soulWeaponInfo);
			}
		}
	}

	private void OpenResultRenewSoulWeaponStatPopup(SoulWeaponInfo soulWeaponInfo, SoulWeaponInfo resultSoulWeaponInfo)
	{
		if (soulWeaponInfo != null)
		{
			ResultRenewSoulWeaponStatPopup resultRenewSoulWeaponStatPopup = GetInstance<ResultRenewSoulWeaponStatPopup>();
			if (resultRenewSoulWeaponStatPopup != null)
			{
				resultRenewSoulWeaponStatPopup.Open(this, soulWeaponInfo, resultSoulWeaponInfo);
			}
		}
	}

	private void OpenRestoreSoulStoneRewardPopup(RestoreSoulStoneRewardInfo rewardInfo, bool isGetBonus)
	{
		BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetItemResultInfos(rewardInfo.ItemResults).SetEquipResultInfos(rewardInfo.EquipItemResults).SetCurrencyResultInfos3(rewardInfo.CurrencyResults), isGlobal: false);
	}

	private void OpenRenewMessagePopup(SoulWeaponInfo soulWeaponInfo, RequirePrice requirePrice, Action action)
	{
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("SoulWeaponRenewTicketItemIndex");
		ItemInfo itemInfo = GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Get(valueInt);
		if (itemInfo == null || (itemInfo != null && itemInfo.Count == 0))
		{
			RequestSoulWeaponRenewWithCurrency(soulWeaponInfo, requirePrice, action);
			return;
		}
		LocalizationManager.GetString(NShared.NLocalization.EquipOptionRenew.ASK_RENEW_OPTION_TITLE);
		string stringDirect = LocalizationManager.GetStringDirect("SOULWEAPON_CHANGE_COUPONUSE");
		string confirmLabel = LocalizationManager.GetString(NShared.NLocalization.Common.USE);
		string cancelLabel = LocalizationManager.GetString(NShared.NLocalization.Common.COMMON_CANCEL);
		IconConfirmPopup.GodKingStyleOpen(stringDirect, confirmLabel, cancelLabel, delegate
		{
			SingletonAuto<SoulWeaponWebManager>.instance.RequestSoulWeaponRenewOption(soulWeaponInfo, useCoupon: true);
		}, delegate
		{
			RequestSoulWeaponRenewWithCurrency(soulWeaponInfo, requirePrice, action);
		}).AddItem(valueInt, 0);
	}

	private void RequestSoulWeaponRenewWithCurrency(SoulWeaponInfo soulWeaponInfo, RequirePrice requirePrice, Action action)
	{
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string stringDirect = LocalizationManager.GetStringDirect("SOULWEAPON_ASK_CHANGE_2");
		string stringDirect2 = LocalizationManager.GetStringDirect("SOULWEAPON_INFO_UPGRADE_2 ");
		PricedMessagePopup.Open(title, stringDirect, stringDirect2, PricedMessagePopup.EPriceType.Gem, requirePrice.ReqPrice, delegate
		{
			if (CheckEnoughCurrency(requirePrice, action))
			{
				SingletonAuto<SoulWeaponWebManager>.instance.RequestSoulWeaponRenewOption(soulWeaponInfo, useCoupon: false);
			}
		});
	}

	private void OpenTransferMessagePopup(int soulWeaponSlotIndex, int specialWeaponSlotIndex, RequirePrice requirePrice, Action action)
	{
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string message = LocalizationManager.GetStringDirect("SOULWEAPON_INFO_ACTION_POPUP_ASK") + " (" + LocalizationManager.GetStringDirect("SOULWEAPON_INFO_ACTION_POPUP_INFO") + ")";
		MessagePopup.Open(title, message, delegate
		{
			if (CheckEnoughCurrency(requirePrice, action))
			{
				SingletonAuto<SoulWeaponWebManager>.instance.RequestTransition(soulWeaponSlotIndex, specialWeaponSlotIndex);
			}
		}, null);
	}

	private void OpenRestoreSoulStoneMessagePopup()
	{
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string stringDirect = LocalizationManager.GetStringDirect("SOULWEAPON_ACTION_RESTORE_INFO");
		MessagePopup.Open(title, stringDirect, delegate
		{
			SingletonAuto<SoulWeaponWebManager>.instance.RequestRestoreSoulStone();
		}, null);
	}

	private void OpenSelectedSoulStoneMessagePopup(int itemIndex)
	{
		SingletonAuto<SoulWeaponWebManager>.instance.RequestSelectedSoulStone(itemIndex);
	}

	private void OpenMoveGodKingTemplePopup()
	{
		MoveGodKingTemplePopup moveGodKingTemplePopup = GetInstance<MoveGodKingTemplePopup>();
		if (moveGodKingTemplePopup != null)
		{
			moveGodKingTemplePopup.Open(this);
		}
	}

	private void OpenResultBreakSoulWeaponRewardPopup(ItemResultInfo[] itemResults)
	{
		string stringDirect = LocalizationManager.GetStringDirect("SOULWEAPON_BREAK_INFO_COMPLETE");
		BaseRewardPopup.OpenPopup(new BaseRewardPopup.PopupOpenData().SetTitle(stringDirect).SetItemResultInfos(itemResults), isGlobal: false);
	}

	private void OpenEclipseDungeonRewardPopup()
	{
		EclipseDungeonRewardPopup eclipseDungeonRewardPopup = GetInstance<EclipseDungeonRewardPopup>();
		if (eclipseDungeonRewardPopup != null)
		{
			eclipseDungeonRewardPopup.Open();
		}
	}

	private void OpenNotEnoughSoulStoneFragment(string itemName)
	{
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string stringDirect = LocalizationManager.GetStringDirect("SOULWEAPON_POPUP_SOULFRAGMENT", itemName);
		MessagePopup.Open(title, stringDirect, delegate
		{
			SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Equipment.ToString());
		}, null);
	}

	private void OpenNotEnoughGoldPopup(Action action)
	{
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string message = LocalizationManager.GetString(NShared.NLocalization.System.MOVE_GOLD_SHOP_CONFIRM);
		MessagePopup.Open(title, message, delegate
		{
			if (action != null)
			{
				Vespa_System_Action_Extension_Method.fire(action);
			}
			SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Gold.ToString());
		}, null);
	}

	private void OpenNotEnoughGemPopup(Action action)
	{
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		string message = LocalizationManager.GetString(NShared.NLocalization.System.MOVE_GEM_SHOP_CONFIRM);
		MessagePopup.Open(title, message, delegate
		{
			if (action != null)
			{
				Vespa_System_Action_Extension_Method.fire(action);
			}
			SceneAutoBehaviour<PayShopManagement>.instance.OpenNewPayShop(NewPayShopCategoryType.Ruby.ToString());
		}, null);
	}

	private bool CheckEnoughCurrency(RequirePrice requirePrice, Action action)
	{
		if (requirePrice.ReqPriceCurrencyType == CurrencyType.Gold)
		{
			if (requirePrice.ReqPrice > GlobalAutoBehaviour<AccountManager>.instance.UserManager.Gold)
			{
				OpenNotEnoughGoldPopup(action);
				return false;
			}
		}
		else if (requirePrice.ReqPriceCurrencyType == CurrencyType.Gem && requirePrice.ReqPrice > GlobalAutoBehaviour<AccountManager>.instance.UserManager.TotalGem)
		{
			OpenNotEnoughGemPopup(action);
			return false;
		}
		return true;
	}

	private void CloseSoulWeaponLobbyWindow()
	{
		SoulWeaponLobby soulWeaponLobby = GetInstance<SoulWeaponLobby>();
		if (soulWeaponLobby != null)
		{
			soulWeaponLobby.Close();
		}
	}

	private void CloseRenewSoulWeaponStatPopup()
	{
		RenewSoulWeaponStatPopup renewSoulWeaponStatPopup = GetInstance<RenewSoulWeaponStatPopup>();
		if (renewSoulWeaponStatPopup != null)
		{
			renewSoulWeaponStatPopup.Close();
		}
	}

	private void CloseConfirmRenewSoulWeaponStatPopup()
	{
		ConfirmRenewSoulWeaponStatPopup confirmRenewSoulWeaponStatPopup = GetInstance<ConfirmRenewSoulWeaponStatPopup>();
		if (confirmRenewSoulWeaponStatPopup != null)
		{
			confirmRenewSoulWeaponStatPopup.Close();
		}
	}

	private void CloseResultRenewSoulWeaponStatPopup()
	{
		ResultRenewSoulWeaponStatPopup resultRenewSoulWeaponStatPopup = GetInstance<ResultRenewSoulWeaponStatPopup>();
		if (resultRenewSoulWeaponStatPopup != null)
		{
			resultRenewSoulWeaponStatPopup.Close();
		}
	}

	private void CloseTransferSoulWeaponPopup()
	{
		TransferSoulWeaponPopup transferSoulWeaponPopup = GetInstance<TransferSoulWeaponPopup>();
		if (transferSoulWeaponPopup != null)
		{
			transferSoulWeaponPopup.Close();
		}
	}

	private void RefreshSoulWeaponManageWindow(SoulWeaponInfo soulWeaponInfo)
	{
		EquipItemInfo equipItemInfoBySoulWeaponInfo = GetEquipItemInfoBySoulWeaponInfo(soulWeaponInfo);
		if (equipItemInfoBySoulWeaponInfo != null)
		{
			SoulWeaponManageWindow soulWeaponManageWindow = GetInstance<SoulWeaponManageWindow>();
			if (soulWeaponManageWindow != null && soulWeaponManageWindow.IsOpen())
			{
				soulWeaponManageWindow.Refresh(equipItemInfoBySoulWeaponInfo);
			}
		}
	}

	private void RefreshSoulWeaponReinforceWindow(SoulWeaponInfo soulWeaponInfo)
	{
		EquipItemInfo equipItemInfoBySoulWeaponInfo = GetEquipItemInfoBySoulWeaponInfo(soulWeaponInfo);
		if (equipItemInfoBySoulWeaponInfo != null)
		{
			SoulWeaponReinforceWindow soulWeaponReinforceWindow = GetInstance<SoulWeaponReinforceWindow>();
			if (soulWeaponReinforceWindow != null && soulWeaponReinforceWindow.IsOpen())
			{
				soulWeaponReinforceWindow.Refresh(equipItemInfoBySoulWeaponInfo);
			}
		}
	}

	private void RefreshRestoreSoulStonWindow(RestoreSoulStoneRewardInfo rewardInfo)
	{
		SoulStoneRestoreWindow soulStoneRestoreWindow = GetInstance<SoulStoneRestoreWindow>();
		if (soulStoneRestoreWindow != null && soulStoneRestoreWindow.IsOpen())
		{
			soulStoneRestoreWindow.Refresh(rewardInfo);
		}
	}

	private void RefreshSoulWeaponUpgradeWindow(SoulWeaponInfo soulWeaponInfo)
	{
		EquipItemInfo equipItemInfoBySoulWeaponInfo = GetEquipItemInfoBySoulWeaponInfo(soulWeaponInfo);
		if (equipItemInfoBySoulWeaponInfo != null)
		{
			SoulWeaponUpgradeWindow soulWeaponUpgradeWindow = GetInstance<SoulWeaponUpgradeWindow>();
			if (soulWeaponUpgradeWindow != null && soulWeaponUpgradeWindow.IsOpen())
			{
				soulWeaponUpgradeWindow.Refresh(equipItemInfoBySoulWeaponInfo);
			}
		}
	}

	private void RefreshInven()
	{
		SceneAutoBehaviour<MyHeroManagement>.instance.CloseInvenEquipInfoUIContainer();
		SceneAutoBehaviour<MyHeroManagement>.instance.SetHero3DViewEquipItemIcon();
		SceneAutoBehaviour<InventoryManagement>.instance.CloseInvenEquipInfoUIContainer();
		SceneAutoBehaviour<InventoryManagement>.instance.RefreshHeroEquipList();
	}

	private void RefreshSoulWeaponLobby()
	{
		SoulWeaponLobby soulWeaponLobby = GetInstance<SoulWeaponLobby>();
		if (soulWeaponLobby.IsOpen())
		{
			soulWeaponLobby.SetSoulWeaponButtonState();
		}
	}

	private IEnumerator RefreshSoulWeaponLiberation(float duration)
	{
		yield return new NVespa.NTask.NCustomYieldInstruction.WaitForSeconds(duration);
		SoulWeaponLiberationWindow soulWeaponLiberationWindow = GetInstance<SoulWeaponLiberationWindow>();
		if (soulWeaponLiberationWindow.IsOpen())
		{
			soulWeaponLiberationWindow.Refresh(null);
		}
		yield return null;
	}

	private void RefreshSoulWeaponReinforceButtonState()
	{
		SoulWeaponReinforceWindow soulWeaponReinforceWindow = GetInstance<SoulWeaponReinforceWindow>();
		if (soulWeaponReinforceWindow != null)
		{
			soulWeaponReinforceWindow.IsWaitReinforceResult = false;
			soulWeaponReinforceWindow.SetReinforceButtonState(enable: true);
		}
	}

	private void EnableSoulWeaponLiberationState()
	{
		SoulWeaponLiberationWindow soulWeaponLiberationWindow = GetInstance<SoulWeaponLiberationWindow>();
		if (soulWeaponLiberationWindow != null)
		{
			soulWeaponLiberationWindow.EnableLiberationButtonState(enable: true);
		}
	}

	private EquipItemInfo GetEquipItemInfoBySoulWeaponInfo(SoulWeaponInfo soulWeaponInfo)
	{
		if (soulWeaponInfo == null)
		{
			return null;
		}
		return GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.Get(soulWeaponInfo.EquipItemSlotIndex);
	}

	void SoulWeaponLobby.IListener.MoveGodkingTemple()
	{
		OpenMoveGodKingTemplePopup();
	}

	void SoulWeaponLobby.IListener.EnterSoulWeaponContents()
	{
		OpenSoulWeaponContentsWindow();
	}

	void SoulWeaponLobby.IListener.EnterManageSoulWeapon()
	{
		if (GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponManager.HaveSoulWeapon())
		{
			SoulWeaponInfo soulWeaponInfo = GlobalAutoBehaviour<AccountManager>.instance.SoulWeaponManager.GetValues()[0];
			EquipItemInfo equipItemInfoBySoulWeaponInfo = GetEquipItemInfoBySoulWeaponInfo(soulWeaponInfo);
			OpenSoulWeaponManageWindow(equipItemInfoBySoulWeaponInfo, isSelectFirst: true);
		}
	}

	void SoulWeaponLobby.IListener.EnterDungeon()
	{
		OpenGodkingTrialDungeonList();
	}

	void SoulWeaponLobby.IListener.EnterGodkingRewardPopup()
	{
		OpenGodkingTrialRewardPopup();
	}

	void SoulWeaponLobby.IListener.EnterEclipse()
	{
		OpenEclipseMain();
	}

	void SoulWeaponLobby.IListener.OpenEclipseDungeonRewardPopup()
	{
		OpenEclipseDungeonRewardPopup();
	}

	void SoulWeaponManageWindow.IListener.OpenSoulWeaponReinforceWindow(EquipItemInfo selectedSoulWeaponEquipItemInfo)
	{
		OpenSoulWeaponReinforceWindow(selectedSoulWeaponEquipItemInfo, isSelectFirst: false);
	}

	void SoulWeaponManageWindow.IListener.OpenSoulWeaponUpgradeWindow(EquipItemInfo selectedSoulWeaponEquipItemInfo)
	{
		OpenSoulWeaponUpgradeWindow(selectedSoulWeaponEquipItemInfo, isSelectFirst: false);
	}

	void SoulWeaponManageWindow.IListener.OpenTransferSoulWeaponPopup(EquipItemInfo selectedEquipItemInfo)
	{
		OpenTransferSoulWeaponPopup(selectedEquipItemInfo);
	}

	void SoulWeaponManageWindow.IListener.OpenRenewSoulWeaponStatPopup(EquipItemInfo selectedEquipItemInfo)
	{
		if (selectedEquipItemInfo != null && selectedEquipItemInfo.IsSoulWeapon())
		{
			SoulWeaponInfo soulWeaponInfo = selectedEquipItemInfo.SoulWeaponInfo;
			if (IsProcessingRenewOption(soulWeaponInfo))
			{
				OpenConfirmRenewSoulWeaponStatPopup(soulWeaponInfo);
			}
			else
			{
				OpenRenewSoulWeaponStatPopup(selectedEquipItemInfo);
			}
		}
	}

	private bool IsProcessingRenewOption(SoulWeaponInfo soulWeaponInfo)
	{
		if (soulWeaponInfo.RenewOptionRatio1 != 0 || soulWeaponInfo.RenewOptionRatio2 != 0 || soulWeaponInfo.RenewOptionBonusRatio1 != 0 || soulWeaponInfo.RenewOptionBonusRatio2 != 0)
		{
			return true;
		}
		return false;
	}

	void SoulWeaponWebManager.IListener.OnSoulWeaponInjection(SoulWeaponInfo soulWeaponInfo)
	{
		RefreshInven();
		RefreshSoulWeaponManageWindow(soulWeaponInfo);
		RefreshSoulWeaponReinforceWindow(soulWeaponInfo);
	}

	void SoulWeaponWebManager.IListener.OnSoulWeaponLiberation(SoulWeaponInfo soulWeaponInfo)
	{
		RefreshInven();
		RefreshSoulWeaponLobby();
		VespaCoroutine.Start(RefreshSoulWeaponLiberation(0.5f));
		OpenSoulWeaponLiberationResultWindow(soulWeaponInfo);
	}

	void SoulWeaponWebManager.IListener.OnSoulWeaponReinforce(SoulWeaponInfo soulWeaponInfo, SoulWeaponInfo resultSoulWeaponInfo, bool isSuccess)
	{
		if (resultSoulWeaponInfo != null)
		{
			RefreshSoulWeaponManageWindow(resultSoulWeaponInfo);
			RefreshInven();
			SoulWeaponReinforceWindow soulWeaponReinforceWindow = GetInstance<SoulWeaponReinforceWindow>();
			if (soulWeaponReinforceWindow != null)
			{
				EnableTopMostMenu(enable: false);
				soulWeaponReinforceWindow.PlayAnimation(soulWeaponInfo, isSuccess);
			}
		}
	}

	void SoulWeaponWebManager.IListener.OnSoulWeaponTransition(int equipItemSlotIndex, SoulWeaponInfo resultSoulWeaponInfo)
	{
		CloseTransferSoulWeaponPopup();
		RefreshInven();
		RefreshSoulWeaponManageWindow(resultSoulWeaponInfo);
		OpenResultTransferSoulWeaponPopup(resultSoulWeaponInfo);
	}

	void SoulWeaponWebManager.IListener.OnSoulWeaponUpgrade(SoulWeaponInfo originSoulWeaponInfo, SoulWeaponInfo soulWeaponInfo, Action onClose)
	{
		RefreshSoulWeaponManageWindow(soulWeaponInfo);
		RefreshSoulWeaponUpgradeWindow(soulWeaponInfo);
		RefreshInven();
		EquipItemInfo equipItemInfoBySoulWeaponInfo = GetEquipItemInfoBySoulWeaponInfo(soulWeaponInfo);
		if (equipItemInfoBySoulWeaponInfo != null)
		{
			OpenSoulWeaponUpgradeResultWindow(originSoulWeaponInfo, equipItemInfoBySoulWeaponInfo, onClose);
		}
	}

	void SoulWeaponWebManager.IListener.OnSoulWeaponRenewOption(SoulWeaponInfo soulWeaponInfo)
	{
		CloseRenewSoulWeaponStatPopup();
		CloseResultRenewSoulWeaponStatPopup();
		OpenConfirmRenewSoulWeaponStatPopup(soulWeaponInfo);
	}

	void SoulWeaponWebManager.IListener.OnSoulWeaponConfirmRenewOption(SoulWeaponInfo soulWeaponInfo, SoulWeaponInfo resultSoulWeaponInfo)
	{
		OpenResultRenewSoulWeaponStatPopup(soulWeaponInfo, resultSoulWeaponInfo);
		RefreshInven();
		RefreshSoulWeaponManageWindow(soulWeaponInfo);
		CloseConfirmRenewSoulWeaponStatPopup();
	}

	void SoulWeaponWebManager.IListener.OnRestoreSoulStone(RestoreSoulStoneRewardInfo rewardInfo)
	{
		RefreshRestoreSoulStonWindow(rewardInfo);
	}

	void SoulWeaponWebManager.IListener.OnBreakSoulWeapon(ItemResultInfo[] itemResults)
	{
		RefreshInven();
		OpenResultBreakSoulWeaponRewardPopup(itemResults);
	}

	void SoulWeaponWebManager.IListener.OnGetBonusSoulStone(RestoreSoulStoneRewardInfo rewardInfo)
	{
		RefreshRestoreSoulStonWindow(rewardInfo);
	}

	void SoulWeaponWebManager.IListener.FailSoulWeaponReinforce()
	{
		RefreshSoulWeaponReinforceButtonState();
	}

	void SoulWeaponWebManager.IListener.FailSoulWeaponLiberation()
	{
		EnableSoulWeaponLiberationState();
	}

	void SoulWeaponLiberationWindow.IListener.LiberationSoulWeapon(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo == null)
		{
			EnableSoulWeaponLiberationState();
		}
		else
		{
			SingletonAuto<SoulWeaponWebManager>.instance.RequesetLiberation(equipItemInfo.SlotIndex);
		}
	}

	void SoulWeaponUpgradeWindow.IListener.UpgradeSoulWeapon(EquipItemInfo equipItemInfo, RequirePrice requirePrice, Action onClose)
	{
		if (equipItemInfo == null || equipItemInfo.SoulWeaponInfo == null)
		{
			Vespa_System_Action_Extension_Method.fire(onClose);
		}
		else if (CheckEnoughCurrency(requirePrice, onClose))
		{
			SingletonAuto<SoulWeaponWebManager>.instance.RequestUpgrade(equipItemInfo.SoulWeaponInfo, onClose);
		}
		else
		{
			Vespa_System_Action_Extension_Method.fire(onClose);
		}
	}

	void SoulWeaponUpgradeResultWindow.IListener.CloseUpgradeResultWindow()
	{
		EnableTopMostMenu(enable: true);
	}

	void SoulStoneRestoreWindow.IListener.OnClickRestoreButton()
	{
		OpenRestoreSoulStoneMessagePopup();
	}

	void SoulStoneRestoreWindow.IListener.OnClickSelectedSoulStoneButton(int itemIndex)
	{
		OpenSelectedSoulStoneMessagePopup(itemIndex);
	}

	void SoulStoneRestoreWindow.IListener.GetBonus()
	{
		SingletonAuto<SoulWeaponWebManager>.instance.RequestGetSoulStoneMileageReward();
	}

	void SoulStoneRestoreWindow.IListener.OpenSoulStoneRestoreRewardPopup(RestoreSoulStoneRewardInfo rewardInfo, bool isGetBonus)
	{
		OpenRestoreSoulStoneRewardPopup(rewardInfo, isGetBonus);
		RefreshRestoreSoulStonWindow(null);
	}

	void SoulStoneRestoreWindow.IListener.NotEnoughSoulStoneFragment(int itemIndex)
	{
		ItemData data = GameRoot.Instance.ItemDataContainer.GetData(itemIndex);
		if (data != null)
		{
			OpenNotEnoughSoulStoneFragment(data.Name);
		}
	}

	void SoulWeaponReinforceWindow.IListener.EtherInjection(EquipItemInfo equipItemInfo, RequirePrice requirePrice, int[] itemInfos, bool excessSuccessRate)
	{
		if (equipItemInfo == null || itemInfos.Length == 0 || !CheckEnoughCurrency(requirePrice, null))
		{
			return;
		}
		if (excessSuccessRate)
		{
			string stringDirect = LocalizationManager.GetStringDirect("ETHERREINFORCE_IMPROVE_NOTICE_01");
			MessagePopup.Open(LocalizationManager.GetString(NShared.NLocalization.Common.INFORM), stringDirect, delegate
			{
				SingletonAuto<SoulWeaponWebManager>.instance.RequestInjection(equipItemInfo.SlotIndex, itemInfos);
			}, null, NShared.NLocalization.Common.COMMON_CONFIRM, NShared.NLocalization.Common.COMMON_CANCEL);
		}
		else
		{
			SingletonAuto<SoulWeaponWebManager>.instance.RequestInjection(equipItemInfo.SlotIndex, itemInfos);
		}
	}

	void SoulWeaponReinforceWindow.IListener.ReinforceSoulWeapon(EquipItemInfo equipItemInfo, long reqPrice)
	{
		if (equipItemInfo != null)
		{
			if (reqPrice > GlobalAutoBehaviour<AccountManager>.instance.UserManager.Gold)
			{
				OpenNotEnoughGoldPopup(null);
				RefreshSoulWeaponReinforceButtonState();
			}
			else
			{
				SingletonAuto<SoulWeaponWebManager>.instance.RequestReinforce(equipItemInfo.SoulWeaponInfo);
			}
		}
	}

	void SoulWeaponReinforceWindow.IListener.ResultReinforceSoulWeapon(SoulWeaponInfo soulWeaponInfo, bool isSuccess, Action onClose)
	{
		OpenSoulWeaponReinforceResultWindow(soulWeaponInfo, isSuccess, onClose);
	}

	void SoulWeaponReinforceWindow.IListener.OpenPopupDoNotTryReinforce()
	{
		string stringDirect = LocalizationManager.GetStringDirect("ETHERREINFORCE_IMPROVE_NOTICE_02");
		MessagePopup.Open(LocalizationManager.GetString(NShared.NLocalization.Common.INFORM), stringDirect);
	}

	void SoulWeaponContentsWindow.IListener.OnclickSoulStoneRestoreButton()
	{
		OpenSoulStoneRestoreWindow();
	}

	void SoulWeaponContentsWindow.IListener.OnClickSoulWeaponLiberationButton()
	{
		OpenSoulWeaponLiberationWindow(null, isSelectFirst: false);
	}

	void SoulWeaponContentsWindow.IListener.OnClickSoulWeaponManageButton()
	{
		OpenSoulWeaponManageWindow(null, isSelectFirst: true);
	}

	void SoulWeaponContentsWindow.IListener.OnclickSoulWeaponReinforceButton()
	{
		OpenSoulWeaponReinforceWindow(null, isSelectFirst: true);
	}

	void SoulWeaponContentsWindow.IListener.OnClickSoulWeaponUpgradeButton()
	{
		OpenSoulWeaponUpgradeWindow(null, isSelectFirst: true);
	}

	void TransferSoulWeaponPopup.IListener.TransferSoulWeapon(int soulWeaponSlotIndex, int specialWeaponSlotIndex, RequirePrice requirePrice)
	{
		OpenTransferMessagePopup(soulWeaponSlotIndex, specialWeaponSlotIndex, requirePrice, delegate
		{
			CloseTransferSoulWeaponPopup();
		});
	}

	void RenewSoulWeaponStatPopup.IListener.RenewSoulWeaponStat(SoulWeaponInfo soulWeaponInfo, RequirePrice requirePrice)
	{
		OpenRenewMessagePopup(soulWeaponInfo, requirePrice, delegate
		{
			CloseRenewSoulWeaponStatPopup();
		});
	}

	void ConfirmRenewSoulWeaponStatPopup.IListener.ConfirmRenewSoulWeaponStat(SoulWeaponInfo originSoulWeaponInfo, SoulWeaponInfo soulWeaponInfo, bool isRenew)
	{
		SingletonAuto<SoulWeaponWebManager>.instance.RequestSoulWeaponConfirmRenewOption(originSoulWeaponInfo, soulWeaponInfo.EquipItemSlotIndex, isRenew);
	}

	void ResultRenewSoulWeaponStatPopup.IListener.RetryRenew(SoulWeaponInfo soulWeaponInfo, RequirePrice requirePrice)
	{
		OpenRenewMessagePopup(soulWeaponInfo, requirePrice, delegate
		{
			CloseResultRenewSoulWeaponStatPopup();
		});
	}

	void SoulStoneRestoreRewardPopup.IListener.GetBonus()
	{
		SingletonAuto<SoulWeaponWebManager>.instance.RequestGetSoulStoneMileageReward();
	}

	void SoulWeaponReinforceResultWindow.IListener.CloseResultWindow(SoulWeaponInfo soulWeaponInfo)
	{
		OpenSoulWeaponReinforceWindowBySoulWeaponInfo(soulWeaponInfo, isSelectFirst: false);
	}

	void BreakSoulWeaponPopup.IListener.BreakSoulWeapon(EquipItemInfo equipItemInfo)
	{
		if (equipItemInfo != null)
		{
			SingletonAuto<SoulWeaponWebManager>.instance.RequestBreakSoulWeapon(new int[1] { equipItemInfo.SlotIndex });
		}
	}

	public void OpenEclipseMain(bool openByEntryMenu = false)
	{
		GetInstance<EclipseDeckDetailPopup>().Close();
		GetInstance<EclipseMain>().Open(this);
	}

	public void OpenEclipseMainWithReqeust(bool openByEntryMenu)
	{
		if (RestoredPortal.GodkingAccess())
		{
			RequestGetEclipseInfo(showLoading: true, delegate
			{
				OpenEclipseMain(openByEntryMenu);
			}, null);
		}
	}

	public void RequestGetEclipseInfo(bool showLoading, Action<NShared.GetEclipseInfo.Request, NShared.GetEclipseInfo.Response> successAction, Action failAction)
	{
		if (_eclipseWebManager == null)
		{
			InitEclipseWebManager();
		}
		_eclipseWebManager.RequestGetEclipsInfo(showLoading, successAction, failAction);
	}

	public void ReOpenEclipseMain()
	{
		GetInstance<EclipseMain>().Close();
		GetInstance<EclipseBattleReady>().Close();
		if (GetInstance<SoulWeaponLobby>().IsOpen())
		{
			GetInstance<SoulWeaponLobby>().Close();
			OpenSoulWeaponLobby(delegate
			{
				OpenEclipseMain();
			});
		}
		else
		{
			OpenEclipseMainWithReqeust(openByEntryMenu: true);
		}
	}

	public void RefreshEclipseMain()
	{
		RequestGetEclipseInfo(showLoading: true, delegate
		{
			EclipseMain eclipseMain = GetInstance<EclipseMain>();
			if (eclipseMain != null)
			{
				eclipseMain.RefreshEclipseMain();
			}
		}, null);
	}

	public void OpenTutorialEclipsePartySetting()
	{
		GetInstance<EclipseDeckSetting>().OpenTutorial(this, EclipseMaxDeckCount);
	}

	public void EclipseReturnState(Action OnEnd)
	{
		RequestGetEclipseInfo(showLoading: true, delegate
		{
			OpenEclipseMain();
			if (OnEnd != null)
			{
				Vespa_System_Action_Extension_Method.fire(OnEnd);
			}
		}, null);
	}

	private void RequestSetEclipsDeck(EclipseDeckResult[] deckInfos, Action action)
	{
		string heroInfos = Json.Serialize(BaseJsonMarshaler.EncodeArray(deckInfos, JsonMarshaler.EncodeJsonObject));
		NShared.SetEclipseDeck.Request request = new NShared.SetEclipseDeck.Request
		{
			HeroInfos = heroInfos
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SetEclipseDeck.Response response)
		{
			if (response.Result != NShared.SetEclipseDeck.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.Apply(response.DeckResults);
				Vespa_System_Action_Extension_Method.fire(action);
			}
		});
	}

	private void RequestGiveUpEclipseDungeon(Action action)
	{
		NShared.GiveUpEclipseDungeon.Request request = new NShared.GiveUpEclipseDungeon.Request
		{
			MatchIndex = GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetEclipseInfo().MatchIndex
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GiveUpEclipseDungeon.Response response)
		{
			if (response.Result != NShared.GiveUpEclipseDungeon.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.ItemManager.Apply(response.ItemResults);
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.CurrencyResults);
				GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.Apply(response.EclipseDungeonInfo);
				OpenEclipseRewardPopup(response.ItemResults, null, response.CurrencyResults);
				Vespa_System_Action_Extension_Method.fire(action);
			}
		});
	}

	public void OpenEclipseRewardPopup(ItemResultInfo[] itemResults, EquipItemInfo[] equipItemInfos, CurrencyResultInfo3[] currencyResults)
	{
		EclipseRewardPopup.OpenEclipseRewardPopup(new BaseRewardPopup.PopupOpenData().SetItemResultInfos(itemResults).SetEquipResultInfos(equipItemInfos).SetCurrencyResultInfos3(currencyResults), isGlobal: false, null, this);
	}

	void EclipseMain.Listener.OnClickBattleReady()
	{
		if (_eclipseWebManager != null)
		{
			_eclipseWebManager.RequestGetEclipsDeck(delegate
			{
				GetInstance<EclipseBattleReady>().Open(this, null, EclipseMaxDeckCount);
			});
		}
	}

	void EclipseMain.Listener.OnClickDeckSetting()
	{
		long stamina = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetStamina(StaminaType.EclipseKey);
		int num = 0;
		if (stamina <= num)
		{
			string stringDirect = LocalizationManager.GetStringDirect("ECLIPSE_NOT_ENOUGH_ECLIPSE_KEY_MESSAGE_POPUP_DESC");
			MessagePopup.Open(LocalizationManager.GetString(NShared.NLocalization.Common.INFORM), stringDirect);
		}
		else if (_eclipseWebManager != null)
		{
			_eclipseWebManager.RequestGetEclipsDeck(delegate
			{
				GetInstance<EclipseDeckSetting>().Open(this, EclipseMaxDeckCount, null);
			});
		}
	}

	void EclipseMain.Listener.OnClickRewardInfo()
	{
		GetInstance<EclipseRewardInfo>().Open(null);
	}

	void EclipseDeckSetting.Listener.OnClickStartBattle(EclipseDeckResult[] deckInfos)
	{
		new LocalAccountBool(i_default: false, $"Eclipse_BattlePopup_Skip)");
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
		Action<int, int> onYes = delegate(int ticketCount, int gameSpeed)
		{
			RequestSetEclipsDeck(deckInfos, delegate
			{
				BattlePartySetting.ExtraOptionData extraOption = new BattlePartySetting.ExtraOptionData
				{
					EnterTicketCount = ticketCount,
					OnlineGameSpeedRatio = gameSpeed
				};
				BeginEclipse(extraOption);
			});
		};
		GetInstance<EclipseStartPopup>().Open(title, isContinue: false, onYes);
	}

	private void StartEclipseBatte()
	{
		EclipseInfo eclipseInfo = GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetEclipseInfo();
		if (eclipseInfo == null)
		{
			return;
		}
		if (Vespa_String_Extension_Method2.TryParseNetworkTime2(eclipseInfo.ExpireTime, out var o_dateTime) && Mathf.Max((int)(o_dateTime - GlobalAutoBehaviour<AccountManager>.instance.GetCurrentServerUTCTime()).TotalSeconds, 0) <= 0)
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("RESULT_TYPE.ECLIPSE_BATTLE_TIME_OVER"));
			return;
		}
		EclipseDungeonInfo eclipseRewardInfo = GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetEclipseRewardInfo();
		if (eclipseRewardInfo != null)
		{
			new LocalAccountBool(i_default: false, $"Eclipse_Battle_Continue_Popup_Skip)");
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
			Action<int> onYes = delegate(int fastGameSpeed)
			{
				BattlePartySetting.ExtraOptionData extraOption = new BattlePartySetting.ExtraOptionData
				{
					OnlineGameSpeedRatio = fastGameSpeed,
					EnterTicketCount = eclipseRewardInfo.EnterTicketCount
				};
				BeginEclipse(extraOption);
			};
			GetInstance<EclipseStartPopup>().Open(title, isContinue: true, onYes);
		}
	}

	public void BeginEclipse(BattlePartySetting.ExtraOptionData extraOption = null)
	{
		EclipseInfo eclipseInfo = GlobalAutoBehaviour<AccountManager>.instance.EclipseManager.GetEclipseInfo();
		int chapterIndex = eclipseInfo.ChapterIndex;
		int dungeonIndex = eclipseInfo.DungeonIndex;
		EnterDungeonData enterDungeon = new EnterDungeonData(chapterIndex, dungeonIndex, dungeonIndex, NShared.DungeonDifficultyType.Easy);
		SingletonAuto<BattleContext>.instance.RequestBeginEclipse(enterDungeon, extraOption);
	}

	void EclipseBattleReady.Listener.OnClickStartBattle()
	{
		StartEclipseBatte();
	}

	void EclipseBattleReady.Listener.OnClickGiveUp()
	{
		string stringDirect = LocalizationManager.GetStringDirect("ECLIPSE_BATTLE_GIVE_UP_POPUP_TITLE");
		string stringDirect2 = LocalizationManager.GetStringDirect("ECLIPSE_BATTLE_GIVE_UP_POPUP_DESC");
		MessagePopup3Button.Open(stringDirect, stringDirect2, delegate
		{
			RequestGiveUpEclipseDungeon(null);
		}, null);
	}

	void EclipseBattleReady.Listener.OnClickGetReward()
	{
		RequestGiveUpEclipseDungeon(null);
	}

	void EclipseBattleReady.Listener.OnClickDeckDetailInfo(CachedHeroInfo[] heroInfos, string titleName)
	{
		GetInstance<EclipseDeckDetailPopup>().Open(null, heroInfos, titleName, LocalizationManager.GetStringDirect("ECLIPSE_CONTINUE_TEAM_HERO_EQUIP_POPUP_DESC"));
	}

	void EclipseMain.ReturnStateListener.SetHideRoot(bool enable)
	{
		GetInstance<EclipseMain>().SetHideRoot(enable);
		GetInstance<SoulWeaponLobby>().InitEclipseKeyGauge();
	}

	void MoveGodKingTemplePopup.IListener.MoveGodKingTemple(int chapterIndex, int dungeonIndex)
	{
		SceneAutoBehaviour<LobbyManagement>.instance.MoveMap(chapterIndex, dungeonIndex);
		CloseSoulWeaponLobbyWindow();
	}

	void MoveGodKingTemplePopup.IListener.Close()
	{
		CloseSoulWeaponLobbyWindow();
	}

	void BattlePartySetting.Listener.StartBattle(EnterDungeonData enterDungeon, BattlePartySetting.BattlePartyInfo battlePartyInfo, BattlePartySetting.ExtraOptionData extraOption)
	{
		if (enterDungeon.OriginalDungeonData.BattleType == BattleType.GodkingTrial)
		{
			StartGodkingTrialDungeon(enterDungeon, battlePartyInfo, extraOption);
		}
	}

	bool BattlePartySetting.Listener.OnCloseRequest(BattlePartySetting closeThis)
	{
		return true;
	}

	void BattlePartySetting.Listener.OpenHeroDetailView(CachedHeroInfo heroInfo)
	{
	}

	bool BattlePartySetting.Listener.OpenAutoNextBattleSelect()
	{
		return false;
	}

	void EclipseRewardPopup.IListener.ProgressNextTeam()
	{
	}

	void EclipseRewardPopup.IListener.ShowEclipseBattleRecord()
	{
		EclipseBattleRecordPopup.Open(this);
	}

	void EclipseBattleRecordPopup.IListener.ShowEclipseClearWaveReward(int deckIndex, int waveIndex)
	{
		EclipseClearWaveRewardPopup.Open(deckIndex, waveIndex);
	}
}
