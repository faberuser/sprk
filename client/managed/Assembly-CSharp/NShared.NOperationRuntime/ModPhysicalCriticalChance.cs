namespace NShared.NOperationRuntime;

public class ModPhysicalCriticalChance : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.PhysicalCriticalChance };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
