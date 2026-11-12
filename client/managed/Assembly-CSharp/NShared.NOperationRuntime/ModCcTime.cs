namespace NShared.NOperationRuntime;

public class ModCcTime : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.CcTime };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
