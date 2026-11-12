namespace NShared.NCampaign.NCampaignState;

public class WaitUntilKarmaCardPick : CampaignState
{
	private TeamType _winner;

	private bool _timeouted;

	private bool _waveEndBattleLogCreated;

	private BattleLog _waveCloseBattleLog;

	private WinDeciderType _winDecider;

	private bool _withdraw;

	public override void OnInit()
	{
		base.OnInit();
		WaveCloseBattleLog waveCloseBattleLog = new WaveCloseBattleLog();
		waveCloseBattleLog.Timeouted = _timeouted;
		_waveCloseBattleLog = _updater.BattleInstance.BattleLogContainer.AddLog(waveCloseBattleLog);
	}

	public override void OnUpdate()
	{
		base.OnUpdate();
	}
}
