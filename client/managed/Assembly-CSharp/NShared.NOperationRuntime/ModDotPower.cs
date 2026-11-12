namespace NShared.NOperationRuntime;

public class ModDotPower : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[2]
	{
		StatType.PhysicalDotPower,
		StatType.MagicalDotPower
	};

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
