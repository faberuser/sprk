using System;
using System.Collections.Generic;
using System.Linq;
using NShared.NCampaign;
using NShared.NUtil;

namespace NShared;

public class BattleSimulationInstance
{
	private CampaignUpdater _updater;

	private BattleInstance _battleInstance;

	private BattleSimulationInfo _simulationInfo;

	private BattleSimulationResultInfo _resultInfo;

	private const int DeltaTimeRatio = 100;

	public string UID { get; private set; }

	public BattleInstance BattleInstance => _battleInstance;

	public bool IsBattleStart { get; private set; }

	public bool IsBattleEnd { get; private set; }

	public bool IsBattleStopped { get; private set; }

	public DateTime BattleStartTime { get; private set; }

	public DateTime BattleEndTime { get; private set; }

	public void Init(string uid, BattleSimulationInfo simulationInfo)
	{
		UID = uid;
		_simulationInfo = simulationInfo;
	}

	public void Init(BattleSimulationInfo simulationInfo)
	{
		Init("Test", simulationInfo);
	}

	public void BattleStart(bool enableBattleLog = false)
	{
		if (_simulationInfo == null)
		{
			TestBattleStart();
			return;
		}
		int chapterIndex = _simulationInfo.ChapterIndex;
		int dungeonIndex = _simulationInfo.DungeonIndex;
		int dungeonDifficulty = _simulationInfo.DungeonDifficulty;
		BattleInstanceExtraData extraData = _simulationInfo.ExtraData;
		BattleInstanceBuilder battleInstanceBuilder = CreateBattleInstance(chapterIndex, dungeonIndex, dungeonDifficulty, extraData);
		List<IBattleLogRuntime> enterPlayerBattleLogList = CreateEnterBattleLog(_simulationInfo.AccountInfos);
		battleInstanceBuilder.SetEnterPlayerBattleLogList(enterPlayerBattleLogList);
		SettingBattleStart(battleInstanceBuilder, enableBattleLog);
	}

	public void Stop()
	{
		IsBattleStart = false;
		IsBattleEnd = true;
		IsBattleStopped = true;
	}

	public void TestBattleStart()
	{
		int chapterIndex = 7;
		int dungeonIndex = 1;
		int difficulty = 0;
		TestBattleStart(chapterIndex, dungeonIndex, difficulty);
	}

	public void TestBattleStart(int chapterIndex, int dungeonIndex, int difficulty)
	{
		BattleInstanceBuilder battleInstanceBuilder = CreateBattleInstance(chapterIndex, dungeonIndex, difficulty, null);
		int num = 1;
		int star = 5;
		int level = 90;
		battleInstanceBuilder.AddPlayer(num, TeamType.Left);
		battleInstanceBuilder.AddMonsterForTest(num, 1, star, level, 0, 1);
		battleInstanceBuilder.AddMonsterForTest(num, 2, star, level, 0, 1);
		battleInstanceBuilder.AddMonsterForTest(num, 3, star, level, 0, 1);
		battleInstanceBuilder.AddMonsterForTest(num, 4, star, level, 0, 1);
		SettingBattleStart(battleInstanceBuilder);
	}

	private BattleInstanceBuilder CreateBattleInstance(int chapterIndex, int dungeonIndex, int difficulty, BattleInstanceExtraData extraData)
	{
		BattleInstanceBuilder battleInstanceBuilder = new BattleInstanceBuilder();
		battleInstanceBuilder.SetCampaignDungeon(chapterIndex, dungeonIndex, difficulty);
		battleInstanceBuilder.SetExtraData(extraData);
		battleInstanceBuilder.EnableSnapshot(enable: false);
		return battleInstanceBuilder;
	}

	private void SettingBattleStart(BattleInstanceBuilder builder, bool enableBattleLog = false)
	{
		BattleStartTime = DateTime.Now;
		BattleEndTime = DateTime.Now;
		int dungeonIndex = builder.DungeonIndex;
		int randomSeedByMonth = SprkDispatch.Seed(RandomTable.GetRandomByMonth(Time.GetMonthFromMillennium(), dungeonIndex), UID);
		_updater = builder.BuildUpdater<BattleInstance>(-1, randomSeedByMonth);
		_battleInstance = _updater.BattleInstance;
		IsBattleStart = true;
		IsBattleEnd = false;
		IsBattleStopped = false;
		_battleInstance.SetDeltaTimeReductionRatio(100);
		_updater.StartBattle();
	}

