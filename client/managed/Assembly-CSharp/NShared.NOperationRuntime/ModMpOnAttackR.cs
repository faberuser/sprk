namespace NShared.NOperationRuntime;

public class ModMpOnAttackR : SetMultedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.MpOnAttack };

	public override StatType[] GetMultedStatTypes()
	{
		return StatTypes;
	}
}
