namespace NShared.NOperationRuntime;

public class ModWeakness : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[2]
	{
		StatType.PhysicalWeakness,
		StatType.MagicalWeakness
	};

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
