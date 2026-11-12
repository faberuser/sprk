namespace NShared.NOperationRuntime;

public class ModPhysicalBlockChance : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.PhysicalBlockChance };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
