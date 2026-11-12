using System;

namespace NShared;

public abstract class BattleModeUpdater
{
	private bool _isBattleEnd;

	private bool _isBattleIncompletelyEnd;

	private TeamType _winner;

	private IBattleModeState _state;

	public BattleInstance BattleInstance { get; private set; }

	public Action OnWaveEnd { get; private set; }

	public Action OnWaveReady { get; private set; }

	public Action<TeamType> OnWavePause { get; private set; }

	public Action OnBattleEnd { get; private set; }

	public bool BattleStarted { get; private set; }

	public int ChapterIndex { get; protected set; }

	public int DungeonIndex { get; protected set; }

	public DungeonDifficultyType Difficulty { get; private set; }

	public bool Scenario { get; private set; }

	public int WaveIndex { get; set; }

	public int MaxWaveIndex { get; set; }

	public int TotalWaveIndex { get; set; }

	public int MaxStageLevel { get; set; }

	public int ForcedRandomSeed { get; set; }

	public bool GiveUp { get; set; }

	public bool IsWavePause { get; set; }

	public BattleModeUpdater(BattleInstance battleInstance, int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty, bool scenario)
	{
		BattleInstance = battleInstance;
		ChapterIndex = chapterIndex;
		DungeonIndex = dungeonIndex;
		Difficulty = difficulty;
		Scenario = scenario;
		WaveIndex = 1;
		MaxWaveIndex = 0;
		MaxStageLevel = 0;
		InitTotalWaveIndex();
		ForcedRandomSeed = -1;
	}

	private void InitTotalWaveIndex()
	{
		TotalWaveIndex = 1;
		for (int i = 2; GameRoot.Instance.CampaignWaveDataContainer.GetData(ChapterIndex, DungeonIndex, Difficulty, Scenario, i) != null; i++)
		{
			TotalWaveIndex = i;
		}
	}

	public void SetWaveEndCallback(Action onWaveEnd)
	{
		OnWaveEnd = onWaveEnd;
	}

	public void SetWaveReadyCallback(Action onWaveReady)
	{
		OnWaveReady = onWaveReady;
	}

	public void SetWavePauseCallback(Action<TeamType> onWavePause)
	{
		OnWavePause = onWavePause;
	}

	public void SetBattleEndCallback(Action onBattleEnd)
	{
		OnBattleEnd = onBattleEnd;
	}

	public void StartBattle(bool started = true)
	{
		BattleStarted = started;
	}

	public void SetWavePause(bool isPause)
	{
		IsWavePause = isPause;
	}

	public void SetWavePauseFalse()
	{
		IsWavePause = false;
	}

	public void SetBattleEnd(bool incomplete)
	{
		_isBattleEnd = true;
		_isBattleIncompletelyEnd = incomplete;
		if (OnBattleEnd != null)
		{
			OnBattleEnd();
		}
	}

	public bool IsBattleEnd()
	{
		return _isBattleEnd;
	}

	public bool IsBattleIncompletelyEnd()
	{
		return _isBattleIncompletelyEnd;
	}

	public void SetWaveEnd(TeamType winner)
	{
		_winner = winner;
		if (OnWaveEnd != null)
		{
			OnWaveEnd();
		}
	}

	public TeamType GetWinner()
	{
		return _winner;
	}

	public void StartState(IBattleModeState state)
	{
		_state = state;
		state?.Init();
	}

	public virtual void Update()
	{
		if (_state != null)
		{
			IBattleModeState nextState = _state.GetNextState();
			if (nextState != null)
			{
				StartState(nextState);
			}
			else
			{
				_state.Update();
			}
		}
	}

	public virtual bool SetNextWave()
	{
		int waveIndex = WaveIndex + 1;
		int maxWaveIndex = MaxWaveIndex + 1;
		MaxWaveIndex = maxWaveIndex;
		BattleInstance.Snapshot.StatisticsManager.ResetCurrentWaveKill();
		if (GameRoot.Instance.CampaignWaveDataContainer.GetData(ChapterIndex, DungeonIndex, Difficulty, Scenario, waveIndex) != null)
		{
			WaveIndex = waveIndex;
			return true;
		}
		return false;
	}
}
