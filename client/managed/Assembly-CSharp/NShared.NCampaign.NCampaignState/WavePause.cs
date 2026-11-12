using System.Collections.Generic;
using System.Linq;

namespace NShared.NCampaign.NCampaignState;

public class WavePause : CampaignState
{
	private enum EclipseUpdaterConnected
	{
		None,
		Online,
		Offline
	}

	private TeamType _winner;

	private List<BattleLog> _battleLogs;

	private bool battleLogProcessDone;

	private EclipseUpdaterConnected Connected;

	private EclipseOfflineModeUpdater eclipseOfflineUpdater;

	private EclipseModeUpdater eclipseOnlineUpdater;

	private bool CheckEachWaveEachTeam
	{
		get
		{
			if (_updater == null)
			{
				return false;
			}
			if (_updater.BattleInstance == null)
			{
				return false;
			}
			return _updater.BattleInstance.CheckEachWaveEachTeam;
		}
	}

	private List<BattleCreatureCreateInfo> BattleCreatureCreateInfoList
	{
		get
		{
			if (_updater is EclipseModeUpdater eclipseModeUpdater)
			{
				return eclipseModeUpdater.GetCurrentHeroDeck();
			}
			if (CheckEachWaveEachTeam)
			{
				if (!(_updater is CampaignUpdater { EachWaveEachTeamBattleCreatureCreateInfoList: var eachWaveEachTeamBattleCreatureCreateInfoList }))
				{
					return null;
				}
				if (eachWaveEachTeamBattleCreatureCreateInfoList == null || eachWaveEachTeamBattleCreatureCreateInfoList.Count < _updater.WaveIndex)
				{
					return null;
				}
				return eachWaveEachTeamBattleCreatureCreateInfoList[_updater.WaveIndex - 1];
			}
			return null;
		}
	}

	public WavePause(TeamType winner = TeamType.Right)
	{
		_winner = winner;
	}

	public override void OnInit()
	{
		base.OnInit();
		if (_updater is EclipseOfflineModeUpdater)
		{
			Connected = EclipseUpdaterConnected.Offline;
			eclipseOfflineUpdater = _updater as EclipseOfflineModeUpdater;
		}
		else
		{
			if (!(_updater is EclipseModeUpdater))
			{
				Connected = EclipseUpdaterConnected.None;
				return;
			}
			Connected = EclipseUpdaterConnected.Offline;
			eclipseOnlineUpdater = _updater as EclipseModeUpdater;
		}
		if (Connected == EclipseUpdaterConnected.Offline)
		{
			if (_winner == TeamType.Left)
			{
				AddNextDungeonBattleLog(eclipseOfflineUpdater);
			}
		}
		else if (_winner == TeamType.Left)
		{
			AddNextDungeonBattleLog();
		}
		else
		{
			InitializeDungeonWithNextTeam(eclipseOnlineUpdater);
		}
	}

	private void AddNextDungeonBattleLog(EclipseOfflineModeUpdater eclipseModeUpdater)
	{
		if (!eclipseModeUpdater.TrySettingNextDungeon())
		{
			eclipseModeUpdater.ResetWaveIndex();
			int level = eclipseModeUpdater.IncreaseCreatureLevel();
			int eclipseDungeonIncreaseLevelStep = EclipseOfflineModeUpdater.GetEclipseDungeonIncreaseLevelStep();
			if (!CanIncreseCreatureLevel(level, eclipseDungeonIncreaseLevelStep))
			{
				_updater.SetBattleEnd(incomplete: false);
				return;
			}
		}
		if (_winner == TeamType.Left)
		{
			AddNextDungeonBattleLog();
		}
		NextDungeonStartBattleLog runtime = new NextDungeonStartBattleLog
		{
			ChapterIndex = eclipseModeUpdater.ChapterIndex,
			DungeonIndex = eclipseModeUpdater.DungeonIndex
		};
		BattleLog item = _updater.BattleInstance.BattleLogContainer.AddLog(runtime);
		_battleLogs = new List<BattleLog>();
		_battleLogs.Add(item);
	}

	private void AddNextDungeonBattleLog()
	{
		if (!(_updater is EclipseModeUpdater eclipseModeUpdater))
		{
			return;
		}
		if (!eclipseModeUpdater.TrySettingNextDungeon())
		{
			eclipseModeUpdater.ResetWaveIndex();
			int level = eclipseModeUpdater.IncreaseCreatureLevel();
			int eclipseDungeonIncreaseLevelStep = EclipseModeUpdater.GetEclipseDungeonIncreaseLevelStep();
			if (!CanIncreseCreatureLevel(level, eclipseDungeonIncreaseLevelStep))
			{
				_updater.SetBattleEnd(incomplete: false);
				return;
			}
		}
		NextDungeonStartBattleLog runtime = new NextDungeonStartBattleLog
		{
			ChapterIndex = eclipseModeUpdater.ChapterIndex,
			DungeonIndex = eclipseModeUpdater.DungeonIndex
		};
		BattleLog item = _updater.BattleInstance.BattleLogContainer.AddLog(runtime);
		_battleLogs = new List<BattleLog>();
		_battleLogs.Add(item);
	}

