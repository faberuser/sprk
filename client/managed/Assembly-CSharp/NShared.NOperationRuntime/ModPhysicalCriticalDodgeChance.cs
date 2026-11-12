namespace NShared.NOperationRuntime;

public class ModPhysicalCriticalDodgeChance : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.PhysicalCriticalDodgeChance };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
