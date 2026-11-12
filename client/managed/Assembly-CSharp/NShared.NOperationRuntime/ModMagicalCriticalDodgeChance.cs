namespace NShared.NOperationRuntime;

public class ModMagicalCriticalDodgeChance : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.MagicalCriticalDodgeChance };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
