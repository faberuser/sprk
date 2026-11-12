using NGame2.NBattleField;
using NGame2.NContentsDefine;
using NGame2.NContentsInfo;
using NGame2.NEventTrigger;
using NGame2.NMatch;
using NGame2.NScene;
using NGame2.NScene.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NGame2.NUI.NWindow.NChatting;
using NShared;
using NShared.NEvent;
using NShared.SetGuildArenaMatchResult;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager.NBattle;

public class ArenaManagement : BaseManagement<ArenaManagement>, PvpResult.Listener, PvpStatus.Listener, GuildArenaMatchResult.Listener
{
	private MatchResultInfo _matchResultInfo;

	private ReservedActionQueue _reservedActionQueue = new ReservedActionQueue();

	private bool _pvpResultEffectEnded;

	protected override void Awake()
	{
		base.Awake();
		AddInstance<PvpResult>("Resources/Prefab2/Window/Battle/PvpResult");
		AddInstance<BattleStats>("Resources/Prefab2/Window/Battle/BattleStats");
		AddInstance<PvpStatus>("Resources/Prefab2/Window/Battle/PvpStatus");
		AddInstance<PvpResultScore>("Resources/Prefab2/Window/Battle/PvpResultScore");
		AddInstance<DuelWaitingPopup>("Resources/Prefab2/Window/Match/DuelWaitingPopup");
		AddInstance<GuildArenaMatchResult>("Resources/Prefab2/Window/Battle/GuildArenaMatchResult");
	}

	protected override void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			base.OnDestroy();
		}
	}

	public void Open()
	{
	}

	public void OpenStatus(PvpStatus.MatchUserInfo userInfo1, PvpStatus.MatchUserInfo userInfo2, MatchArenaType arenaType)
	{
		NGame2.NContentsInfo.MatchArena.MatchSeasonInfo matchSeasonInfo = SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.GetMatchSeasonInfo(arenaType);
		OpenStatus(userInfo1, userInfo2, arenaType, matchSeasonInfo);
	}

	public void OpenStatus(PvpStatus.MatchUserInfo userInfo1, PvpStatus.MatchUserInfo userInfo2, MatchArenaType arenaType, NGame2.NContentsInfo.MatchArena.MatchSeasonInfo seasonInfo)
	{
		base.TaskFactory.Run(delegate(PvpStatus window)
		{
			window.Open(userInfo1, userInfo2, this, arenaType, seasonInfo);
		}).Commit();
	}

	public void OpenPvpResult_Banpick(MatchResultInfo matchResultInfo, PlayerBattleBanPickInfo banPickInfo)
	{
		if (matchResultInfo != null && banPickInfo != null)
		{
			_matchResultInfo = matchResultInfo;
			CloseOtherWindows();
			GetInstance<PvpResult>().Open_BanPickOnly(matchResultInfo, banPickInfo, this);
			ShowTierChangePopup(matchResultInfo);
		}
	}

	public void OpenPvpResult_Normal(MatchResultInfo matchResultInfo)
	{
		if (matchResultInfo != null)
		{
			_matchResultInfo = matchResultInfo;
			CloseOtherWindows();
			SingletonAuto<EventTriggerManager>.instance.RaiseEvent(new BattleEndRes
			{
				MatchResult = matchResultInfo
			});
			GetInstance<PvpResult>().Open_Pvp(matchResultInfo, this);
			ShowTierChangePopup(matchResultInfo);
		}
	}

	public void OpenGuildArenaMatchResult(NShared.SetGuildArenaMatchResult.Response response)
	{
		if (response != null)
		{
			CloseOtherWindows();
			GetInstance<GuildArenaMatchResult>().Open(this, response.Win, response.MySessionScore, response.AddSessionScore, response.RewardItems);
		}
	}

	private void ShowTierChangePopup(MatchResultInfo matchResultInfo)
	{
		int prevTierIndex = matchResultInfo.PrevTierIndex;
		int newTierIndex = matchResultInfo.NewTierIndex;
		if (newTierIndex > prevTierIndex)
		{
			VisualTierRank newVisualTierRank = VisualTierRank.CreateFromServer(matchResultInfo.NewTierRank);
			_ = matchResultInfo.HeroExpResults;
			TierChangePopup.Open(prevTierIndex, newTierIndex, newVisualTierRank, matchResultInfo.ArenaType);
		}
	}

	protected override void ChainUpdate()
	{
		base.ChainUpdate();
		UpdateReservedPopup();
	}

	private void UpdateReservedPopup()
	{
		if (_pvpResultEffectEnded)
		{
			_reservedActionQueue.Update();
		}
	}

	private void ReturnToLobby(LoadingManager.ELoadingType loadingType)
	{
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(new OnClickReturnToMap());
		GlobalAutoBehaviour<LoadingManager>.instance.MoveScene(LoadingManager.ESceneType.Lobby, loadingType);
	}

	private void CloseOtherWindows()
	{
		CloseGiveupMenu();
		CloseChattingEmoticonSelectPopup();
	}

	private void CloseGiveupMenu()
	{
		if (SceneAutoBehaviour<BattleManagement>.isValidInstance)
		{
			SceneAutoBehaviour<BattleManagement>.instance.ClosePvpGiveUpMenu();
		}
	}

	private void CloseChattingEmoticonSelectPopup()
	{
		if (ChattingEmoticonSelectPopup.IsOpenedWindow())
		{
			ChattingEmoticonSelectPopup.CloseWindow();
		}
	}

	void PvpResult.Listener.OnArena(PvpResult caller)
	{
		ReturnToLobby(LoadingManager.ELoadingType.Default);
	}

	void PvpResult.Listener.OnStatistics(PvpResult caller)
	{
		base.TaskFactory.Run(delegate(BattleStats window)
		{
			BattleInstance stoppedBattleInstance = SingletonAuto<BattleFacade>.instance.GetStoppedBattleInstance();
			window.Open(stoppedBattleInstance);
		}).Commit();
	}

	void PvpResult.Listener.OnRetry(PvpResult caller)
	{
		MatchResultInfo matchResultInfo = caller.MatchResultInfo;
		if (matchResultInfo.ArenaType == MatchArenaType.Normal)
		{
			SingletonUpdater<MatchManager>.instance.RetryMatch_Pvp(matchResultInfo);
			return;
		}
		MatchReturnState item = new MatchReturnState(matchResultInfo.GetPvpType(), retry: true, matchResultInfo);
		LobbyScene2.ListReturnState.Clear();
		LobbyScene2.ListReturnState.Add(item);
		ReturnToLobby(LoadingManager.ELoadingType.Default);
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

	void GuildArenaMatchResult.Listener.OnArena(GuildArenaMatchResult caller)
	{
		ReturnToLobby(LoadingManager.ELoadingType.Default);
	}

	void GuildArenaMatchResult.Listener.OnStatistics(GuildArenaMatchResult caller)
	{
		base.TaskFactory.Run(delegate(BattleStats window)
		{
			BattleInstance stoppedBattleInstance = SingletonAuto<BattleFacade>.instance.GetStoppedBattleInstance();
			window.Open(stoppedBattleInstance);
		}).Commit();
	}

	void GuildArenaMatchResult.Listener.OnEffectEnd(GuildArenaMatchResult caller)
	{
	}
}
