namespace NShared.NOperationRuntime;

public class ModSkillLevelSlot3 : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.SkillLevelSlot3 };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
