namespace NShared.NOperationRuntime;

public class ModMaxMpR : SetMultedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.MaxMp };

	public override StatType[] GetMultedStatTypes()
	{
		return StatTypes;
	}
}
