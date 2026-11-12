namespace NShared.NCampaign.NCampaignState;

public class WaveReady : CampaignState
{
	public override void OnInit()
	{
		base.OnInit();
	}

	public override void OnUpdate()
	{
		if (_updater.BattleStarted)
		{
			long num = _updater.BattleInstance.Uid;
			if (_updater is EclipseModeUpdater)
			{
				num = (_updater as EclipseModeUpdater).GetMatchAccountId();
			}
			_updater.BattleInstance.Log("Wave start!! uid=" + num + " chapter=" + _updater.ChapterIndex + " dungeon=" + _updater.DungeonIndex + " wave=" + _updater.WaveIndex + " maxWave=" + _updater.MaxWaveIndex);
			EndState(new WavePlaying());
		}
	}
}
