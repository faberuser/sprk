namespace NShared.NOperationRuntime;

public class ModAttackSpeedR : SetMultedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.AttackSpeed };

	public override StatType[] GetMultedStatTypes()
	{
		return StatTypes;
	}
}
