namespace NShared.NCampaign.NCampaignState;

public class WaveInit : CampaignState
{
	public override void OnInit()
	{
		base.OnInit();
		WaveReadyBattleLog waveReadyBattleLog = new BattleLogCreator(_updater.BattleInstance).Create(_updater);
		if (waveReadyBattleLog == null)
		{
			EndState(new WaveEnd());
			return;
		}
		_updater.BattleInstance.BattleLogContainer.AddLog(waveReadyBattleLog).OnProcessed = delegate
		{
			if (_updater.OnWaveReady != null)
			{
				_updater.OnWaveReady();
			}
			EndState(new WaveReady());
		};
	}
}
