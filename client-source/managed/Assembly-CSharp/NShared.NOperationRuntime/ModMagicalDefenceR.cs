namespace NShared.NOperationRuntime;

public class ModMagicalDefenceR : SetMultedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.MagicalDefense };

	public override StatType[] GetMultedStatTypes()
	{
		return StatTypes;
	}
}
