namespace NShared.NOperationRuntime;

public class ModSkillLevelSlot1 : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.SkillLevelSlot1 };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
