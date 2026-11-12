namespace NShared.NOperationRuntime;

public class ModCooltimeR : SetMultedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.Cooltime };

	public override StatType[] GetMultedStatTypes()
	{
		return StatTypes;
	}
}
