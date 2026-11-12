namespace NShared.NOperationRuntime;

public class ModMpRegenRatioMR : SetMultedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.MpRegenRatioM };

	public override StatType[] GetMultedStatTypes()
	{
		return StatTypes;
	}
}
