using System;
using System.Collections.Generic;
using System.Text;
using NShared.NUtil;
using NVespa;

namespace NShared;

public class BattleCreature : SnapshotObject
{
	public const string DefaultAiScriptName = "DefaultAi";

	public const int DieDurationMsToDisabled = 5000;

	private readonly SnapshotObjectMember<int> _uidHolder;

	private readonly SnapshotObjectMember<SkillController> _skillControllerHolder;

	private readonly SnapshotObjectMember<OperationController> _operationControllerHolder;

	private readonly SnapshotObjectMember<AiController> _aiControllerHolder;

	private readonly SnapshotObjectMember<StateController> _stateControllerHolder;

	private readonly SnapshotObjectMember<StatController> _statControllerHolder;

	private readonly SnapshotObjectMember<TeamController> _teamControllerHolder;

	private readonly SnapshotObjectMember<TransformController> _transformControllerHolder;

	private readonly SnapshotObjectMember<RandomTable> _randomTableHolder;

	private readonly SnapshotObjectMember<BattleCreatureRestingInfo> _restingInfoHolder;

	private readonly SnapshotObjectMember<int> _summonerUidHolder;

	private readonly SnapshotObjectMember<long> _lastRageTimeMsHolder;

	private readonly SnapshotObjectMember<long> _diedTimeMsHolder;

	private readonly SnapshotObjectMember<BattleCreatureData> _creatureDataHolder;

	private readonly SnapshotObjectMember<SoulWeaponSkillController> _soulWeaponSkillControllerHolder;

	private readonly SnapshotObjectMember<DungeonSkillController> _DungeonSkillControllerHolder;

	private BattleCreatureStockData _stockData;

	public BattleInstance BattleInstance { get; private set; }

	public CreatureData CreatureData => _stockData.CreatureData;

	public BattleCreatureCreateInfo CreateInfo => _stockData.CreateInfo;

	public ResultCreatureInfo InitialInfo => _stockData.InitialInfo;

	public long PlayerId => _stockData.PlayerId;

	public int CreatureIndex => CreatureData.Index;

	public int Uid => _uidHolder.Get();

	public SkillController SkillController => _skillControllerHolder.Get();

	public OperationController OperationController => _operationControllerHolder.Get();

	public AiController BehaviorController => _aiControllerHolder.Get();

	public StateController StateController => _stateControllerHolder.Get();

	public StatController StatController => _statControllerHolder.Get();

	public TeamController TeamController => _teamControllerHolder.Get();

	public TransformController TransformController => _transformControllerHolder.Get();

	public int SummonerUid => _summonerUidHolder.Get();

	public BattleCreatureRestingInfo RestingInfo => _restingInfoHolder.Get();

	public bool IsResting => RestingInfo != null;

	public BattleCreatureData BattleCreatureData => _creatureDataHolder.Get();

	public SoulWeaponSkillController SoulWeaponSkillController => _soulWeaponSkillControllerHolder.Get();

	public int GroupIndex
	{
		get
		{
			if (_stockData.CreateInfo == null)
			{
				return 0;
			}
			return _stockData.CreateInfo.GroupIndex;
		}
	}

	public DungeonSkillController DungeonSkillController => _DungeonSkillControllerHolder.Get();

	public long CreatedTime => _stockData.CreatedTime;

	public BattleMetaDataContainer BattleMetaDataContainer { get; private set; }

	public ManaHistoryTracker ManaHistoryTracker { get; private set; }

	public ThreatHistoryTracker ThreatHistoryTracker { get; private set; }

	public int AggroToEnemy { get; set; }

	public BattleCreature()
	{
		_uidHolder = new SnapshotObjectMember<int>(this, 0);
		_randomTableHolder = new SnapshotObjectMember<RandomTable>(this);
		_skillControllerHolder = new SnapshotObjectMember<SkillController>(this);
		_operationControllerHolder = new SnapshotObjectMember<OperationController>(this);
		_aiControllerHolder = new SnapshotObjectMember<AiController>(this);
		_stateControllerHolder = new SnapshotObjectMember<StateController>(this);
		_statControllerHolder = new SnapshotObjectMember<StatController>(this);
		_teamControllerHolder = new SnapshotObjectMember<TeamController>(this);
		_transformControllerHolder = new SnapshotObjectMember<TransformController>(this);
		_summonerUidHolder = new SnapshotObjectMember<int>(this, 0);
		_lastRageTimeMsHolder = new SnapshotObjectMember<long>(this, 0L);
		_diedTimeMsHolder = new SnapshotObjectMember<long>(this, 0L);
		_restingInfoHolder = new SnapshotObjectMember<BattleCreatureRestingInfo>(this);
		_creatureDataHolder = new SnapshotObjectMember<BattleCreatureData>(this);
		_soulWeaponSkillControllerHolder = new SnapshotObjectMember<SoulWeaponSkillController>(this);
		_DungeonSkillControllerHolder = new SnapshotObjectMember<DungeonSkillController>(this);
		BattleMetaDataContainer = new BattleMetaDataContainer();
		ManaHistoryTracker = new ManaHistoryTracker();
		ThreatHistoryTracker = new ThreatHistoryTracker();
	}

