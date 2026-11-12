namespace NShared.NOperationRuntime;

public class ModHpStealPower : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.HpStealPower };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
