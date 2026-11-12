namespace NShared.NOperationRuntime;

public class ModMpOnTimeR : SetMultedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.MpOnTime };

	public override StatType[] GetMultedStatTypes()
	{
		return StatTypes;
	}
}
