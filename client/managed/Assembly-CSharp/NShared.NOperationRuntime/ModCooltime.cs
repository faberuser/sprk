namespace NShared.NOperationRuntime;

public class ModCooltime : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.Cooltime };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
