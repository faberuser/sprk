namespace NShared.NOperationValueReaderRuntime;

public class LastAttack : CreatureReaderBase
{
	public LastAttack(CreatureType creatureType)
		: base(creatureType)
	{
	}

	protected override long GetStat(BattleCreature creature, string[] arguments, OperationInfo operatinInfo)
	{
		return operatinInfo.Creature.StatController.PrevAttackInfo?.FinalAmount ?? 0;
	}
}
