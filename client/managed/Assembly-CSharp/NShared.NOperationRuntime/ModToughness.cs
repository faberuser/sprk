namespace NShared.NOperationRuntime;

public class ModToughness : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[2]
	{
		StatType.PhysicalToughness,
		StatType.MagicalToughness
	};

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
