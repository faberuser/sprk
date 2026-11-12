namespace NShared.NOperationRuntime;

public class ModBlockPower : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[2]
	{
		StatType.PhysicalBlockPower,
		StatType.MagicalBlockPower
	};

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
