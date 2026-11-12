namespace NShared.NOperationRuntime;

public class ModMpOnTime : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.MpOnTime };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
