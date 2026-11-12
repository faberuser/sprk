using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NBattleField.NManager.NCampaign;
using NGame2.NBattleField.NManager.NCampaign.NState;
using NGame2.NBattleField.NManager.NUtil;
using NGame2.NBattleView;
using NGame2.NEventTrigger;
using NGame2.NUI.NManager.NBattle;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NShared.NEvent;
using NShared.NLocalization;
using NShared.NUtil;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;
using NVespa.NSocket;
using UnityEngine;

namespace NGame2.NBattleField.NManager.NPlay;

public class OnlinePlay : SceneAutoBehaviour<OnlinePlay>, StateBase.IListener, IPlay, BattleFacade.IBattleLogRaiser
{
	public interface IDataProvider : NGame2.NBattleField.NManager.NPlay.IDataProvider
	{
		int GetChapterIndex();

		int GetDungeonIndex();

		NShared.DungeonDifficultyType GetDifficulty();

		OnlineBattleCallback GetBattleCallback();

		int GetWorldMapEventIndex();

		void OnReturnToLobby();

		IRequester GetSocketConnector();

		string GetError();

		int OnlineGameSpeedRatio();

		List<List<BattleCreatureCreateInfo>> GetBattleCreatureCreateInfoListToWaveList();

		BattleInstanceExtraData GetExtraData();
	}

	public const float MaxWaitStartTime = 100f;

	private IDataProvider _dataProvider;

	private DateTime _lastPingTime;

	private DateTime _lastStateCheckedTime = DateTime.UtcNow;

	private DateTime _startedTime;

	private CampaignStateUpdater _stateUpdater;

	private bool _battleInvalidated;

	private Action _onReadyBattleScene;

	private WaitPartyMemberPopup _multiplayWaitingPopup;

	private bool _stopBattle;

	private bool _battleCancel;

	private bool _campaignBegin;

	private bool _recvBattleEnter;

	private DateTime _waitingMessageCheck;

	private float _waitingMessageDelay;

	private PendingContainer<ReserveOnSkillReq> _reserveSkillPendingContainer = new PendingContainer<ReserveOnSkillReq>();

	private PendingContainer<ReserveOffSkillReq> _reserveOffSkillPendingContainer = new PendingContainer<ReserveOffSkillReq>();

	public long UserId => GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id;

	void IPlay.Setup(NGame2.NBattleField.NManager.NPlay.IDataProvider dataProvider, Action onReadyBattle)
	{
		_dataProvider = dataProvider as IDataProvider;
		_startedTime = DateTime.UtcNow;
		_onReadyBattleScene = onReadyBattle;
		_battleCancel = false;
		_campaignBegin = false;
		_recvBattleEnter = false;
		_waitingMessageCheck = DateTime.MaxValue;
		_waitingMessageDelay = (float)GameRoot.Instance.ConstantDataContainer.GetValueInt("WaitingMessageDelay", 4000) / 1000f;
		VespaCoroutine.Start(this, coSetup());
		SetReserveSkillManager();
	}

	private void SetReserveSkillManager()
	{
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("OnlinePlayPendingSkillExpireTime");
		if (valueInt != 0)
		{
			_reserveSkillPendingContainer.SetValid(isValid: true);
			_reserveSkillPendingContainer.SetExpireTime((float)valueInt / 1000f);
			_reserveSkillPendingContainer.Clear();
			_reserveOffSkillPendingContainer.SetValid(isValid: true);
			_reserveOffSkillPendingContainer.SetExpireTime((float)valueInt / 1000f);
			_reserveOffSkillPendingContainer.Clear();
		}
	}

	private IEnumerator coSetup()
	{
		_stopBattle = false;
		yield return VespaCoroutine.Start(this, coSetupFieldImage());
		SingletonAuto<BattleFacade>.CreateInstance();
		yield return VespaCoroutine.Start(this, coSetupBattleManager());
		yield return new WaitForSeconds(0.5f);
	}

