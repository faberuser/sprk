namespace NShared.NOperationRuntime;

public class ModPhysicalAttackPower : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.PhysicalAttackPower };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
