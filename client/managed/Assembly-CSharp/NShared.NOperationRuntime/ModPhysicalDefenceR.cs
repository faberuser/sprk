namespace NShared.NOperationRuntime;

public class ModPhysicalDefenceR : SetMultedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.PhysicalDefense };

	public override StatType[] GetMultedStatTypes()
	{
		return StatTypes;
	}
}
