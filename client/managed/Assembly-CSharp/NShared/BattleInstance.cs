using System;
using System.Collections.Generic;
using NGame2.NDev;
using NShared.NUtil;
using NVespa;

namespace NShared;

public class BattleInstance
{
	public const int DefaultDeltaTimeMs = 100;

	private DateTime _lastTickTime;

	private int _deltaTimeMs = 100;

	private bool _snapshotEnabled;

	private List<ResultCreatureInfo> _creatureInfoList;

	private BattleEventUtility _battleEventUtility;

	public bool DevParam_LegacySkillModifierCheck;

	public bool DevParam_LegacyOnDieSkill;

	public bool DevParam_LegacySkillAttrCcCheck;

	public bool DevParam_LegacyBlockDashSizeCheck;

	public bool DevParam_LegacyOnStartSkill;

	public static bool DevParam_LegacyModSkillRecalculate;

	public bool IsStartBattle;

	public DateTime StartBattleTime;

	public DateTime EndBattleTime;

	private DateTime _serverTimeSetTime = DateTime.UtcNow;

	private long _serverTimeMs;

	private bool _applyServerTime;

	public bool DisableGiveDamage { get; set; }

	public bool AllResistCc { get; set; }

	public long Uid { get; private set; }

	public CampaignDungeonData DungeonData { get; private set; }

	public int ChapterIndex
	{
		get
		{
			if (DungeonData == null)
			{
				return 0;
			}
			return DungeonData.ChapterIndex;
		}
	}

	public int DungeonIndex
	{
		get
		{
			if (DungeonData == null)
			{
				return 0;
			}
			return DungeonData.DungeonIndex;
		}
	}

	public int Difficulty { get; private set; }

	public bool IsScenario { get; private set; }

	public Timer GlobalTimer { get; private set; }

	public Timer MainTimer { get; private set; }

	public BattlePlayerContainer PlayerContainer { get; private set; }

	public BattleCreatureContainer CreatureContainer { get; private set; }

	public BattleAffixContainer AffixContainer { get; private set; }

	public BattleInstanceConstant InstanceConstant { get; private set; }

	public bool InfiniteBattleTime { get; private set; }

	public int MaxBattleTime { get; private set; }

	public int ExtraBattleTime { get; private set; }

	public CampaignFieldData FieldData { get; private set; }

	public BattleArea BattleArea { get; private set; }

	public BattleDefineData BattleDefine { get; private set; }

	public BattleLogContainer BattleLogContainer { get; private set; }

	public BattleSnapshotManager BattleSnapshotManager { get; private set; }

	public IBattleEventServiceContext BattleEventContext { get; private set; }

	public IBattleEventRaiser BattleEventRaiser { get; private set; }

	public BattleInstanceSnapshot Snapshot { get; private set; }

	public int DeltaTimeMs => _deltaTimeMs;

	public bool SnapshotEnabled => _snapshotEnabled;

	public int RandomSeedByMonth { get; private set; }

	public int[] ProjectileGroupIndicesFromServer { get; set; }

	public int GameSpeedRatio { get; private set; }

	public KarmaManager KarmaManager { get; private set; }

	public BattleInstanceExtraData ExtraData { get; private set; }

	public ShardFlaskManager ShardFlaskManager { get; private set; }

	public bool IsTraceLogToFile { get; set; }

	public bool CheckEachWaveEachTeam
	{
		get
		{
			if (DungeonData == null)
			{
				return false;
			}
			if (DungeonData.BattleType != BattleType.ArcdimRaid && DungeonData.BattleType != BattleType.ArcdimHardRaid)
			{
				return false;
			}
			return true;
		}
	}

	public EBattleFormation GetBattleFormation()
	{
		if (ChapterIndex == 100002)
		{
			return EBattleFormation.Spread4;
		}
		return EBattleFormation.Default;
	}

