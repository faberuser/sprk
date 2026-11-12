namespace NShared.NOperationRuntime;

public class ModMaxMp : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.MaxMp };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
