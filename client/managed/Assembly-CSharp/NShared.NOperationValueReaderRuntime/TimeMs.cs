namespace NShared.NOperationValueReaderRuntime;

public class TimeMs : IOperationValueReader
{
	long IOperationValueReader.Read(string[] arguments, OperationInfo operatinInfo)
	{
		return operatinInfo.Creature.BattleInstance.GetElapsedBattleTimeMs();
	}
}
