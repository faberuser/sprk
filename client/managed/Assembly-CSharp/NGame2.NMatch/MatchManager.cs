using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MiniJSON;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NBattleField;
using NGame2.NConfig;
using NGame2.NContentsDefine;
using NGame2.NContentsInfo;
using NGame2.NEventTrigger;
using NGame2.NScene;
using NGame2.NScene.NLobby;
using NGame2.NSocket;
using NGame2.NUI;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NBattle;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NGameAsset;
using NShared;
using NShared.BeginCampaign;
using NShared.CancelMatch;
using NShared.NEvent;
using NShared.NLocalization;
using NShared.RegisterMatch;
using NShared.SetOfflineMatchResult;
using NShared.WaitMatch;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NUtil;
using NVespa.NWeb;
using UnityEngine;

namespace NGame2.NMatch;

public class MatchManager : SingletonUpdater<MatchManager>
{
	public class OfflineMatchResultInfo
	{
		public int MatchScore;

		public int TierIndex;

		public int TierRank;

		public int TotalRank;
	}

	private class RequestWrapper_Pvp
	{
		public MatchPvpType PvpType { get; private set; }

		public NShared.RegisterMatch.Request Value { get; private set; }

		public RequestWrapper_Pvp(MatchPvpType pvpType, int[] heroIndices, int leaderHeroIndex, long groupId, int tierRank)
		{
			PvpType = pvpType;
			Value = new NShared.RegisterMatch.Request();
			Value.HeroIndices = heroIndices;
			Value.ArenaType = pvpType.ToMatchArenaType();
			Value.DuelType = ((groupId != 0L) ? MatchDuelType.Friendly : MatchDuelType.Arena);
			Value.LeaderHeroIndex = leaderHeroIndex;
			Value.SingleTest = SingletonAuto<ClientDebugNetwork>.instance.SingleMatchTest;
			Value.Rank = tierRank;
			Value.GroupId = groupId;
		}
	}

	private class WaitMatchResponseWrapper
	{
		public MatchPvpType PvpType { get; private set; }

		public bool IsValid => Value != null;

		public NShared.WaitMatch.Response Value { get; private set; }

		public WaitMatchResponseWrapper(NShared.WaitMatch.Response response)
		{
			Value = response;
			if (response != null && response.MatchInfo != null && response.MatchInfo.ArenaType == MatchArenaType.BanPick)
			{
				PvpType = MatchPvpType.BanPick;
			}
			else
			{
				PvpType = MatchPvpType.Normal;
			}
		}
	}

	public static bool ShowDebugLabel;

	private float _registerMatchTime;

	private float _lastRequestWaitMatchTime;

	private bool _offlineArenaRequested;

	private bool _offlinePlaying;

	private bool _isSearchPaused;

	private float _pausedTime;

	private LobbyScene2.IReturnState _lobbyReturnState;

	private RequestWrapper_Pvp _lastRequest_Pvp;

	private RequestWrapper_Pvp _originalRequest_Pvp;

	private OfflineMatchResultInfo _offlineToOnlineInfo_PvpNormal;

	private MatchedAccountInfo _myOfflineMatchInfo_PvpNormal;

	private bool _waitCancelReserved;

	private bool _ReserveTimeout;

	public MatchArenaType ArenaType { get; private set; }

	public MatchDuelType DuelType { get; private set; }

	public MatchInfo MatchInfo { get; private set; }

	public string RelayBattleServerAddress { get; private set; }

	public int RelayBattleServerPort { get; private set; }

	public bool IsWaitingMatchPvp { get; private set; }

	public bool IsMatchMade { get; private set; }

	public bool OfflinePlaying
	{
		get
		{
			return _offlinePlaying;
		}
		set
		{
			_offlinePlaying = value;
		}
	}

	private void ResetReserve()
	{
		_waitCancelReserved = false;
		_ReserveTimeout = false;
	}

	public void CancelReserve(bool timeOut = false)
	{
		_waitCancelReserved = true;
		_ReserveTimeout = timeOut;
	}

	public override void ChainUpdate()
	{
		UpdateReservedOfflineToOnline_PvpNormal();
		UpdateRequestWaitMatch_Pvp();
	}

	private void Reset(bool resetLastRequest)
	{
		IsWaitingMatchPvp = false;
		IsMatchMade = false;
		_registerMatchTime = 0f;
		_lastRequestWaitMatchTime = 0f;
		_offlineArenaRequested = false;
		_offlinePlaying = false;
		_lobbyReturnState = null;
		_offlineToOnlineInfo_PvpNormal = null;
		_isSearchPaused = false;
		_pausedTime = 0f;
		ResetReserve();
		if (resetLastRequest)
		{
			_lastRequest_Pvp = null;
		}
	}

