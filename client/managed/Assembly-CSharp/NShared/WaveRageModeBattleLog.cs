namespace NShared;

[JsonSerializable]
public class WaveRageModeBattleLog : IBattleLogRuntime
{
	BattleLogType IBattleLogRuntime.GetBattleLogType()
	{
		return BattleLogType.WaveRageMode;
	}

	void IBattleLogRuntime.Run(BattleInstance battleInstance, BattleLog battleLog)
	{
		battleInstance.SetRageMode(TeamType.Right);
	}
}
