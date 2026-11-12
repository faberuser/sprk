namespace NShared.NOperationRuntime;

public class ModPhysicalCriticalPower : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.PhysicalCriticalPower };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
