namespace NShared.NOperationRuntime;

public class ModDodgeChance : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[2]
	{
		StatType.PhysicalDodgeChance,
		StatType.MagicalDodgeChance
	};

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
