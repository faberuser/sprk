using System.Collections.Generic;
using System.Linq;
using NGame2.NAccount;
using NGame2.NBattleField;
using NGame2.NBattleField.NManager.NBattleCondition;
using NGame2.NBattleField.NManager.NPlay;
using NGame2.NContentsInfo;
using NGame2.NGuild;
using NGame2.NMatch;
using NGame2.NScene;
using NGame2.NScene.NPreloader;
using NGame2.NSocket;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NBattle;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NShared.BeginCampaign;
using NShared.CancelMatch;
using NShared.GetMatchResult;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NSocket;
using NVespa.NUtil;

namespace NGame2.NBattleContext;

public class ArenaContext : SingletonAuto<ArenaContext>, OnlinePlay.IDataProvider, IDataProvider
{
	public const float EnterBanPickWindowDelay = 5f;

	private OnlineBattleCallback _onlineBattleCallback;

	private PvpContextImpl _impl;

	private string _matchValidityError;

	private bool _isReceiveBattleResultNot;

	public MatchInfo MatchInfo { get; private set; }

	public MatchArena.MatchSeasonInfo SeasonInfo { get; private set; }

	public void Init(MatchInfo matchInfo, MatchArena.MatchSeasonInfo seasonInfo)
	{
		MatchInfo = matchInfo;
		SeasonInfo = seasonInfo;
		_impl = new PvpContextImpl();
		_matchValidityError = null;
		_isReceiveBattleResultNot = false;
		if (_onlineBattleCallback != null)
		{
			_onlineBattleCallback.Close();
			_onlineBattleCallback = null;
		}
		_onlineBattleCallback = new OnlineBattleCallback(GlobalAutoBehaviour<SocketServiceManager>.instance.BattleConnector);
		_onlineBattleCallback.OnCreatureDestroyedCallback = OnCreatureDestroyed;
		_onlineBattleCallback.OnBattleResultCallback = OnBattleResult;
		_onlineBattleCallback.OnBattleEndCallback = OnBattleEnd;
		_onlineBattleCallback.OnBattleBanPickStartCallback = OnBattleBanPickStartCallback;
		_onlineBattleCallback.OnBattleBanPickStartCancelCallback = OnBattleBanPickStartCancelCallback;
		_onlineBattleCallback.OnBattleBanPickNextCallback = OnBattleBanPickNextCallback;
		_onlineBattleCallback.OnBattleBanPickEndCallback = OnBattleBanPickEndCallback;
		_onlineBattleCallback.OnBattleBanPickTimeOutCallback = OnBattleBanPickTimeOutCallback;
		_onlineBattleCallback.OnBattleBanPickGiveUpCallback = OnBattleBanPickGiveUpCallback;
		_onlineBattleCallback.OnBattleMessageCallback = OnBattleMessageCallback;
		_onlineBattleCallback.Init();
	}

	private void OnEnterBattle()
	{
		SceneAutoBehaviour<ArenaManagement>.instance.Open();
		if (MatchInfo.AccountInfos != null)
		{
			MatchedAccountInfo myAccountInfo = MatchInfo.AccountInfos.FirstOrDefault((MatchedAccountInfo e) => e.AccountId == GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId);
			PvpStatus.MatchUserInfo userInfo = new PvpStatus.MatchUserInfo(myAccountInfo);
			MatchedAccountInfo matchedAccountInfo = MatchInfo.AccountInfos.FirstOrDefault((MatchedAccountInfo e) => e != myAccountInfo);
			PvpStatus.MatchUserInfo userInfo2 = ((matchedAccountInfo != null) ? new PvpStatus.MatchUserInfo(matchedAccountInfo) : null);
			SceneAutoBehaviour<ArenaManagement>.instance.OpenStatus(userInfo, userInfo2, MatchInfo.ArenaType, SeasonInfo);
		}
	}

	private void OnCreatureDestroyed(DestroyCreatureNot data)
	{
	}

	private void OnBattleResult(BattleResultNot data)
	{
		_isReceiveBattleResultNot = true;
		SingletonUpdater<Executer>.instance.DelayRun(0.05f, this, delegate
		{
			WaitBattleEnd(data, 25);
		});
	}

