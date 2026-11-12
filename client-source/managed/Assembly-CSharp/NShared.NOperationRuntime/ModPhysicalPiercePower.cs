namespace NShared.NOperationRuntime;

public class ModPhysicalPiercePower : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.PhysicalPiercePower };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
