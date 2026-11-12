using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleField.NManager.NPlay;
using NGame2.NBattleThread;
using NGame2.NBattleThread.NEventService;
using NGame2.NBattleThread.NThreadAction;
using NGame2.NBattleView;
using NGame2.NConfig;
using NGame2.NNodeTree;
using NGame2.NNodeTree.NUtil;
using NShared;
using NShared.NCampaign;
using NShared.NUtil;
using NVespa;
using NVespa.NSingleton;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NBattleField;

public class BattleFacade : SingletonAuto<BattleFacade>
{
	public interface IBattleLogRaiser
	{
		void ReserveSkill(int creatureUid, int skillSlotIndex, int manualTargetUid, bool autoReserved);

		void FireNextSkill(int creatureUid);

		void SetManualTarget(int creatureUid, int targetCreatureUid);

		void SetManualMove(int creatureUid, UnityEngine.Vector3 targetPosition);

		void ReserveDungeonSkill(TeamType teamType, int skillSlotIndex);
	}

	public class NullBattleLogRaiser : IBattleLogRaiser
	{
		void IBattleLogRaiser.ReserveSkill(int creatureUid, int skillSlotIndex, int manualTargetUid, bool autoReserved)
		{
		}

		void IBattleLogRaiser.FireNextSkill(int creatureUid)
		{
		}

		void IBattleLogRaiser.SetManualTarget(int creatureUid, int targetCreatureUid)
		{
		}

		void IBattleLogRaiser.SetManualMove(int creatureUid, UnityEngine.Vector3 targetPosition)
		{
		}

		void IBattleLogRaiser.ReserveDungeonSkill(TeamType teamType, int skillSlotIndex)
		{
		}
	}

	public class CampaignCacheData
	{
		public static CampaignCacheData NullCache = new CampaignCacheData();

		public bool IsBattleEnd;

		public int ChapterIndex;

		public int DungeonIndex;

		public int Difficulty;

		public int WaveIndex;

		public int TotalWaveIndex;

		public bool IsWaveReady;

		public bool IsWaveEnd;

		public bool IsWavePause;

		public bool Scenario;

		public TeamType Winner;
	}

	public class BattleCacheData
	{
		public static BattleCacheData NullCache = new BattleCacheData();

		public bool IsBattleStarted;

		public bool IsBattleEnded;

		public bool IsBattleEnabled;

		public int ChapterIndex;

		public int DungeonIndex;

		public int WaveIndex;

		public int TotalWaveIndex;

		public int MaxWaveIndex;

		public bool InfiniteBattleTime;

		public int MaxBattleTime;

		public int ElapsedBattleTimeMs;

		public int RemainedBattleTimeMs;

		public int RemainedBattleTimeMsIncludeExtraTime;

		public long GlobalCurrentTimeMs;

		public long MainCurrentTimeMs;

		public bool IsInSoloTime;

		public BattleArea BattleArea;

		public BattleType BattleType;
	}

	public class PlayerCacheData
	{
		public static PlayerCacheData NullCache = new PlayerCacheData();

		public static long MyAccountId;

		public static TeamType MyTeamType;

		public long Id;

		public TeamType TeamType = TeamType.None;

		public bool IsMe => MyAccountId == Id;

		public bool IsAlly => MyTeamType == TeamType;
	}

	public class CreatureCacheData
	{
		public static CreatureCacheData NullCache = new CreatureCacheData(0);

		public CreatureData CreatureData;

		public BattleCreatureCreateInfo CreateInfo;

		public CachedHeroInfo HeroInfo;

		public PlayerCacheData Player;

		public TeamType TeamType;

		public NShared.NUtil.Vector3 Position;

		public NShared.NUtil.Vector3 Direction;

		public bool IsDead;

		public bool IsResting;

		public long Hp;

		public long MaxHp;

		public int HpRatioM;

		public int Mp;

		public int MpRatioM;

		public long MaxMp;

		public int MoveSpeedMms;

		public int AttackSpeed;

		public int ManualTargetUid;

		public bool IsInSoloSkill;

		public int ReservedSkillIndex;

		public int ReservedSkillSlotIndex;

		public int ReservedSkillManualTargetUid;

		public bool AutoReservedSkill;

		public int UsingSkillIndex;

		public int UsingFiredSkillIndex;

