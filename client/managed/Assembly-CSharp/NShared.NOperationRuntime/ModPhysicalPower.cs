namespace NShared.NOperationRuntime;

public class ModPhysicalPower : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.PhysicalAttack };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
