using NShared.NCampaign.Karma;

namespace NShared.NCampaign.NCampaignState;

public class WaveEnd : CampaignState
{
	private TeamType _winner;

	private bool CheckPossibleEachWaveEachTeam
	{
		get
		{
			if (!(_updater is CampaignUpdater campaignUpdater))
			{
				return false;
			}
			if (campaignUpdater.EachWaveEachTeamBattleCreatureCreateInfoList == null || campaignUpdater.EachWaveEachTeamBattleCreatureCreateInfoList.Count < campaignUpdater.WaveIndex)
			{
				return false;
			}
			return true;
		}
	}

	public WaveEnd(TeamType winner = TeamType.Right)
	{
		_winner = winner;
	}

	public override void OnInit()
	{
		base.OnInit();
		_updater.StartBattle(started: false);
		_updater.SetWaveEnd(_winner);
		if (_updater is EclipseOfflineModeUpdater)
		{
			EclipseOfflineModeUpdater eclipseOfflineModeUpdater = _updater as EclipseOfflineModeUpdater;
			if (_winner == TeamType.Right)
			{
				_updater.SetBattleEnd(incomplete: false);
			}
			else if (_winner == TeamType.Left)
			{
				eclipseOfflineModeUpdater.SetFirstClearWave(isClear: true);
				if (!_updater.SetNextWave())
				{
					EndState(new WavePause(_winner));
					return;
				}
				eclipseOfflineModeUpdater.RenewClearWave();
				EndState(new WaveInit());
			}
			else
			{
				_updater.SetBattleEnd(incomplete: false);
			}
		}
		else if (_updater is KarmaUpdater)
		{
			if (_winner == TeamType.Right)
			{
				_updater.SetBattleEnd(incomplete: false);
			}
			else if (_winner == TeamType.Left)
			{
				if (_updater.SetNextWave())
				{
					EndState(new WaveInit());
				}
				else
				{
					EndState(new PrepareNextKarmaLevel());
				}
			}
			else
			{
				_updater.SetBattleEnd(incomplete: false);
			}
		}
		else if (!_updater.SetNextWave())
		{
			_updater.SetBattleEnd(incomplete: false);
		}
		else if (_winner == TeamType.Right)
		{
			_updater.SetBattleEnd(incomplete: false);
		}
		else
		{
			if (_updater.BattleInstance == null)
			{
				return;
			}
			if (_updater.BattleInstance.CheckEachWaveEachTeam)
			{
				if (!CheckPossibleEachWaveEachTeam)
				{
					_updater.SetBattleEnd(incomplete: false);
				}
				else
				{
					EndState(new WavePause(_winner));
				}
			}
			else
			{
				EndState(new WaveInit());
			}
		}
	}
}