	public void RegisterMatch_Pvp(MatchPvpType pvpType, int[] heroIndices, int leaderHeroIndex, long groupId, int tierRank, LobbyScene2.IReturnState lobbyReturnState)
	{
		RequestMatchInternal_Pvp(_originalRequest_Pvp = new RequestWrapper_Pvp(pvpType, heroIndices, leaderHeroIndex, groupId, tierRank), lobbyReturnState);
	}

	public void RegisterRematch_Pvp(MatchPvpType pvpType, long groupId, MatchedAccountInfo myInfo, MatchedAccountInfo opponentInfo, bool isNpc = false)
	{
		RequestWrapper_Pvp requestWrapper_Pvp = new RequestWrapper_Pvp(pvpType, myInfo.GetHeroIndices(), myInfo.LeaderHeroIndex, groupId, myInfo.TierIndex);
		requestWrapper_Pvp.Value.DuelType = MatchDuelType.Arena;
		requestWrapper_Pvp.Value.IsRematch = true;
		requestWrapper_Pvp.Value.OpponentIsNpc = isNpc;
		requestWrapper_Pvp.Value.OpponentServerGroup = opponentInfo.ServerGroup;
		requestWrapper_Pvp.Value.OpponentAccountId = opponentInfo.AccountId;
		requestWrapper_Pvp.Value.OpponentTierIndex = opponentInfo.TierIndex;
		requestWrapper_Pvp.Value.OpponentHeroIndices = opponentInfo.GetHeroIndices();
		requestWrapper_Pvp.Value.OpponentLeaderHeroIndex = opponentInfo.LeaderHeroIndex;
		RequestMatchInternal_Pvp(requestWrapper_Pvp, _lobbyReturnState);
	}

