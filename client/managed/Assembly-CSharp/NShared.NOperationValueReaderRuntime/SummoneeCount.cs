namespace NShared.NOperationValueReaderRuntime;

public class SummoneeCount : CreatureReaderBase
{
	public SummoneeCount(CreatureType creatureType)
		: base(creatureType)
	{
	}

	protected override long GetStat(BattleCreature creature, string[] arguments, OperationInfo operatinInfo)
	{
		return operatinInfo.Creature.BattleInstance.CreatureContainer.GetChildCreatureList(operatinInfo.Creature.Uid, aliveOnly: true)?.Count ?? 0;
	}
}
