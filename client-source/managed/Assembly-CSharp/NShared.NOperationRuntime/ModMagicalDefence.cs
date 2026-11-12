namespace NShared.NOperationRuntime;

public class ModMagicalDefence : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.MagicalDefense };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