	private void RequestMatchInternal_Pvp(RequestWrapper_Pvp request, LobbyScene2.IReturnState lobbyReturnState)
	{
		if (request != null && !SprkDispatchSelection.CanRegister(request.Value))
		{
			return;
		}
		RequestWrapper_Pvp request2 = request;
		if (request2 == null)
		{
			return;
		}
		Reset(resetLastRequest: true);
		ArenaType = request2.Value.ArenaType;
		DuelType = request2.Value.DuelType;
		IsWaitingMatchPvp = false;
		ResetReserve();
		_lobbyReturnState = lobbyReturnState;
		_myOfflineMatchInfo_PvpNormal = null;
		LobbyPopupDisabler disabler = new LobbyPopupDisabler();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request2.Value, delegate(NShared.RegisterMatch.Response response)
		{
			disabler.Dispose();
			if (response != null)
			{
				if (response.BaseResult != BaseResultType.Success)
				{
					OnFailRegistered_Pvp(NShared.RegisterMatch.ResultType.Fail);
				}
				else if (response.Result != NShared.RegisterMatch.ResultType.Success)
				{
					OnFailRegistered_Pvp(response.Result);
				}
				else
				{
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.SwordResult);
					GlobalAutoBehaviour<AccountManager>.instance.DeckManager.Apply(response.DeckInfo);
					_lastRequest_Pvp = request2;
					IsWaitingMatchPvp = true;
					_registerMatchTime = Time.time;
					_lastRequestWaitMatchTime = Time.time;
					_offlineArenaRequested = false;
					MatchInfo = null;
					OnRegistered_Pvp(response.AccountInfo);
				}
			}
		});
	}

	private void OnRegistered_Pvp(MatchedAccountInfo accountInfo)
	{
		if (!_offlinePlaying)
		{
			if (ArenaType == MatchArenaType.Normal)
			{
				_myOfflineMatchInfo_PvpNormal = accountInfo;
				InitVsLoadingPlayer(accountInfo, isLeft: true, isOnline: true);
				PvpLoading.instance.ClearRightPlayer();
			}
			else
			{
				SprkHonorDuel.OpenHonorWaiting(SceneAutoBehaviour<MatchManagement>.instance);
			}
		}
	}

	private void OnFailRegistered_Pvp(NShared.RegisterMatch.ResultType resultType)
	{
		if (!_offlinePlaying)
		{
			switch (resultType)
			{
			case NShared.RegisterMatch.ResultType.NotEnoughSword:
				UIHelper.ResolveRecoverStamina(isNotEnough: true, null, (StaminaType)(((ArenaType != MatchArenaType.Normal) ? 1 : 0) * 11 + 2));
				break;
			case NShared.RegisterMatch.ResultType.BanPickPenaltyUserBlocked:
			{
				float num = (float)GameRoot.Instance.ConstantDataContainer.GetValueInt("MatchingBanPickPenaltyUserBlockTime", 600) / 60f;
				GlobalMessagePopup.OpenGlobalError(LocalizationManager.GetStringDirect("MATCH_PICKBAN_PENALTY_POPUP_MSG", (int)num));
				break;
			}
			default:
				GlobalMessagePopup.OpenGlobalError(resultType);
				break;
			case NShared.RegisterMatch.ResultType.Success:
				break;
			}
		}
	}

	public void RetryMatch_Pvp(MatchResultInfo lastMatchResultInfo)
	{
		StaminaType staminaType = StaminaType.Sword;
		staminaType = ((ArenaType != MatchArenaType.Normal) ? StaminaType.Sword2 : StaminaType.Sword);
		if (GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetStamina(staminaType) == 0L)
		{
			bool isSkip = staminaType != StaminaType.Sword;
			UIHelper.ResolveRecoverStamina(isNotEnough: true, null, staminaType, 0, delegate
			{
				RetryRequestMatch_Pvp(lastMatchResultInfo);
			}, isSkip);
		}
		else
		{
			RetryRequestMatch_Pvp(lastMatchResultInfo);
		}
	}

	private void RetryRequestMatch_Pvp(MatchResultInfo lastMatchResultInfo)
	{
		if (lastMatchResultInfo != null)
		{
			_originalRequest_Pvp.Value.Rank = lastMatchResultInfo.NewTierRank;
		}
		MatchReturnState lobbyReturnState = new MatchReturnState(_originalRequest_Pvp.PvpType);
		RequestMatchInternal_Pvp(_originalRequest_Pvp, lobbyReturnState);
	}

	private void CancelMatch()
	{
		Reset(resetLastRequest: false);
		NShared.CancelMatch.Request request = new NShared.CancelMatch.Request();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, WebRequestFlag.None, delegate(NShared.CancelMatch.Response response)
		{
			if (response != null && response.Result != NShared.CancelMatch.ResultType.Fail)
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.SwordResult);
				CloseMatchWaiting();
			}
		});
	}

	private void CloseMatchWaiting()
	{
		if (ArenaType == MatchArenaType.BanPick)
		{
			SceneAutoBehaviour<MatchManagement>.instance.OnMatchWaitingClose();
		}
	}

	public void RequestOfflineArenaResult(bool isWin, int playTime, int[] aliveHeroIndices, string battleLogString, IList<ResultCreatureInfo> creatureInfos, Action<NShared.SetOfflineMatchResult.Response> onResult)
	{
		Reset(resetLastRequest: false);
		NShared.SetOfflineMatchResult.Request request = new NShared.SetOfflineMatchResult.Request();
		request.Win = (isWin ? 1 : 0);
		request.PlayTime = playTime;
		request.AliveHeroIndices = aliveHeroIndices;
		request.BattleLogString = battleLogString;
		if (creatureInfos != null)
		{
			string creatureInfoString = Json.Serialize(BaseJsonMarshaler.EncodeArray(creatureInfos, JsonMarshaler.EncodeJsonObject));
			request.CreatureInfoString = creatureInfoString;
		}
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SetOfflineMatchResult.Response response)
		{
			CancelMatch();
			if (response == null)
			{
				Vespa_System_Action_Extension_Method.fire(onResult, null);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				Vespa_System_Action_Extension_Method.fire(onResult, response);
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.ApplyMatchResult(response.MatchResult);
				SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.ApplyBattleInfo(response.BattleInfo, null);
				Vespa_System_Action_Extension_Method.fire(onResult, response);
			}
		});
	}

	private void UpdateRequestWaitMatch_Pvp()
	{
		if (!IsWaitingMatchPvp || GlobalAutoBehaviour<LoadingManager>.instance.IsSceneLoading)
		{
			return;
		}
		float time = Time.time;
		float num = time - _lastRequestWaitMatchTime;
		float num2 = ContentsDefine.MatchArena.IntervalTime_for_RequestWaitMatch;
		if (SprkHonorDuel.ConfiguredWaitInterval(this) == 0)
		{
			num2 = 0.5f;
		}
		if (num < num2)
		{
			return;
		}
		if (_isSearchPaused)
		{
			_pausedTime = num;
			return;
		}
		_lastRequestWaitMatchTime = 9999999f;
		bool playOfflineMatch = false;
		if (ArenaType == MatchArenaType.Normal && DuelType == MatchDuelType.Arena && !_offlineArenaRequested && (!_lastRequest_Pvp.Value.IsRematch || _lastRequest_Pvp.Value.OpponentIsNpc) && ((_lastRequest_Pvp.Value.IsRematch && _lastRequest_Pvp.Value.OpponentIsNpc) || time - _registerMatchTime > ContentsDefine.MatchArena.WaitingTime_for_RequestOfflineMatch + _pausedTime))
		{
			_offlineArenaRequested = true;
			playOfflineMatch = true;
		}
		NShared.WaitMatch.Request request = new NShared.WaitMatch.Request();
		request.PlayOfflineMatch = playOfflineMatch;
		request.ReserveCancel = _waitCancelReserved;
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, WebRequestFlag.None, delegate(NShared.WaitMatch.Response response)
		{
			_lastRequestWaitMatchTime = Time.time;
			if (response == null)
			{
				IsWaitingMatchPvp = false;
				OnWaitMore_Pvp(NShared.WaitMatch.ResultType.Fail, new WaitMatchResponseWrapper(response));
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				IsWaitingMatchPvp = false;
				OnWaitMore_Pvp(NShared.WaitMatch.ResultType.Fail, new WaitMatchResponseWrapper(response));
			}
			else if (response.Result == NShared.WaitMatch.ResultType.Canceled)
			{
				IsWaitingMatchPvp = false;
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.SwordResult);
				string title = LocalizationManager.GetString(NShared.NLocalization.Common.INFORM);
				string message = (_ReserveTimeout ? LocalizationManager.GetStringDirect("MATCH_PICKBAN_MATCH_OVERTIME") : LocalizationManager.GetStringDirect("MATCH_PICKBAN_MATCH_CANCEL_MYSELF"));
				GlobalMessagePopup.OpenGlobal(title, message, delegate
				{
					SingletonUpdater<MatchManager>.instance.CancelReserve();
					SceneAutoBehaviour<MatchManagement>.instance.GetInstance<MatchWaitingRenewal>().Close();
				}, retryButton: false);
			}
			else if (response.Result == NShared.WaitMatch.ResultType.WaitMore)
			{
				if (playOfflineMatch)
				{
					if (response.MatchedNpcInfo != null)
					{
						GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.SwordResult);
						StartOfflineArena_Pvp(_myOfflineMatchInfo_PvpNormal, response.MatchedNpcInfo, _lastRequest_Pvp.Value.HeroIndices, _lastRequest_Pvp.Value.LeaderHeroIndex, response.ChapterIndex, response.DungeonIndex);
					}
					else
					{
						CancelMatch();
						IsWaitingMatchPvp = false;
						OnWaitMore_Pvp(NShared.WaitMatch.ResultType.Fail, new WaitMatchResponseWrapper(response));
					}
				}
			}
			else
			{
				GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.SwordResult);
				if (response.Result == NShared.WaitMatch.ResultType.Success)
				{
					MatchInfo = response.MatchInfo;
					RelayBattleServerAddress = response.RelayBattleServerAddress;
					RelayBattleServerPort = response.RelayBattleServerPort;
					IsWaitingMatchPvp = false;
					OnWaitMore_Pvp(response.Result, new WaitMatchResponseWrapper(response));
				}
				else
				{
					IsWaitingMatchPvp = false;
					CloseMatchWaiting();
					OnWaitMore_Pvp(response.Result, new WaitMatchResponseWrapper(response));
				}
			}
		});
	}

	private void OnWaitMore_Pvp(NShared.WaitMatch.ResultType resultType, WaitMatchResponseWrapper response)
	{
		LogErrorOnDev(resultType.ToString());
		switch (resultType)
		{
		case NShared.WaitMatch.ResultType.WaitMore:
			return;
		case NShared.WaitMatch.ResultType.Success:
		{
			OfflineMatchResultInfo offlineToOnlineInfo_PvpNormal = null;
			if (ArenaType == MatchArenaType.Normal && DuelType == MatchDuelType.Arena)
			{
				RankResultInfo rankResultInfo = SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.GetRankResultInfo(MatchPvpType.Normal);
				ArenaBattleInfo arenaBattleInfo = SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.GetArenaBattleInfo(MatchPvpType.Normal);
				offlineToOnlineInfo_PvpNormal = new OfflineMatchResultInfo
				{
					MatchScore = arenaBattleInfo.MatchScore,
					TierIndex = arenaBattleInfo.WorldTierIndex,
					TierRank = rankResultInfo.TotalRank,
					TotalRank = rankResultInfo.TotalRank
				};
			}
			if (response.IsValid && response.Value.RankResult != null)
			{
				SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.ApplyBattleInfo(response.Value.BattleInfo, null);
				SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.ApplyRankResult(response.PvpType, response.Value.RankResult);
			}
			IsMatchMade = true;
			ConnectToBattleServerInternal(delegate(bool connected)
			{
				if (!connected)
				{
					LogErrorOnDev("connected == false");
					MatchInfo = null;
					SingletonAuto<BattleFacade>.instance.ForceStopBattle();
					PvpLoading.ReleaseInstance();
					GlobalMessagePopup.OpenGlobalError(NShared.WaitMatch.ResultType.Fail);
				}
				else if (_offlinePlaying)
				{
					LogErrorOnDev("_offlinePlaying");
					_offlineToOnlineInfo_PvpNormal = offlineToOnlineInfo_PvpNormal;
				}
				else
				{
					LogErrorOnDev("onlinePlaying");
					StartArena_Pvp(MatchInfo, showMyInfo: false);
				}
			});
			return;
		}
		}
		SingletonAuto<BattleFacade>.instance.ForceStopBattle();
		PvpLoading.ReleaseInstance();
		Action action = delegate
		{
			if (ArenaType == MatchArenaType.BanPick)
			{
				LogErrorOnDev("ArenaType == NShared.MatchArenaType.BanPick");
				if (response.IsValid)
				{
					GlobalAutoBehaviour<AccountManager>.instance.UserManager.Apply(response.Value.SwordResult);
				}
				SceneAutoBehaviour<MatchManagement>.instance.CloseInstance<MatchWaitingRenewal>();
			}
			else if (_originalRequest_Pvp != null)
			{
				RequestMatchInternal_Pvp(_originalRequest_Pvp, _lobbyReturnState);
			}
			else
			{
				LogErrorOnDev("ReturnToLobby");
				SingletonAuto<BattleContext>.instance.ReturnToLobby();
			}
		};
		if (resultType == NShared.WaitMatch.ResultType.Canceled)
		{
			LogErrorOnDev("action.fire()");
			Vespa_System_Action_Extension_Method.fire(action);
		}
		else
		{
			LogErrorOnDev("OpenGlobalError");
			GlobalMessagePopup.OpenGlobalError(resultType, action);
		}
	}

	private void LogErrorOnDev(string log)
	{
		if (!Define.IsLiveBuild())
		{
			VespaDebug.LogError("OnWaitMore :: " + log);
		}
	}

	private void ConnectToBattleServerInternal(Action<bool> onConnected)
	{
		if (MatchInfo == null)
		{
			Vespa_System_Action_Extension_Method.fire(onConnected, i_argA: false);
			return;
		}
		string text = MatchInfo.BattleServerAddress;
		int num = MatchInfo.BattleServerPort;
		bool sendRelayLogin = false;
		if (!String_Extension_Method.IsNullOrEmpty(RelayBattleServerAddress))
		{
			text = RelayBattleServerAddress;
			num = ((RelayBattleServerPort > 0) ? RelayBattleServerPort : num);
			sendRelayLogin = true;
		}
		if (string.IsNullOrEmpty(text))
		{
			Vespa_System_Action_Extension_Method.fire(onConnected, i_argA: false);
			return;
		}
		GlobalAutoBehaviour<SocketServiceManager>.instance.BattleConnector.Connect(text, num, delegate(bool connected)
		{
			if (!connected)
			{
				Vespa_System_Action_Extension_Method.fire(onConnected, i_argA: false);
			}
			else if (sendRelayLogin)
			{
				string text2 = "";
				text2 += MatchInfo.BattleServerAddress;
				text2 += " ";
				text2 += MatchInfo.BattleServerPort;
				text2 += "\r\n";
				GlobalAutoBehaviour<SocketServiceManager>.instance.BattleConnector.SendText(text2);
				GlobalAutoBehaviour<SocketServiceManager>.instance.BattleConnector.ReserveOneshotAction("OK", 10f, delegate(bool relayed)
				{
					if (!relayed)
					{
						Vespa_System_Action_Extension_Method.fire(onConnected, i_argA: false);
					}
					else
					{
						ProcessBattleLoginInternal(onConnected);
					}
				});
			}
			else
			{
				ProcessBattleLoginInternal(onConnected);
			}
		});
	}

	private void ProcessBattleLoginInternal(Action<bool> onConnected)
	{
		GlobalAutoBehaviour<SocketServiceManager>.instance.BattleConnector.OnDisconnected = null;
		MatchedAccountInfo matchedAccountInfo = Array.Find(MatchInfo.AccountInfos, (MatchedAccountInfo info) => info.AccountId == GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id);
		BattleLoginReq battleLoginReq = new BattleLoginReq();
		battleLoginReq.AccountId = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id;
		battleLoginReq.MatchUid = MatchInfo.Uid;
		battleLoginReq.MatchServerHost = ((matchedAccountInfo != null) ? matchedAccountInfo.MatchServerHost : string.Empty);
		battleLoginReq.GameServerHost = GlobalAutoBehaviour<AccountManager>.instance.ServerHost;
		GlobalAutoBehaviour<SocketServiceManager>.instance.BattleConnector.Request(battleLoginReq, delegate(BattleLoginRes res)
		{
			if (res == null)
			{
				Vespa_System_Action_Extension_Method.fire(onConnected, i_argA: false);
			}
			else if (res == null || res.Result != BattleLoginRes.ResultType.Success)
			{
				Vespa_System_Action_Extension_Method.fire(onConnected, i_argA: false);
			}
			else
			{
				if (ArenaType == MatchArenaType.BanPick)
				{
					GlobalAutoBehaviour<SocketServiceManager>.instance.BattleConnector.OnDisconnected = OnBattleServerDisconnected;
				}
				Vespa_System_Action_Extension_Method.fire(onConnected, i_argA: true);
			}
		});
	}

	private void UpdateReservedOfflineToOnline_PvpNormal()
	{
		if (_offlineToOnlineInfo_PvpNormal != null && SingletonAuto<BattleFacade>.isValidInstance && SingletonAuto<BattleFacade>.instance.IsValidBattle())
		{
			BattleFacade.BattleCacheData battleCache = SingletonAuto<BattleFacade>.instance.GetBattleCache();
			if (battleCache != null && battleCache.IsBattleStarted)
			{
				OfflineMatchResultInfo offlineToOnlineInfo_PvpNormal = _offlineToOnlineInfo_PvpNormal;
				_offlineToOnlineInfo_PvpNormal = null;
				VespaCoroutine.Start(coOfflineToOnline_PvpNormal(offlineToOnlineInfo_PvpNormal));
			}
		}
	}

	public void StartOfflineArena_Pvp(MatchedAccountInfo myMatchInfo, MatchedAccountInfo matchedNpcInfo, int[] heroIndices, int leaderHeroIndex, int chapterIndex, int dungeonIndex)
	{
		_offlinePlaying = true;
		SingletonAuto<BattleContext>.instance.SetOfflineArena(heroIndices, leaderHeroIndex, matchedNpcInfo, chapterIndex, dungeonIndex, myMatchInfo);
		InitVsLoadingPlayer(matchedNpcInfo, isLeft: false, isOnline: false);
		SingletonUpdater<Executer>.instance.DelayRun(ContentsDefine.MatchArena.DelayTime_for_EnterBattleScene, this, MoveToBattleScene);
	}

	private void StartArena_Pvp(MatchInfo matchInfo, bool showMyInfo)
	{
		if (matchInfo == null || matchInfo.AccountInfos == null || matchInfo.AccountInfos.Length == 0)
		{
			return;
		}
		_offlinePlaying = false;
		SingletonAuto<BattleContext>.instance.SetArena(matchInfo);
		if (ArenaType != MatchArenaType.Normal)
		{
			return;
		}
		if (showMyInfo)
		{
			MatchedAccountInfo matchedAccountInfo = matchInfo.AccountInfos.FirstOrDefault((MatchedAccountInfo e) => e.AccountId == GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id);
			if (matchedAccountInfo != null)
			{
				InitVsLoadingPlayer(matchedAccountInfo, isLeft: true, isOnline: true);
			}
		}
		MatchedAccountInfo matchedAccountInfo2 = matchInfo.AccountInfos.FirstOrDefault((MatchedAccountInfo e) => e.AccountId != GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id);
		if (matchedAccountInfo2 != null)
		{
			InitVsLoadingPlayer(matchedAccountInfo2, isLeft: false, isOnline: true);
		}
		SingletonUpdater<Executer>.instance.DelayRun(ContentsDefine.MatchArena.DelayTime_for_EnterBattleScene, this, MoveToBattleScene);
	}

	public void InitVsLoadingPlayer(MatchedAccountInfo accountInfo, bool isLeft, bool isOnline, GuildArenaRankerInfo guildInfo = null, GuildArenaMemberScoreInfo guildArenaScoreInfo = null)
	{
		if (accountInfo == null)
		{
			return;
		}
		int avatarHeroIndex = accountInfo.UserInfo.AvatarHeroIndex;
		string nick = accountInfo.UserInfo.Nick;
		Dictionary<int, CachedHeroInfo>.ValueCollection values = accountInfo.HeroInfos.Values;
		int fightingPower = accountInfo.FightingPower;
		PlayerBattleInfo battleInfo = accountInfo.BattleInfo;
		VisualTierRank visualTierRank = VisualTierRank.CreateFromServer(accountInfo.IsNpc ? (-1) : accountInfo.Rank);
		int num = 0;
		if (battleInfo != null)
		{
			num = NGame2.NContentsDefine.MatchArena.GetTierIndex(ArenaType, battleInfo);
		}
		if (num == 0)
		{
			ArenaBattleInfo arenaBattleInfo = SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.GetArenaBattleInfo(ArenaType);
			if (arenaBattleInfo != null)
			{
				num = NGame2.NContentsDefine.MatchArena.GetTierIndex(ArenaType, arenaBattleInfo);
			}
		}
		int mmr = accountInfo.Mmr;
		int winPercentage = -1;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int successiveWin = 0;
		if (battleInfo != null)
		{
			num3 = accountInfo.BattleInfo.SeasonWin;
			num4 = accountInfo.BattleInfo.SeasonLose;
			successiveWin = accountInfo.BattleInfo.SuccessiveWin;
			int num5 = num3 + num4;
			winPercentage = ((!accountInfo.IsNpc && num5 > 0) ? (accountInfo.BattleInfo.SeasonWin * 100 / num5) : (-1));
			num2 = battleInfo.MatchScore;
		}
		PvpLoading pvpLoading = PvpLoading.instance;
		if (pvpLoading == null)
		{
			return;
		}
		if (isLeft)
		{
			pvpLoading.SetBG();
			pvpLoading.InitLeftPlayer(DuelType, winPercentage, num3, num4, successiveWin, num, visualTierRank, nick, isOnline, avatarHeroIndex, values, guildInfo, guildArenaScoreInfo);
			if (ShowDebugLabel)
			{
				pvpLoading.InitLeftDebugLabel("FP=" + fightingPower + " MMR=" + mmr + " SCORE=" + num2);
			}
		}
		else
		{
			pvpLoading.InitRightPlayer(DuelType, winPercentage, num3, num4, successiveWin, num, visualTierRank, nick, isOnline, avatarHeroIndex, values, guildInfo, guildArenaScoreInfo);
			if (ShowDebugLabel)
			{
				pvpLoading.InitRightDebugLabel("FP=" + fightingPower + " MMR=" + mmr + " SCORE=" + num2);
			}
		}
	}

	public void MoveToBattleScene()
	{
		if (_lobbyReturnState != null && (ArenaType == MatchArenaType.Normal || ArenaType == MatchArenaType.BanPick))
		{
			LobbyScene2.ListReturnState.Clear();
			LobbyScene2.ListReturnState.Add(_lobbyReturnState);
			_lobbyReturnState = null;
		}
		SingletonAuto<BattleContext>.instance.MoveToBattleScene(LoadingManager.ELoadingType.None);
	}

	private IEnumerator coOfflineToOnline_PvpNormal(OfflineMatchResultInfo oldInfo)
	{
		if (SingletonAuto<OfflineArenaContext>.isValidInstance)
		{
			SingletonAuto<OfflineArenaContext>.instance.OfflineArenaResultRequested = true;
		}
		if (MatchInfo == null || oldInfo == null)
		{
			yield break;
		}
		SingletonAuto<BattleFacade>.instance.ForceStopBattle();
		ArenaBattleInfo arenaBattleInfo = SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.GetArenaBattleInfo(MatchPvpType.Normal);
		RankResultInfo rankResultInfo = SingletonAuto<NGame2.NContentsInfo.MatchArena>.instance.GetRankResultInfo(MatchPvpType.Normal);
		int matchScore = arenaBattleInfo.MatchScore;
		int worldTierIndex = arenaBattleInfo.WorldTierIndex;
		int totalRank = rankResultInfo.TotalRank;
		VisualTierRank visualTierRank = VisualTierRank.CreateFromServer(rankResultInfo.TotalRank);
		List<BaseWindow> openingPopupList = new List<BaseWindow>
		{
			PvpNewChallenger.Open(),
			PvpResultScore.Open(matchScore, matchScore - oldInfo.MatchScore, visualTierRank, totalRank - oldInfo.TotalRank, worldTierIndex, ArenaType)
		};
		if (worldTierIndex != oldInfo.TierIndex)
		{
			openingPopupList.Add(TierChangePopup.Open(oldInfo.TierIndex, worldTierIndex, visualTierRank, ArenaType));
		}
		List<BaseWindow> hidingWindowList = new List<BaseWindow>();
		if (SceneAutoBehaviour<BattleManagement>.isValidInstance)
		{
			hidingWindowList.Add(SceneAutoBehaviour<BattleManagement>.instance.GetInstance<HeroParty>());
		}
		if (SceneAutoBehaviour<BattleManagement>.isValidInstance)
		{
			hidingWindowList.Add(SceneAutoBehaviour<BattleManagement>.instance.GetInstance<DamageMeter>());
		}
		if (SceneAutoBehaviour<ArenaManagement>.isValidInstance)
		{
			hidingWindowList.Add(SceneAutoBehaviour<ArenaManagement>.instance.GetInstance<PvpStatus>());
		}
		float maxWaitTime = (float)GameRoot.Instance.ConstantDataContainer.GetValueInt("MatchNewChallengerShowDurationMs", 4000) / 1000f;
		float hideWindowTime = 1f;
		float beginWaitTime = Time.realtimeSinceStartup;
		while (true)
		{
			yield return null;
			float num = Time.realtimeSinceStartup - beginWaitTime;
			if (num >= maxWaitTime)
			{
				break;
			}
			float hidingWindowAlpha = ((num < hideWindowTime) ? (1f - num / hideWindowTime) : 0f);
			hidingWindowList.ForEach(delegate(BaseWindow e)
			{
				if ((bool)e)
				{
					e.SetEventHide(hidingWindowAlpha);
				}
			});
		}
		hidingWindowList.ForEach(delegate(BaseWindow e)
		{
			e.Close();
		});
		hidingWindowList.Clear();
		openingPopupList.ForEach(delegate(BaseWindow e)
		{
			e.Close();
		});
		openingPopupList.Clear();
		PvpLoading pvpLoading = PvpLoading.instance;
		if (pvpLoading != null)
		{
			pvpLoading.Reset();
		}
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(new OnClickRetry());
		StartArena_Pvp(MatchInfo, showMyInfo: true);
	}

	public void EnterBanPickArena(MatchedAccountInfo[] accountInfos)
	{
		SceneAutoBehaviour<MatchManagement>.instance.CloseInstance<MatchBanPick>();
		MatchInfo.AccountInfos = accountInfos;
		SingletonAuto<BattleContext>.instance.SetArena(MatchInfo);
		MatchedAccountInfo accountInfo = MatchInfo.AccountInfos.FirstOrDefault((MatchedAccountInfo info) => info.AccountId == GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id);
		MatchedAccountInfo accountInfo2 = MatchInfo.AccountInfos.FirstOrDefault((MatchedAccountInfo info) => info.AccountId != GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id);
		InitVsLoadingPlayer(accountInfo, isLeft: true, isOnline: true);
		InitVsLoadingPlayer(accountInfo2, isLeft: false, isOnline: true);
		MoveToBattleScene();
	}

	private void OnBattleServerDisconnected()
	{
		GlobalMessagePopup.OpenGlobalError(LocalizationManager.GetString(NShared.BeginCampaign.ResultType.BattleServerNotFound), delegate
		{
			if (ArenaType == MatchArenaType.BanPick)
			{
				SceneAutoBehaviour<MatchManagement>.instance.CloseMatchBanPick();
			}
		});
	}

	public void SetMatchDuelType(MatchDuelType matchDuelType)
	{
		DuelType = matchDuelType;
	}

	public void SetMatchArenaType(MatchArenaType matchType)
	{
		ArenaType = matchType;
	}

	public void TryPauseMatching(Action onSuccess, Action onFail)
	{
		RequestPause request = new RequestPause();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.WaitMatch.Response response)
		{
			if (response.BaseResult == BaseResultType.Success && response.Result == NShared.WaitMatch.ResultType.Success)
			{
				_isSearchPaused = true;
				Vespa_System_Action_Extension_Method.fire(onSuccess);
			}
			else
			{
				_isSearchPaused = false;
				Vespa_System_Action_Extension_Method.fire(onFail);
			}
		});
	}

	public void ResumeWaitingMatchIfPaused(int retryCount = 5)
	{
		if (!_isSearchPaused)
		{
			return;
		}
		RequestResume request = new RequestResume();
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.WaitMatch.Response response)
		{
			if (response.BaseResult == BaseResultType.Success && response.Result == NShared.WaitMatch.ResultType.Success)
			{
				_isSearchPaused = false;
			}
			else if (retryCount > 0)
			{
				ResumeWaitingMatchIfPaused(--retryCount);
			}
			else
			{
				_isSearchPaused = false;
				GlobalMessagePopup.OpenGlobalError(NShared.WaitMatch.ResultType.Canceled, delegate
				{
					MoveToBattleScene();
				});
			}
		});
	}
}
