using NShared.NUtil;

namespace NShared;

public class BattleInstanceSnapshot : SnapshotObject
{
	private readonly SnapshotObjectMember<int> _creatureUidHolder;

	private readonly SnapshotObjectMember<int> _processedBattleLogNoHolder;

	private readonly SnapshotObjectMember<bool> _battleClosingHolder;

	private readonly SnapshotObjectMember<bool> _battleTimeoutedHolder;

	private readonly SnapshotObjectMember<bool> _battleEnabledHolder;

	private readonly SnapshotObjectMember<bool> _battleStartedHolder;

	private readonly SnapshotObjectMember<bool> _battleEndedHolder;

	private readonly SnapshotObjectMember<long> _waveStartTimeMsHolder;

	private readonly SnapshotObjectMember<int> _chapterIndexHolder;

	private readonly SnapshotObjectMember<int> _dungeonIndexHolder;

	private readonly SnapshotObjectMember<int> _waveIndexHolder;

	private readonly SnapshotObjectMember<int> _totalWaveIndexHolder;

	private readonly SnapshotObjectMember<int> _maxWaveIndexHolder;

	private readonly SnapshotObjectMember<int> _maxStageLevelHolder;

	private readonly SnapshotObjectMember<int> _phaseHolder;

	private readonly SnapshotObjectMember<BattleStatisticsManager> _statisticsManagerHolder;

	private readonly SnapshotObjectMember<ProjectileContainer> _projectileContainerHolder;

	private readonly SnapshotObjectMember<TacticManager> _tacticManagerHolder;

	private readonly SnapshotObjectMember<Team> _leftTeamHolder;

	private readonly SnapshotObjectMember<Team> _rightTeamHolder;

	private readonly SnapshotObjectMember<bool> _withdrawHolder;

	private readonly SnapshotObjectMember<bool> _giveUpHolder;

	private readonly SnapshotObjectMember<bool> _cancelHolder;

	private readonly SnapshotObjectMember<TeamType> _giveUpWinTeamTypeHolder;

	private readonly SnapshotObjectMember<IndexableSnapshotDictionary<int, Vector3>> _waveCenter;

	public int CreatureUid
	{
		get
		{
			return _creatureUidHolder.Get();
		}
		set
		{
			_creatureUidHolder.Set(value);
		}
	}

	public int ProcessedBattleLogNo
	{
		get
		{
			return _processedBattleLogNoHolder.Get();
		}
		set
		{
			_processedBattleLogNoHolder.Set(value);
		}
	}

	public bool BattleClosing
	{
		get
		{
			return _battleClosingHolder.Get();
		}
		set
		{
			_battleClosingHolder.Set(value);
		}
	}

	public bool BattleTimeouted
	{
		get
		{
			return _battleTimeoutedHolder.Get();
		}
		set
		{
			_battleTimeoutedHolder.Set(value);
		}
	}

	public bool BattleEnabled
	{
		get
		{
			return _battleEnabledHolder.Get();
		}
		set
		{
			_battleEnabledHolder.Set(value);
		}
	}

	public bool BattleStarted
	{
		get
		{
			return _battleStartedHolder.Get();
		}
		set
		{
			_battleStartedHolder.Set(value);
		}
	}

	public bool BattleEnded
	{
		get
		{
			return _battleEndedHolder.Get();
		}
		set
		{
			_battleEndedHolder.Set(value);
		}
	}

	public long WaveStartTimeMs
	{
		get
		{
			return _waveStartTimeMsHolder.Get();
		}
		set
		{
			_waveStartTimeMsHolder.Set(value);
		}
	}

	public int ChapterIndex
	{
		get
		{
			return _chapterIndexHolder.Get();
		}
		set
		{
			_chapterIndexHolder.Set(value);
		}
	}

	public int DungeonIndex
	{
		get
		{
			return _dungeonIndexHolder.Get();
		}
		set
		{
			_dungeonIndexHolder.Set(value);
		}
	}

	public int WaveIndex
	{
		get
		{
			return _waveIndexHolder.Get();
		}
		set
		{
			_waveIndexHolder.Set(value);
		}
	}

	public int TotalWaveIndex
	{
		get
		{
			return _totalWaveIndexHolder.Get();
		}
		set
		{
			_totalWaveIndexHolder.Set(value);
		}
	}

	public int MaxWaveIndex
	{
		get
		{
			return _maxWaveIndexHolder.Get();
		}
		set
		{
			_maxWaveIndexHolder.Set(value);
		}
	}

	public int MaxStageLevel
	{
		get
		{
			return _maxStageLevelHolder.Get();
		}
		set
		{
			_maxStageLevelHolder.Set(value);
		}
	}

	public int Phase
	{
		get
		{
			return _phaseHolder.Get();
		}
		set
		{
			_phaseHolder.Set(value);
		}
	}

	public BattleStatisticsManager StatisticsManager
	{
		get
		{
			return _statisticsManagerHolder.Get();
		}
		private set
		{
			_statisticsManagerHolder.Set(value);
		}
	}

