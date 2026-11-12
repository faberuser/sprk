using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NContentsDefine;
using NGame2.NContentsInfo;
using NGame2.NMatch;
using NGame2.NOrdeal;
using NGame2.NScene;
using NGame2.NScene.NLobby;
using NGame2.NSocket;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NGameAsset;
using NShared;
using NShared.GetReplayList;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NSocket;
using NVespa.NTask;
using NVespa.NTask.NRuntime;
using UnityEngine;

namespace NGame2.NUI.NManager;

public class MatchManagement : BaseManagement<MatchManagement>, MatchSelect.Listener, MatchMyInfo.Listener, BattlePartySetting.Listener, MatchBanPick.Listener, PvpResult.Listener
{
	public static MatchPvpType SelectedPvpType;

	private bool banPickPending;

	protected override void Awake()
	{
		base.Awake();
		UseBackgroundBlack(use: true);
		AddInstance<MatchSelect>("Resources/Prefab2/Window/Match/MatchSelect");
		AddInstance<MatchMenu>("Resources/Prefab2/Window/Match/MatchMenu");
		AddInstance<GlobalMatchRanking>("Resources/Prefab2/Window/Match/GlobalMatchRanking");
		AddInstance<MatchMyInfo>("Resources/Prefab2/Window/Match/MatchMyInfo");
		AddInstance<MatchTotalInfo>("Resources/Prefab2/Window/Match/MatchTotalInfo");
		AddInstance<MatchBanPickRewardInfo>("Resources/Prefab2/Window/Match/MatchBanPickRewardInfo");
		AddInstance<MatchHistory>("Resources/Prefab2/Window/Match/MatchHistory");
		AddInstance<BattlePartySetting>("Resources/Prefab2/Window/BattlePartySetting/BattlePartySetting");
		AddInstance<MatchBanPick>("Resources/Prefab2/Window/Match/MatchBanPickRenewal");
		AddInstance<PvpResult>("Resources/Prefab2/Window/Battle/PvpResult");
		AddInstance<MatchWaitingRenewal>("Resources/Prefab2/Window/Match/MatchWaitingRenewal");
		AddInstance<MatchBanPickHeroList>("Resources/Prefab2/Window/Match/MatchBanPickHeroList");
	}

	private void Reset()
	{
		SelectedPvpType = MatchPvpType.Normal;
	}

	public void OpenMatchSelect(bool immediately = false, CampaignNodeData nodeData = null, NShared.EntryMenu menu = null)
	{
		menu = RestoredPortal.ArenaMenu(menu);
		CloseBut();
		Reset();
		OpenMatchSelectAsync(immediately).Commit();
		GetInstance<MatchSelect>().SetArena(menu.ChildIndices);
	}

	public void OpenMatchInfo(MatchPvpType pvpType, Action onEnd = null)
	{
		Reset();
		SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.RequestSeasonInfo(pvpType, delegate(NGame2.NContentsInfo.MatchArena.MatchSeasonInfo seasonInfo)
		{
			if (seasonInfo == null)
			{
				CenterMessage.Open(LocalizationManager.GetStringDirect("MATCH_PICKBAN_SEASON_INFO_REAL_END"));
			}
			else
			{
				SelectedPvpType = pvpType;
				Vespa_Task_ExtensionMethod.ADDTASK(new GroupTask().ADDTASK(OpenMatchInfoAsync(pvpType)), onEnd).Commit();
			}
		});
	}

	public void OpenMatchType(MatchPvpType pvpType, Action onEnd = null)
	{
		MatchSelect matchSelect = GetInstance<MatchSelect>();
		if ((bool)matchSelect && !matchSelect.IsOpen())
		{
			OpenMatchSelect(immediately: true);
		}
		OpenMatchInfo(pvpType, onEnd);
	}

	public void OpenMatchType2(MatchPvpType pvpType, Action onEnd = null)
	{
		MatchSelect matchSelect = GetInstance<MatchSelect>();
		if ((bool)matchSelect && !matchSelect.IsOpen())
		{
			OpenMatchSelect(immediately: true);
		}
		SelectedPvpType = pvpType;
		Vespa_Task_ExtensionMethod.ADDTASK(new GroupTask().ADDTASK(OpenMatchInfoAsync(pvpType)), onEnd).Commit();
	}

