namespace NShared.NOperationValueReaderRuntime;

internal class BossCcMaxGauge : IOperationValueReader
{
	long IOperationValueReader.Read(string[] arguments, OperationInfo operatinInfo)
	{
		if (!(operatinInfo.BattleInstance.Snapshot.TacticManager.Tactic is BossCcGauge bossCcGauge))
		{
			return 0L;
		}
		return bossCcGauge.MaxGauge;
	}
}
