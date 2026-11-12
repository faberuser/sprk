namespace NShared.NOperationRuntime;

public class ModHpRatioOnKill : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.HpRatioOnKill };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