	private ITask OpenMatchSelectAsync(bool immediately)
	{
		return base.TaskFactory.Open(delegate(MatchSelect window)
		{
			window.Init(this);
		}, immediately);
	}

	private ITask OpenMatchInfoAsync(MatchPvpType pvpType)
	{
		return new GroupTask().ADDTASK(RequestMatchRankAsync(pvpType)).ADDTASK(RequestSeasonInfoAsync(pvpType)).ADDTASK(base.TaskFactory.Open<MatchMenu>())
			.ADDTASK(base.TaskFactory.Open(delegate(MatchMyInfo window)
			{
				window.InitMyInfo(this, pvpType);
				window.UpdateRank();
				window.UpdateSeasonInfo();
			}))
			.ADDTASK(GetOpenMatchRankingTask(pvpType));
	}

	private void InitMatchRanking(int tierIndex, MatchPvpType pvpType)
	{
		SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.ClearOldRankerList(pvpType, ContentsDefine.MatchArena.Duration_Short);
		MatchTierType initialTierType = NGame2.NContentsDefine.MatchArena.GetTierData(pvpType.ToMatchArenaType(), tierIndex)?.Tier ?? MatchTierType.Bronze;
		GetInstance<GlobalMatchRanking>().Init(pvpType, initialTierType);
	}

	public void OpenMatchRewardInfo()
	{
		if (SelectedPvpType == MatchPvpType.Normal)
		{
			GetInstance<MatchTotalInfo>().Open(MatchPvpType.Normal);
		}
		else
		{
			GetInstance<MatchBanPickRewardInfo>().Open(MatchPvpType.BanPick);
		}
	}

	public void CheckFirstViewSeasonReward_BanPick()
	{
		NGame2.NContentsInfo.MatchArena.MatchSeasonInfo matchSeasonInfo = SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.GetMatchSeasonInfo(MatchPvpType.BanPick);
		if (matchSeasonInfo != null && matchSeasonInfo.MatchSeasonType != MatchSeasonType.Free)
		{
			string.Format("{0}_{1}", MatchArenaType.BanPick, "CheckFirstViewSeasonReward");
			LocalAccountInt localAccountInt = new LocalAccountInt(0, string.Format("{0}_{1}", MatchArenaType.BanPick, "CheckFirstViewSeasonReward"));
			if (localAccountInt.Value < matchSeasonInfo.SeasonIndex)
			{
				MatchBanPickRewardInfo matchBanPickRewardInfo = GetInstance<MatchBanPickRewardInfo>();
				matchBanPickRewardInfo.Open(MatchPvpType.BanPick);
				matchBanPickRewardInfo.DirectToggle(matchBanPickRewardInfo.ToggleSeasonReward);
				int seasonIndex = matchSeasonInfo.SeasonIndex;
				localAccountInt.Value = seasonIndex;
			}
		}
	}

	private ITask GetOpenMatchRankingTask(MatchPvpType pvpType)
	{
		return base.TaskFactory.Open<GlobalMatchRanking>(delegate
		{
			ArenaBattleInfo arenaBattleInfo = SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.GetArenaBattleInfo(pvpType);
			if (arenaBattleInfo != null)
			{
				int tierIndex = NGame2.NContentsDefine.MatchArena.GetTierIndex(pvpType.ToMatchArenaType(), arenaBattleInfo);
				InitMatchRanking(tierIndex, pvpType);
			}
		});
	}

	private ITask RequestMatchRankAsync(MatchPvpType pvpType)
	{
		bool isComplete = false;
		SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.RequestMatchRank(pvpType, delegate
		{
			isComplete = true;
		});
		return new WaitTask(() => !isComplete);
	}

	private ITask RequestSeasonInfoAsync(MatchPvpType pvpType)
	{
		bool isComplete = false;
		SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.RequestSeasonInfo(pvpType, delegate
		{
			isComplete = true;
		});
		return new WaitTask(() => !isComplete);
	}

