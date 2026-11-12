namespace NShared.NOperationRuntime;

public class ModMaxHpDefenceR : SetMultedStat
{
	private static readonly StatType[] StatTypes = new StatType[3]
	{
		StatType.MaxHp,
		StatType.PhysicalDefense,
		StatType.MagicalDefense
	};

	public override StatType[] GetMultedStatTypes()
	{
		return StatTypes;
	}
}
