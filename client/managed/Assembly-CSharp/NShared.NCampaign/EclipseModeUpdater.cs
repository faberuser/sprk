using System;
using System.Collections.Generic;

namespace NShared.NCampaign;

public class EclipseModeUpdater : CampaignUpdater
{
	private Dictionary<int, List<BattleCreatureCreateInfo>> CreatureCreateBattleLogList = new Dictionary<int, List<BattleCreatureCreateInfo>>();

	private EclipseDungeonData[] DungeonList;

	private long _matchAccountId;

	private int _currentDeckIndex;

	private int _currentLevel;

	private static int EclipseDungeonIncreaseLevelStep = -1;

	private const string EclipseDungeonIncreaseLevelStepKey = "EclipseDungeonIncreaseLevelStep";

	public int InitialDungeonIndex { get; private set; }

	public int InitialChapterIndex { get; private set; }

	public int InitialWaveIndex { get; private set; }

	public int InitialMaxWaveIndex { get; private set; }

	public int InitialLevel { get; private set; }

	public bool IsContinuousBattle { get; private set; }

	public EclipseModeUpdater(BattleInstance battleInstance, int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty, bool scenario, List<IBattleLogRuntime> initialBattleLogList, int randomSeedByMonth, int[] projectileGroupIndicesFromServer = null)
		: base(battleInstance, chapterIndex, dungeonIndex, difficulty, scenario, initialBattleLogList, randomSeedByMonth, projectileGroupIndicesFromServer)
	{
	}

	public void SetAllEclipseDeck(Dictionary<int, List<BattleCreatureCreateInfo>> deckList)
	{
		CreatureCreateBattleLogList = deckList;
	}

	public void AddDeck(int deckIndex, List<BattleCreatureCreateInfo> deckList)
	{
		if (CreatureCreateBattleLogList.ContainsKey(deckIndex))
		{
			CreatureCreateBattleLogList[deckIndex] = deckList;
		}
		else
		{
			CreatureCreateBattleLogList.Add(deckIndex, deckList);
		}
	}

	public void SetAllDungeonData(EclipseDungeonData[] dungeonList)
	{
		DungeonList = dungeonList;
	}

	public void SetCurrentDeckIndex(int index)
	{
		_currentDeckIndex = index;
	}

	public int GetCurrentDeckIndex()
	{
		return _currentDeckIndex;
	}

	public void IncreaseCurrentDeckIndex()
	{
		_currentDeckIndex++;
	}

	public void SetMaxWaveIndex(int index)
	{
		base.MaxWaveIndex = index;
	}

	public int GetMaxWaveIndex()
	{
		return base.MaxWaveIndex;
	}

	public void SetContinousBattle(bool isContinuous)
	{
		IsContinuousBattle = isContinuous;
	}

	public void SetInitialChapterIndex(int chapterIndex)
	{
		InitialChapterIndex = chapterIndex;
	}

	public void SetInitialDungeonIndex(int dungeonIndex)
	{
		InitialDungeonIndex = dungeonIndex;
	}

	public void SetInitialWaveIndex(int waveIndex)
	{
		InitialWaveIndex = waveIndex;
	}

	public void SetInitialMaxWaveIndex(int maxWaveIndex)
	{
		InitialMaxWaveIndex = maxWaveIndex;
	}

	public void InitializeDungeonSetting()
	{
		base.ChapterIndex = InitialChapterIndex;
		base.DungeonIndex = InitialDungeonIndex;
		base.WaveIndex = InitialWaveIndex;
		base.MaxWaveIndex = InitialMaxWaveIndex;
		_currentLevel = 0;
	}

	public void SetWaveIndex(int index)
	{
		base.WaveIndex = index;
	}

	public bool TrySettingNextDungeon()
	{
		if (DungeonList == null)
		{
			return false;
		}
		int num = Array.FindIndex(DungeonList, (EclipseDungeonData data) => data.ChapterIndex == base.ChapterIndex && data.DungeonIndex == base.DungeonIndex);
		if (num <= -1 || num + 1 >= DungeonList.Length)
		{
			return false;
		}
		EclipseDungeonData eclipseDungeonData = DungeonList[num + 1];
		base.ChapterIndex = eclipseDungeonData.ChapterIndex;
		base.DungeonIndex = eclipseDungeonData.DungeonIndex;
		ResetWaveIndex();
		Log("Next Eclipse : chapter=" + base.ChapterIndex + " dungeon=" + base.DungeonIndex + " waveIndex=" + base.WaveIndex);
		return true;
	}

	public void ResetWaveIndex()
	{
		base.WaveIndex = 1;
	}

	public int IncreaseCreatureLevel()
	{
		_currentLevel++;
		return _currentLevel;
	}

	public int GetCurrentCreatureLevel()
	{
		return _currentLevel;
	}

	public List<BattleCreatureCreateInfo> PeekNextHeroDeck()
	{
		_currentDeckIndex++;
		List<BattleCreatureCreateInfo> currentHeroDeck = GetCurrentHeroDeck();
		_currentDeckIndex--;
		return currentHeroDeck;
	}

	public List<BattleCreatureCreateInfo> GetCurrentHeroDeck()
	{
		if (!CreatureCreateBattleLogList.ContainsKey(_currentDeckIndex))
		{
			return null;
		}
		return CreatureCreateBattleLogList[_currentDeckIndex];
	}

	public void Log(object message)
	{
	}

	public static int GetEclipseDungeonIncreaseLevelStep()
	{
		if (EclipseDungeonIncreaseLevelStep <= -1)
		{
			EclipseDungeonIncreaseLevelStep = GameRoot.Instance.ConstantDataContainer.GetValueInt("EclipseDungeonIncreaseLevelStep", 5);
		}
		return EclipseDungeonIncreaseLevelStep;
	}

	public void SetMatchAccountId(long matchAccountId)
	{
		_matchAccountId = matchAccountId;
	}

	public long GetMatchAccountId()
	{
		return _matchAccountId;
	}
}
