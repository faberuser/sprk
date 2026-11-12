namespace NShared.NOperationValueReaderRuntime;

public class RemainTimeMs : IOperationValueReader
{
	long IOperationValueReader.Read(string[] arguments, OperationInfo operatinInfo)
	{
		return operatinInfo.Creature.BattleInstance.GetRemainBattleTimeMs(includeExtraTime: false);
	}
}
