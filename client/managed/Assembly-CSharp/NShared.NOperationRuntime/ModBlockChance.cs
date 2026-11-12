namespace NShared.NOperationRuntime;

public class ModBlockChance : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[2]
	{
		StatType.PhysicalBlockChance,
		StatType.MagicalBlockChance
	};

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
