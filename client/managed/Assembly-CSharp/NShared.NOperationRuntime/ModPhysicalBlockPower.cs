namespace NShared.NOperationRuntime;

public class ModPhysicalBlockPower : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.PhysicalBlockPower };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
