namespace NShared.NOperationRuntime;

public class ModAttackSpeed : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.AttackSpeed };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