	public BattleInstance()
	{
		BattleConstant.StaticInitialize();
		GameRoot.Instance.StateActionFactory.Init();
		DevParam_LegacySkillModifierCheck = DevParam.GetDevParamBool("LegacySkillModifierCheck");
		DevParam_LegacyOnDieSkill = DevParam.GetDevParamBool("LegacyOnDieSkill");
		DevParam_LegacySkillAttrCcCheck = DevParam.GetDevParamBool("LegacySkillAttrCcCheck");
		DevParam_LegacyBlockDashSizeCheck = DevParam.GetDevParamBool("LegacyBlockDashSizeCheck");
		DevParam_LegacyOnStartSkill = DevParam.GetDevParamBool("LegacyOnStartSkill");
		DevParam_LegacyModSkillRecalculate = DevParam.GetDevParamBool("LegacyModSkillRecalculate");
		BattleConstant.LegacySoloTime = DevParam.GetDevParamBool("LegacySoloTime");
		BattleConstant.LegacySameSkillCancel = DevParam.GetDevParamBool("LegacySameSkillCancel");
		_lastTickTime = DateTime.UtcNow;
		_creatureInfoList = new List<ResultCreatureInfo>();
		_battleEventUtility = new BattleEventUtility();
		using (new SnapshotWritableBlock())
		{
			GlobalTimer = new Timer();
			MainTimer = new Timer();
			PlayerContainer = new BattlePlayerContainer(this);
			CreatureContainer = new BattleCreatureContainer(this);
			AffixContainer = new BattleAffixContainer(this);
			BattleArea = new BattleArea(this);
			BattleLogContainer = new BattleLogContainer(this);
			BattleSnapshotManager = new BattleSnapshotManager(this);
			Snapshot = new BattleInstanceSnapshot();
			InstanceConstant = new BattleInstanceConstant();
			KarmaManager = new KarmaManager(this);
			ShardFlaskManager = new ShardFlaskManager(this);
		}
	}

	public void Init()
	{
		using (new SnapshotWritableBlock())
		{
			Snapshot.Init(this);
			KarmaManager.Init();
		}
	}

	public void SetCampaignDungeon(int chapterIndex, int dungeonIndex, int difficulty, bool isScenario)
	{
		CampaignDungeonData data = GameRoot.Instance.CampaignDungeonDataContainer.GetData(chapterIndex, dungeonIndex);
		if (data == null)
		{
			VespaDebug.LogError("Failed to find DungeonData : chapter=" + chapterIndex + " dungeon=" + dungeonIndex);
		}
		else
		{
			SetCampaignDungeon(data, difficulty, isScenario);
		}
	}

	private void SetCampaignDungeon(CampaignDungeonData dungeonData, int difficulty, bool isScenario)
	{
		dungeonData = SprkArenaPractice.CombatDungeon(dungeonData);
		if (dungeonData != null)
		{
			DungeonData = dungeonData;
			Difficulty = difficulty;
			IsScenario = isScenario;
			SetConstant(dungeonData.Constant);
			SetBattleArea(dungeonData.BattleField);
			SetBattleDefineData(dungeonData.GetBattleDefine(isScenario));
		}
	}

	public void SetRandomSeedByMonth(int seed)
	{
		RandomSeedByMonth = seed;
		if (KarmaManager.Enabled)
		{
			KarmaManager.InitRandomTable(seed);
		}
	}

	public void SetProjectileGroupIndicesFromServer(int[] indices)
	{
		ProjectileGroupIndicesFromServer = indices;
	}

	private void SetConstant(string[] values)
	{
		InstanceConstant.Init(values);
		InfiniteBattleTime = InstanceConstant.InfiniteTime;
	}

	private void SetBattleArea(string fieldName)
	{
		if (string.IsNullOrEmpty(fieldName))
		{
			return;
		}
		CampaignFieldData data = GameRoot.Instance.CampaignFieldDataContainer.GetData(fieldName);
		if (data == null)
		{
			return;
		}
		FieldData = data;
		List<BattleAreaElementData> battleAreaDataList = FieldData.GetBattleAreaDataList();
		for (int i = 0; i < battleAreaDataList.Count; i++)
		{
			BattleAreaElementData battleAreaElementData = battleAreaDataList[i];
			if (battleAreaElementData != null)
			{
				BattleArea.InitByGroupIndex(battleAreaElementData.AreaBottomM, battleAreaElementData.AreaTopM, battleAreaElementData.AreaLeftM, battleAreaElementData.AreaRightM, battleAreaElementData.Angle, battleAreaElementData.BattleAreaGroupIndex);
			}
		}
	}

	private void SetBattleDefineData(BattleDefineData battleDefineData)
	{
		BattleDefine = battleDefineData;
		MaxBattleTime = battleDefineData.MaxTime;
		ExtraBattleTime = battleDefineData.ExtraTime;
	}

	public void EnableSnapshot(bool enable)
	{
		_snapshotEnabled = enable;
	}

