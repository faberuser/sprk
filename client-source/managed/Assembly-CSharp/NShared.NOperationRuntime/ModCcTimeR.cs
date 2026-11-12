namespace NShared.NOperationRuntime;

public class ModCcTimeR : SetMultedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.CcTime };

	public override StatType[] GetMultedStatTypes()
	{
		return StatTypes;
	}
}
