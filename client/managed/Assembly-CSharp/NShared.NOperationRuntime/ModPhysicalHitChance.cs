namespace NShared.NOperationRuntime;

public class ModPhysicalHitChance : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.PhysicalHitChance };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