	private void OnBattleEnd(BattleEndNot data)
	{
	}

	private void OnBattleBanPickStartCallback(BattleBanPickStartNot data)
	{
		if (MatchInfo.AccountInfos.FirstOrDefault((MatchedAccountInfo e) => e.AccountId != GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id) == null)
		{
			VespaDebug.LogError("opponentInfo is null");
			return;
		}
		GlobalAutoBehaviour<SocketServiceManager>.instance.BattleConnector.OnDisconnected = null;
		MatchManagement matchManagement = SceneAutoBehaviour<MatchManagement>.instance;
		matchManagement.GetInstance<MatchWaitingRenewal>().SetOpponent(MatchInfo.AccountInfos);
		SingletonUpdater<Executer>.instance.DelayRun(SprkHonorDuel.BanPickRevealDelay(), this, delegate
		{
			matchManagement.CloseInstance<MatchWaitingRenewal>();
			SceneAutoBehaviour<MatchManagement>.instance.OpenMatchBanPick(MatchInfo.AccountInfos);
			BattleBanPickStartReq data2 = new BattleBanPickStartReq();
			GlobalAutoBehaviour<SocketServiceManager>.instance.BattleConnector.Request(data2, delegate(BattleBanPickStartRes res)
			{
				if (res == null)
				{
					VespaDebug.LogError("BattleBanPickStartRes is null");
				}
				else if (res.Result != BattleBanPickStartRes.ResultType.Success)
				{
					CenterMessage.Open("BattleBanPickStartRes.ResultType : " + res.Result);
				}
				else
				{
					GlobalAutoBehaviour<SocketServiceManager>.instance.BattleConnector.OnDisconnected = OnBattleServerDisconnectedMatchBanPick;
				}
			});
		});
	}

	private void OnBattleServerDisconnectedMatchBanPick()
	{
		if (SceneAutoBehaviour<MatchManagement>.instance.IsOpened<MatchBanPick>())
		{
			SingletonUpdater<Executer>.instance.DelayRun(1f, this, CheckBanPickDisconectedError);
		}
	}

	private void CheckBanPickDisconectedError()
	{
		if (!_isReceiveBattleResultNot)
		{
			ShowBanPickDisconnectedErrorPopup();
		}
	}

	private void ShowBanPickDisconnectedErrorPopup()
	{
		GlobalMessagePopup.OpenGlobalError(LocalizationManager.GetString(NShared.BeginCampaign.ResultType.BattleServerNotFound), delegate
		{
			SceneAutoBehaviour<MatchManagement>.instance.CloseMatchBanPick();
		});
	}