	public void SetTraceBattleEventAccountId(long accountId)
	{
		_battleEventUtility.SetTraceBattleEventAccountId(accountId);
	}

	public void SetBattleEventRaiser(IBattleEventRaiser battleEventRaiser)
	{
		BattleEventRaiser = battleEventRaiser;
	}

	public void SetUid(long uid)
	{
		Uid = uid;
	}

	public void SetBattleEventContext(IBattleEventServiceContext battleEventContext)
	{
		BattleEventContext = battleEventContext;
	}

	public void SetExtraData(BattleInstanceExtraData data)
	{
		ExtraData = data;
		if (data != null)
		{
			AffixContainer.Set(data.AffixIndices, data.InfinityLevel);
			if (ExtraData.FlaskItemIndex != 0)
			{
				ShardFlaskManager.Set();
			}
		}
	}

	public void SetUpdateDeltaTimeMs(int deltaTimeMs)
	{
		_deltaTimeMs = System.Math.Max(deltaTimeMs, 1);
	}

	public void Log(object message)
	{
	}

	public void CloseBattle(bool timeouted)
	{
		Snapshot.BattleClosing = true;
		Snapshot.BattleTimeouted = timeouted;
		OnWaveClosed(timeouted);
	}

	public bool IsBattleClosing()
	{
		return Snapshot.BattleClosing;
	}

	public bool IsBattleClosingOrDisabled()
	{
		if (!IsBattleClosing())
		{
			return !IsBattleEnabled();
		}
		return true;
	}

	public void ProcessBattleEnd()
	{
		DisableBattle();
		SetBattleEnded(ended: true);
		Snapshot.BattleClosing = false;
		bool battleTimeouted = Snapshot.BattleTimeouted;
		OnWaveEnded(battleTimeouted);
		Snapshot.Withdraw = false;
		EndBattleTime = DateTime.UtcNow;
	}

	public bool IsBattleEnded()
	{
		return Snapshot.BattleEnded;
	}

	public void SetBattleEnded(bool ended)
	{
		Snapshot.BattleEnded = ended;
		Log("Battle ended=" + ended);
	}

	public bool IsBattleStarted()
	{
		return Snapshot.BattleStarted;
	}

	public void OnWaveReadyBattle()
	{
		DisableBattle();
		WaveStartTimeSetting();
		SetBattleStarted(started: false);
		SetBattleEnded(ended: false);
		RaiseBattleEvent(new WaveReady());
	}

	public void OnWaveStartBattle()
	{
		EnableBattle();
		SetBattleStarted(started: true);
		SetBattleEnded(ended: false);
		CreateEnvironmentProjectile();
		AdjustKarmaCard();
	}

	public void WaveStartTimeSetting()
	{
		Snapshot.WaveStartTimeMs = MainTimer.GetCurrentTimeMs();
	}

	public bool IsWaveStartedTime()
	{
		return Snapshot.WaveStartTimeMs == MainTimer.GetCurrentTimeMs();
	}

	public void SetBattleStarted(bool started)
	{
		Snapshot.BattleStarted = started;
		if (started)
		{
			if (!IsStartBattle)
			{
				StartBattleTime = DateTime.UtcNow;
				IsStartBattle = true;
			}
			_lastTickTime = DateTime.UtcNow;
			for (int i = 0; i < CreatureContainer.GetCreatureCount(); i++)
			{
				CreatureContainer.GetCreatureByIndex(i).OnWaveStart(Snapshot.WaveIndex);
			}
		}
	}

	public bool IsBattleEnabled()
	{
		return Snapshot.BattleEnabled;
	}

	public void EnableBattle()
	{
		Snapshot.BattleEnabled = true;
		Snapshot.BattleClosing = false;
	}

	public void DisableBattle()
	{
		Snapshot.BattleEnabled = false;
	}

