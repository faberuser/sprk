namespace NShared.NOperationRuntime;

public class ModMagicalCriticalPower : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.MagicalCriticalPower };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