		public int UsingSkillSlotIndex;

		public int[] CooltimeSkillIndices;

		public int[] SuppressedUsingCheckSkillIndices;

		public int[] DisabledAutoUseSkillIndices;

		public int[] ChangedSkillIndices;

		public long EnterTimeMs;

		public int EnterWaveIndex;

		public long TotalDamage;

		public long TotalDamaged;

		public long TotalHeal;

		public long TotalHealed;

		public long TotalThreatInUnitTime;

		public long TotalManaHeal;

		public long TotalManaRestored;

		public long TotalManaRestoredInUnitTime;

		public int AutoAiSkillIndex;

		public bool IsEquippedSoulWeapon;

		public bool IsUseableSoulWeaponSkill;

		public int SoulWeaponStackCount;

		public int SoulWeaponMaxStackCount;

		public int SoulWeaponStackRatioM;

		public SkillUseResult SoulWeaponCanUseSkillResult;

		public int SoulWeaponUsableCount;

		public bool IsReservedSoulWeaponSkill;

		public int GroupIndex;

		public bool HasDungeonSkill;

		public DungeonSkillStateInfo[] DungeonSkillStateInfos;

		public int ManaAddictLevel;

		public long CreatedTime;

		public int Uid { get; private set; }

		public CreatureCacheData(int uid)
		{
			Uid = uid;
		}

		public bool IsDeadOrResting()
		{
			if (!IsDead)
			{
				return IsResting;
			}
			return true;
		}

		public int GetChangedSkillndex(int fromSkillIndex)
		{
			int num = 0;
			while (ChangedSkillIndices != null && num < ChangedSkillIndices.Length / 2)
			{
				if (ChangedSkillIndices[num * 2] == fromSkillIndex)
				{
					return ChangedSkillIndices[num * 2 + 1];
				}
				num++;
			}
			return fromSkillIndex;
		}

		public DungeonSkillStateInfo GetDungeonSkillStateInfo(int slotIndex)
		{
			int num = 0;
			while (DungeonSkillStateInfos != null && num < DungeonSkillStateInfos.Length)
			{
				DungeonSkillStateInfo dungeonSkillStateInfo = DungeonSkillStateInfos[num];
				if (dungeonSkillStateInfo != null && dungeonSkillStateInfo.SkillSlotIndex == slotIndex)
				{
					return dungeonSkillStateInfo;
				}
				num++;
			}
			return null;
		}
	}

	public class EclipseMessageCacheData
	{
		public bool IsContinuousBattle;

		public bool IsContinuousBattleChanged;

		public EclipseStatus SendEclipseStatus;

		public bool SendEclipseStatusChanged;
	}

	public class EclipseResultCacheData
	{
		public int PlayTime;

		public long TotalDamage;

		public int ChapterIndex;

		public int DungeonIndex;

		public int WaveIndex;

		public int MaxWaveIndex;

		public int CreatureLevel;

		public int DeckIndex;

		public string BattleLogString;

		public string CreatureInfoString;
	}

	public class KarmaCacheData
	{
		public int ClearWaveCount;

		public int ClearLevel;

		public int CurrentWaveKillCount;

		public int TotalKillCount;
	}

	public abstract class BattleConditionBase
	{
		public abstract bool IsLastKill();
	}

	private static NullBattleLogRaiser _nullBattleLogRaiser = new NullBattleLogRaiser();

	public bool IsContinuousBattle;

	public bool IsSetNextDungeon;

	public bool IsSetNextTeam;

	public EclipseStatus SendedEclipseStatus;

	public EclipseResultCacheData EclipseResultData = new EclipseResultCacheData();

	public KarmaCacheData KarmaCache = new KarmaCacheData();

	private IBattleLogRaiser _battleLogRaiser;

	private ThreadInstance _threadInstance;

	private ThreadContext _threadContext;

	private MessageRouter _messageRouter;

	private IncreaseDic<int, CreatureCacheData> _creatureBackupDic = new IncreaseDic<int, CreatureCacheData>();

	public IBattleLogRaiser BattleLogRaiser
	{
		get
		{
			if (_battleLogRaiser == null)
			{
				return _nullBattleLogRaiser;
			}
			return _battleLogRaiser;
		}
		set
		{
			_battleLogRaiser = value;
		}
	}

