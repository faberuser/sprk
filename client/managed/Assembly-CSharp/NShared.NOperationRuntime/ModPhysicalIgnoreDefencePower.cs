namespace NShared.NOperationRuntime;

public class ModPhysicalIgnoreDefencePower : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.PhysicalIgnoreDefensePower };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
