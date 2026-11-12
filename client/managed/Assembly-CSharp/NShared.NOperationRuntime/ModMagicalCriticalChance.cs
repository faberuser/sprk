namespace NShared.NOperationRuntime;

public class ModMagicalCriticalChance : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.MagicalCriticalChance };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
