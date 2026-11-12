namespace NShared.NOperationRuntime;

public class ModMagicalAttackPower : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.MagicalAttackPower };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
