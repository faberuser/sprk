namespace NShared.NOperationRuntime;

public class ModMagicalIgnoreDefencePower : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.MagicalIgnoreDefensePower };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
