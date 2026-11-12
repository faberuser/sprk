namespace NShared.NOperationRuntime;

public class ModMagicalBlockChance : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.MagicalBlockChance };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