	public bool IsOnlineMode { get; private set; }

	public BattleConditionBase BattleCondition { get; set; }

	public bool CampaignWaveReady { get; set; }

	public bool CampaignWaveEnd { get; set; }

	public bool CampaignWavePause { get; set; }

	public IRuntime NodeTreeRuntime { get; set; }

	public decimal NodeTreeFlowNumber { get; set; }

	public BattleFacade()
	{
		GameRoot.Instance.SnapshotEnabled = true;
		GameRoot.Instance.BattleLogFactory.SetSerializer(JsonMarshaler.EncodeJsonString);
		GameRoot.Instance.BattleLogFactory.SetDeserializer(JsonMarshaler.Parse);
		_threadContext = new ThreadContext();
		_messageRouter = new MessageRouter();
		SingletonAuto<ClientDebugEvent>.CreateInstance();
		SingletonAuto<ClientDebugNetwork>.CreateInstance();
		SingletonAuto<ClientDebugTutorial>.CreateInstance();
	}

	public void Reset()
	{
		BattleCondition = null;
		ResetBattle();
	}

	private void ResetBattle()
	{
		NodeTreeStop();
		CampaignWaveReady = false;
		CampaignWaveEnd = false;
		if (_threadInstance != null)
		{
			_threadInstance.Close();
			_threadInstance = null;
		}
		_threadContext = new ThreadContext();
		_messageRouter = new MessageRouter();
		_battleLogRaiser = null;
		_creatureBackupDic.Clear();
	}

	public void CreateBattle(int chapterIndex, int dungeonIndex, int difficulty, bool isScenario, bool isOnlineMode, bool autoStart, BattleInstanceExtraData extraData, int onlineGameSpeedRatio = 1)
	{
		IsOnlineMode = isOnlineMode;
		ResetBattle();
		PrepareBeforeStartThread();
		_threadInstance = new ThreadInstance();
		_threadInstance.Open(chapterIndex, dungeonIndex, difficulty, isScenario, isOnlineMode, extraData, onlineGameSpeedRatio);
		if (autoStart)
		{
			_threadInstance.StartThread();
		}
	}

	public void StartBattle()
	{
		if (_threadInstance != null)
		{
			_threadInstance.StartThread();
		}
	}

	public void NodeTreeStart(TreeData nodeTree)
	{
		if (BattleViewGenericSingleton<CreatureManager>.IsValidInstance)
		{
			NodeTreeStop();
			BuildChildRootRuntime buildChildRootRuntime = default(BuildChildRootRuntime);
			buildChildRootRuntime.Open(nodeTree);
			buildChildRootRuntime.AddFunc(VariableKeyword.FlowNumber, () => NodeTreeFlowNumber);
			NodeTreeFlowNumber = 0m;
			NodeTreeRuntime = buildChildRootRuntime.Build(BattleViewGenericSingleton<CreatureManager>.instance, null, null, 0);
			if (NodeTreeRuntime != null)
			{
				SceneAutoBehaviour<RuntimeGlobalManager>.instance.AddRuntime(NodeTreeRuntime.CaptureSelf(), immediatelyStart: true);
			}
		}
	}

	public void NodeTreeStop()
	{
		if (NodeTreeRuntime != null)
		{
			NodeTreeRuntime.RequestStop(ERequestStop.Normal);
			NodeTreeRuntime.ReleaseSelf();
			NodeTreeRuntime = null;
		}
	}

	public bool NodeTreePlaying()
	{
		if (NodeTreeRuntime == null)
		{
			return false;
		}
		return NodeTreeRuntime.IsPlaying;
	}

	private void PrepareBeforeStartThread()
	{
		PlayerCacheData.MyAccountId = 0L;
		PlayerCacheData.MyTeamType = TeamType.None;
		if (GlobalAutoBehaviour<AccountManager>.isValidInstance)
		{
			PlayerCacheData.MyAccountId = GlobalAutoBehaviour<AccountManager>.instance.UserManager.Id;
		}
	}

	public void UpdateOnline()
	{
		if (_threadInstance != null)
		{
			UpdateCommon();
		}
	}

	public void UpdateOffline(int elapsedTick)
	{
		if (_threadInstance != null)
		{
			_threadInstance.AdvanceOfflineTime(elapsedTick);
			UpdateCommon();
		}
	}