	public void RefreshMatchInfo_BanPickOnly()
	{
		SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.RequestMatchRank(MatchPvpType.BanPick, delegate
		{
			SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.RequestSeasonInfo(MatchPvpType.BanPick, delegate
			{
				ArenaBattleInfo arenaBattleInfo = SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.GetArenaBattleInfo(MatchPvpType.BanPick);
				if (arenaBattleInfo != null)
				{
					MatchMyInfo matchMyInfo = GetInstance<MatchMyInfo>(openedOnly: true);
					if (matchMyInfo != null)
					{
						matchMyInfo.InitMyInfo(this, MatchPvpType.BanPick);
						matchMyInfo.UpdateRank();
						matchMyInfo.UpdateSeasonInfo();
					}
					if (GetInstance<GlobalMatchRanking>(openedOnly: true) != null)
					{
						int tierIndex = NGame2.NContentsDefine.MatchArena.GetTierIndex(MatchArenaType.BanPick, arenaBattleInfo);
						InitMatchRanking(tierIndex, MatchPvpType.BanPick);
					}
				}
			});
		});
	}

	public void UpdateMyBattleInfoIfSame(MatchPvpType pvpType)
	{
		if (IsOpened<MatchMyInfo>() && GetInstance<MatchMyInfo>().PvpType == pvpType)
		{
			GetInstance<MatchMyInfo>().UpdateBattleInfo();
		}
	}

	public void UpdateSeasonDataIfSame(MatchPvpType pvpType)
	{
		if (IsOpened<MatchMyInfo>())
		{
			MatchMyInfo matchMyInfo = GetInstance<MatchMyInfo>();
			if (matchMyInfo.PvpType == pvpType)
			{
				matchMyInfo.UpdateSeasonInfo();
			}
		}
	}

	public void RefreshRankers()
	{
		SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.ClearOldRankerList(SelectedPvpType, ContentsDefine.MatchArena.Duration_Short);
		if (GetInstance<MatchMyInfo>().IsShowing())
		{
			OpenMatchInfoAsync(SelectedPvpType).Commit();
		}
	}

	public void OpenMatchBanPick(MatchedAccountInfo[] matchedAccountInfo)
	{
		GetInstance<MatchBanPick>().Open(this, matchedAccountInfo);
	}

	public void CloseMatchBanPick()
	{
		MatchBanPick matchBanPick = GetInstance<MatchBanPick>(openedOnly: true);
		if (matchBanPick != null)
		{
			matchBanPick.Close();
		}
	}

	public void OpenMatchWaiting_BanPickOnly()
	{
		GetInstance<MatchWaitingRenewal>().OpenMatchWaiting();
	}

	public void OnMatchWaitingClose()
	{
		if (GetInstance<MatchWaitingRenewal>().IsOpen())
		{
			GetInstance<MatchWaitingRenewal>().Close();
		}
	}

	public void OnMatchBanPickChangeState(MatchBanPickState state, BanPickAccountInfo[] banPickAccountInfos, int[] heroIndices, int remainTime)
	{
		GetInstance<MatchBanPick>().OnRecvChangeState(state, null, banPickAccountInfos, heroIndices, remainTime);
	}

	public void OnMatchBanPickEndState(MatchedAccountInfo[] accountInfos, BanPickAccountInfo[] banPickAccountInfos)
	{
		GetInstance<MatchBanPick>().OnRecvChangeState(MatchBanPickState.PHASE10_BATTLE_START, accountInfos, banPickAccountInfos, null, 0);
	}

	public void OnMatchBanPickTimeOutState(long ownerAccountId)
	{
		GetInstance<MatchBanPick>().OnTimeoutState(ownerAccountId);
	}

	public void OnMatchBanPickGiveUp(long ownerAccountId)
	{
	}

