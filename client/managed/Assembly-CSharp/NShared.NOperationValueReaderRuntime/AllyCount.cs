namespace NShared.NOperationValueReaderRuntime;

public class AllyCount : IOperationValueReader, IStatReader
{
	StatType IStatReader.GetStatType()
	{
		return StatType.AllyCount;
	}

	long IOperationValueReader.Read(string[] arguments, OperationInfo operatinInfo)
	{
		int teamId = operatinInfo.Creature.TeamController.TeamId;
		return operatinInfo.Creature.BattleInstance.CreatureContainer.GetAliveCreatureCountOfTeam(teamId);
	}
}