	private void UpdateCommon()
	{
		_threadInstance.ThreadActionSender.Flush();
		while (true)
		{
			ThreadContext threadContext = _threadInstance.DequeueThreadContext();
			if (threadContext == null)
			{
				break;
			}
			_threadContext = threadContext;
			for (int i = 0; i < _creatureBackupDic.Count; i++)
			{
				KeyValuePair<int, CreatureCacheData> keyValuePairByIndex = _creatureBackupDic.GetKeyValuePairByIndex(i);
				_threadContext.CreatureDic.AddAndOverride(keyValuePairByIndex.Key, keyValuePairByIndex.Value);
			}
			while (_threadContext.BattleCallbackQueue.Count > 0)
			{
				BaseBattleCallback callback = _threadContext.BattleCallbackQueue.Dequeue();
				_messageRouter.OnBattleCallback(callback);
			}
			if (_threadContext.CampaignCache != null)
			{
				if (_threadContext.CampaignCache.IsWaveReady)
				{
					CampaignWaveReady = true;
				}
				if (_threadContext.CampaignCache.IsWaveEnd)
				{
					CampaignWaveEnd = true;
				}
				if (_threadContext.CampaignCache.IsWavePause)
				{
					CampaignWavePause = true;
				}
			}
			if (_threadContext.EclipseMessageCache != null)
			{
				if (_threadContext.EclipseMessageCache.IsContinuousBattleChanged)
				{
					IsContinuousBattle = _threadContext.EclipseMessageCache.IsContinuousBattle;
				}
				if (_threadContext.EclipseMessageCache.SendEclipseStatusChanged)
				{
					EclipseStatus eclipseStatus = _threadContext.EclipseMessageCache.SendEclipseStatus;
					EclipseStatus sendedEclipseStatus = SendedEclipseStatus;
					if (sendedEclipseStatus != EclipseStatus.None)
					{
						eclipseStatus = EclipseOfflineModeUpdater.GetHighPriorityStatus(sendedEclipseStatus, eclipseStatus);
					}
					if (sendedEclipseStatus != eclipseStatus)
					{
						SendedEclipseStatus = eclipseStatus;
						if (_threadContext.EclipseResultCache != null)
						{
							EclipseResultData.PlayTime = _threadContext.EclipseResultCache.PlayTime;
							EclipseResultData.TotalDamage = _threadContext.EclipseResultCache.TotalDamage;
							EclipseResultData.ChapterIndex = _threadContext.EclipseResultCache.ChapterIndex;
							EclipseResultData.DungeonIndex = _threadContext.EclipseResultCache.DungeonIndex;
							EclipseResultData.WaveIndex = _threadContext.EclipseResultCache.WaveIndex;
							EclipseResultData.MaxWaveIndex = _threadContext.EclipseResultCache.MaxWaveIndex;
							EclipseResultData.CreatureLevel = _threadContext.EclipseResultCache.CreatureLevel;
							EclipseResultData.DeckIndex = _threadContext.EclipseResultCache.DeckIndex;
							EclipseResultData.BattleLogString = _threadContext.EclipseResultCache.BattleLogString;
							EclipseResultData.CreatureInfoString = _threadContext.EclipseResultCache.CreatureInfoString;
						}
					}
				}
			}
			if (_threadContext.KarmaCache != null)
			{
				KarmaCache.ClearWaveCount = _threadContext.KarmaCache.ClearWaveCount;
				KarmaCache.ClearLevel = _threadContext.KarmaCache.ClearLevel;
				KarmaCache.CurrentWaveKillCount = _threadContext.KarmaCache.CurrentWaveKillCount;
				KarmaCache.TotalKillCount = _threadContext.KarmaCache.TotalKillCount;
			}
		}
	}

	public void AddThreadSync(IThreadAction threadAction)
	{
		if (_threadInstance != null)
		{
			threadAction.IsWaiting = true;
			_threadInstance.ThreadActionSender.Enqueue(threadAction);
		}
	}

	public void AddThreadCommonAction(Action<BattleInstance> action)
	{
		AddThreadSync(new CommonAction(action));
	}

	public bool IsValidBattle()
	{
		if (_threadInstance == null)
		{
			return false;
		}
		return _threadInstance.ISVALID();
	}

