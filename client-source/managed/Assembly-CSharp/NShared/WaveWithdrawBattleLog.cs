namespace NShared;

[JsonSerializable]
public class WaveWithdrawBattleLog : IBattleLogRuntime
{
	BattleLogType IBattleLogRuntime.GetBattleLogType()
	{
		return BattleLogType.WaveWithdraw;
	}

	void IBattleLogRuntime.Run(BattleInstance battleInstance, BattleLog battleLog)
	{
		battleInstance.Snapshot.Withdraw = true;
	}
}
