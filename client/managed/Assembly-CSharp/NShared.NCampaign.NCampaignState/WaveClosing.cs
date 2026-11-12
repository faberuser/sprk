namespace NShared.NCampaign.NCampaignState;

public class WaveClosing : CampaignState
{
	private TeamType _winner;

	private bool _timeouted;

	private bool _waveEndBattleLogCreated;

	private BattleLog _waveCloseBattleLog;

	private WinDeciderType _winDecider;

	private bool _withdraw;

	public WaveClosing(TeamType winner, bool timeouted, WinDeciderType winDecider = WinDeciderType.None, bool withdraw = false)
	{
		_winner = winner;
		_timeouted = timeouted;
		_winDecider = winDecider;
		_withdraw = withdraw;
	}

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
		if (_updater.BattleInstance.IsBattleEnded())
		{
			EndState(new WaveEnd(_winner));
		}
		else if (_waveCloseBattleLog.Processed && !_updater.BattleInstance.CreatureContainer.AnyCreatureUsingSkill() && !_waveEndBattleLogCreated)
		{
			_waveEndBattleLogCreated = true;
			WaveEndBattleLog waveEndBattleLog = new WaveEndBattleLog();
			waveEndBattleLog.winner = _winner;
			waveEndBattleLog._winDecider = _winDecider;
			waveEndBattleLog._withdraw = _withdraw;
			_updater.BattleInstance.BattleLogContainer.AddLog(waveEndBattleLog);
		}
	}
}
