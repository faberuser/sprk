namespace NShared.NOperationRuntime;

public class ModMagicalHitChance : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.MagicalHitChance };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