	private IEnumerator coSetupFieldImage()
	{
		int chapterIndex = _dataProvider.GetChapterIndex();
		int dungeonIndex = _dataProvider.GetDungeonIndex();
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data != null)
		{
			bool isComplete = false;
			GuildFlagData guildFlagData = _dataProvider.GetGuildFlagData();
			BattleViewGenericSingleton<FieldMapManager2>.instance.Open(data.BattleField, guildFlagData, delegate
			{
				isComplete = true;
			});
			while (!isComplete)
			{
				yield return null;
			}
		}
	}

	private IEnumerator coSetupBattleManager()
	{
		yield return null;
		_lastPingTime = DateTime.UtcNow;
		int chapterIndex = _dataProvider.GetChapterIndex();
		int dungeonIndex = _dataProvider.GetDungeonIndex();
		NShared.DungeonDifficultyType difficulty = _dataProvider.GetDifficulty();
		BattleInstanceExtraData extraData = _dataProvider.GetExtraData();
		int num = _dataProvider.OnlineGameSpeedRatio();
		SingletonAuto<BattleFacade>.instance.CreateBattle(chapterIndex, dungeonIndex, (int)difficulty, isScenario: false, isOnlineMode: true, autoStart: true, extraData, num);
		SingletonAuto<BattleFacade>.instance.BattleLogRaiser = this;
		EBattleMenu battleMenuType = _dataProvider.GetBattleMenuType();
		int worldMapEventIndex = _dataProvider.GetWorldMapEventIndex();
		SceneAutoBehaviour<BattleManagement>.instance.OpenMenu(UserId, chapterIndex, dungeonIndex, 0, isScenario: false, difficulty, battleMenuType, extraData, worldMapEventIndex, num);
		SceneAutoBehaviour<BattleManagement>.instance.SetMenuHide(BattleManagement.EMenuHideFlag.InitCampaign, skipAni: true);
		yield return null;
		if (_dataProvider.GetBattleType() == BattleType.Arena)
		{
			Game3DRoot.instance.GameCamera.View = SceneAutoBehaviour<CameraManager2>.instance;
			SceneAutoBehaviour<CameraManager2>.instance.SwitchCam_Battle(99);
		}
		_stateUpdater = new CampaignStateUpdater(this);
		_stateUpdater.SetState(new CampaignOpening(MakeBattleSceneReady));
		_dataProvider.OnEnterBattle();
		SendOnlineBattleEnter(delegate
		{
			_recvBattleEnter = true;
		});
	}

	protected override void OnDestroy()
	{
		if (!SystemStatus.IsApplicationQuit)
		{
			base.OnDestroy();
		}
	}

	public void SendOnlineBattleEnter(Action<BattleEnterRes> onResult = null)
	{
		BattleEnterReq data = new BattleEnterReq();
		_dataProvider.GetSocketConnector().Request(data, onResult);
	}

	public void SendOnlineBattleStart(Action<BattleStartRes> onResult = null)
	{
		BattleStartReq data = new BattleStartReq();
		_dataProvider.GetSocketConnector().Request(data, onResult);
	}

	public void ChainUpdate()
	{
		if (!_stopBattle)
		{
			UpdateConnectionPing();
			UpdateConnectionState();
			UpdateWaitingMessage();
			if (_dataProvider != null)
			{
				_dataProvider.OnUpdate();
			}
			if (_stateUpdater != null)
			{
				_stateUpdater.Update();
			}
			SingletonAuto<BattleFacade>.instance.UpdateOnline();
		}
	}

	private void UpdateConnectionPing()
	{
		BattleFacade.BattleCacheData battleCache = SingletonAuto<BattleFacade>.instance.GetBattleCache();
		if (!battleCache.IsBattleStarted || _dataProvider.GetSocketConnector().State != ConnectState.Connected || (DateTime.UtcNow - _lastPingTime).TotalSeconds < 5.0)
		{
			return;
		}
		_lastPingTime = DateTime.UtcNow;
		PingReq pingReq = new PingReq();
		pingReq.ClientTimeMs = battleCache.GlobalCurrentTimeMs;
		_dataProvider.GetSocketConnector().Request(pingReq, delegate(PingRes res)
		{
			if (ProfileManager.IsLogEnabled)
			{
				GlobalAutoBehaviour<ProfileManager>.instance.RecordResponseTime((float)(DateTime.UtcNow - _lastPingTime).TotalMilliseconds);
			}
			OnPingRes(res);
		});
	}

	private void MakeBattleSceneReady()
	{
		Vespa_System_Action_Extension_Method.fire(_onReadyBattleScene);
		_onReadyBattleScene = null;
	}

	private void HandleErrorAndReturnToLobby(string message)
	{
		if (!_battleInvalidated)
		{
			_battleInvalidated = true;
			_battleCancel = true;
			MakeBattleSceneReady();
			GlobalMessagePopup.OpenGlobalError(message, delegate
			{
				SingletonAuto<BattleContext>.instance.ReturnToLobby(_dataProvider.OnReturnToLobby);
			});
			SingletonAuto<BattleFacade>.instance.ForceStopBattle();
		}
	}

	private void UpdateConnectionState()
	{
		if ((DateTime.UtcNow - _lastStateCheckedTime).TotalSeconds < 3.0)
		{
			return;
		}
		_lastStateCheckedTime = DateTime.UtcNow;
		if (!SingletonAuto<BattleFacade>.isValidInstance || _battleInvalidated)
		{
			return;
		}
		string error = _dataProvider.GetError();
		if (!string.IsNullOrEmpty(error))
		{
			HandleErrorAndReturnToLobby(error);
			return;
		}
		bool isBattleEnd = _dataProvider.GetBattleCallback().IsBattleEnd;
		bool flag = _dataProvider.GetSocketConnector().State == ConnectState.Connected;
		if (!flag && !isBattleEnd)
		{
			HandleErrorAndReturnToLobby(LocalizationManager.GetString(Match.MATCH_BATTLE_SERVER_DISCONNECTED));
		}
		else
		{
			if (!SingletonAuto<BattleFacade>.instance.IsValidBattle())
			{
				return;
			}
			BattleFacade.BattleCacheData battleCache = SingletonAuto<BattleFacade>.instance.GetBattleCache();
			_ = GameRoot.Instance.CampaignDungeonDataContainer.GetData(_dataProvider.GetChapterIndex(), _dataProvider.GetDungeonIndex()).BattleType;
			TimeSpan timeSpan = DateTime.UtcNow - _startedTime;
			if (battleCache.MainCurrentTimeMs <= 0 && !battleCache.IsBattleStarted && timeSpan.TotalSeconds > 100.0)
			{
				MakeBattleSceneReady();
				_dataProvider.GetSocketConnector().Close();
				GlobalMessagePopup.OpenGlobalError(LocalizationManager.GetString(Match.MATCH_BATTLE_CANCELED), delegate
				{
					SingletonAuto<BattleContext>.instance.ReturnToLobby(_dataProvider.OnReturnToLobby);
				});
				SingletonAuto<BattleFacade>.instance.ForceStopBattle();
				_battleCancel = true;
				_battleInvalidated = true;
			}
			else if (!battleCache.IsBattleStarted)
			{
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.Append("[OnlinePlay State] IsSocketConnected = " + flag);
				stringBuilder.Append(" Elapsed = " + timeSpan.TotalSeconds);
				stringBuilder.Append(" WaveIndex = " + battleCache.WaveIndex);
				stringBuilder.Append(" IsBattleEnded = " + isBattleEnd);
			}
		}
	}

	private void UpdateWaitingMessage()
	{
		if (!SingletonAuto<BattleFacade>.instance.IsValidBattle() || !_recvBattleEnter || (_dataProvider.GetBattleMenuType() & EBattleMenu.WaitingMessage) == EBattleMenu.None)
		{
			return;
		}
		if (_waitingMessageCheck == DateTime.MaxValue)
		{
			_waitingMessageCheck = DateTime.UtcNow;
		}
		else if (!((DateTime.UtcNow - _waitingMessageCheck).TotalSeconds < (double)_waitingMessageDelay))
		{
			if (_campaignBegin || _battleCancel)
			{
				HideWaitingMessage();
			}
			else
			{
				ShowWaitingMessage();
			}
		}
	}

	private void OnPingRes(PingRes res)
	{
		if (res != null)
		{
			long serverTimeMs = res.ServerTimeMs;
			SingletonAuto<BattleFacade>.instance.SetServerTimeMs(serverTimeMs);
		}
	}

	private void ShowWaitingMessage()
	{
		string message = LocalizationManager.GetString(Multiplay.MULTIPLAY_BATTLE_WAITING_USER);
		if (_multiplayWaitingPopup == null)
		{
			_multiplayWaitingPopup = WaitPartyMemberPopup.Open(message);
		}
		else
		{
			_multiplayWaitingPopup.SetMessage(message);
		}
	}

	private void HideWaitingMessage()
	{
		if (_multiplayWaitingPopup != null)
		{
			_multiplayWaitingPopup.Close();
			_multiplayWaitingPopup = null;
		}
	}

	void BattleFacade.IBattleLogRaiser.ReserveSkill(int creatureUid, int skillSlotIndex, int manualTargetUid, bool autoReserved)
	{
		if (!SingletonAuto<BattleFacade>.instance.IsValidBattle() || SingletonAuto<BattleFacade>.instance.GetCreatureCache(creatureUid).Uid == 0)
		{
			return;
		}
		if (skillSlotIndex < 0)
		{
			ReserveOffSkillReq req = new ReserveOffSkillReq();
			req.CreatureUid = creatureUid;
			if (_reserveOffSkillPendingContainer.IsValidInstance)
			{
				if (_reserveOffSkillPendingContainer.IsPending(req))
				{
					return;
				}
				_reserveOffSkillPendingContainer.Add(req);
			}
			DateTime sendTime = DateTime.UtcNow;
			_dataProvider.GetSocketConnector().Request<ReserveOffSkillReq, BaseCreatureActionRes>(req, delegate
			{
				if (ProfileManager.IsLogEnabled)
				{
					GlobalAutoBehaviour<ProfileManager>.instance.RecordResponseTime((float)(DateTime.UtcNow - sendTime).TotalMilliseconds);
				}
				if (_reserveOffSkillPendingContainer.IsValidInstance)
				{
					_reserveOffSkillPendingContainer.Remove(req);
				}
			});
			return;
		}
		ReserveOnSkillReq req2 = new ReserveOnSkillReq();
		req2.CreatureUid = creatureUid;
		req2.SkillSlotIndex = skillSlotIndex;
		req2.ManualTargetUid = manualTargetUid;
		req2.AutoReserved = autoReserved;
		if (_reserveSkillPendingContainer.IsValidInstance)
		{
			if (_reserveSkillPendingContainer.IsPending(req2))
			{
				return;
			}
			_reserveSkillPendingContainer.Add(req2);
		}
		DateTime sendTime2 = DateTime.UtcNow;
		_dataProvider.GetSocketConnector().Request<ReserveOnSkillReq, BaseCreatureActionRes>(req2, delegate
		{
			if (ProfileManager.IsLogEnabled)
			{
				GlobalAutoBehaviour<ProfileManager>.instance.RecordResponseTime((float)(DateTime.UtcNow - sendTime2).TotalMilliseconds);
			}
			if (_reserveSkillPendingContainer.IsValidInstance)
			{
				_reserveSkillPendingContainer.Remove(req2);
			}
		});
	}

	void BattleFacade.IBattleLogRaiser.FireNextSkill(int creatureUid)
	{
		if (SingletonAuto<BattleFacade>.instance.GetCreatureCache(creatureUid).Uid != 0)
		{
			FireNextSkillReq fireNextSkillReq = new FireNextSkillReq();
			fireNextSkillReq.CreatureUid = creatureUid;
			_dataProvider.GetSocketConnector().Request<FireNextSkillReq, BaseCreatureActionRes>(fireNextSkillReq, delegate
			{
			});
		}
	}

	void BattleFacade.IBattleLogRaiser.SetManualTarget(int creatureUid, int targetCreatureUid)
	{
		if (SingletonAuto<BattleFacade>.instance.IsValidBattle())
		{
			BattleFacade.CreatureCacheData creatureCache = SingletonAuto<BattleFacade>.instance.GetCreatureCache(creatureUid);
			BattleFacade.CreatureCacheData creatureCache2 = SingletonAuto<BattleFacade>.instance.GetCreatureCache(targetCreatureUid);
			if (creatureCache.Uid != 0 && creatureCache2.Uid != 0)
			{
				SetManualTargetReq setManualTargetReq = new SetManualTargetReq();
				setManualTargetReq.CreatureUid = creatureUid;
				setManualTargetReq.TargetCreatureUid = targetCreatureUid;
				_dataProvider.GetSocketConnector().Request<SetManualTargetReq, BaseCreatureActionRes>(setManualTargetReq, delegate
				{
				});
			}
		}
	}

	void BattleFacade.IBattleLogRaiser.SetManualMove(int creatureUid, UnityEngine.Vector3 targetPosition)
	{
		if (SingletonAuto<BattleFacade>.instance.IsValidBattle())
		{
			SetManualMoveReq setManualMoveReq = new SetManualMoveReq();
			setManualMoveReq.CreatureUid = creatureUid;
			setManualMoveReq.Position = new NShared.NUtil.Vector3(targetPosition);
			_dataProvider.GetSocketConnector().Request<SetManualMoveReq, BaseCreatureActionRes>(setManualMoveReq, delegate
			{
			});
		}
	}

	void BattleFacade.IBattleLogRaiser.ReserveDungeonSkill(TeamType teamType, int skillSlotIndex)
	{
	}

	void StateBase.IListener.OnCampaignBegin()
	{
		_campaignBegin = true;
		if (PvpLoading.isValidInstance)
		{
			PvpLoading.instance.PlayVsWidgetAndClose();
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.CampaignManager != null)
		{
			int chapterIndex = _dataProvider.GetChapterIndex();
			int dungeonIndex = _dataProvider.GetDungeonIndex();
			SingletonAuto<EventTriggerManager>.instance.RaiseEvent(new BattleBegin
			{
				ChapterIndex = chapterIndex,
				DungeonIndex = dungeonIndex
			});
		}
	}

	void StateBase.IListener.OnCampaignReady()
	{
		SceneAutoBehaviour<BattleManagement>.instance.ReleaseMenuHide(BattleManagement.EMenuHideFlag.InitCampaign);
	}

	void StateBase.IListener.OnCampaignEnd()
	{
		SceneAutoBehaviour<BattleManagement>.instance.OnBattleEnd();
		IEvent triggerEvent = new BattleEnd
		{
			ChapterIndex = _dataProvider.GetChapterIndex(),
			DungeonIndex = _dataProvider.GetDungeonIndex()
		};
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent);
	}

	void StateBase.IListener.StartBattle()
	{
		VespaCoroutine.StartScene(coStartBattle());
	}

	private IEnumerator coStartBattle()
	{
		while (PvpLoading.isValidInstance)
		{
			yield return null;
		}
		_dataProvider.OnWaveMoveEnd();
		while (!_dataProvider.CanStartBattle())
		{
			yield return null;
		}
		if (BattleViewGenericSingleton<FieldMapManager2>.IsValidInstance)
		{
			BattleViewGenericSingleton<FieldMapManager2>.instance.PlayGuildFlag();
		}
		SendOnlineBattleStart();
	}

	bool StateBase.IListener.IsBattleEnd()
	{
		return _dataProvider.GetBattleCallback().IsBattleEnd;
	}

	int StateBase.IListener.GetChapterIndex()
	{
		return _dataProvider.GetChapterIndex();
	}

	int StateBase.IListener.GetDungeonIndex()
	{
		return _dataProvider.GetDungeonIndex();
	}

	NShared.DungeonDifficultyType StateBase.IListener.GetDifficulty()
	{
		return _dataProvider.GetDifficulty();
	}

	bool StateBase.IListener.IsScenarioDungeon()
	{
		return false;
	}

	int StateBase.IListener.GetCurrentWaveIndex()
	{
		return SingletonAuto<BattleFacade>.instance.GetBattleCache().WaveIndex;
	}

	bool StateBase.IListener.IsWaveReady(bool resetValue)
	{
		if (!BattleViewGenericSingleton<CreatureManager>.IsValidInstance)
		{
			return false;
		}
		if (!BattleViewGenericSingleton<CreatureManager>.instance.IsOnlineWaveReady)
		{
			return false;
		}
		if (resetValue)
		{
			BattleViewGenericSingleton<CreatureManager>.instance.IsOnlineWaveReady = false;
		}
		return true;
	}

	bool StateBase.IListener.IsWavePause(bool resetValue)
	{
		return false;
	}

	bool StateBase.IListener.IsWaveEnd(int waveIndex, bool resetValue)
	{
		if (!_dataProvider.GetBattleCallback().IsWaveEnd)
		{
			return false;
		}
		BattleFacade.BattleCacheData battleCache = SingletonAuto<BattleFacade>.instance.GetBattleCache();
		if (SingletonAuto<BattleFacade>.instance.IsValidBattle() && battleCache.WaveIndex == waveIndex && !battleCache.IsBattleEnded)
		{
			DateTime waveEndedTime = _dataProvider.GetBattleCallback().WaveEndedTime;
			if ((DateTime.UtcNow - waveEndedTime).TotalSeconds < 5.0)
			{
				return false;
			}
			VespaDebug.LogError("Waiting battle end expired!");
		}
		if (resetValue)
		{
			_dataProvider.GetBattleCallback().IsWaveEnd = false;
		}
		return true;
	}

	string StateBase.IListener.GetCurrentWaveBgm()
	{
		return string.Empty;
	}

	StateBase.SwitchCameraType[] StateBase.IListener.GetIgnoreSwitchCameraTypes()
	{
		return null;
	}

	BattleStartEffectData StateBase.IListener.GetBattleStartEffectData()
	{
		return _dataProvider.GetBattleStartEffectData();
	}

	bool StateBase.IListener.IsSetNextDungeon(bool resetValue)
	{
		bool isSetNextDungeon = _dataProvider.GetBattleCallback().IsSetNextDungeon;
		if (resetValue)
		{
			_dataProvider.GetBattleCallback().IsSetNextDungeon = false;
		}
		return isSetNextDungeon;
	}

	bool StateBase.IListener.IsContinuousBattle()
	{
		return _dataProvider.GetBattleCallback().IsContinuousBattle;
	}

	EnterDungeonData StateBase.IListener.GetEnterDungeonData()
	{
		return _dataProvider.GetEnterDungeon();
	}

	void StateBase.IListener.OnWaveResume(Action callback)
	{
		BattleWaveResumeReq data = new BattleWaveResumeReq();
		_dataProvider.GetSocketConnector().Request<BattleWaveResumeReq, BattleWaveResumeRes>(data, delegate
		{
			Vespa_System_Action_Extension_Method.fire(callback);
		});
	}

	bool StateBase.IListener.IsSetNextTeam(bool resetValue)
	{
		bool isSetNextTeam = _dataProvider.GetBattleCallback().IsSetNextTeam;
		if (resetValue)
		{
			_dataProvider.GetBattleCallback().IsSetNextTeam = false;
		}
		return isSetNextTeam;
	}

	TeamType StateBase.IListener.WaveWinner()
	{
		return _dataProvider.GetBattleCallback().Winner;
	}

	public void StopBattle()
	{
		_stopBattle = true;
		_dataProvider.GetSocketConnector().Close();
		SingletonAuto<BattleFacade>.instance.ForceStopBattle();
		_battleCancel = true;
	}
}
