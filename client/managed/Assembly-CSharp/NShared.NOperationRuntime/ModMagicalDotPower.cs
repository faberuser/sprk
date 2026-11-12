namespace NShared.NOperationRuntime;

public class ModMagicalDotPower : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.MagicalDotPower };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
