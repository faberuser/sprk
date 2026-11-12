namespace NShared.NOperationRuntime;

public class ModIgnoreBlockPower : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[2]
	{
		StatType.PhysicalIgnoreBlockPower,
		StatType.MagicalIgnoreBlockPower
	};

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
