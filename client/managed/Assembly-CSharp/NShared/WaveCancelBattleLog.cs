namespace NShared;

[JsonSerializable]
public class WaveCancelBattleLog : IBattleLogRuntime
{
	BattleLogType IBattleLogRuntime.GetBattleLogType()
	{
		return BattleLogType.WaveCancel;
	}

	void IBattleLogRuntime.Run(BattleInstance battleInstance, BattleLog battleLog)
	{
		battleInstance.Snapshot.Cancel = true;
	}
}
