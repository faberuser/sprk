using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NBattleContext;
using NGame2.NBattleField.NManager.NCampaign;
using NGame2.NBattleField.NManager.NCampaign.NState;
using NGame2.NBattleView;
using NGame2.NUI.NManager.NBattle;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NBattleField.NManager.NPlay;

public class RePlay : SceneAutoBehaviour<RePlay>, IPlay, StateBase.IListener
{
	public interface IDataProvider : NGame2.NBattleField.NManager.NPlay.IDataProvider
	{
		int GetChapterIndex();

		int GetDungeonIndex();

		DungeonDifficultyType GetDifficulty();

		BaseBattleLog[] GetBattleLogs();

		void HandleReplayEnd();
	}

	protected IDataProvider _dataProvider;

	private CampaignStateUpdater _stateUpdater;

	private Action _onReadyBattleScene;

	private BaseBattleLog[] _delayedBattleLogs;

	private StateBase.SwitchCameraType[] _ignoreSwitchCameraTyps;

	void IPlay.Setup(NGame2.NBattleField.NManager.NPlay.IDataProvider dataProvider, Action onReadyBattle)
	{
		_dataProvider = dataProvider as IDataProvider;
		_onReadyBattleScene = onReadyBattle;
		_delayedBattleLogs = null;
		InitIgnoreSwitchCamTypes();
		VespaCoroutine.Start(this, coSetup());
	}

	private void InitIgnoreSwitchCamTypes()
	{
		BattleType battleType = _dataProvider.GetBattleType();
		if (battleType != BattleType.Arena && battleType != BattleType.ArenaBanPick)
		{
			_ignoreSwitchCameraTyps = null;
		}
		else
		{
			_ignoreSwitchCameraTyps = new StateBase.SwitchCameraType[1];
		}
	}

	private IEnumerator coSetup()
	{
		yield return VespaCoroutine.Start(this, coSetupFieldImage());
		SingletonAuto<BattleFacade>.CreateInstance();
		yield return VespaCoroutine.Start(this, coSetupBattleManager());
	}

	private IEnumerator coSetupBattleManager()
	{
		yield return null;
		int chapterIndex = _dataProvider.GetChapterIndex();
		int dungeonIndex = _dataProvider.GetDungeonIndex();
		DungeonDifficultyType difficulty = _dataProvider.GetDifficulty();
		BaseBattleLog[] battleLogs = _dataProvider.GetBattleLogs();
		EBattleMenu replayBattleMenu = GameRoot.Instance.BattleDefineDataContainer.GetReplayBattleMenu();
		List<BaseBattleLog> list = new List<BaseBattleLog>();
		List<BaseBattleLog> list2 = new List<BaseBattleLog>();
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(battleLogs); i++)
		{
			BaseBattleLog baseBattleLog = battleLogs[i];
			if (baseBattleLog != null && baseBattleLog.Type == BattleLogType.PlayerEnter && list2.Count == 0)
			{
				list.Add(baseBattleLog);
			}
			else
			{
				list2.Add(baseBattleLog);
			}
		}
		_delayedBattleLogs = list2.ToArray();
		SingletonAuto<BattleFacade>.instance.CreateBattle(chapterIndex, dungeonIndex, (int)difficulty, isScenario: false, isOnlineMode: false, autoStart: false, null);
		SingletonAuto<BattleFacade>.instance.AddReceivedLog(list.ToArray());
		SingletonAuto<BattleFacade>.instance.StartBattle();
		SceneAutoBehaviour<BattleManagement>.instance.OpenMenu(0L, chapterIndex, dungeonIndex, dungeonIndex, isScenario: false, difficulty, replayBattleMenu, null);
		SceneAutoBehaviour<BattleManagement>.instance.SetMenuHide(BattleManagement.EMenuHideFlag.InitCampaign, skipAni: true);
		if (_dataProvider.GetBattleType() == BattleType.Arena)
		{
			Game3DRoot.instance.GameCamera.View = SceneAutoBehaviour<CameraManager2>.instance;
			SceneAutoBehaviour<CameraManager2>.instance.SwitchCam_Battle(99);
		}
		_stateUpdater = new CampaignStateUpdater(this);
		_stateUpdater.SetState(new CampaignOpening(_onReadyBattleScene));
		_dataProvider.OnEnterBattle();
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

	public void ChainUpdate()
	{
		if (_stateUpdater != null)
		{
			_stateUpdater.Update();
		}
		SingletonAuto<BattleFacade>.instance.UpdateOffline(GlobalTimer.deltaTick);
	}

	void StateBase.IListener.OnCampaignBegin()
	{
	}

	void StateBase.IListener.OnCampaignReady()
	{
	}

	void StateBase.IListener.OnCampaignEnd()
	{
		_dataProvider.HandleReplayEnd();
	}

	void StateBase.IListener.StartBattle()
	{
		VespaCoroutine.StartScene(coStartBattle());
	}

	private IEnumerator coStartBattle()
	{
		_dataProvider.OnWaveMoveEnd();
		while (!_dataProvider.CanStartBattle())
		{
			yield return null;
		}
		if (BattleViewGenericSingleton<FieldMapManager2>.IsValidInstance)
		{
			BattleViewGenericSingleton<FieldMapManager2>.instance.PlayGuildFlag();
		}
		SingletonAuto<BattleFacade>.instance.AddReceivedLog(_delayedBattleLogs);
	}

	bool StateBase.IListener.IsBattleEnd()
	{
		return SingletonAuto<BattleFacade>.instance.GetBattleCache().IsBattleEnded;
	}

	int StateBase.IListener.GetChapterIndex()
	{
		return _dataProvider.GetChapterIndex();
	}

	int StateBase.IListener.GetDungeonIndex()
	{
		return _dataProvider.GetDungeonIndex();
	}

	DungeonDifficultyType StateBase.IListener.GetDifficulty()
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
		return true;
	}

	bool StateBase.IListener.IsWaveEnd(int waveIndex, bool resetValue)
	{
		BattleFacade.BattleCacheData battleCache = SingletonAuto<BattleFacade>.instance.GetBattleCache();
		if (battleCache == null)
		{
			return false;
		}
		if (battleCache.WaveIndex != waveIndex)
		{
			return true;
		}
		if (battleCache.WaveIndex == battleCache.TotalWaveIndex && battleCache.IsBattleEnded)
		{
			return true;
		}
		return false;
	}

	StateBase.SwitchCameraType[] StateBase.IListener.GetIgnoreSwitchCameraTypes()
	{
		return _ignoreSwitchCameraTyps;
	}

	string StateBase.IListener.GetCurrentWaveBgm()
	{
		return string.Empty;
	}

	BattleStartEffectData StateBase.IListener.GetBattleStartEffectData()
	{
		return _dataProvider.GetBattleStartEffectData();
	}

	bool StateBase.IListener.IsSetNextDungeon(bool reset)
	{
		return false;
	}

	bool StateBase.IListener.IsSetNextTeam(bool reset)
	{
		return false;
	}

	bool StateBase.IListener.IsContinuousBattle()
	{
		return false;
	}

	EnterDungeonData StateBase.IListener.GetEnterDungeonData()
	{
		return null;
	}

	void StateBase.IListener.OnWaveResume(Action callback)
	{
		Vespa_System_Action_Extension_Method.fire(callback);
	}

	TeamType StateBase.IListener.WaveWinner()
	{
		return SingletonAuto<BattleFacade>.instance.GetCampaignCache().Winner;
	}

	bool StateBase.IListener.IsWavePause(bool resetValue)
	{
		return false;
	}
}
