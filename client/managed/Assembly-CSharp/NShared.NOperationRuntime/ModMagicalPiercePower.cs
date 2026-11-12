namespace NShared.NOperationRuntime;

public class ModMagicalPiercePower : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.MagicalPiercePower };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
