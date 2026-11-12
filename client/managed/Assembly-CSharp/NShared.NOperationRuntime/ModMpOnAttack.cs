namespace NShared.NOperationRuntime;

public class ModMpOnAttack : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.MpOnAttack };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
