namespace NShared.NOperationRuntime;

public class ModPhysicalDotPower : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.PhysicalDotPower };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
