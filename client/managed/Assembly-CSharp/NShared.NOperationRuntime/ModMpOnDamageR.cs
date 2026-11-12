namespace NShared.NOperationRuntime;

public class ModMpOnDamageR : SetMultedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.MpOnDamage };

	public override StatType[] GetMultedStatTypes()
	{
		return StatTypes;
	}
}
