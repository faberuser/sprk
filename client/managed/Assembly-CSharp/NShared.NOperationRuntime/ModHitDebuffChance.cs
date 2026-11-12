namespace NShared.NOperationRuntime;

public class ModHitDebuffChance : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.HitDebuffChance };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