	public ProjectileContainer ProjectileContainer
	{
		get
		{
			return _projectileContainerHolder.Get();
		}
		private set
		{
			_projectileContainerHolder.Set(value);
		}
	}

	public TacticManager TacticManager
	{
		get
		{
			return _tacticManagerHolder.Get();
		}
		private set
		{
			_tacticManagerHolder.Set(value);
		}
	}

	public Team LeftTeam => _leftTeamHolder.Get();

	public Team RightTeam => _rightTeamHolder.Get();

	public bool Withdraw
	{
		get
		{
			return _withdrawHolder.Get();
		}
		set
		{
			_withdrawHolder.Set(value);
		}
	}

	public bool GiveUp
	{
		get
		{
			return _giveUpHolder.Get();
		}
		set
		{
			_giveUpHolder.Set(value);
		}
	}

	public bool Cancel
	{
		get
		{
			return _cancelHolder.Get();
		}
		set
		{
			_cancelHolder.Set(value);
		}
	}

	public TeamType GiveUpWinTeamType
	{
		get
		{
			return _giveUpWinTeamTypeHolder.Get();
		}
		set
		{
			_giveUpWinTeamTypeHolder.Set(value);
		}
	}

	public IndexableSnapshotDictionary<int, Vector3> WaveCenter => _waveCenter.Get();

	public BattleInstanceSnapshot()
	{
		_creatureUidHolder = new SnapshotObjectMember<int>(this, 0);
		_processedBattleLogNoHolder = new SnapshotObjectMember<int>(this, 0);
		_battleClosingHolder = new SnapshotObjectMember<bool>(this, value: false);
		_battleTimeoutedHolder = new SnapshotObjectMember<bool>(this, value: false);
		_battleEnabledHolder = new SnapshotObjectMember<bool>(this, value: false);
		_battleStartedHolder = new SnapshotObjectMember<bool>(this, value: false);
		_battleEndedHolder = new SnapshotObjectMember<bool>(this, value: false);
		_waveStartTimeMsHolder = new SnapshotObjectMember<long>(this, 0L);
		_chapterIndexHolder = new SnapshotObjectMember<int>(this, 0);
		_dungeonIndexHolder = new SnapshotObjectMember<int>(this, 0);
		_waveIndexHolder = new SnapshotObjectMember<int>(this, 0);
		_totalWaveIndexHolder = new SnapshotObjectMember<int>(this, 0);
		_maxWaveIndexHolder = new SnapshotObjectMember<int>(this, 0);
		_maxStageLevelHolder = new SnapshotObjectMember<int>(this, 0);
		_phaseHolder = new SnapshotObjectMember<int>(this, 0);
		_statisticsManagerHolder = new SnapshotObjectMember<BattleStatisticsManager>(this);
		_projectileContainerHolder = new SnapshotObjectMember<ProjectileContainer>(this);
		_tacticManagerHolder = new SnapshotObjectMember<TacticManager>(this);
		_leftTeamHolder = new SnapshotObjectMember<Team>(this);
		_rightTeamHolder = new SnapshotObjectMember<Team>(this);
		_withdrawHolder = new SnapshotObjectMember<bool>(this, value: false);
		_giveUpHolder = new SnapshotObjectMember<bool>(this, value: false);
		_cancelHolder = new SnapshotObjectMember<bool>(this, value: false);
		_giveUpWinTeamTypeHolder = new SnapshotObjectMember<TeamType>(this, TeamType.Left);
		_waveCenter = new SnapshotObjectMember<IndexableSnapshotDictionary<int, Vector3>>(this);
	}

	public void Init(BattleInstance battleInstance)
	{
		_statisticsManagerHolder.Alloc().Init(battleInstance);
		_projectileContainerHolder.Alloc().Init(battleInstance);
		_tacticManagerHolder.Alloc().Init(battleInstance);
		_leftTeamHolder.Alloc().Init(TeamType.Left, rightDirection: true);
		_rightTeamHolder.Alloc().Init(TeamType.Right, rightDirection: false);
		_leftTeamHolder.Get().SetEnemyTeam(_rightTeamHolder.Get());
		_rightTeamHolder.Get().SetEnemyTeam(_leftTeamHolder.Get());
		_leftTeamHolder.Get().InitEnvironmentCreature(battleInstance);
		_rightTeamHolder.Get().InitEnvironmentCreature(battleInstance);
		_waveCenter.Alloc().Clear();
	}

	public void Update(long currentGlobalTimeMs, int deltaTimeMs)
	{
		_leftTeamHolder.Get().EnvironmentCreature?.Update(currentGlobalTimeMs, deltaTimeMs);
		_rightTeamHolder.Get().EnvironmentCreature?.Update(currentGlobalTimeMs, deltaTimeMs);
		_projectileContainerHolder.Get().Update(currentGlobalTimeMs, deltaTimeMs);
		_tacticManagerHolder.Get().Update(currentGlobalTimeMs, deltaTimeMs);
	}
}
