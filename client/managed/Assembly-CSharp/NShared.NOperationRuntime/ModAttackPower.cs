namespace NShared.NOperationRuntime;

public class ModAttackPower : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[2]
	{
		StatType.PhysicalAttackPower,
		StatType.MagicalAttackPower
	};

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
