namespace NShared.NOperationRuntime;

public class ModMpOnDamage : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.MpOnDamage };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
