namespace NShared.NOperationRuntime;

public class ModSkillLevelSlot4 : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.SkillLevelSlot4 };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
