namespace NShared.NOperationRuntime;

public class ModCriticalWeakness : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[2]
	{
		StatType.PhysicalCriticalWeakness,
		StatType.MagicalCriticalWeakness
	};

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
