using System;
using System.Collections.Generic;

namespace NShared.NCampaign.NCampaignState;

public class WavePlaying : CampaignState
{
	public static bool DisableWinDecider;

	public static bool ForceDecideWinner;

	public static TeamType WinnerTeamType;

	private bool _startBattleLogProcessed;

	private bool _rageModeBattleLogCreated;

	private List<BattleLog> _delayedCreatureBattleLogList = new List<BattleLog>();

	private HashSet<int> _wakingUpCreatureUidList = new HashSet<int>();

	private CampaignWaveData _waveData;

	private IWinDecider _winDecider;

	public override void OnInit()
	{
		base.OnInit();
		int chapterIndex = _updater.ChapterIndex;
		int dungeonIndex = _updater.DungeonIndex;
		DungeonDifficultyType difficulty = _updater.Difficulty;
		bool scenario = _updater.Scenario;
		int waveIndex = _updater.WaveIndex;
		_waveData = GameRoot.Instance.CampaignWaveDataContainer.GetData(chapterIndex, dungeonIndex, difficulty, scenario, waveIndex);
		_winDecider = ((_waveData != null) ? GameRoot.Instance.WinDeciderFactory.Create(_waveData.WinDeciderType, _waveData.WinDeciderValue) : null);
		_winDecider = _winDecider ?? GameRoot.Instance.WinDeciderFactory.Create(WinDeciderType.DefeatAllEnemy, null);
		WaveStartBattleLog runtime = new WaveStartBattleLog();
		_updater.BattleInstance.BattleLogContainer.AddLog(runtime).OnProcessed = delegate
		{
			_startBattleLogProcessed = true;
		};
		_delayedCreatureBattleLogList.Clear();
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (!_startBattleLogProcessed)
		{
			return;
		}
		DelayedSpawnType delayedSpawnType = ((_waveData != null) ? _waveData.DelayedSpawnType : DelayedSpawnType.AdvanceSpawnTime);
		int aliveCreatureCountOfTeam = _updater.BattleInstance.CreatureContainer.GetAliveCreatureCountOfTeam(TeamType.Left);
		int aliveCreatureCountOfTeam2 = _updater.BattleInstance.CreatureContainer.GetAliveCreatureCountOfTeam(TeamType.Right);
		bool flag = aliveCreatureCountOfTeam == 0 || aliveCreatureCountOfTeam2 == 0;
		long currentTimeMs = _updater.BattleInstance.MainTimer.GetCurrentTimeMs();
		currentTimeMs = Math.Max(currentTimeMs - _updater.BattleInstance.Snapshot.WaveStartTimeMs, 0L);
		BattleCreatureList restingCreatureOfTeam = _updater.BattleInstance.CreatureContainer.GetRestingCreatureOfTeam(TeamType.Right, (_waveData != null) ? _waveData.WaveIndex : 0);
		if (restingCreatureOfTeam != null && restingCreatureOfTeam.Count > 0)
		{
			for (int i = 0; i < restingCreatureOfTeam.Count; i++)
			{
				BattleCreature battleCreature = restingCreatureOfTeam[i];
				int creatureUid = battleCreature.Uid;
				if (_wakingUpCreatureUidList.Contains(creatureUid))
				{
					continue;
				}
				BattleCreatureRestingInfo restingInfo = battleCreature.RestingInfo;
				if (restingInfo != null && (aliveCreatureCountOfTeam2 == 0 || (restingInfo.WakeUpWaveTimeMs != 0 && currentTimeMs >= restingInfo.WakeUpWaveTimeMs)))
				{
					_wakingUpCreatureUidList.Add(creatureUid);
					_updater.BattleInstance.BattleLogContainer.AddLog(new CreatureWakeUpBattleLog
					{
						CreatureUid = creatureUid
					}).OnProcessed = delegate
					{
						_wakingUpCreatureUidList.Remove(creatureUid);
					};
				}
			}
		}
		List<CreatureCreateBattleLog> delayedCreatureLogList = (_updater as CampaignUpdater).DelayedCreatureLogList;
		if (delayedCreatureLogList.Count > 0 && flag && _delayedCreatureBattleLogList.Count == 0)
		{
			switch (delayedSpawnType)
			{
			case DelayedSpawnType.AdvanceSpawnTime:
			{
				int num = int.MaxValue;
				for (int num2 = 0; num2 < delayedCreatureLogList.Count; num2++)
				{
					CreatureCreateBattleLog creatureCreateBattleLog = delayedCreatureLogList[num2];
					num = Math.Min(num, creatureCreateBattleLog.CreateInfo.DelayMs);
				}
				int num3 = (int)Math.Max(num - currentTimeMs, 0L);
				for (int num4 = 0; num4 < delayedCreatureLogList.Count; num4++)
				{
					delayedCreatureLogList[num4].CreateInfo.DelayMs -= num3;
				}
				break;
			}
			case DelayedSpawnType.DoNotSpawn:
				delayedCreatureLogList.Clear();
				break;
			}
		}
		for (int num5 = 0; num5 < delayedCreatureLogList.Count; num5++)
		{
			CreatureCreateBattleLog creatureCreateBattleLog2 = delayedCreatureLogList[num5];
			if (creatureCreateBattleLog2.CreateInfo.DelayMs <= currentTimeMs)
			{
				delayedCreatureLogList.RemoveAt(num5);
				CreatureCreateBattleLog creatureCreateBattleLog3 = new CreatureCreateBattleLog();
				creatureCreateBattleLog3.PlayerId = creatureCreateBattleLog2.PlayerId;
				creatureCreateBattleLog3.CreateInfo = creatureCreateBattleLog2.CreateInfo;
				BattleLog battleLog = _updater.BattleInstance.BattleLogContainer.AddLog(creatureCreateBattleLog3);
				_delayedCreatureBattleLogList.Add(battleLog);
				battleLog.OnProcessed = delegate
				{
					_delayedCreatureBattleLogList.Remove(battleLog);
				};
			}
		}
		if (_updater.BattleInstance.GetRemainBattleTimeMs(includeExtraTime: false) <= 0 && _updater.BattleInstance.ExtraBattleTime > 0 && !_rageModeBattleLogCreated)
		{
			_rageModeBattleLogCreated = true;
			WaveRageModeBattleLog runtime = new WaveRageModeBattleLog();
			_updater.BattleInstance.BattleLogContainer.AddLog(runtime);
		}
		OnUpdateWinDecider();
	}

