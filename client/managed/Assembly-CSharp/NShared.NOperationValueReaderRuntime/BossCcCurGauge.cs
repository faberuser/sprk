namespace NShared.NOperationValueReaderRuntime;

internal class BossCcCurGauge : IOperationValueReader, IStatReader
{
	StatType IStatReader.GetStatType()
	{
		return StatType.BossCcCurGauge;
	}

	long IOperationValueReader.Read(string[] arguments, OperationInfo operatinInfo)
	{
		if (!(operatinInfo.BattleInstance.Snapshot.TacticManager.Tactic is BossCcGauge bossCcGauge))
		{
			return 0L;
		}
		return bossCcGauge.CurGauge;
	}
}
