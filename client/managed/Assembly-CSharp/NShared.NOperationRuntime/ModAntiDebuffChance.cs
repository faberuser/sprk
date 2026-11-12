namespace NShared.NOperationRuntime;

public class ModAntiDebuffChance : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.AntiDebuffChance };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
