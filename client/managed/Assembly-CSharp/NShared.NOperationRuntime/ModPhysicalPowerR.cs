namespace NShared.NOperationRuntime;

public class ModPhysicalPowerR : SetMultedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.PhysicalAttack };

	public override StatType[] GetMultedStatTypes()
	{
		return StatTypes;
	}
}
