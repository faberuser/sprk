namespace NShared.NOperationRuntime;

public class ModHpRatioOnAttack : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.HpRatioOnAttack };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
