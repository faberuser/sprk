namespace NShared.NOperationRuntime;

public class ModCriticalPower : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[2]
	{
		StatType.PhysicalCriticalPower,
		StatType.MagicalCriticalPower
	};

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
