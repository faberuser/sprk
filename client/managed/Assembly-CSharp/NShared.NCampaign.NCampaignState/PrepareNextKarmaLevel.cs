using System.Collections.Generic;
using System.Linq;
using NShared.NCampaign.Karma;

namespace NShared.NCampaign.NCampaignState;

public class PrepareNextKarmaLevel : CampaignState
{
	private readonly List<BattleLog> _battleLogs = new List<BattleLog>();

	private bool battleLogProcessDone;

	private KarmaUpdater karmaUpdater;

	public override void OnInit()
	{
		base.OnInit();
		karmaUpdater = _updater as KarmaUpdater;
		if (karmaUpdater == null)
		{
			_updater.SetBattleEnd(incomplete: false);
		}
		else
		{
			SetNextLevel();
		}
	}

	private void SetNextLevel()
	{
		karmaUpdater.SetNextLevel();
		_ = karmaUpdater.CurrentKarmaLevel;
		NextDungeonStartBattleLog runtime = new NextDungeonStartBattleLog
		{
			ChapterIndex = karmaUpdater.ChapterIndex,
			DungeonIndex = karmaUpdater.DungeonIndex
		};
		BattleLog item = _updater.BattleInstance.BattleLogContainer.AddLog(runtime);
		_battleLogs.Add(item);
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
		if (!battleLogProcessDone)
		{
			if (_battleLogs == null || !_battleLogs.All((BattleLog log) => log.Processed))
			{
				return;
			}
			if (_updater.OnWavePause != null)
			{
				_updater.OnWavePause(TeamType.Left);
			}
			battleLogProcessDone = true;
		}
		if (!_updater.IsWavePause)
		{
			EndState(new WaveInit());
		}
	}
}
