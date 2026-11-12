namespace NShared.NOperationRuntime;

public class ModHitChance : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[2]
	{
		StatType.PhysicalHitChance,
		StatType.MagicalHitChance
	};

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
