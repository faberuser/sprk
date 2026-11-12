namespace NShared.NOperationRuntime;

public class ModHpOnKill : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.HpOnKill };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
