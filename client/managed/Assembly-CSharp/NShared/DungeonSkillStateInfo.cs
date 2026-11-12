namespace NShared;

public class DungeonSkillStateInfo
{
	public int SkillSlotIndex { get; set; }

	public int BaseSkillIndex { get; set; }

	public int BaseStateRatioM { get; set; }

	public int EnhancedSkillIndex { get; set; }

	public int EnhancedStateRatioM { get; set; }

	public int UseableSkillCount { get; set; }

	public SkillUseResult SkillCanUseResult { get; set; }
}
