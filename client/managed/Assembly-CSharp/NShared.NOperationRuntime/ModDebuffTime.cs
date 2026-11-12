namespace NShared.NOperationRuntime;

public class ModDebuffTime : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.DebuffTime };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
