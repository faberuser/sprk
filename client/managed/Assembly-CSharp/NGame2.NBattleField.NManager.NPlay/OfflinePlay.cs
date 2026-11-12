using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NBattleField.NManager.NCampaign;
using NGame2.NBattleField.NManager.NCampaign.NState;
using NGame2.NBattleThread.NThreadAction;
using NGame2.NBattleView;
using NGame2.NEventTrigger;
using NGame2.NScene.NLobbyScenePrerequisites;
using NGame2.NUI.NManager.NBattle;
using NGame2.NUI.NWindow.NBattle;
using NShared;
using NShared.NCampaign;
using NShared.NEvent;
using NShared.NUtil;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NBattleField.NManager.NPlay;

public class OfflinePlay : SceneAutoBehaviour<OfflinePlay>, IInspectorGuiDrawer, StateBase.IListener, IPlay, BattleFacade.IBattleLogRaiser
{
	public class ResponseData
	{
		public RewardItemInfo[] DropItems;

		public int DropGold;

		public bool CheckIntegrity;
	}

	public interface IDataProvider : NGame2.NBattleField.NManager.NPlay.IDataProvider
	{
		List<IBattleLogRuntime> CreateInitialBattleLog();

		int GetChapterIndex();

		int GetDungeonIndex();

		DungeonDifficultyType GetDifficulty();

		bool IsScenarioDungeon();

		int GetRequestDungeonIndex();

		ResponseData GetResponseData();

		int GetWorldMapEventIndex();

		void HandleBattleBegin();

		void HandleBattleEnd(TeamType winTeamType, BattleInstance battleInstance);

		List<List<BattleCreatureCreateInfo>> GetBattleCreatureCreateInfoListToWaveList();

		BattleInstanceExtraData GetExtraData();
	}

	protected CampaignStateUpdater _stateUpdater;

	protected IDataProvider _dataProvider;

	private Action _onReadyBattleScene;

	public long UserId => GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id;

	void IPlay.Setup(NGame2.NBattleField.NManager.NPlay.IDataProvider dataProvider, Action onReadyBattle)
	{
		_dataProvider = dataProvider as IDataProvider;
		_onReadyBattleScene = onReadyBattle;
		VespaCoroutine.Start(this, coSetup());
	}

