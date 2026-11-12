namespace NShared.NOperationRuntime;

public class ModPhysicalDodgeChance : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.PhysicalDodgeChance };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