	private void CreateEnvironmentProjectile()
	{
		CampaignWaveData waveData = GetWaveData();
		if (waveData == null)
		{
			return;
		}
		int num = 1;
		while (true)
		{
			CampaignWaveData.EnvironmentProjectileInfo environmentProjectileInfo = waveData.GetEnvironmentProjectileInfo(num);
			if (environmentProjectileInfo == null)
			{
				break;
			}
			int projectileIndex = environmentProjectileInfo.ProjectileIndex;
			TeamType teamType = environmentProjectileInfo.TeamType;
			Vector3 offset = new Vector3(environmentProjectileInfo.OffsetMx, 0L, environmentProjectileInfo.OffsetMz);
			Snapshot.ProjectileContainer.AddProjectile(projectileIndex, teamType, offset);
			num++;
		}
		int randomProjectileGroupIndex = DungeonData.GetRandomProjectileGroupIndex(RandomSeedByMonth);
		if (randomProjectileGroupIndex != -1)
		{
			SkillProjectileGroupData data = GameRoot.Instance.SkillProjectileGroupDataContainer.GetData(randomProjectileGroupIndex);
			if (data != null)
			{
				List<SkillProjectileGroupData.SkillProjectileInfo> skillProjectileInfos = data.GetSkillProjectileInfos();
				if (skillProjectileInfos != null)
				{
					foreach (SkillProjectileGroupData.SkillProjectileInfo item in skillProjectileInfos)
					{
						Snapshot.ProjectileContainer.AddProjectile(item.Index, item.Team, Vector3.zero);
					}
				}
			}
		}
		if (ProjectileGroupIndicesFromServer == null || ProjectileGroupIndicesFromServer.Length == 0)
		{
			return;
		}
		for (int i = 0; i < ProjectileGroupIndicesFromServer.Length; i++)
		{
			int key = ProjectileGroupIndicesFromServer[i];
			SkillProjectileGroupData data2 = GameRoot.Instance.SkillProjectileGroupDataContainer.GetData(key);
			if (data2 == null)
			{
				continue;
			}
			List<SkillProjectileGroupData.SkillProjectileInfo> skillProjectileInfos2 = data2.GetSkillProjectileInfos();
			if (skillProjectileInfos2 == null || skillProjectileInfos2.Count == 0)
			{
				continue;
			}
			foreach (SkillProjectileGroupData.SkillProjectileInfo item2 in skillProjectileInfos2)
			{
				Snapshot.ProjectileContainer.AddProjectile(item2.Index, item2.Team, Vector3.zero);
			}
		}
	}

	public bool IsInSoloTime()
	{
		for (int i = 0; i < CreatureContainer.GetCreatureCount(); i++)
		{
			BattleCreature creatureByIndex = CreatureContainer.GetCreatureByIndex(i);
			if (!creatureByIndex.StatController.IsDead() && creatureByIndex.SkillController.IsInSoloTime())
			{
				return true;
			}
		}
		return false;
	}

	public void OnWaveClosed(bool timeouted)
	{
		for (int i = 0; i < CreatureContainer.GetCreatureCount(); i++)
		{
			CreatureContainer.GetCreatureByIndex(i).OnWaveClosed(timeouted);
		}
		Snapshot.ProjectileContainer.Clear();
	}

	public void OnWaveEnded(bool timeouted)
	{
		for (int i = 0; i < CreatureContainer.GetCreatureCount(); i++)
		{
			CreatureContainer.GetCreatureByIndex(i).OnWaveEnded(timeouted);
		}
		if (KarmaManager != null)
		{
			KarmaManager.WaveEndEvent();
		}
	}

	public void SetChapterDungeonIndex(int chapterIndex, int dungeonIndex)
	{
		Snapshot.ChapterIndex = chapterIndex;
		Snapshot.DungeonIndex = dungeonIndex;
	}

	public void SetWaveIndex(int waveIndex, int totalWaveIndex)
	{
		Snapshot.WaveIndex = waveIndex;
		Snapshot.TotalWaveIndex = totalWaveIndex;
	}

	public void SetMaxWaveIndex(int maxWaveIndex)
	{
		Snapshot.MaxWaveIndex = maxWaveIndex;
	}

	public void SetMaxStageLevel(int maxStageLevel)
	{
		Snapshot.MaxStageLevel = maxStageLevel;
	}

	public void SetWaveCenter(WaveCenterPositionInfo[] waveCenterPositionInfos)
	{
		Snapshot.WaveCenter.Clear();
		if (waveCenterPositionInfos == null || waveCenterPositionInfos.Length == 0)
		{
			return;
		}
		foreach (WaveCenterPositionInfo waveCenterPositionInfo in waveCenterPositionInfos)
		{
			if (waveCenterPositionInfo != null)
			{
				Snapshot.WaveCenter.Remove(waveCenterPositionInfo.GroupIndex);
				Snapshot.WaveCenter.Add(waveCenterPositionInfo.GroupIndex, waveCenterPositionInfo.WaveCenter);
			}
		}
	}

