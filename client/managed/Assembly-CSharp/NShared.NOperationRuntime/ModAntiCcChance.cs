namespace NShared.NOperationRuntime;

public class ModAntiCcChance : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.AntiCcChance };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