	private IEnumerator coSetup()
	{
		yield return VespaCoroutine.Start(this, coSetupFieldImage());
		int chapterIndex = _dataProvider.GetChapterIndex();
		int dungeonIndex = _dataProvider.GetDungeonIndex();
		DungeonDifficultyType difficulty = _dataProvider.GetDifficulty();
		int requestDungeonIndex = _dataProvider.GetRequestDungeonIndex();
		bool isScenario = _dataProvider.IsScenarioDungeon();
		BattleInstanceExtraData extraData = _dataProvider.GetExtraData();
		SingletonAuto<BattleFacade>.CreateInstance();
		yield return null;
		EclipseOfflineModeUpdater.IFuncEclipse funcEclipse = null;
		if (_dataProvider is EclipseOfflineModeUpdater.IFuncEclipse)
		{
			funcEclipse = _dataProvider as EclipseOfflineModeUpdater.IFuncEclipse;
			SingletonAuto<BattleFacade>.instance.IsContinuousBattle = funcEclipse.GetIsContinousBattle();
		}
		int num = funcEclipse?.GetEclipseGameSpeedRatio() ?? 1;
		SingletonAuto<BattleFacade>.instance.CreateBattle(chapterIndex, dungeonIndex, (int)difficulty, isScenario, isOnlineMode: false, autoStart: false, extraData, num);
		SingletonAuto<BattleFacade>.instance.BattleLogRaiser = this;
		int worldMapEventIndex = _dataProvider.GetWorldMapEventIndex();
		EBattleMenu battleMenuType = _dataProvider.GetBattleMenuType();
		ResponseData responseData = _dataProvider.GetResponseData();
		RewardItemInfo[] rewardItems = responseData?.DropItems;
		int dropGold = responseData?.DropGold ?? 0;
		SceneAutoBehaviour<DropItemManager>.instance.Open(chapterIndex, dungeonIndex, difficulty, isScenario, rewardItems, dropGold);
		SceneAutoBehaviour<BattleManagement>.instance.OpenMenu(UserId, chapterIndex, dungeonIndex, requestDungeonIndex, isScenario, difficulty, battleMenuType, extraData, worldMapEventIndex, num);
		SceneAutoBehaviour<BattleManagement>.instance.SetMenuHide(BattleManagement.EMenuHideFlag.InitCampaign, skipAni: true);
		List<IBattleLogRuntime> battleLogList = _dataProvider.CreateInitialBattleLog();
		int monthFromMillennium = RequestEnterLobbyWrapper.MonthFromMillennium;
		int randomByMonth = RandomTable.GetRandomByMonth(monthFromMillennium, dungeonIndex);
		int[] projectileGroupIndexFromServer = GlobalAutoBehaviour<AccountManager>.instance.UserManager.GetProjectileGroupIndexFromServer(_dataProvider.GetBattleType());
		if (_dataProvider.GetBattleType() == BattleType.Eclipse)
		{
			if (_dataProvider is EclipseContext && funcEclipse != null)
			{
				SingletonAuto<BattleFacade>.instance.CreateEclipseUpdater(chapterIndex, dungeonIndex, difficulty, isScenario, battleLogList, randomByMonth, projectileGroupIndexFromServer, funcEclipse, _dataProvider);
			}
		}
		else if (_dataProvider.GetBattleType() == BattleType.KarmaRaid)
		{
			if (_dataProvider is PunishmentRaidContext)
			{
				int randomByMonth2 = RandomTable.GetRandomByMonth(RequestEnterLobbyWrapper.SecRandomSeed, monthFromMillennium);
				SingletonAuto<BattleFacade>.instance.CreateKarmaUpdater(chapterIndex, dungeonIndex, difficulty, isScenario, battleLogList, randomByMonth2, projectileGroupIndexFromServer, _dataProvider);
			}
		}
		else
		{
			SingletonAuto<BattleFacade>.instance.CreateCampaignUpdater(chapterIndex, dungeonIndex, difficulty, isScenario, battleLogList, randomByMonth, projectileGroupIndexFromServer);
			SingletonAuto<BattleFacade>.instance.SetEachWaveEachTeamBattleCreatureCreateInfoList(_dataProvider.GetBattleCreatureCreateInfoListToWaveList());
		}
		SingletonAuto<BattleFacade>.instance.StartBattle();
		_stateUpdater = new CampaignStateUpdater(this);
		_stateUpdater.SetState(new CampaignOpening(MakeBattleSceneReady));
		_dataProvider.OnEnterBattle();
	}

	private IEnumerator coSetupFieldImage()
	{
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(_dataProvider.GetChapterIndex(), _dataProvider.GetDungeonIndex());
		if (data != null)
		{
			bool isComplete = false;
			HashString battleField = data.BattleField;
			GuildFlagData guildFlagData = _dataProvider.GetGuildFlagData();
			BattleViewGenericSingleton<FieldMapManager2>.instance.Open(battleField, guildFlagData, delegate
			{
				isComplete = true;
			});
			while (!isComplete)
			{
				yield return null;
			}
		}
	}

	private void MakeBattleSceneReady()
	{
		Vespa_System_Action_Extension_Method.fire(_onReadyBattleScene);
		_onReadyBattleScene = null;
	}

	public void ChainUpdate()
	{
		if (_dataProvider != null)
		{
			_dataProvider.OnUpdate();
		}
		if (_stateUpdater != null)
		{
			_stateUpdater.Update();
		}
		SingletonAuto<BattleFacade>.instance.UpdateOffline(GlobalTimer.deltaTick);
	}

	void BattleFacade.IBattleLogRaiser.ReserveSkill(int creatureUid, int skillSlotIndex, int manualTargetUid, bool autoReserved)
	{
		SingletonAuto<BattleFacade>.instance.AddThreadCommonAction(delegate(BattleInstance battleInstance)
		{
			if (skillSlotIndex < 0)
			{
				battleInstance.BattleLogContainer.AddLog(new CreatureReserveOffSkillBattleLog
				{
					CreatureUid = creatureUid
				});
			}
			else
			{
				battleInstance.BattleLogContainer.AddLog(new CreatureReserveOnSkillBattleLog
				{
					CreatureUid = creatureUid,
					SkillSlotIndex = skillSlotIndex,
					ManualTargetUid = manualTargetUid,
					AutoReserved = autoReserved
				});
			}
		});
	}

	void BattleFacade.IBattleLogRaiser.FireNextSkill(int creatureUid)
	{
		SingletonAuto<BattleFacade>.instance.AddThreadCommonAction(delegate(BattleInstance battleInstance)
		{
			CreatureFireNextSkillBattleLog runtime = new CreatureFireNextSkillBattleLog
			{
				CreatureUid = creatureUid
			};
			battleInstance.BattleLogContainer.AddLog(runtime);
		});
	}