	public void Init(BattleInstance battleInstance, BattleCreatureStockData stockData)
	{
		BattleInstance = battleInstance;
		_stockData = stockData;
		BattleMetaDataContainer.Init(this);
		ManaHistoryTracker.Init(this);
		ThreatHistoryTracker.Init(this);
	}

	public void InitSnapshot(int uid)
	{
		if (_stockData == null)
		{
			VespaDebug.LogError("InitSnapshot: _stockData is null.");
			return;
		}
		RandomTable randomTable = new RandomTable();
		randomTable.Init(this, CreateInfo.RandomSeed);
		_uidHolder.Set(uid);
		_randomTableHolder.Set(randomTable);
		_skillControllerHolder.Alloc().Init(this);
		_operationControllerHolder.Alloc().Init(this);
		_aiControllerHolder.Alloc().Init(this);
		_stateControllerHolder.Alloc().Init(this);
		_statControllerHolder.Alloc().Init(this);
		_teamControllerHolder.Alloc().Init(this);
		_transformControllerHolder.Alloc().Init(this);
		_creatureDataHolder.Alloc().Init(_stockData.CreatureData);
		_soulWeaponSkillControllerHolder.Alloc().Init(this);
		if (BattleInstance != null)
		{
			TeamController.SetTeam(BattleInstance.GetTeamOfPlayer(PlayerId));
			_DungeonSkillControllerHolder.Alloc().Init(BattleInstance.DungeonData, this);
		}
		TransformController.SetPosition(CreateInfo.Position, raiseEvent: false, SetPosition.SetPositionMethodType.None, 0L);
		if (CreateInfo.IsInitialResting)
		{
			BeginResting(0, 0, wakeUpFomAlly: false, WakeUpPivotType.Center, 0);
		}
	}

	public void ChangeInitInfo(CreatureData creatureData, int creatureStar, StatData statData)
	{
		StatController.DirtyFinalStatAll();
		_stockData.CreatureData = creatureData;
		CreateInfo.HeroInfo.Star = (sbyte)creatureStar;
		CreateInfo.StatData = statData;
	}

	public string GetCreatureKey()
	{
		return ResultCreatureInfo.GetKey((CreatureData != null) ? CreatureData.Index : 0, CreateInfo.WaveSlotIndex, TeamController.Team.Id);
	}

	public void SetSummoner(int summonerUid)
	{
		_summonerUidHolder.Set(summonerUid);
	}

