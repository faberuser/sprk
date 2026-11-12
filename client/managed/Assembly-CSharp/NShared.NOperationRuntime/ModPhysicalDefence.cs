namespace NShared.NOperationRuntime;

public class ModPhysicalDefence : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.PhysicalDefense };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
