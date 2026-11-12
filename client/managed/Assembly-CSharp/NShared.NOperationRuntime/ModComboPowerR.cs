namespace NShared.NOperationRuntime;

public class ModComboPowerR : SetMultedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.ComboPower };

	public override StatType[] GetMultedStatTypes()
	{
		return StatTypes;
	}
}
