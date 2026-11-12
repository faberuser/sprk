namespace NShared.NOperationRuntime;

public class ModHpOnDamage : SetAddedStat
{
	public override StatType[] GetAddedStatTypes()
	{
		return new StatType[1] { StatType.HpOnDamage };
	}
}
