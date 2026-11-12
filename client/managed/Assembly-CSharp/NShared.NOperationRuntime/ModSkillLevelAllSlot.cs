namespace NShared.NOperationRuntime;

public class ModSkillLevelAllSlot : SetAddedStat
{
	private static readonly StatType[] StatTypes = new StatType[1] { StatType.SkillLevelAllSlot };

	public override StatType[] GetAddedStatTypes()
	{
		return StatTypes;
	}
}
