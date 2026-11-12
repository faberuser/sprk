namespace NShared.NOperationRuntime;

public class ModDefenceR : SetMultedStat
{
	private static readonly StatType[] StatTypes = new StatType[2]
	{
		StatType.PhysicalDefense,
		StatType.MagicalDefense
	};

	public override StatType[] GetMultedStatTypes()
	{
		return StatTypes;
	}
}
