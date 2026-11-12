namespace NShared.NOperationRuntime;

public class ModMagicalWeakness : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.MagicalWeakness };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