	private void InitializeDungeonWithNextTeam(EclipseModeUpdater eclipseModeUpdater)
	{
		if (eclipseModeUpdater.PeekNextHeroDeck() == null)
		{
			_updater.SetBattleEnd(incomplete: false);
			return;
		}
		if (eclipseModeUpdater.OnWavePause != null)
		{
			eclipseModeUpdater.OnWavePause(_winner);
		}
		eclipseModeUpdater.IncreaseCurrentDeckIndex();
		eclipseModeUpdater.InitializeDungeonSetting();
	}

	private bool CanIncreseCreatureLevel(int level, int step)
	{
		int chapterIndex = _updater.ChapterIndex;
		int dungeonIndex = _updater.DungeonIndex;
		DungeonDifficultyType difficulty = _updater.Difficulty;
		bool scenario = _updater.Scenario;
		short maxLevel = GameRoot.Instance.CreatureLevelFactorDataContainer.GetMaxLevel(CreatureLevelFactorData.FactorType.Monster);
		int num = 1;
		while (true)
		{
			CampaignWaveData data = GameRoot.Instance.CampaignWaveDataContainer.GetData(chapterIndex, dungeonIndex, difficulty, scenario, num);
			if (data == null)
			{
				break;
			}
			if (data.Level + level * step > maxLevel)
			{
				return false;
			}
			foreach (CampaignWaveData.CreatureDetailInfo creature in data.GetCreatureList())
			{
				if (creature.Level != data.Level && creature.Level + level * step > maxLevel)
				{
					return false;
				}
			}
			num++;
		}
		return true;
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (Connected == EclipseUpdaterConnected.Offline)
		{
			if (_winner == TeamType.Left)
			{
				WaitUntilDungeonSetting(eclipseOfflineUpdater);
			}
		}
		else if (Connected == EclipseUpdaterConnected.Online)
		{
			if (_winner == TeamType.Left)
			{
				WaitUntilDungeonSetting(eclipseOnlineUpdater);
			}
		}
		else if (_winner == TeamType.Left && CheckEachWaveEachTeam)
		{
			AddInitializeDungeonBattleLog();
		}
	}

	private void InitializeDungeonWithNextTeam()
	{
		EclipseModeUpdater eclipseModeUpdater = _updater as EclipseModeUpdater;
		if (eclipseModeUpdater != null && eclipseModeUpdater.PeekNextHeroDeck() == null)
		{
			_updater.SetBattleEnd(incomplete: false);
			return;
		}
		if (_updater.OnWavePause != null)
		{
			_updater.OnWavePause(_winner);
		}
		if (eclipseModeUpdater != null)
		{
			eclipseModeUpdater.IncreaseCurrentDeckIndex();
			eclipseModeUpdater.InitializeDungeonSetting();
		}
	}

	private void WaitUntilDungeonSetting(EclipseOfflineModeUpdater eclipseModeUpdater)
	{
		if (!battleLogProcessDone)
		{
			if (_battleLogs == null || !_battleLogs.All((BattleLog log) => log.Processed))
			{
				return;
			}
			if (eclipseModeUpdater.OnWavePause != null)
			{
				eclipseModeUpdater.OnWavePause(_winner);
			}
			battleLogProcessDone = true;
		}
		if (!_updater.IsWavePause)
		{
			EndState(new WaveInit());
		}
	}

	private void WaitUntilDungeonSetting(EclipseModeUpdater eclipseModeUpdater)
	{
		if (eclipseModeUpdater == null)
		{
			return;
		}
		if (!battleLogProcessDone)
		{
			if (_battleLogs == null || !_battleLogs.All((BattleLog log) => log.Processed))
			{
				return;
			}
			if (eclipseModeUpdater.OnWavePause != null)
			{
				eclipseModeUpdater.OnWavePause(_winner);
			}
			battleLogProcessDone = true;
		}
		if (!_updater.IsWavePause)
		{
			EndState(new WaveInit());
		}
	}

	private void AddInitializeDungeonBattleLog()
	{
		if (_updater.IsWavePause)
		{
			return;
		}
		if (_battleLogs == null && _updater != null && _updater.BattleInstance != null)
		{
			BattleLogCreator battleLogCreator = new BattleLogCreator(_updater.BattleInstance);
			_battleLogs = battleLogCreator.Create(BattleCreatureCreateInfoList);
		}
		if (_battleLogs.All((BattleLog log) => log.Processed))
		{
			if (CheckEachWaveEachTeam && _updater.OnWavePause != null)
			{
				_updater.OnWavePause(_winner);
			}
			EndState(new WaveInit());
		}
	}
}