	void BattleFacade.IBattleLogRaiser.SetManualTarget(int creatureUid, int targetCreatureUid)
	{
		SingletonAuto<BattleFacade>.instance.AddThreadCommonAction(delegate(BattleInstance battleInstance)
		{
			CreatureManualTargetBattleLog runtime = new CreatureManualTargetBattleLog
			{
				CreatureUid = creatureUid,
				TargetCreatureUid = targetCreatureUid
			};
			battleInstance.BattleLogContainer.AddLog(runtime);
		});
	}

	void BattleFacade.IBattleLogRaiser.SetManualMove(int creatureUid, UnityEngine.Vector3 targetPosition)
	{
		SingletonAuto<BattleFacade>.instance.AddThreadCommonAction(delegate(BattleInstance battleInstance)
		{
			CreatureManualMoveBattleLog runtime = new CreatureManualMoveBattleLog
			{
				CreatureUid = creatureUid,
				Position = new NShared.NUtil.Vector3(targetPosition)
			};
			battleInstance.BattleLogContainer.AddLog(runtime);
		});
	}

	void BattleFacade.IBattleLogRaiser.ReserveDungeonSkill(TeamType teamType, int skillSlotIndex)
	{
		SingletonAuto<BattleFacade>.instance.AddThreadCommonAction(delegate(BattleInstance battleInstance)
		{
			battleInstance.BattleLogContainer.AddLog(new CreatureReserveOnDungeonSkillBattleLog
			{
				TeamType = teamType,
				SkillSlotIndex = skillSlotIndex
			});
		});
	}

