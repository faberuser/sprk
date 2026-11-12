namespace NGame2.NUI.NComponent2.NBattle;

public struct ReplaceSkillData
{
	public EReplaceSkillType Type;

	public int SkillIndex;

	public ReplaceSkillData(EReplaceSkillType type, int skillIndex)
	{
		Type = type;
		SkillIndex = skillIndex;
	}
}
