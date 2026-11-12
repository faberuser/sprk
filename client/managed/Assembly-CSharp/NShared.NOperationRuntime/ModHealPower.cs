namespace NShared.NOperationRuntime;

public class ModHealPower : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.HealPower };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
