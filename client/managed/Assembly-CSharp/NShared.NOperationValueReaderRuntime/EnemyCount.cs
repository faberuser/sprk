namespace NShared.NOperationValueReaderRuntime;

public class EnemyCount : IOperationValueReader, IStatReader
{
	StatType IStatReader.GetStatType()
	{
		return StatType.EnemyCount;
	}

	long IOperationValueReader.Read(string[] arguments, OperationInfo operatinInfo)
	{
		int teamId = operatinInfo.Creature.TeamController.TeamId;
		return operatinInfo.Creature.BattleInstance.CreatureContainer.GetAliveCreatureCountOfOtherTeam(teamId);
	}
}
