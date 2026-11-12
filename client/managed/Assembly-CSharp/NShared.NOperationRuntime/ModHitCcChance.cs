namespace NShared.NOperationRuntime;

public class ModHitCcChance : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.HitCcChance };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
