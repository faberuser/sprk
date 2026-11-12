namespace NShared.NOperationRuntime;

public class ModAllSkillPowerR : SetMultedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.SkillPower };

	public override StatType[] GetMultedStatTypes()
	{
		return StatTypes;
	}
}
