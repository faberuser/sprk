namespace NShared.NOperationRuntime;

public class ModPhysicalIgnoreBlockPower : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.PhysicalIgnoreBlockPower };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