	public void CreateEclipseUpdater(int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty, bool isScenario, List<IBattleLogRuntime> battleLogList, int randomSeedByMonth, int[] projectileGroupIndicesFromServer, EclipseOfflineModeUpdater.IFuncEclipse FuncEclipse, IDataProvider dataProvider)
	{
		if (_threadInstance != null && dataProvider != null && dataProvider.GetBattleType() == BattleType.Eclipse)
		{
			_threadInstance.CreateEclipseUpdate(chapterIndex, dungeonIndex, difficulty, isScenario, battleLogList, randomSeedByMonth, projectileGroupIndicesFromServer, FuncEclipse, dataProvider);
		}
	}

	public void CreateKarmaUpdater(int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty, bool isScenario, List<IBattleLogRuntime> battleLogList, int randomSeedByMonth, int[] projectileGroupIndicesFromServer, IDataProvider dataProvider)
	{
		if (_threadInstance != null && dataProvider != null && dataProvider.GetBattleType() == BattleType.KarmaRaid)
		{
			_threadInstance.CreateKarmaUpdate(chapterIndex, dungeonIndex, difficulty, isScenario, battleLogList, randomSeedByMonth, projectileGroupIndicesFromServer, dataProvider);
		}
	}

	public void CreateCampaignUpdater(int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty, bool isScenario, List<IBattleLogRuntime> battleLogList, int randomSeedByMonth, int[] projectileGroupIndicesFromServer)
	{
		if (_threadInstance != null)
		{
			_threadInstance.CreateCampaignUpdate(chapterIndex, dungeonIndex, difficulty, isScenario, battleLogList, randomSeedByMonth, projectileGroupIndicesFromServer);
		}
	}

	public CampaignCacheData GetCampaignCache()
	{
		if (_threadContext.CampaignCache != null)
		{
			return _threadContext.CampaignCache;
		}
		return CampaignCacheData.NullCache;
	}

	public BattleCacheData GetBattleCache()
	{
		return _threadContext.BattleCache;
	}

	public void SetServerTimeMs(long timeMs)
	{
		if (SprkConquest.FilterClock(ref timeMs))
		{
			long timeMs2 = timeMs;
			AddThreadCommonAction(delegate(BattleInstance battleInstance)
			{
				battleInstance.SetServerTimeMs(timeMs2);
			});
		}
	}

	public UnityEngine.Vector3 ValidateBattlePos(int groupIndex, UnityEngine.Vector3 position)
	{
		BattleCacheData battleCache = GetBattleCache();
		if (!IsValidBattle() || battleCache.BattleArea == null)
		{
			return position;
		}
		NShared.NUtil.Vector3 position2 = new NShared.NUtil.Vector3(position);
		battleCache.BattleArea.ValidateByGroupIndex(ref position2, groupIndex);
		return position2;
	}

	public bool IsActiveSkill(FireSkill callback)
	{
		if (callback == null)
		{
			return false;
		}
		CreatureCacheData creatureCache = GetCreatureCache(callback.CreatureUid);
		if (creatureCache == null)
		{
			return false;
		}
		if (creatureCache.CreatureData == null)
		{
			return false;
		}
		return creatureCache.CreatureData.IsActiveSkill(callback.FiredSkillId);
	}

	public int GetCreatureCount()
	{
		return _threadContext.CreatureDic.Count;
	}

	public int GetTeamKillMeter(TeamType teamType)
	{
		return _threadContext.KillTeamMeterDic.GETVALUE(teamType, 0);
	}

	public int GetCurrentWaveKillMeter(TeamType teamType)
	{
		return _threadContext.CurrentWaveTeamKillMeterDic.GETVALUE(teamType, 0);
	}

	public int GetTotalKillTeamMeter(TeamType teamType)
	{
		return _threadContext.TotalKillTeamMeterDic.GETVALUE(teamType, 0);
	}

	public PlayerCacheData GetPlayerCache(long id)
	{
		return _threadContext.PlayerDic.GETVALUE(id, PlayerCacheData.NullCache);
	}

	public CreatureCacheData[] GetAllCreatureCache()
	{
		return _threadContext.CreatureDic.GetAllValues();
	}

	public CreatureCacheData GetCreatureCacheByIndex(int index)
	{
		if (!_threadContext.CreatureDic.IsValidIndex(index))
		{
			return CreatureCacheData.NullCache;
		}
		return _threadContext.CreatureDic.GetValueByIndex(index);
	}

