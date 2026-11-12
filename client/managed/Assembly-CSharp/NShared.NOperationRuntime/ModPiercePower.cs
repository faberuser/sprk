namespace NShared.NOperationRuntime;

public class ModPiercePower : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[2]
	{
		StatType.PhysicalPiercePower,
		StatType.MagicalPiercePower
	};

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
