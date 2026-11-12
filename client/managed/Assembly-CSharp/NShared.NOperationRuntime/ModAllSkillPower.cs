namespace NShared.NOperationRuntime;

public class ModAllSkillPower : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.SkillPower };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
