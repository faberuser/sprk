namespace NShared;

[JsonSerializable]
public class WaveCloseBattleLog : IBattleLogRuntime
{
	public bool Timeouted { get; set; }

	BattleLogType IBattleLogRuntime.GetBattleLogType()
	{
		return BattleLogType.WaveClose;
	}

	void IBattleLogRuntime.Run(BattleInstance battleInstance, BattleLog battleLog)
	{
		battleInstance.CloseBattle(Timeouted);
		battleInstance.RaiseBattleEvent(new WaveClose());
	}
}