	public CampaignWaveData GetWaveData()
	{
		int waveIndex = Snapshot.WaveIndex;
		return GameRoot.Instance.CampaignWaveDataContainer.GetData(ChapterIndex, DungeonIndex, (DungeonDifficultyType)Difficulty, IsScenario, waveIndex);
	}

	public void SetServerTimeMs(long serverTimeMs)
	{
		_serverTimeMs = serverTimeMs;
		_serverTimeSetTime = DateTime.UtcNow;
		_applyServerTime = true;
	}

	private long GetCurrentServerTimeMs()
	{
		double totalMilliseconds = (DateTime.UtcNow - _serverTimeSetTime).TotalMilliseconds;
		return (long)((double)_serverTimeMs + totalMilliseconds);
	}

	public void SetDeltaTimeReductionRatio(int ratio)
	{
		GameSpeedRatio = ratio;
	}

	public int GetElapsedBattleTimeMs()
	{
		long currentTimeMs = MainTimer.GetCurrentTimeMs();
		long waveStartTimeMs = Snapshot.WaveStartTimeMs;
		return (int)System.Math.Max(currentTimeMs - waveStartTimeMs, 0L);
	}

	public long GetElapsedPureBattleTimeMs()
	{
		TimeSpan timeSpan = EndBattleTime - StartBattleTime;
		int val = (int)(timeSpan.TotalSeconds - (double)(GlobalTimer.GetCurrentTimeMs() / 1000));
		val = System.Math.Max(val, 0);
		double num = timeSpan.TotalMilliseconds - (double)(val * 1000);
		long num2 = GlobalTimer.GetCurrentTimeMs() - MainTimer.GetCurrentTimeMs();
		return System.Math.Max((long)(num - (double)num2), 0L);
	}

	public int GetRemainBattleTimeMs(bool includeExtraTime)
	{
		if (InfiniteBattleTime)
		{
			return 999999;
		}
		int elapsedBattleTimeMs = GetElapsedBattleTimeMs();
		return System.Math.Max((MaxBattleTime + (includeExtraTime ? ExtraBattleTime : 0)) * 1000 - elapsedBattleTimeMs, 0);
	}

	public bool IsBattleTimeEnd()
	{
		if (!InfiniteBattleTime && MaxBattleTime > 0)
		{
			return GetRemainBattleTimeMs(includeExtraTime: true) <= 0;
		}
		return false;
	}

	public virtual int Update()
	{
		long currentTimeMs = GlobalTimer.GetCurrentTimeMs();
		int num = 0;
		int num2 = System.Math.Max(10 / GameSpeedRatio, 1);
		while (true)
		{
			int num3 = (int)(DateTime.UtcNow - _lastTickTime).TotalMilliseconds;
			int num4 = System.Math.Max(_deltaTimeMs / GameSpeedRatio, 1);
			if (_applyServerTime)
			{
				long num5 = GetCurrentServerTimeMs() - currentTimeMs;
				if (num5 >= num2)
				{
					num4 = (int)((double)num4 * 0.9);
				}
				else if (num5 <= -num2)
				{
					num4 = (int)((double)num4 * 1.1);
				}
			}
			if (num3 < num4 || num > 100)
			{
				break;
			}
			_lastTickTime = _lastTickTime.AddMilliseconds(num4);
			UpdateTick(_snapshotEnabled);
			num++;
		}
		return num;
	}

	public virtual int EnhancedUpdate()
	{
		long currentTimeMs = GlobalTimer.GetCurrentTimeMs();
		int num = 0;
		while (true)
		{
			int num2 = (int)(DateTime.UtcNow - _lastTickTime).TotalMilliseconds;
			int num3 = System.Math.Max(_deltaTimeMs / GameSpeedRatio, 1);
			if (_applyServerTime)
			{
				long num4 = GetCurrentServerTimeMs() - currentTimeMs;
				if (num4 <= 10)
				{
					num3 = (int)((double)num3 * 1.1);
				}
				else if (num4 >= 50)
				{
					num3 = (int)((double)num3 * 0.95);
				}
				else if (num4 >= 150)
				{
					num3 = (int)((double)num3 * 0.9);
				}
				else if (num4 >= 400)
				{
					num3 = (int)((double)num3 * 0.6);
				}
			}
			if (num2 < num3 || num > 2)
			{
				break;
			}
			_lastTickTime = _lastTickTime.AddMilliseconds(num3);
			UpdateTick(_snapshotEnabled);
			num++;
		}
		return num;
	}

