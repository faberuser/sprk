using System.Collections.Generic;
using NShared.NCampaign;

namespace NShared;

public class BattleInstanceBuilder
{
	public const int DefaultGameSpeedRatio = 1;

	private List<BattlePlayerCreateInfo> _playerCreateInfoList = new List<BattlePlayerCreateInfo>();

	private List<Tuple<long, BattleCreatureCreateInfo>> _creatureCreateInfoList = new List<Tuple<long, BattleCreatureCreateInfo>>();

	private List<IBattleLogRuntime> _enterPlayerBattleLogList = new List<IBattleLogRuntime>();

	private IBattleEventServiceContext _battleEventServiceContext;

	private IBattleEventRaiser _battleEventRaiser;

	private bool _enableSnapshot;

	private int _updateDeltaTimeMs;

	private int _chapterIndex;

	private int _dungeonIndex;

	private int _difficulty;

	private bool _isScenario;

	private int _gameSpeedRatio = 1;

	private bool _enableTraceLogToFile;

	private List<long> _traceAccountIds = new List<long>();

	private BattleInstanceExtraData _extraData;

	public int ChapterIndex => _chapterIndex;

	public int DungeonIndex => _dungeonIndex;

	public int Dfficulty => _difficulty;

	public void SetCampaignDungeon(int chapterIndex, int dungeonIndex, int difficulty, bool isScenario = false)
	{
		_chapterIndex = chapterIndex;
		_dungeonIndex = dungeonIndex;
		_difficulty = difficulty;
		_isScenario = isScenario;
	}

	public void SetGameSpeedRatio(int gameSpeedRatio)
	{
		if (gameSpeedRatio > 1)
		{
			_gameSpeedRatio = gameSpeedRatio;
		}
	}

	public BattleInstanceBuilder SetBattleEventContext(IBattleEventServiceContext context)
	{
		_battleEventServiceContext = context;
		return this;
	}

	public BattleInstanceBuilder SetBattleEventRaiser(IBattleEventRaiser battleEventRaiser)
	{
		_battleEventRaiser = battleEventRaiser;
		return this;
	}

	public BattleInstanceBuilder SetUpdateDeltaTimeMs(int timeMs)
	{
		_updateDeltaTimeMs = timeMs;
		return this;
	}

	public BattleInstanceBuilder AddPlayer(long playerId, TeamType teamType)
	{
		_playerCreateInfoList.Add(new BattlePlayerCreateInfo
		{
			Id = playerId,
			TeamType = teamType
		});
		return this;
	}

	public BattleInstanceBuilder AddMonsterForTest(long playerId, int creatureIndex, int star, int level, int randomSeed, int tier)
	{
		Tuple<long, BattleCreatureCreateInfo> item = new Tuple<long, BattleCreatureCreateInfo>(playerId, BattleCreatureCreateInfo.Create(creatureIndex, star, level, randomSeed, tier, MonsterGradeType.Normal, 0));
		_creatureCreateInfoList.Add(item);
		return this;
	}

	public BattleInstanceBuilder EnableSnapshot(bool enable)
	{
		_enableSnapshot = enable;
		return this;
	}

	public BattleInstanceBuilder SetExtraData(BattleInstanceExtraData extraData)
	{
		_extraData = extraData;
		return this;
	}

	public BattleInstanceBuilder EnableTraceLogToFile(bool isEnable)
	{
		_enableTraceLogToFile = isEnable;
		return this;
	}

	public void SetTraceBattleEventAccountId(long accountId)
	{
		if (!_traceAccountIds.Contains(accountId))
		{
			_traceAccountIds.Add(accountId);
		}
	}

	public T Build<T>() where T : BattleInstance, new()
	{
		T val = new T();
		val.SetBattleEventContext(_battleEventServiceContext);
		val.SetBattleEventRaiser(_battleEventRaiser);
		val.EnableSnapshot(_enableSnapshot);
		val.SetCampaignDungeon(_chapterIndex, _dungeonIndex, _difficulty, _isScenario);
		val.SetDeltaTimeReductionRatio(_gameSpeedRatio);
		val.SetExtraData(_extraData);
		if (_updateDeltaTimeMs > 0)
		{
			val.SetUpdateDeltaTimeMs(_updateDeltaTimeMs);
		}
		val.IsTraceLogToFile = _enableTraceLogToFile;
		foreach (long traceAccountId in _traceAccountIds)
		{
			val.SetTraceBattleEventAccountId(traceAccountId);
		}
		val.Init();
		return val;
	}

	public CampaignUpdater BuildUpdater<T>(int forcedMonsterRandomSeed = -1, int randomSeedByMonth = -1, int[] projectileGroupIndicesFromServer = null) where T : BattleInstance, new()
	{
		return BuildUpdater<T>(_chapterIndex, _dungeonIndex, (DungeonDifficultyType)_difficulty, _isScenario, forcedMonsterRandomSeed, randomSeedByMonth, projectileGroupIndicesFromServer);
	}

	public CampaignUpdater BuildUpdater<T>(int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty, bool isScenario, int forcedMonsterRandomSeed = -1, int randomSeedByMonth = -1, int[] projectileGroupIndicesFromServer = null) where T : BattleInstance, new()
	{
		T battleInstance = Build<T>();
		List<IBattleLogRuntime> list = _enterPlayerBattleLogList;
		if (list == null || list.Count <= 0)
		{
			list = MakeEnterPlayerBattleLogList();
		}
		return new CampaignUpdater(battleInstance, chapterIndex, dungeonIndex, difficulty, isScenario, list, randomSeedByMonth, projectileGroupIndicesFromServer)
		{
			ForcedRandomSeed = forcedMonsterRandomSeed
		};
	}

	private List<IBattleLogRuntime> MakeEnterPlayerBattleLogList()
	{
		List<IBattleLogRuntime> list = new List<IBattleLogRuntime>();
		for (int i = 0; i < _playerCreateInfoList.Count; i++)
		{
			BattlePlayerCreateInfo battlePlayerCreateInfo = _playerCreateInfoList[i];
			PlayerEnterBattleLog playerEnterBattleLog = new PlayerEnterBattleLog();
			playerEnterBattleLog.PlayerCreateInfo = battlePlayerCreateInfo;
			List<BattleCreatureCreateInfo> list2 = new List<BattleCreatureCreateInfo>();
			for (int j = 0; j < _creatureCreateInfoList.Count; j++)
			{
				Tuple<long, BattleCreatureCreateInfo> tuple = _creatureCreateInfoList[j];
				if (tuple.Item1 == battlePlayerCreateInfo.Id)
				{
					list2.Add(tuple.Item2);
				}
			}
			playerEnterBattleLog.CreatureCreateInfos = list2.ToArray();
			list.Add(playerEnterBattleLog);
		}
		return list;
	}

	public void SetEnterPlayerBattleLogList(List<IBattleLogRuntime> enterPlayerBattleLogList)
	{
		_enterPlayerBattleLogList = enterPlayerBattleLogList;
	}
}