	private void OnBattleBanPickStartCancelCallback(BattleBanPickStartCancelNot data)
	{
		NShared.CancelMatch.Request request = new NShared.CancelMatch.Request();
		request.ArenaType = MatchArenaType.BanPick;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.CancelMatch.Response response)
		{
			if (response == null)
			{
				MessagePopup.OpenError("", ReturnToLobby);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult, ReturnToLobby);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.SwordResult);
				SceneAutoBehaviour<MatchManagement>.instance.OnMatchBanPickCancel();
			}
		});
	}

	private void OnBattleBanPickNextCallback(BattleBanPickNextNot data)
	{
		SceneAutoBehaviour<MatchManagement>.instance.OnMatchBanPickChangeState(data.BanPickState, data.BanPickAccountInfo, data.HeroIndices, data.RemainTimeSec);
	}

	private void OnBattleBanPickEndCallback(BattleBanPickEndNot data)
	{
		SceneAutoBehaviour<MatchManagement>.instance.OnMatchBanPickEndState(data.AccountInfos, data.BanPickAccountInfo);
	}

	private void OnBattleBanPickTimeOutCallback(BattleBanPickTimeOutNot data)
	{
		SceneAutoBehaviour<MatchManagement>.instance.OnMatchBanPickTimeOutState(data.AccountId);
	}

	private void OnBattleBanPickGiveUpCallback(BattleBanPickGiveUpNot data)
	{
		SceneAutoBehaviour<MatchManagement>.instance.OnMatchBanPickGiveUp(data.AccountId);
	}

	private void OnBattleMessageCallback(BattleMessageNot data)
	{
		SceneAutoBehaviour<BattleManagement>.instance.ShowEmoticonByBattleMessage(data);
	}

	private void WaitBattleEnd(BattleResultNot data, int remainWaitCount)
	{
		if (data.Dropped)
		{
			remainWaitCount = 0;
		}
		BattleResultNot data2 = data;
		int remainWaitCount2 = remainWaitCount;
		BattleFacade.BattleCacheData battleCache = SingletonAuto<BattleFacade>.instance.GetBattleCache();
		if (remainWaitCount2 <= 0 || (SingletonAuto<BattleFacade>.instance.IsValidBattle() && battleCache.IsBattleEnded))
		{
			if (SingletonAuto<BattleFacade>.instance.IsValidBattle() && !battleCache.IsBattleEnded)
			{
				SingletonAuto<BattleFacade>.instance.ForceStopBattle();
			}
			GlobalAutoBehaviour<SocketServiceManager>.instance.BattleConnector.Close();
			SingletonUpdater<Executer>.instance.DelayRun(0.05f, this, delegate
			{
				RequestGetMatchResult(allowRetry: true);
			});
		}
		else
		{
			SingletonUpdater<Executer>.instance.DelayRun(0.1f, this, delegate
			{
				WaitBattleEnd(data2, remainWaitCount2 - 1);
			});
		}
	}

	private void RequestGetMatchResult(bool allowRetry)
	{
		NShared.GetMatchResult.Request request = new NShared.GetMatchResult.Request();
		request.MatchUid = MatchInfo.Uid;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetMatchResult.Response response)
		{
			if (PvpLoading.isValidInstance)
			{
				PvpLoading.ReleaseInstance();
			}
			if (response == null)
			{
				MessagePopup.OpenError("", ReturnToLobby);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				MessagePopup.OpenError(response.BaseResult, ReturnToLobby);
			}
			else if (allowRetry && (response.Result == NShared.GetMatchResult.ResultType.MatchResultNotFound || response.Result == NShared.GetMatchResult.ResultType.MatchUidMismatch))
			{
				SingletonUpdater<Executer>.instance.DelayRun(1f, this, delegate
				{
					RequestGetMatchResult(allowRetry: false);
				});
			}
			else if (response.Result != NShared.GetMatchResult.ResultType.Success)
			{
				MessagePopup.OpenError(response.Result, ReturnToLobby);
			}
			else if (response.MatchResult != null)
			{
				GlobalAutoBehaviour<AccountManager>.instance.ApplyMatchResult(response.MatchResult);
				SingletonAuto<MatchArena>.instance.ApplyBattleInfo(response.BattleInfo, response.BattleBanPickInfo);
				if (response.MatchResult.Canceled)
				{
					MessagePopup.OpenError(LocalizationManager.GetString(Match.MATCH_BATTLE_CANCELED), ReturnToLobby);
				}
				else if (SingletonUpdater<MatchManager>.instance.ArenaType == MatchArenaType.BanPick)
				{
					if (GlobalAutoBehaviour<LoadingManager>.instance.CurrentSceneType == LoadingManager.ESceneType.Lobby)
					{
						SceneAutoBehaviour<MatchManagement>.instance.RefreshMatchInfo_BanPickOnly();
						SceneAutoBehaviour<MatchManagement>.instance.OpenResult(response.MatchResult, response.BattleBanPickInfo);
					}
					else if (GlobalAutoBehaviour<LoadingManager>.instance.CurrentSceneType == LoadingManager.ESceneType.Battle2)
					{
						SceneAutoBehaviour<ArenaManagement>.instance.OpenPvpResult_Banpick(response.MatchResult, response.BattleBanPickInfo);
					}
				}
				else
				{
					SceneAutoBehaviour<ArenaManagement>.instance.OpenPvpResult_Normal(response.MatchResult);
				}
			}
		});
	}

	private void ReturnToLobby()
	{
		SingletonAuto<BattleContext>.instance.ReturnToLobby();
	}

	void IDataProvider.Preload(BattleScenePreloader preloader)
	{
		if (MatchInfo == null)
		{
			return;
		}
		preloader.EnterDungeon = Vespa_Common_Class_Extension_Method.Interface((IDataProvider)this).GetEnterDungeon();
		preloader.PreloadDungeonMonster = false;
		MatchedAccountInfo[] accountInfos = MatchInfo.AccountInfos;
		foreach (MatchedAccountInfo obj in accountInfos)
		{
			bool loadCreatureImage = obj.AccountId == GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId;
			foreach (CachedHeroInfo value in obj.HeroInfos.Values)
			{
				preloader.AddCreature(value, null, loadCreatureImage);
			}
		}
	}

	List<CachedHeroInfo> IDataProvider.GetPreloadBattleLogs()
	{
		return new List<CachedHeroInfo>();
	}

	bool IDataProvider.CanNextDungeon()
	{
		return false;
	}

	EnterDungeonData IDataProvider.GetEnterDungeon()
	{
		return new EnterDungeonData(MatchInfo.ChapterIndex, MatchInfo.DungeonIndex, MatchInfo.DungeonIndex, NShared.DungeonDifficultyType.Easy);
	}

	void IDataProvider.OnEnterBattle()
	{
		OnEnterBattle();
	}

	void IDataProvider.OnWaveMoveEnd()
	{
		_impl.StartCountdown();
	}

	bool IDataProvider.CanStartBattle()
	{
		return _impl.CheckCountdownEnd();
	}

	BattleStartEffectData IDataProvider.GetBattleStartEffectData()
	{
		return _impl.GetBattleStartEffectData();
	}

	IPlay IDataProvider.GetBattlePlay()
	{
		return SceneAutoBehaviour<OnlinePlay>.instance;
	}

	BattleType IDataProvider.GetBattleType()
	{
		return BattleType.Arena;
	}

	BattleDefineData IDataProvider.GetBattleDefine()
	{
		return GameRoot.Instance.CampaignDungeonDataContainer.GetData(MatchInfo.ChapterIndex, MatchInfo.DungeonIndex)?.GetBattleDefine(isScenario: false);
	}

	BattleFacade.BattleConditionBase IDataProvider.CreateBattleCondition()
	{
		return new ArenaCondition();
	}

	bool IDataProvider.IsOnline()
	{
		return true;
	}

	void IDataProvider.OnUpdate()
	{
	}

	bool IDataProvider.IsTowerBattle()
	{
		return false;
	}

	int IDataProvider.GetTowerFloor()
	{
		return 0;
	}

	string IDataProvider.GetPreloadFieldName()
	{
		return null;
	}

	bool IDataProvider.GetPrealodCreateMonster()
	{
		return true;
	}

	GuildFlagData IDataProvider.GetGuildFlagData()
	{
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(MatchInfo.ChapterIndex, MatchInfo.DungeonIndex);
		if (data == null)
		{
			return null;
		}
		if (MatchInfo == null)
		{
			return null;
		}
		MatchedAccountInfo matchedAccountInfo = null;
		MatchedAccountInfo matchedAccountInfo2 = null;
		MatchedAccountInfo[] accountInfos = MatchInfo.AccountInfos;
		foreach (MatchedAccountInfo matchedAccountInfo3 in accountInfos)
		{
			if (matchedAccountInfo3.TeamType == TeamType.Left)
			{
				matchedAccountInfo = matchedAccountInfo3;
			}
			if (matchedAccountInfo3.TeamType == TeamType.Right)
			{
				matchedAccountInfo2 = matchedAccountInfo3;
			}
		}
		_ = GlobalAutoBehaviour<GuildManager>.instance.MyGuildManager.Info;
		GuildFlagData guildFlagData = new GuildFlagData();
		guildFlagData.EGuildFlagDisplayType = data.EGuildFlagDisplayType;
		if (matchedAccountInfo != null && matchedAccountInfo.GuildInfo != null && matchedAccountInfo.GuildInfo.Length != 0)
		{
			guildFlagData.LeftGuildFlag = (byte)((matchedAccountInfo.GuildInfo == null) ? 1 : matchedAccountInfo.GuildInfo[0].Flag);
			guildFlagData.LeftGuildLogo = (byte)((matchedAccountInfo.GuildInfo == null) ? 1 : matchedAccountInfo.GuildInfo[0].Logo);
			guildFlagData.LeftGuildFlagBG = (byte)((matchedAccountInfo.GuildInfo == null) ? 1 : matchedAccountInfo.GuildInfo[0].Back);
			guildFlagData.ExistLeftGuild = matchedAccountInfo.GuildInfo != null;
		}
		if (matchedAccountInfo2 != null && matchedAccountInfo2.GuildInfo != null && matchedAccountInfo2.GuildInfo.Length != 0)
		{
			guildFlagData.RightGuildFlag = (byte)((matchedAccountInfo2.GuildInfo == null) ? 1 : matchedAccountInfo2.GuildInfo[0].Flag);
			guildFlagData.RightGuildLogo = (byte)((matchedAccountInfo2.GuildInfo == null) ? 1 : matchedAccountInfo2.GuildInfo[0].Logo);
			guildFlagData.RightGuildFlagBG = (byte)((matchedAccountInfo2.GuildInfo == null) ? 1 : matchedAccountInfo2.GuildInfo[0].Back);
			guildFlagData.ExistRightGuild = matchedAccountInfo2.GuildInfo != null;
		}
		return guildFlagData;
	}

	int OnlinePlay.IDataProvider.GetChapterIndex()
	{
		return MatchInfo.ChapterIndex;
	}

	int OnlinePlay.IDataProvider.GetDungeonIndex()
	{
		return MatchInfo.DungeonIndex;
	}

	NShared.DungeonDifficultyType OnlinePlay.IDataProvider.GetDifficulty()
	{
		return NShared.DungeonDifficultyType.Easy;
	}

	EBattleMenu IDataProvider.GetBattleMenuType()
	{
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(MatchInfo.ChapterIndex, MatchInfo.DungeonIndex);
		if (data == null)
		{
			return EBattleMenu.TimeStatus | EBattleMenu.SystemButton | EBattleMenu.WaveStatus | EBattleMenu.LootingStatus | EBattleMenu.Retry | EBattleMenu.DamageMeter | EBattleMenu.HeroParty | EBattleMenu.BoosterStatus | EBattleMenu.QuestStatus | EBattleMenu.SystemButtonAuto;
		}
		return data.GetBattleDefine(isScenario: false)?.BattleMenu ?? (EBattleMenu.TimeStatus | EBattleMenu.SystemButton | EBattleMenu.WaveStatus | EBattleMenu.LootingStatus | EBattleMenu.Retry | EBattleMenu.DamageMeter | EBattleMenu.HeroParty | EBattleMenu.BoosterStatus | EBattleMenu.QuestStatus | EBattleMenu.SystemButtonAuto);
	}

	void OnlinePlay.IDataProvider.OnReturnToLobby()
	{
	}

	OnlineBattleCallback OnlinePlay.IDataProvider.GetBattleCallback()
	{
		return _onlineBattleCallback;
	}

	int OnlinePlay.IDataProvider.GetWorldMapEventIndex()
	{
		return 0;
	}

	IRequester OnlinePlay.IDataProvider.GetSocketConnector()
	{
		return GlobalAutoBehaviour<SocketServiceManager>.instance.BattleConnector;
	}

	string OnlinePlay.IDataProvider.GetError()
	{
		return _matchValidityError;
	}

	int OnlinePlay.IDataProvider.OnlineGameSpeedRatio()
	{
		return 1;
	}

	List<List<BattleCreatureCreateInfo>> OnlinePlay.IDataProvider.GetBattleCreatureCreateInfoListToWaveList()
	{
		return null;
	}

	BattleInstanceExtraData OnlinePlay.IDataProvider.GetExtraData()
	{
		return null;
	}
}
