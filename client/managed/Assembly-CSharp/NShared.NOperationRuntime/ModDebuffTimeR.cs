namespace NShared.NOperationRuntime;

public class ModDebuffTimeR : SetMultedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.DebuffTime };

	public override StatType[] GetMultedStatTypes()
	{
		return StatTypes;
	}
}
