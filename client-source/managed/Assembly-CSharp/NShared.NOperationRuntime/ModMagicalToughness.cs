namespace NShared.NOperationRuntime;

public class ModMagicalToughness : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.MagicalToughness };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
