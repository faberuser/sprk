namespace NShared.NOperationRuntime;

public class ModMagicalBlockPower : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.MagicalBlockPower };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
