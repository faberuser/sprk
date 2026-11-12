namespace NShared.NOperationRuntime;

public class ModPhysicalToughness : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.PhysicalToughness };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
