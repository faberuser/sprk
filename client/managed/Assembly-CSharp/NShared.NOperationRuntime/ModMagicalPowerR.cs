namespace NShared.NOperationRuntime;

public class ModMagicalPowerR : SetMultedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.MagicalAttack };

	public override StatType[] GetMultedStatTypes()
	{
		return StatTypes;
	}
}
