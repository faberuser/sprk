namespace NShared.NOperationRuntime;

public class ModPhysicalCriticalWeakness : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.PhysicalCriticalWeakness };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
