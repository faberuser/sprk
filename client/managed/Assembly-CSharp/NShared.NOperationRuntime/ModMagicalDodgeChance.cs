namespace NShared.NOperationRuntime;

public class ModMagicalDodgeChance : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.MagicalDodgeChance };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
