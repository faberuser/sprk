namespace NShared.NOperationRuntime;

public class ModIgnoreDefencePower : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[2]
	{
		StatType.PhysicalIgnoreDefensePower,
		StatType.MagicalIgnoreDefensePower
	};

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
