namespace NShared.NOperationRuntime;

public class ModMpRegenRatioM : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.MpRegenRatioM };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
