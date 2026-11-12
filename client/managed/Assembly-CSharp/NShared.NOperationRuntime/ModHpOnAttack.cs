namespace NShared.NOperationRuntime;

public class ModHpOnAttack : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.HpOnAttack };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
