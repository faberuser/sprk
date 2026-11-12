namespace NShared.NOperationRuntime;

public class ModMaxHpDefence : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[3]
	{
		StatType.MaxHp,
		StatType.PhysicalDefense,
		StatType.MagicalDefense
	};

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
