using NShared.NUtil;

namespace NShared.NOperationValueReaderRuntime;

public class DistanceReader : CreatureReaderBase
{
	public DistanceReader(CreatureType creatureType)
		: base(creatureType)
	{
	}

	protected override long GetStat(BattleCreature creature, string[] arguments, OperationInfo operatinInfo)
	{
		BattleCreature creature2 = operatinInfo.Creature;
		if (creature2 == creature)
		{
			return 0L;
		}
		Vector3 position = creature.TransformController.Position;
		Vector3 position2 = creature2.TransformController.Position;
		return Vector3.DistanceWithoutY(position, position2);
	}
}
