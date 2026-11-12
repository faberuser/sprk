namespace NShared.NOperationRuntime;

public class ModHpRatioOnDamage : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.HpRatioOnDamage };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
