namespace NShared.NOperationRuntime;

public class ModMagicalCriticalWeakness : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.MagicalCriticalWeakness };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
