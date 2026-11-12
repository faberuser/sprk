namespace NShared.NOperationRuntime;

public class ModCriticalChance : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[2]
	{
		StatType.PhysicalCriticalChance,
		StatType.MagicalCriticalChance
	};

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