	private List<IBattleLogRuntime> CreateEnterBattleLog(BattleSimulationAccountInfo[] infos)
	{
		List<IBattleLogRuntime> list = new List<IBattleLogRuntime>();
		if (infos == null)
		{
			return list;
		}
		lock (infos)
		{
			for (int i = 0; i < infos.Count(); i++)
			{
				PlayerEnterBattleLog playerEnterBattleLog = new PlayerEnterBattleLog();
				BattleSimulationAccountInfo battleSimulationAccountInfo = infos[i];
				playerEnterBattleLog.PlayerCreateInfo = new BattlePlayerCreateInfo
				{
					Id = battleSimulationAccountInfo.UserInfo.AccountId,
					TeamType = TeamType.Left
				};
				BattleCreatureCreateInfo.BuffSkillInfo buffSkillInfo = new BattleCreatureCreateInfo.BuffSkillInfo
				{
					AccountBuffDataBases = battleSimulationAccountInfo.AccountBuffs,
					GuildSkillDataBases = battleSimulationAccountInfo.GuildSkills,
					ClassBuffDataBasess = battleSimulationAccountInfo.ClassBuffDataBases
				};
				BattleCreatureCreateInfo.BuffStatInfo buffStatInfo = new BattleCreatureCreateInfo.BuffStatInfo
				{
					ExtraStatDatas = battleSimulationAccountInfo.ExtraStatDatas,
					PetStatDataBases = battleSimulationAccountInfo.PetStatDataBases
				};
				List<BattleCreatureCreateInfo> list2 = new List<BattleCreatureCreateInfo>();
				int num = 0;
				foreach (KeyValuePair<int, CachedHeroInfo> heroInfo in battleSimulationAccountInfo.HeroInfos)
				{
					CachedHeroInfo value = heroInfo.Value;
					CachedHeroInfo cachedHeroInfo = new CachedHeroInfo(value, value.GetEquipItemInfoList());
					cachedHeroInfo.Validate();
					BattleCreatureCreateInfo item = BattleCreatureCreateInfo.CreateMyCreatureCreateInfo(cachedHeroInfo, isLeader: false, isAi: true, buffSkillInfo, buffStatInfo);
					list2.Add(item);
					num++;
				}
				playerEnterBattleLog.CreatureCreateInfos = list2.ToArray();
				list.Add(playerEnterBattleLog);
			}
			return list;
		}
	}

	public void Update()
	{
		if (IsBattleStart && !IsBattleEnd)
		{
			_updater.Update();
			if (!_updater.IsBattleEnd() && !_updater.BattleStarted)
			{
				_updater.StartBattle();
			}
			_battleInstance.UpdateTick(snapshotEnabled: false);
			SnapshotObjectPoolContainer.FlushPendingRelease();
			if (IsBattleStopped)
			{
				OnBattleStopped();
			}
			else if (_updater.IsBattleEnd())
			{
				OnBattleEnd();
			}
		}
	}

	private void OnBattleStopped()
	{
		BattleEndTime = DateTime.Now;
		_resultInfo = new BattleSimulationResultInfo
		{
			Uid = UID,
			IsWin = false,
			RealTime = 0L
		};
	}

	private void OnBattleEnd()
	{
		IsBattleStart = false;
		IsBattleEnd = true;
		IsBattleStopped = false;
		BattleEndTime = DateTime.Now;
		_resultInfo = new BattleSimulationResultInfo
		{
			Uid = UID,
			IsWin = IsWin(),
			RealTime = GetPurePlayTimeMainTimer()
		};
	}

	public BattleSimulationResultInfo GetBattleSimulationResultInfo()
	{
		return _resultInfo;
	}

	public bool IsWin()
	{
		if (_updater == null)
		{
			return false;
		}
		return _updater.GetWinner() == TeamType.Left;
	}

	public long GetPurePlayTime()
	{
		if (_battleInstance == null || _battleInstance.GlobalTimer == null)
		{
			return 0L;
		}
		return _battleInstance.GlobalTimer.GetCurrentTimeMs();
	}

	public long GetPurePlayTimeMainTimer()
	{
		if (_battleInstance == null || _battleInstance.MainTimer == null)
		{
			return 0L;
		}
		return _battleInstance.MainTimer.GetCurrentTimeMs();
	}

	public long GetPlayTime()
	{
		if (_battleInstance == null)
		{
			return 0L;
		}
		return _battleInstance.GetElapsedBattleTimeMs();
	}

	public long GetSimulationTime()
	{
		if (!IsBattleEnd)
		{
			if (!IsBattleStart)
			{
				return 0L;
			}
			return (long)(DateTime.Now - BattleStartTime).TotalMilliseconds;
		}
		return (long)(BattleEndTime - BattleStartTime).TotalMilliseconds;
	}

	public void Clear()
	{
		_updater = null;
		_battleInstance = null;
		_simulationInfo = null;
	}
}