	public CreatureCacheData GetCreatureCache(int creatureUid)
	{
		return _threadContext.CreatureDic.GETVALUE(creatureUid, CreatureCacheData.NullCache);
	}

	public int GetAliveCreatureCountOfTeam(TeamType teamType, bool includeResting = false)
	{
		int num = 0;
		for (int i = 0; i < _threadContext.CreatureDic.Count; i++)
		{
			CreatureCacheData valueByIndex = _threadContext.CreatureDic.GetValueByIndex(i);
			if (valueByIndex.TeamType == teamType && !valueByIndex.IsDead && (includeResting || !valueByIndex.IsResting))
			{
				num++;
			}
		}
		return num;
	}

	public int GetAliveCreatureCountOfTeamAndGroup(TeamType teamType, int groupIndex, bool includeResting = false)
	{
		int num = 0;
		for (int i = 0; i < GetCreatureCount(); i++)
		{
			CreatureCacheData creatureCacheByIndex = GetCreatureCacheByIndex(i);
			if (creatureCacheByIndex.TeamType == teamType && creatureCacheByIndex.GroupIndex == groupIndex && !creatureCacheByIndex.IsDead && (includeResting || !creatureCacheByIndex.IsResting))
			{
				num++;
			}
		}
		return num;
	}

	public int GetWaveIndex()
	{
		return _threadContext.BattleCache.WaveIndex;
	}

	public int GetTotalWaveIndex()
	{
		return _threadContext.BattleCache.TotalWaveIndex;
	}

	public BattleInstance GetBattleInstance()
	{
		if (_threadInstance == null)
		{
			return null;
		}
		return _threadInstance._battleInstance;
	}

	public void ForceStopBattle()
	{
		if (_threadInstance != null)
		{
			_threadInstance.StopBattleThread();
		}
	}

	public BattleInstance GetStoppedBattleInstance()
	{
		if (_threadInstance == null)
		{
			return null;
		}
		return _threadInstance.GetStopedBattleInstance();
	}

	public void AddReceivedLog(BaseBattleLog[] logs)
	{
		if (logs != null && logs.Length != 0)
		{
			AddThreadCommonAction(delegate(BattleInstance battleInstance)
			{
				battleInstance.BattleLogContainer.AddReceivedLog(logs);
			});
		}
	}

	public void FireSkillBattleLog(int creatureUid, int skillSlotIndex)
	{
		AddThreadCommonAction(delegate(BattleInstance battleInstance)
		{
			CreatureFireSkillBattleLog runtime = new CreatureFireSkillBattleLog
			{
				CreatureUid = creatureUid,
				SkillSlotIndex = skillSlotIndex
			};
			battleInstance.BattleLogContainer.AddLog(runtime);
		});
	}

	public void FireEventSkillBattleLog(int creatureUid, int skillIndex, int manualTargetUid, Action onFire = null)
	{
		Executer globalExecuter = SingletonUpdater<Executer>.instance;
		AddThreadCommonAction(delegate(BattleInstance battleInstance)
		{
			CreatureFireEventSkillBattleLog runtime = new CreatureFireEventSkillBattleLog
			{
				CreatureUid = creatureUid,
				SkillIndex = skillIndex,
				ManualTargetUid = manualTargetUid
			};
			battleInstance.BattleLogContainer.AddLog(runtime);
			globalExecuter.NextRun(this, delegate
			{
				Vespa_System_Action_Extension_Method.fire(onFire);
			});
		});
	}

	public void EventRemoveCreatureFromBattle(int creatureUid)
	{
		if (_threadContext == null)
		{
			return;
		}
		CreatureCacheData vALUE = _threadContext.CreatureDic.GETVALUE(creatureUid, null);
		if (vALUE != null)
		{
			_creatureBackupDic.AddAndOverride(creatureUid, vALUE);
			AddThreadCommonAction(delegate(BattleInstance battleInstance)
			{
				battleInstance.CreatureContainer.Destroy(creatureUid, raiseEvent: false);
			});
		}
	}

