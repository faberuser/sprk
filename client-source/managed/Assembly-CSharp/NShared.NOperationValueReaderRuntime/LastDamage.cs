namespace NShared.NOperationValueReaderRuntime;

public class LastDamage : CreatureReaderBase
{
	public LastDamage(CreatureType creatureType)
		: base(creatureType)
	{
	}

	protected override long GetStat(BattleCreature creature, string[] arguments, OperationInfo operatinInfo)
	{
		return operatinInfo.Creature.StatController.PrevDamageInfo?.FinalAmount ?? 0;
	}
}
