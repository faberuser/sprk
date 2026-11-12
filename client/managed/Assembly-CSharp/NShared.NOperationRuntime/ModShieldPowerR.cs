namespace NShared.NOperationRuntime;

public class ModShieldPowerR : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.ShieldPowerR };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
