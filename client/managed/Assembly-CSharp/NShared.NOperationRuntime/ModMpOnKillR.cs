namespace NShared.NOperationRuntime;

public class ModMpOnKillR : SetMultedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.MpOnKill };

	public override StatType[] GetMultedStatTypes()
	{
		return StatTypes;
	}
}