	public override string ToString()
	{
		if (CreatureData == null)
		{
			return "null creature";
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(CreatureData.Name);
		stringBuilder.Append(" (Uid=");
		stringBuilder.Append(Uid);
		stringBuilder.Append(")");
		return stringBuilder.ToString();
	}

	private bool NeedDestroy()
	{
		if (CreatureData == null || !CreatureData.IsDestroyOnDied)
		{
			return false;
		}
		long num = _diedTimeMsHolder.Get();
		if (num <= 0)
		{
			return false;
		}
		return BattleInstance.MainTimer.GetCurrentTimeMs() - num >= 5000;
	}

	public void Update(long currentTimeMs, int deltaTimeMs)
	{
		if (NeedDestroy())
		{
			BattleInstance.CreatureContainer.Destroy(Uid, raiseEvent: true);
			return;
		}
		SkillController.Update(deltaTimeMs);
		OperationController.Update(deltaTimeMs);
		StateController.Update();
		StatController.Update();
		BehaviorController.Update(deltaTimeMs);
		TransformController.Update(currentTimeMs, deltaTimeMs);
		SoulWeaponSkillController.Update();
		DungeonSkillController.Update();
		UpdateRageMode();
		ManaHistoryTracker.Update();
		ThreatHistoryTracker.Update();
	}

	public List<int> GetSkillIndexList(bool includeExtra)
	{
		List<int> skillIndexList = CreatureData.GetSkillIndexList(includeExtra);
		if (includeExtra && CreateInfo.AddExtraSkill != null)
		{
			for (int i = 0; i < CreateInfo.AddExtraSkill.Length; i++)
			{
				skillIndexList.Add(CreateInfo.AddExtraSkill[i].SkillIndex);
			}
		}
		return skillIndexList;
	}

	public int GetUserSkillLevel(int skillIndex)
	{
		if (CreatureData == null || CreateInfo == null)
		{
			return 0;
		}
		CachedHeroInfo heroInfo = CreateInfo.HeroInfo;
		if (heroInfo == null)
		{
			return 0;
		}
		if (CreateInfo.AddExtraSkill != null)
		{
			for (int i = 0; i < CreateInfo.AddExtraSkill.Length; i++)
			{
				ExtraSkillInfo extraSkillInfo = CreateInfo.AddExtraSkill[i];
				if (extraSkillInfo.SkillIndex == skillIndex)
				{
					if (extraSkillInfo.SkillLevel > 0)
					{
						return extraSkillInfo.SkillLevel;
					}
					return heroInfo.Level;
				}
			}
		}
		PassiveSkillInfo passiveSkill = SkillController.PassiveSkillContainer.GetPassiveSkill(skillIndex);
		if (passiveSkill != null)
		{
			return passiveSkill.SkillLevel;
		}
		bool learnByLevel = !CreateInfo.IsAi;
		int skillLevelBySkillIndex = heroInfo.GetSkillLevelBySkillIndex(CreatureData, skillIndex, learnByLevel);
		int skillSlotByIndex = CreatureData.GetSkillSlotByIndex(skillIndex, includeLinkedSkill: true);
		int skillSlotAddedLevel = StatController.GetSkillSlotAddedLevel(skillSlotByIndex);
		return skillLevelBySkillIndex + skillSlotAddedLevel;
	}

	private void UseWaveStartSkill()
	{
		OnWaveStart onWaveStart = SnapshotObjectPool<NShared.OnWaveStart>.Alloc();
		onWaveStart.TriggerCreature = this;
		onWaveStart.TargetCreatures = new BattleCreature[1] { this };
		SkillController.UseSkillByTrigger_ReleaseIfNotUsed(onWaveStart);
	}

	private void StopMyPassiveSkill()
	{
		for (int i = 0; i < BattleInstance.CreatureContainer.GetCreatureCount(); i++)
		{
			BattleCreature creatureByIndex = BattleInstance.CreatureContainer.GetCreatureByIndex(i);
			CancelMyOperation(creatureByIndex, passiveOnly: true, 0);
		}
	}

	public void CancelMyOperation(BattleCreature targetCreature, bool passiveOnly, int skillIndex)
	{
		if (targetCreature != null)
		{
			targetCreature.OperationController.RemoveOperationOfFiredCreature(Uid, passiveOnly, skillIndex);
			SkillController.OperationQueue.CancelReserve(targetCreature.Uid, skillIndex);
		}
	}

	private void ApplyOtherPassiveSkillToMe()
	{
		for (int i = 0; i < BattleInstance.CreatureContainer.GetCreatureCount(); i++)
		{
			BattleCreature creatureByIndex = BattleInstance.CreatureContainer.GetCreatureByIndex(i);
			if (creatureByIndex != this)
			{
				creatureByIndex.SkillController.ApplyFiredPassiveSkillToCreature(this);
			}
		}
	}

	public void SetRageMode()
	{
		_lastRageTimeMsHolder.Set(BattleInstance.MainTimer.GetCurrentTimeMs());
		UseRageSkill();
	}

	private void UpdateRageMode()
	{
		if (IsResting || BattleConstant.RageModeIntervalTimeMs == 0)
		{
			return;
		}
		long num = _lastRageTimeMsHolder.Get();
		if (num != 0L)
		{
			long currentTimeMs = BattleInstance.MainTimer.GetCurrentTimeMs();
			if (currentTimeMs - num >= BattleConstant.RageModeIntervalTimeMs)
			{
				_lastRageTimeMsHolder.Set(currentTimeMs);
				UseRageSkill();
			}
		}
	}

	private void UseRageSkill()
	{
		foreach (SkillData onRageSkill in GameRoot.Instance.SkillDataContainer.GetOnRageSkillList())
		{
			SkillController.UseSkill(onRageSkill, new SkillUseOption(checkOnly: false, checkLevel: false, checkMp: false, isEventSkill: true), null);
		}
	}

	public void ManualMove(Vector3 position)
	{
		SkillController.StopUsingDurationalSkill(isCancel: true);
		SkillController.CancelReservedSkill();
		TransformController.MoveToPosition(position, 0, 0, 0L, isManualMove: true);
	}

	private void InitStat()
	{
		long maxHp = StatController.GetMaxHp();
		int maxMp = StatController.GetMaxMp();
		long hp = maxHp;
		int num = ((BattleInstance.BattleDefine != null) ? BattleInstance.BattleDefine.StartMpRatioM : 0);
		int mp = maxMp * num / 1000;
		long forceMinHp = CreateInfo.ForceMinHp;
		if (InitialInfo != null)
		{
			hp = InitialInfo.Hp;
			mp = InitialInfo.Mp;
			if (InitialInfo.MaxHp > 0)
			{
				maxHp = InitialInfo.MaxHp;
				StatController.OverrideMaxHp(maxHp);
			}
			if (InitialInfo.MaxMp > 0)
			{
				maxMp = InitialInfo.MaxMp;
				StatController.OverrideMaxMp(maxMp);
			}
		}
		StatController.SetHp(hp);
		StatController.SetMp(mp);
		StatController.SetMinHp(forceMinHp);
	}

	public bool IsAliveAndPlaying()
	{
		if (StatController.IsAlive())
		{
			return !IsResting;
		}
		return false;
	}

	public void Die(int skillIndex, long operationUid)
	{
		StatController.SetHp(0L);
		OnDied(null, skillIndex, operationUid);
		StatController.SetHp(0L);
		RaiseBattleEvent(new Damage
		{
			CreatureUid = Uid,
			OperationUid = operationUid,
			Amount = 0L,
			FinalHp = 0L,
			FinalPercentage = 0,
			DamageFlag = DamageFlag.None,
			DamageStateType = string.Empty,
			IsDead = true
		});
	}

	public void BeginPlaying()
	{
		Vector3 vector = CreateInfo.Position;
		if (_restingInfoHolder.Get() != null)
		{
			bool wakeUpFromAlly = _restingInfoHolder.Get().WakeUpFromAlly;
			WakeUpPivotType wakeUpPivotType = _restingInfoHolder.Get().WakeUpPivotType;
			int wakeUpOffset = _restingInfoHolder.Get().WakeUpOffset;
			_restingInfoHolder.Set(null);
			vector = BattleInstance.CreatureContainer.GetWakeUpPosition(this, wakeUpFromAlly, wakeUpPivotType, wakeUpOffset);
		}
		RaiseBattleEvent(new SpawnCreature
		{
			CreatureUid = Uid,
			PlayerId = PlayerId,
			CreateInfo = CreateInfo,
			SpawnPosition = vector
		});
		TransformController.SetPosition(vector, raiseEvent: true, SetPosition.SetPositionMethodType.WakeUp, 0L);
		OnBeginPlaying onBeginPlaying = SnapshotObjectPool<OnBeginPlaying>.Alloc();
		onBeginPlaying.TriggerCreature = this;
		onBeginPlaying.TargetCreatures = new BattleCreature[1] { this };
		onBeginPlaying.WaveIndex = BattleInstance.Snapshot.WaveIndex;
		SkillController.UseSkillByTrigger_ReleaseIfNotUsed(onBeginPlaying);
		MakeCreatureCountStatDirty();
	}

	public void BeginResting(int wakeUpWaveIndex, int wakeUpWaveTimeMs, bool wakeUpFomAlly, WakeUpPivotType wakeUpPivotType, int wakeUpOffset)
	{
		_restingInfoHolder.Alloc();
		_restingInfoHolder.Get().Init(wakeUpWaveIndex, wakeUpWaveTimeMs, wakeUpFomAlly, wakeUpPivotType, wakeUpOffset);
		OnBeginResting(raiseDestroyEvent: true);
	}

	public void OnCreated()
	{
		TransformController.InitDirectionByTeam();
		SkillController.InitDungeonePassiveEnvironment();
		SkillController.InitPassiveSkill();
		SkillController.InitGuildSkill();
		SkillController.InitAccountBuff();
		SkillController.InitClassBuff();
		SkillController.InitGuildArenaBuff();
		SkillController.InitOrdealArenaSkill();
		SkillController.InitAffixSkill();
		SkillController.InitSoulWeaponSkill();
		StatController.InitExtraStats();
		InitStat();
		BattleMetaDataContainer.InitEquipTypeCount();
		BattleInstance.CreatureContainer.UpdateRangeBonus(TeamController.TeamType);
		SkillController.PassiveSkillContainer.UpdateOnStartSkillCondition();
		_ = BattleInstance.DevParam_LegacyOnStartSkill;
		ApplyOtherPassiveSkillToMe();
		InitStat();
		SkillController.BeginSkillInitialCooltime();
		if (CreateInfo.IsAi)
		{
			if (BattleInstance.DungeonData.BattleType == BattleType.GuildArena)
			{
				BehaviorController.Start(isSmartAi: true);
			}
			else
			{
				BehaviorController.Start(CreateInfo.IsSmartAi);
			}
			int firstSkillSlotIndex = CreateInfo.FirstSkillSlotIndex;
			if (firstSkillSlotIndex != 0)
			{
				BehaviorController.SetFirstSkill(firstSkillSlotIndex);
			}
		}
		BeginPlaying();
	}

	public int GetEquipTypeCount(EquipItemEquipType equipType)
	{
		return BattleMetaDataContainer.GetEquipTypeCount(equipType);
	}

	public void OnWaveStart(int waveIndex)
	{
		if (!IsResting)
		{
			UseWaveStartSkill();
			SkillController.BeginAutoSkillInitialCooltime();
		}
	}

	public void OnDied(BattleCreature attacker, int skillIndex, long operationUid)
	{
		if (attacker != null)
		{
			attacker.StatController.RecoverHpByStat(StatType.HpOnKill, StatType.HpRatioOnKill, HealFlag.HpOnKill | HealFlag.Supplemently);
			attacker.StatController.RecoverMpByStat(StatType.MpOnKill, StatType.None, ChangeMp.ReasonType.MpOnKill, 1000L);
			BattleInstance.Snapshot.StatisticsManager.AddKillMeter(attacker.Uid, Uid, 1);
			BattleInstance.Snapshot.StatisticsManager.AddKillTeamMeter(attacker.TeamController.TeamType, 1);
			BattleInstance.Snapshot.StatisticsManager.AddKillCreatureIndex(attacker.Uid, CreatureIndex, 1);
			if (SummonerUid == 0)
			{
				TeamType teamType = attacker.TeamController.TeamType;
				BattleInstance.Snapshot.StatisticsManager.AddCurrentWaveKillTeamMeter(teamType, 1);
				BattleInstance.Snapshot.StatisticsManager.AddTotalKillTeamMeter(teamType, 1);
			}
		}
		OnDie onDie = SnapshotObjectPool<OnDie>.Alloc();
		onDie.TriggerCreature = attacker;
		onDie.TargetCreatures = new BattleCreature[1] { this };
		onDie.SkillIndex = skillIndex;
		SkillController.UseSkillByTrigger_ReleaseIfNotUsed(onDie);
		if (attacker != null)
		{
			BattleCreatureList aliveCreatures = BattleInstance.CreatureContainer.GetAliveCreatures();
			for (int i = 0; i < aliveCreatures.Count; i++)
			{
				BattleCreature battleCreature = aliveCreatures[i];
				OnCreatureKill onCreatureKill = SnapshotObjectPool<OnCreatureKill>.Alloc();
				onCreatureKill.TriggerCreature = attacker;
				onCreatureKill.TargetCreatures = new BattleCreature[1] { this };
				battleCreature.SkillController.UseSkillByTrigger_ReleaseIfNotUsed(onCreatureKill);
			}
		}
		if (attacker != null)
		{
			OnKill onKill = SnapshotObjectPool<OnKill>.Alloc();
			onKill.TriggerCreature = attacker;
			onKill.TargetCreatures = new BattleCreature[1] { this };
			onKill.SkillIndex = skillIndex;
			attacker.SkillController.UseSkillByTrigger_ReleaseIfNotUsed(onKill);
		}
		OnBeginResting(raiseDestroyEvent: false);
		_diedTimeMsHolder.Set(BattleInstance.MainTimer.GetCurrentTimeMs());
		if (!BattleInstance.DevParam_LegacyOnDieSkill)
		{
			SkillController.OperationQueue.Update(0);
		}
	}

	public void OnBeginResting(bool raiseDestroyEvent)
	{
		BehaviorController.Stop();
		OperationController.Clear(clearPassive: false);
		StateController.Clear();
		SkillController.StopUsingDurationalSkill(isCancel: true);
		StopMyPassiveSkill();
		BattleInstance.CreatureContainer.UpdateRangeBonus(TeamController.TeamType);
		MakeCreatureCountStatDirty();
		if (raiseDestroyEvent)
		{
			RaiseBattleEvent(new DestroyCreature
			{
				CreatureUid = Uid
			});
		}
	}

	private void MakeCreatureCountStatDirty()
	{
		BattleCreatureList aliveCreaturesOfOtherTeam = BattleInstance.CreatureContainer.GetAliveCreaturesOfOtherTeam(TeamController.TeamType);
		BattleInstance.MakeCretureSatatDirty(aliveCreaturesOfOtherTeam, StatType.EnemyCount);
		BattleInstance.MakeCretureSatatDirty(aliveCreaturesOfOtherTeam, StatType.TargetableEnemyCount);
		BattleCreatureList aliveCreaturesOfTeam = BattleInstance.CreatureContainer.GetAliveCreaturesOfTeam(TeamController.TeamType);
		BattleInstance.MakeCretureSatatDirty(aliveCreaturesOfTeam, StatType.AllyCount);
	}

	public void OnWaveClosed(bool timeouted)
	{
		if (!IsResting && !StatController.IsDead())
		{
			SkillController.CancelUsingDurationalSkillOnWaveEnd();
			_lastRageTimeMsHolder.Reset();
		}
	}

	public void OnWaveEnded(bool timeouted)
	{
		if (!IsResting && !StatController.IsDead())
		{
			SkillController.ResetOnWaveEnd();
			OperationController.ClearEnemyOperationOnWaveEnd();
			OperationController.ClearExpireOnWaveEndOperation();
			if (!timeouted)
			{
				StatController.RecoverHpByStat(StatType.HpOnWave, StatType.HpRatioOnWave, HealFlag.HpOnWave | HealFlag.Supplemently);
				StatController.RecoverMpByStat(StatType.MpOnWave, StatType.None, ChangeMp.ReasonType.MpOnWave, 1000L);
			}
		}
	}

	public int NextRandom(int max, string desc = null)
	{
		return _randomTableHolder.Get().NextRandom(max, desc);
	}

	public int NextRandom(int min, int max, string desc = null)
	{
		return _randomTableHolder.Get().NextRandom(min, max, desc);
	}

	public void Shuffle<T>(IList<T> list)
	{
		for (int num = list.Count - 1; num > 0; num--)
		{
			int index = NextRandom(num + 1, "BattleCreature.Shuffle");
			T value = list[index];
			list[index] = list[num];
			list[num] = value;
		}
	}

	public bool TestChanceRateResult(long chanceRate, string desc = null)
	{
		if (chanceRate <= 0)
		{
			return false;
		}
		return NextRandom(1000, desc) < chanceRate;
	}

	public long GetAccumulatedHitRatio(int attackerLevel, long ratio)
	{
		return GetAccumulatedHitRatio(CreateInfo.HeroInfo.Level, attackerLevel, ratio);
	}

	public long GetAccumulatedDodgeRatio(int attackerLevel, long ratio)
	{
		return GetAccumulatedHitRatio(attackerLevel, CreateInfo.HeroInfo.Level, ratio);
	}

	public static long GetAccumulatedHitRatio(int myLevel, int attackerLevel, long ratio)
	{
		int num = myLevel - attackerLevel;
		if (num == 0)
		{
			return ratio;
		}
		int levelChanceAttenuationFactor = BattleConstant.LevelChanceAttenuationFactor;
		if (levelChanceAttenuationFactor == 0 || levelChanceAttenuationFactor == 1000)
		{
			return ratio;
		}
		if (num > 0)
		{
			return System.Math.Min(System.Math.Max(ratio * GameRoot.Instance.ExponentialTableK.GetAccRatio(levelChanceAttenuationFactor, num) / 1000, 0L), 1000L);
		}
		long accRatio = GameRoot.Instance.ExponentialTableK.GetAccRatio(levelChanceAttenuationFactor, -num);
		if (accRatio <= 0)
		{
			return 1000L;
		}
		return System.Math.Min(System.Math.Max(ratio * 1000 / accRatio, 0L), 1000L);
	}

	public void RaiseBattleEvent<T>(T data) where T : BaseBattleCallback
	{
		BattleInstance.RaiseBattleEvent(data);
	}

	public void QueueBattleEvent<T>(T data) where T : BaseBattleCallback
	{
		BattleInstance.QueueBattleEvent(data);
	}

	public bool IsDummyCreature()
	{
		return CreateInfo.IsDummy;
	}
}
