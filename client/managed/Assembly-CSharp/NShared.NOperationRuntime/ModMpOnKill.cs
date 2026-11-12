namespace NShared.NOperationRuntime;

public class ModMpOnKill : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.MpOnKill };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