	public void CreateBattleCreature(TeamType teamType, BattleCreatureCreateInfo createInfo, int creatureUid, Action<int> onCreate)
	{
		Executer globalExecuter = SingletonUpdater<Executer>.instance;
		AddThreadCommonAction(delegate(BattleInstance battleInstance)
		{
			using (new SnapshotWritableBlock())
			{
				long playerId = battleInstance.PlayerContainer.GetPlayerByTeam(teamType)?.Id ?? 0;
				creatureUid = battleInstance.CreatureContainer.Create(playerId, createInfo, creatureUid)?.Uid ?? 0;
			}
			globalExecuter.NextRun(this, delegate
			{
				Vespa_System_Action_Extension_Method.fire(onCreate, creatureUid);
			});
		});
	}

	public void DestroyBattleCreature(int creatureUid, Action<bool> onDestroy)
	{
		Executer globalExecuter = SingletonUpdater<Executer>.instance;
		AddThreadCommonAction(delegate(BattleInstance battleInstance)
		{
			bool result;
			using (new SnapshotWritableBlock())
			{
				result = battleInstance.CreatureContainer.Destroy(creatureUid, raiseEvent: true);
			}
			globalExecuter.NextRun(this, delegate
			{
				Vespa_System_Action_Extension_Method.fire(onDestroy, result);
			});
		});
	}

	public void CreatureSetPosition(int creatureUid, UnityEngine.Vector3 position, Action onComplete)
	{
		Executer globalExecuter = SingletonUpdater<Executer>.instance;
		AddThreadCommonAction(delegate(BattleInstance battleInstance)
		{
			using (new SnapshotWritableBlock())
			{
				battleInstance.CreatureContainer.GetCreature(creatureUid)?.TransformController.SetPosition(new NShared.NUtil.Vector3(position), raiseEvent: false, SetPosition.SetPositionMethodType.None, 0L);
			}
			globalExecuter.NextRun(this, delegate
			{
				Vespa_System_Action_Extension_Method.fire(onComplete);
			});
		});
	}

	public void CreatureMoveToPosition(int creatureUid, UnityEngine.Vector3 position, float approachDistance, Action onMove)
	{
		Executer globalExecuter = SingletonUpdater<Executer>.instance;
		AddThreadCommonAction(delegate(BattleInstance battleInstance)
		{
			using (new SnapshotWritableBlock())
			{
				battleInstance.CreatureContainer.GetCreature(creatureUid)?.TransformController.MoveToPosition(new NShared.NUtil.Vector3(position), (int)(approachDistance * 1000f), 0, 0L, isManualMove: false);
			}
			globalExecuter.NextRun(this, delegate
			{
				Vespa_System_Action_Extension_Method.fire(onMove);
			});
		});
	}

	public decimal CreatureCustomStatValue(long creatureUid, StatType statType)
	{
		CreatureCacheData vALUE = _threadContext.CreatureDic.GETVALUE((int)creatureUid, CreatureCacheData.NullCache);
		return statType switch
		{
			StatType.Hp => vALUE.Hp, 
			StatType.HpRatioM => vALUE.HpRatioM, 
			StatType.Mp => vALUE.Mp, 
			StatType.MpRatioM => vALUE.MpRatioM, 
			_ => 0m, 
		};
	}

	public CreatureCacheData GetEnvironmentCreatureCache(TeamType teamType)
	{
		return _threadContext.EnvironmentCreatureDic.GETVALUE(teamType, CreatureCacheData.NullCache);
	}

	public bool IsBattleEnabled()
	{
		if (!IsValidBattle())
		{
			return false;
		}
		return GetBattleCache()?.IsBattleEnabled ?? false;
	}

	public void SetEachWaveEachTeamBattleCreatureCreateInfoList(List<List<BattleCreatureCreateInfo>> list)
	{
		if (list == null)
		{
			return;
		}
		AddThreadSync(new CampaignUpdaterAction(delegate(CampaignUpdater campaignUpdater)
		{
			List<BattleCreatureCreateInfo> list2 = null;
			for (int i = 0; i < list.Count; i++)
			{
				list2 = list[i];
				if (list2 != null && list2.Count > 0)
				{
					campaignUpdater.AddEachWaveEachTeamBattleCreatureCreateInfo(list2);
				}
			}
		}));
	}

	public void ChangeBattleField(HashString fieldName)
	{
		BattleViewGenericSingleton<FieldMapManager2>.instance.Open(fieldName, null, null);
	}
}
