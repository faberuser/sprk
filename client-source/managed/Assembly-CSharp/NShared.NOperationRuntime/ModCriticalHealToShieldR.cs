namespace NShared.NOperationRuntime;

public class ModCriticalHealToShieldR : SetMultedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.CriticalHealToShield };

	public override StatType[] GetMultedStatTypes()
	{
		return StatTypes;
	}
}
