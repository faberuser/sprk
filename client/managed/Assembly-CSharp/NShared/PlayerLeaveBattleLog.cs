namespace NShared;

public class PlayerLeaveBattleLog : IBattleLogRuntime
{
	BattleLogType IBattleLogRuntime.GetBattleLogType()
	{
		return BattleLogType.PlayerLeave;
	}

	void IBattleLogRuntime.Run(BattleInstance battleInstance, BattleLog battleLog)
	{
	}
}