	private void OnUpdateWinDecider()
	{
		if (DisableWinDecider || _delayedCreatureBattleLogList.Count > 0 || _wakingUpCreatureUidList.Count > 0 || _winDecider == null)
		{
			return;
		}
		TeamType teamType = TeamType.Left;
		bool flag = _winDecider.Decide(_updater.BattleInstance, out teamType);
		if (ForceDecideWinner)
		{
			flag = true;
			teamType = WinnerTeamType;
		}
		if (!flag)
		{
			if (_updater.BattleInstance.Snapshot.Withdraw)
			{
				EndState(new WaveClosing(TeamType.Right, timeouted: false, _winDecider.GetWinDeciderType(), withdraw: true));
			}
			else if (_updater.BattleInstance.Snapshot.Cancel)
			{
				EndState(new WaveClosing(TeamType.Right, timeouted: false, _winDecider.GetWinDeciderType(), withdraw: true));
			}
			else if (_updater.BattleInstance.Snapshot.GiveUp)
			{
				_updater.GiveUp = true;
				EndState(new WaveClosing(_updater.BattleInstance.Snapshot.GiveUpWinTeamType, timeouted: false, _winDecider.GetWinDeciderType()));
			}
		}
		else
		{
			bool timeouted = _updater.BattleInstance.GetRemainBattleTimeMs(includeExtraTime: true) <= 0;
			EndState(new WaveClosing(teamType, timeouted, _winDecider.GetWinDeciderType()));
		}
	}
}