	public virtual void UpdateTick(bool snapshotEnabled)
	{
		long currentTimeMs = GlobalTimer.GetCurrentTimeMs();
		if (!IsBattleStarted())
		{
			using (new SnapshotWritableBlock())
			{
				BattleLogContainer.ProcessLog(currentTimeMs);
				return;
			}
		}
		if (snapshotEnabled)
		{
			if (BattleLogContainer.RestoreTimeMs >= 0)
			{
				using (new SnapshotWritableBlock())
				{
					currentTimeMs = BattleLogContainer.RestoreTimeMs;
					BattleSnapshotManager.RestoreSnapshot(currentTimeMs);
					BattleLogContainer.RemoveFutureBattleLogs(currentTimeMs);
					BattleLogContainer.RestoreTimeMs = -1L;
					return;
				}
			}
			BattleSnapshotManager.Update(_deltaTimeMs);
			BattleLog nextBattleLog = BattleLogContainer.GetNextBattleLog();
			if (nextBattleLog != null)
			{
				long globalTimeMs = nextBattleLog.GlobalTimeMs;
				if (globalTimeMs < currentTimeMs)
				{
					using (new SnapshotWritableBlock())
					{
						BattleSnapshotManager.RestoreSnapshot(globalTimeMs);
					}
					currentTimeMs = GlobalTimer.GetCurrentTimeMs();
				}
			}
		}
		using (new SnapshotWritableBlock())
		{
			if (BattleLogContainer.ProcessLog(currentTimeMs) > 0)
			{
				BattleSnapshotManager.ReserveAddSnapshot();
			}
			if (IsBattleEnabled())
			{
				CreatureContainer.Update(currentTimeMs, _deltaTimeMs);
				ShardFlaskManager.Update(currentTimeMs);
				Snapshot.Update(currentTimeMs, _deltaTimeMs);
			}
		}
		if (IsBattleEnabled())
		{
			long currentTimeMs2 = currentTimeMs + _deltaTimeMs;
			GlobalTimer.Set(currentTimeMs2);
			if (!IsInSoloTime())
			{
				MainTimer.Advance(_deltaTimeMs);
			}
		}
	}

	public BattlePlayer AddPlayer(BattlePlayerCreateInfo playerCreateInfo)
	{
		BattlePlayer battlePlayer = PlayerContainer.Create(playerCreateInfo);
		if (battlePlayer == null)
		{
			return null;
		}
		return battlePlayer;
	}

	public Dictionary<int, int> GetKillCount(TeamType teamType)
	{
		Dictionary<int, int> killCountDictionary = new Dictionary<int, int>();
		BattleCreatureList allCreaturesOfTeam = CreatureContainer.GetAllCreaturesOfTeam((int)teamType);
		for (int i = 0; i < allCreaturesOfTeam.Count; i++)
		{
			BattleCreature battleCreature = allCreaturesOfTeam[i];
			Snapshot.StatisticsManager.GetKillCreatureIndexStatistics(battleCreature.Uid)?.Values.ForEachIrregular(delegate(KeyValuePair<int, long> pair)
			{
				int key = pair.Key;
				int num = (int)pair.Value;
				if (killCountDictionary.ContainsKey(key))
				{
					killCountDictionary[key] += num;
				}
				else
				{
					killCountDictionary[key] = num;
				}
			});
		}
		return killCountDictionary;
	}

	public void SetRageMode(TeamType teamType)
	{
		BattleCreatureList allCreaturesOfTeam = CreatureContainer.GetAllCreaturesOfTeam((int)teamType);
		for (int i = 0; i < allCreaturesOfTeam.Count; i++)
		{
			allCreaturesOfTeam[i].SetRageMode();
		}
	}

	public void MakeCretureSatatDirty(BattleCreatureList creatures, StatType statType)
	{
		if (creatures != null && creatures.Count != 0)
		{
			for (int i = 0; i < creatures.Count; i++)
			{
				creatures[i]?.StatController.MakeStatDirty(statType);
			}
		}
	}

	public void SetInitialCreatureInfo(ResultCreatureInfo[] creatureInfos)
	{
		if (creatureInfos != null)
		{
			for (int i = 0; i < creatureInfos.Length; i++)
			{
				_creatureInfoList.Add(creatureInfos[i]);
			}
		}
	}

