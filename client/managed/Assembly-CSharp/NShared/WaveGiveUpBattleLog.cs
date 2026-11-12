namespace NShared;

[JsonSerializable]
public class WaveGiveUpBattleLog : IBattleLogRuntime
{
	public TeamType GiveUpWinTeamType { get; set; }

	BattleLogType IBattleLogRuntime.GetBattleLogType()
	{
		return BattleLogType.WaveGiveUp;
	}

	void IBattleLogRuntime.Run(BattleInstance battleInstance, BattleLog battleLog)
	{
		battleInstance.Snapshot.GiveUpWinTeamType = GiveUpWinTeamType;
		battleInstance.Snapshot.GiveUp = true;
	}
}
