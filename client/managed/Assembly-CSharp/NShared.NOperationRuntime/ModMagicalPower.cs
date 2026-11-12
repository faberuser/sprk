namespace NShared.NOperationRuntime;

public class ModMagicalPower : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.MagicalAttack };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
