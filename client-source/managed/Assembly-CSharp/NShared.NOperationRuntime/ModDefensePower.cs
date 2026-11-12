namespace NShared.NOperationRuntime;

public class ModDefensePower : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[2]
	{
		StatType.PhysicalDefensePower,
		StatType.MagicalDefensePower
	};

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