	void StateBase.IListener.OnCampaignBegin()
	{
		if (PvpLoading.isValidInstance)
		{
			PvpLoading.instance.PlayVsWidgetAndClose();
		}
		if (GlobalAutoBehaviour<AccountManager>.instance.CampaignManager != null)
		{
			int chapterIndex = _dataProvider.GetChapterIndex();
			int dungeonIndex = _dataProvider.GetDungeonIndex();
			IEvent triggerEvent = ((!_dataProvider.IsScenarioDungeon()) ? ((BaseNodeEvent)new BattleBegin
			{
				ChapterIndex = chapterIndex,
				DungeonIndex = dungeonIndex
			}) : ((BaseNodeEvent)new ScenarioBattleBegin
			{
				ChapterIndex = chapterIndex,
				DungeonIndex = dungeonIndex
			}));
			SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent, delegate
			{
				_dataProvider.HandleBattleBegin();
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
		SingletonAuto<BattleFacade>.instance.ForceStopBattle();
		int chapterIndex = _dataProvider.GetChapterIndex();
		int dungeonIndex = _dataProvider.GetDungeonIndex();
		TeamType winner = SingletonAuto<BattleFacade>.instance.GetCampaignCache().Winner;
		BattleInstance battleInstance = SingletonAuto<BattleFacade>.instance.GetStoppedBattleInstance();
		IEvent triggerEvent = ((!_dataProvider.IsScenarioDungeon()) ? ((BaseNodeEvent)new BattleEnd
		{
			ChapterIndex = chapterIndex,
			DungeonIndex = dungeonIndex,
			Winner = winner
		}) : ((BaseNodeEvent)new ScenarioBattleEnd
		{
			ChapterIndex = chapterIndex,
			DungeonIndex = dungeonIndex,
			Winner = winner
		}));
		SingletonAuto<EventTriggerManager>.instance.RaiseEvent(triggerEvent, delegate
		{
			_dataProvider.HandleBattleEnd(winner, battleInstance);
		});
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
		SingletonAuto<BattleFacade>.instance.AddThreadSync(new CampaignUpdaterAction(delegate(CampaignUpdater campaignUpdater)
		{
			campaignUpdater.StartBattle();
		}));
	}

	bool StateBase.IListener.IsBattleEnd()
	{
		return SingletonAuto<BattleFacade>.instance.GetCampaignCache().IsBattleEnd;
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
		return _dataProvider.IsScenarioDungeon();
	}

	int StateBase.IListener.GetCurrentWaveIndex()
	{
		return SingletonAuto<BattleFacade>.instance.GetWaveIndex();
	}

	bool StateBase.IListener.IsWaveReady(bool resetValue)
	{
		bool campaignWaveReady = SingletonAuto<BattleFacade>.instance.CampaignWaveReady;
		if (resetValue)
		{
			SingletonAuto<BattleFacade>.instance.CampaignWaveReady = false;
		}
		return campaignWaveReady;
	}

	bool StateBase.IListener.IsWaveEnd(int waveIndex, bool resetValue)
	{
		bool campaignWaveEnd = SingletonAuto<BattleFacade>.instance.CampaignWaveEnd;
		if (resetValue)
		{
			SingletonAuto<BattleFacade>.instance.CampaignWaveEnd = false;
		}
		return campaignWaveEnd;
	}

	bool StateBase.IListener.IsWavePause(bool resetValue)
	{
		bool campaignWavePause = SingletonAuto<BattleFacade>.instance.CampaignWavePause;
		if (resetValue)
		{
			SingletonAuto<BattleFacade>.instance.CampaignWavePause = false;
		}
		return campaignWavePause;
	}

	string StateBase.IListener.GetCurrentWaveBgm()
	{
		BattleFacade.CampaignCacheData campaignCache = SingletonAuto<BattleFacade>.instance.GetCampaignCache();
		int chapterIndex = _dataProvider.GetChapterIndex();
		int dungeonIndex = _dataProvider.GetDungeonIndex();
		DungeonDifficultyType difficulty = _dataProvider.GetDifficulty();
		bool scenario = _dataProvider.IsScenarioDungeon();
		int waveIndex = campaignCache.WaveIndex;
		CampaignWaveData data = GameRoot.Instance.CampaignWaveDataContainer.GetData(chapterIndex, dungeonIndex, difficulty, scenario, waveIndex);
		if (data == null)
		{
			return string.Empty;
		}
		if (String_Extension_Method.IsNullOrEmpty(data.BgmCode))
		{
			return string.Empty;
		}
		return data.BgmCode;
	}

	StateBase.SwitchCameraType[] StateBase.IListener.GetIgnoreSwitchCameraTypes()
	{
		return null;
	}

	BattleStartEffectData StateBase.IListener.GetBattleStartEffectData()
	{
		return _dataProvider.GetBattleStartEffectData();
	}

	bool StateBase.IListener.IsSetNextDungeon(bool reset)
	{
		BattleType battleType = _dataProvider.GetBattleType();
		if (battleType == BattleType.Eclipse || battleType == BattleType.KarmaRaid)
		{
			bool campaignWavePause = SingletonAuto<BattleFacade>.instance.CampaignWavePause;
			if (reset)
			{
				SingletonAuto<BattleFacade>.instance.IsSetNextDungeon = false;
			}
			return campaignWavePause;
		}
		return false;
	}

	bool StateBase.IListener.IsSetNextTeam(bool reset)
	{
		if (_dataProvider == null)
		{
			return false;
		}
		if (_dataProvider.GetBattleType() == BattleType.Eclipse)
		{
			bool isSetNextTeam = SingletonAuto<BattleFacade>.instance.IsSetNextTeam;
			if (reset)
			{
				SingletonAuto<BattleFacade>.instance.IsSetNextTeam = false;
			}
			return isSetNextTeam;
		}
		List<List<BattleCreatureCreateInfo>> battleCreatureCreateInfoListToWaveList = _dataProvider.GetBattleCreatureCreateInfoListToWaveList();
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(battleCreatureCreateInfoListToWaveList) && battleCreatureCreateInfoListToWaveList.Count > 0)
		{
			return true;
		}
		return false;
	}

	bool StateBase.IListener.IsContinuousBattle()
	{
		if (_dataProvider.GetBattleType() == BattleType.Eclipse)
		{
			return SingletonAuto<BattleFacade>.instance.IsContinuousBattle;
		}
		return false;
	}

	EnterDungeonData StateBase.IListener.GetEnterDungeonData()
	{
		if (_dataProvider.GetBattleType() == BattleType.Eclipse)
		{
			return _dataProvider.GetEnterDungeon();
		}
		return null;
	}

	void StateBase.IListener.OnWaveResume(Action callback)
	{
		BattleType battleType = _dataProvider.GetBattleType();
		if (battleType == BattleType.Eclipse || battleType == BattleType.KarmaRaid)
		{
			SingletonAuto<BattleFacade>.instance.AddThreadSync(new CampaignUpdaterAction(delegate(CampaignUpdater campaignUpdater)
			{
				campaignUpdater.SetWavePauseFalse();
			}));
		}
		Vespa_System_Action_Extension_Method.fire(callback);
	}

	TeamType StateBase.IListener.WaveWinner()
	{
		return SingletonAuto<BattleFacade>.instance.GetCampaignCache().Winner;
	}
}