	public void OpenResult(MatchResultInfo matchResultInfo, PlayerBattleBanPickInfo banPickInfo)
	{
		InspectHeroInfo inspectHeroInfo = GlobalAutoBehaviour<PopupManegement>.instance.FindWindow<InspectHeroInfo>();
		if (inspectHeroInfo != null)
		{
			inspectHeroInfo.Close();
		}
		GetInstance<MatchBanPick>().GiveUpPending = false;
		if (!matchResultInfo.Win)
		{
			GetInstance<PvpResult>().DepthOrder = EDepthOrder.Topmost;
			GetInstance<PvpResult>().Open_BanPickOnly(matchResultInfo, null, this);
			return;
		}
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.WARNING);
		string stringDirect = LocalizationManager.GetStringDirect("MATCH_PICKBAN _MATCH_CANCEL_WIN_MSG");
		MessagePopup.Open(title, stringDirect, delegate
		{
			GetInstance<PvpResult>().DepthOrder = EDepthOrder.Topmost;
			GetInstance<PvpResult>().Open_BanPickOnly(matchResultInfo, banPickInfo, this);
		});
	}

	public bool IsOpenedBanPickWindow()
	{
		if (!IsOpened<MatchWaitingRenewal>())
		{
			return IsOpened<MatchBanPick>();
		}
		return true;
	}

	public ItemData<CostumeSelectItemData> GetBanPickSeasonCostumeReward(ItemType itemType)
	{
		IList<MatchBanPickTierRewardData> allData = GameRoot.Instance.GlobalBanPickTierRewardDataContainer.GetAllData();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(allData))
		{
			return null;
		}
		MatchBanPickTierRewardData matchBanPickTierRewardData = allData[0];
		for (int i = 1; i < allData.Count; i++)
		{
			MatchBanPickTierRewardData matchBanPickTierRewardData2 = allData[i];
			if (matchBanPickTierRewardData.MinRank >= matchBanPickTierRewardData2.MinRank)
			{
				if (matchBanPickTierRewardData.MinRank > matchBanPickTierRewardData2.MinRank)
				{
					matchBanPickTierRewardData = matchBanPickTierRewardData2;
				}
				else if (matchBanPickTierRewardData.MaxRank >= matchBanPickTierRewardData2.MaxRank && matchBanPickTierRewardData.MaxRank > matchBanPickTierRewardData2.MaxRank)
				{
					matchBanPickTierRewardData = matchBanPickTierRewardData2;
				}
			}
		}
		RewardData data = GameRoot.Instance.RewardDataContainer.GetData(matchBanPickTierRewardData.ShowRewardIndex);
		if (data == null)
		{
			return null;
		}
		List<RewardElementData> rewardItemList = data.GetRewardItemList();
		if (__Extension_Method_ICollection__.IsNullOrEmpty(rewardItemList))
		{
			return null;
		}
		ItemData<CostumeSelectItemData> itemData = null;
		foreach (RewardElementData item in rewardItemList)
		{
			itemData = GameRoot.Instance.ItemDataContainer.GetData<CostumeSelectItemData>(item.ItemCode);
			if (itemData.Type == itemType)
			{
				return itemData;
			}
		}
		return null;
	}

	void MatchSelect.Listener.OnEnterPvp(MatchArenaType matchArenaType)
	{
		switch (matchArenaType)
		{
		case MatchArenaType.Normal:
			OpenMatchType(MatchPvpType.Normal);
			break;
		case MatchArenaType.BanPick:
			OpenMatchType(MatchPvpType.BanPick, delegate
			{
				SceneAutoBehaviour<ContentsNoticePopupManagement>.instance.OpenContentsResultPopup(ContentsNoticeType.BanPick);
			});
			break;
		case MatchArenaType.Ordeal:
			GlobalAutoBehaviour<OrdealArenaManager>.instance.RequestOrdealInfo();
			break;
		}
	}

	public void RequestCloseFromMatchMenu()
	{
		GetInstance<MatchMyInfo>().Close();
		if (GetInstance<GlobalMatchRanking>().IsOpen())
		{
			GetInstance<GlobalMatchRanking>(openedOnly: true).Close();
		}
	}

	public void ReOpenComponent()
	{
		GetInstance<MatchMyInfo>().ReOpen();
		if (GetInstance<GlobalMatchRanking>().IsOpen())
		{
			GetInstance<GlobalMatchRanking>(openedOnly: true).ReOpen();
		}
	}

	public void OpenMatchHistory(MatchPvpType pvpType)
	{
		NShared.GetReplayList.Request request = new NShared.GetReplayList.Request
		{
			Type = pvpType.ToReplayType(),
			Count = 10
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetReplayList.Response response)
		{
			if (response == null || response.BaseResult != BaseResultType.Success || response.Result != ResultType.Success)
			{
				MessagePopup.OpenError(response.Result);
			}
			else
			{
				MatchHistory matchHistory = GetInstance<MatchHistory>();
				matchHistory.Open(delegate
				{
					matchHistory.Init(response.Replays, pvpType);
				});
			}
		});
	}

	void MatchMyInfo.Listener.OnRewardInfo()
	{
		OpenMatchRewardInfo();
	}

	void MatchMyInfo.Listener.OnShop()
	{
	}

	void MatchMyInfo.Listener.OnHeroView()
	{
		GetInstance<MatchBanPickHeroList>().Open();
	}

	void MatchMyInfo.Listener.OnPartySelect()
	{
		CurrencyMaxCap.WarningOrExecute(ContentsDefine.CurrencyMaxCap.GetCurrencyTypeOnEndBattle(SelectedPvpType.ToMatchArenaType()), OnPartySelect_Real);
	}

	private void OnPartySelect_Real()
	{
		int[] heroIndices;
		if (SelectedPvpType == MatchPvpType.Normal)
		{
			EnterDungeonData enterDungeon = new EnterDungeonData();
			DefaultBattlePartySettingDataProdiver dataProvider = new DefaultBattlePartySettingDataProdiver(GetType().Name, StaminaType.Sword, 1, Battle.MATCH_START_BUTTON_LABEL, enterDungeon);
			GetInstance<BattlePartySetting>().Open(this, dataProvider);
		}
		else if (SceneAutoBehaviour<MultiPartyManagement>.instance.CheckEnterBattle() && CheckBanPickCondition(out heroIndices, OnFailCheckBanPickCondition))
		{
			NGame2.NContentsInfo.MatchArena.MatchSeasonInfo matchSeasonInfo = SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.GetMatchSeasonInfo(SelectedPvpType);
			if (matchSeasonInfo != null && matchSeasonInfo.ActiveTier && !UIHelper.IsEnoughStamina(StaminaType.Sword2, 1, 0))
			{
				string title = LocalizationManager.GetString(NShared.NLocalization.Common.WARNING);
				string stringDirect = LocalizationManager.GetStringDirect("CONTENTS_ENTER_IMPOSSIBLE_REGEN");
				MessagePopup.Open(title, stringDirect);
			}
			else
			{
				StartBanPickArena(heroIndices);
			}
		}
	}

	private static void OnFailCheckBanPickCondition()
	{
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("MatchingBanPickHeroLevelMin");
		int valueInt2 = GameRoot.Instance.ConstantDataContainer.GetValueInt("MatchingBanPickHeroStarMin");
		int valueInt3 = GameRoot.Instance.ConstantDataContainer.GetValueInt("MatchingBanPickHeroCountMin");
		string stringDirect = LocalizationManager.GetStringDirect("ERROR");
		string message = "";
		NGame2.NContentsInfo.MatchArena.MatchSeasonInfo matchSeasonInfo = SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.GetMatchSeasonInfo(MatchPvpType.BanPick);
		if (matchSeasonInfo.MatchSeasonType == MatchSeasonType.Free)
		{
			message = LocalizationManager.GetStringDirect("MATCH_PICKBAN_CONDITION_FAIL_PRE", valueInt3);
		}
		else if (matchSeasonInfo.MatchSeasonType == MatchSeasonType.Regular)
		{
			message = LocalizationManager.GetStringDirect("MATCH_PICKBAN_CONDITION_FAIL", valueInt, valueInt2, valueInt3);
		}
		MessagePopup.Open(stringDirect, message);
	}

	public static bool CheckBanPickCondition(out int[] heroIndices, Action onFail = null)
	{
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("MatchingBanPickHeroLevelMin");
		int valueInt2 = GameRoot.Instance.ConstantDataContainer.GetValueInt("MatchingBanPickHeroStarMin");
		int num = GameRoot.Instance.ConstantDataContainer.GetValueInt("MatchingBanPickHeroCountMin");
		heroIndices = GetBanPickHeroIndices(valueInt, valueInt2);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(heroIndices))
		{
			return false;
		}
		NGame2.NContentsInfo.MatchArena.MatchSeasonInfo matchSeasonInfo = SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.GetMatchSeasonInfo(MatchPvpType.BanPick);
		if (matchSeasonInfo != null && matchSeasonInfo.LimitedHeroCount != 0)
		{
			num = matchSeasonInfo.LimitedHeroCount;
		}
		if (__Extension_Method_ICollection__.COUNT(heroIndices) < num)
		{
			Vespa_System_Action_Extension_Method.fire(onFail);
			return false;
		}
		return true;
	}

	private static int[] GetBanPickHeroIndices(int cutLev, int cutStar)
	{
		List<CachedHeroInfo> heroInfoList = GlobalAutoBehaviour<AccountManager>.instance.GetHeroInfoList(sort: true);
		List<int> list = new List<int>();
		foreach (CachedHeroInfo item in heroInfoList)
		{
			if (item.Level >= cutLev && item.Star >= cutStar)
			{
				list.Add(item.HeroIndex);
			}
		}
		return SprkDispatchSelection.AvailableHeroes(list.ToArray());
	}

	void BattlePartySetting.Listener.StartBattle(EnterDungeonData enterDungeon, BattlePartySetting.BattlePartyInfo battlePartyInfo, BattlePartySetting.ExtraOptionData extraOption)
	{
		RankResultInfo rankResultInfo = SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.GetRankResultInfo(SelectedPvpType);
		MatchReturnState lobbyReturnState = new MatchReturnState(SelectedPvpType);
		int tierRank = rankResultInfo?.TotalRank ?? (-1);
		SingletonUpdater<MatchManager>.instance.RegisterMatch_Pvp(MatchPvpType.Normal, battlePartyInfo.heroIndices, battlePartyInfo.leaderHeroIndex, 0L, tierRank, lobbyReturnState);
	}

	bool BattlePartySetting.Listener.OnCloseRequest(BattlePartySetting caller)
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

	public void OnSelectConfirm(MatchBanPickState state, int[] heroIndices, bool isPenalty = false)
	{
		if (banPickPending)
		{
			Debug.LogError("banPickPending");
			return;
		}
		banPickPending = true;
		BattleBanPickReq battleBanPickReq = new BattleBanPickReq();
		battleBanPickReq.BanPickState = state;
		battleBanPickReq.HeroIndices = heroIndices;
		battleBanPickReq.IsPenalty = isPenalty;
		GlobalAutoBehaviour<SocketServiceManager>.instance.BattleConnector.Request(battleBanPickReq, delegate(BattleBanPickRes res)
		{
			banPickPending = false;
			if (res == null)
			{
				VespaDebug.LogError("BattleBanPickRes is null");
			}
			else if (res.Result == BattleBanPickRes.ResultType.NotCorrectHero)
			{
				VespaDebug.LogError("BattleBanPickRes :: NotCorrectHero -> CancelBanPick");
				CenterMessage.Open(LocalizationManager.GetStringDirect("MATCH_PICKBAN_ALREADY_SELECT_HERO"));
				GetInstance<MatchBanPick>().OnRecvChangeState(res.BanPickState, null, res.BanPickAccountInfo, heroIndices, res.RemainTimeSec);
			}
			else if (res.Result == BattleBanPickRes.ResultType.HeroNotFound)
			{
				VespaDebug.LogError("BattleBanPickRes :: HeroNotFound");
				CenterMessage.Open(LocalizationManager.GetStringDirect("MATCH_PICKBAN_INEXSTENT_HERO"));
				GetInstance<MatchBanPick>().OnRecvChangeState(res.BanPickState, null, res.BanPickAccountInfo, heroIndices, res.RemainTimeSec);
			}
			else if (res.Result == BattleBanPickRes.ResultType.NotCorrectState || res.Result == BattleBanPickRes.ResultType.NotCorrectTeamType || res.Result == BattleBanPickRes.ResultType.NotCorrectHero || res.Result == BattleBanPickRes.ResultType.NotCorrectHeroCount)
			{
				VespaDebug.LogError("BattleBanPickRes :: OnRecvChangeState(newState");
				GetInstance<MatchBanPick>().OnRecvChangeState(res.BanPickState, null, res.BanPickAccountInfo, heroIndices, res.RemainTimeSec);
			}
			else if (res.Result != BattleBanPickRes.ResultType.Success)
			{
				VespaDebug.LogError("BattleBanPickRes :: Result != Success");
			}
			else
			{
				GetInstance<MatchBanPick>().OnResponseConfirm(res.BanPickAccountInfo);
			}
		});
	}

	public void OnMatchBanPickCancel()
	{
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.WARNING);
		string stringDirect = LocalizationManager.GetStringDirect("MATCH_PICKBAN _MATCH_CANCEL_MSG");
		MessagePopup.Open(title, stringDirect, delegate
		{
			GetInstance<MatchWaitingRenewal>().Close();
			GetInstance<MatchBanPick>().Close();
		});
	}

	public void OnMatchBanPickClose()
	{
		if (GlobalAutoBehaviour<SocketServiceManager>.isValidInstance && GlobalAutoBehaviour<SocketServiceManager>.instance.BattleConnector.State == ConnectState.Disconnected)
		{
			string title = LocalizationManager.GetString(NShared.NLocalization.Common.WARNING);
			string stringDirect = LocalizationManager.GetStringDirect("MATCH_PICKBAN _MATCH_CANCEL_MSG");
			MessagePopup.Open(title, stringDirect, delegate
			{
				GetInstance<MatchBanPick>().Close();
			});
			return;
		}
		string title2 = LocalizationManager.GetString(NShared.NLocalization.Common.WARNING);
		string stringDirect2 = LocalizationManager.GetStringDirect("MATCH_PICKBAN _GIVEUP_MSG");
		MessagePopup.Open(title2, stringDirect2, delegate
		{
			BattleBanPickGiveUpReq data = new BattleBanPickGiveUpReq();
			GlobalAutoBehaviour<SocketServiceManager>.instance.BattleConnector.Request(data, delegate(BattleBanPickGiveUpRes res)
			{
				if (res != null && res.Result == BattleBanPickGiveUpRes.ResultType.Success)
				{
					GetInstance<MatchBanPick>().GiveUpPending = true;
				}
			});
		}, null);
	}

	void PvpResult.Listener.OnArena(PvpResult caller)
	{
		GetInstance<MatchWaitingRenewal>().Close();
		GetInstance<MatchBanPick>().Close();
		GetInstance<PvpResult>().Close();
		if (GlobalAutoBehaviour<LoadingManager>.instance.CurrentSceneType == LoadingManager.ESceneType.Battle2)
		{
			return;
		}
		int chapterIndex = NGame2.NAccount.UserCampaignManager.ChapterIndex;
		int dungeonIndex = NGame2.NAccount.UserCampaignManager.DungeonIndex;
		CampaignNodeData data = GameRoot.Instance.CampaignNodeDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data != null)
		{
			EntryMenuSettingData dataByMenuType = GameRoot.Instance.EntryMenuSettingDataContainer.GetDataByMenuType(data.EntryNPCCode);
			if (dataByMenuType != null)
			{
				SingletonAuto<BgmManager2>.instance.Request(dataByMenuType.EventBgmCode, isEvent: true);
			}
		}
	}

	void PvpResult.Listener.OnStatistics(PvpResult caller)
	{
	}

	void PvpResult.Listener.OnRetry(PvpResult caller)
	{
		MatchResultInfo matchResultInfo = caller.MatchResultInfo;
		if (matchResultInfo != null && matchResultInfo.ArenaType == MatchArenaType.BanPick)
		{
			LobbyScene2.ListReturnState.Clear();
			GetInstance<MatchBanPick>().Close();
			GetInstance<PvpResult>().Close();
			GetInstance<MatchWaitingRenewal>().DepthOrder = EDepthOrder.Topmost;
			SingletonUpdater<MatchManager>.instance.RetryMatch_Pvp(matchResultInfo);
		}
	}

	private void StartBanPickArena(int[] heroIndices)
	{
		string title = LocalizationManager.GetString(NShared.NLocalization.Common.WARNING);
		string stringDirect = LocalizationManager.GetStringDirect("MATCH_PICKBAN_WARNING_MSG");
		MessagePopup.Open(title, stringDirect, delegate
		{
			MatchReturnState lobbyReturnState = new MatchReturnState(MatchPvpType.BanPick);
			SingletonUpdater<MatchManager>.instance.RegisterMatch_Pvp(MatchPvpType.BanPick, heroIndices, 1, 0L, 0, lobbyReturnState);
		}, null);
	}
}
