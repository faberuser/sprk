namespace NShared.NOperationRuntime;

public class ModCriticalDodgeChance : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[2]
	{
		StatType.PhysicalCriticalDodgeChance,
		StatType.MagicalCriticalDodgeChance
	};

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
