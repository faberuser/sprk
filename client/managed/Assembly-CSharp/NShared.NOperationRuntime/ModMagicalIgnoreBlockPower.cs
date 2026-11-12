namespace NShared.NOperationRuntime;

public class ModMagicalIgnoreBlockPower : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.MagicalIgnoreBlockPower };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
