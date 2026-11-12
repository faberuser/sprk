namespace NShared.NOperationRuntime;

public class ModSkillLevelSlot2 : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.SkillLevelSlot2 };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