	public ResultCreatureInfo GetInitialCreatureInfo(int heroIndex, int waveSlotIndex, int teamId)
	{
		string key = ResultCreatureInfo.GetKey(heroIndex, waveSlotIndex, teamId);
		for (int i = 0; i < _creatureInfoList.Count; i++)
		{
			ResultCreatureInfo resultCreatureInfo = _creatureInfoList[i];
			if (resultCreatureInfo != null && resultCreatureInfo.Key == key && resultCreatureInfo.TeamId == teamId)
			{
				return resultCreatureInfo;
			}
		}
		return null;
	}

	public Team GetTeam(TeamType teamType)
	{
		return teamType switch
		{
			TeamType.Left => Snapshot.LeftTeam, 
			TeamType.Right => Snapshot.RightTeam, 
			_ => null, 
		};
	}

	public Team GetTeamOfPlayer(long playerId)
	{
		TeamType teamType = PlayerContainer.GetPlayer(playerId)?.TeamType ?? TeamType.Right;
		return GetTeam(teamType);
	}

	public BattleCreature GetEnvironmentCreatureOfTeam(TeamType teamType)
	{
		if (teamType == TeamType.Left && Snapshot.LeftTeam != null)
		{
			return Snapshot.LeftTeam.EnvironmentCreature;
		}
		if (teamType == TeamType.Right && Snapshot.RightTeam != null)
		{
			return Snapshot.RightTeam.EnvironmentCreature;
		}
		return null;
	}

	public void RaiseBattleEvent<T>(T data) where T : BaseBattleCallback
	{
		if (BattleEventRaiser != null)
		{
			data.GlobalTimestamp = GlobalTimer.GetCurrentTimeMs();
			BattleEventRaiser.RaiseBattleEvent(BattleEventContext, data);
			_battleEventUtility.AddEvent(this, isComment: false, data, isQueuedEvent: false);
			OnBattleEvent(data);
		}
	}

	public void QueueBattleEvent<T>(T data) where T : BaseBattleCallback
	{
		if (BattleEventRaiser != null)
		{
			BattleEventRaiser.QueueBattleEvent(BattleEventContext, data);
			_battleEventUtility.AddEvent(this, isComment: false, data, isQueuedEvent: true);
		}
	}

	public void TraceBattleEvent(bool isComment, params string[] logs)
	{
		_battleEventUtility.AddEvent(this, isComment, logs);
	}

	protected virtual void OnBattleEvent(BaseBattleCallback battleEvent)
	{
	}

	public int GetCampaignResultStar(bool win, DungeonDifficultyType difficulty)
	{
		if (!win)
		{
			return 0;
		}
		int num = 3;
		for (int i = 0; i < CreatureContainer.GetCreatureCount(); i++)
		{
			BattleCreature creatureByIndex = CreatureContainer.GetCreatureByIndex(i);
			if (creatureByIndex.StatController.IsDead() && creatureByIndex.TeamController.TeamType == TeamType.Left)
			{
				num--;
			}
		}
		num = System.Math.Max(1, num);
		return (int)difficulty * 10 + num;
	}

	public long GetBossMonsterDamagedAmount()
	{
		BattleCreature battleCreature = null;
		for (int i = 0; i < CreatureContainer.GetCreatureCount(); i++)
		{
			BattleCreature creatureByIndex = CreatureContainer.GetCreatureByIndex(i);
			if (creatureByIndex.TeamController.TeamType == TeamType.Right && creatureByIndex.CreateInfo.IsBoss)
			{
				battleCreature = creatureByIndex;
				break;
			}
		}
		if (battleCreature == null)
		{
			return 0L;
		}
		long num = 0L;
		if (battleCreature.StatController.GetMinHp() == 0L)
		{
			long num2 = battleCreature.StatController.GetMaxHp();
			ResultCreatureInfo initialInfo = battleCreature.InitialInfo;
			if (initialInfo != null && initialInfo.Hp != 0L)
			{
				num2 = ((initialInfo.Hp > 0) ? initialInfo.Hp : 0);
			}
			long hp = battleCreature.StatController.Hp;
			return System.Math.Max(num2 - hp, 0L);
		}
		return Snapshot.StatisticsManager.TotalDamagedMeter(battleCreature.Uid);
	}

	private void GraphLogSkillDelay(long globalTimeMs)
	{
	}

	private void AdjustKarmaCard()
	{
		KarmaManager.AdjustSelectCard();
	}
}
