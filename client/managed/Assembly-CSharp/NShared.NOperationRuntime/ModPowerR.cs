namespace NShared.NOperationRuntime;

public class ModPowerR : SetMultedStat
{
	private static readonly StatType[] StatTypes = new StatType[2]
	{
		StatType.PhysicalAttack,
		StatType.MagicalAttack
	};

	public override StatType[] GetMultedStatTypes()
	{
		return StatTypes;
	}
}
