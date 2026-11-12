namespace NShared.NOperationRuntime;

public class ModPhysicalWeakness : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.PhysicalWeakness };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
