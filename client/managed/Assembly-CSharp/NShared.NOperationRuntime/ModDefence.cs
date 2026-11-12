namespace NShared.NOperationRuntime;

public class ModDefence : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[2]
	{
		StatType.PhysicalDefense,
		StatType.MagicalDefense
	};

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
