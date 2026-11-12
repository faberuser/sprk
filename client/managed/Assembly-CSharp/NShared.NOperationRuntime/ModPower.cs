namespace NShared.NOperationRuntime;

public class ModPower : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[2]
	{
		StatType.PhysicalAttack,
		StatType.MagicalAttack
	};

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
