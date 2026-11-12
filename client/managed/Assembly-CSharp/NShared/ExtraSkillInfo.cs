namespace NShared;

[JsonSerializable]
public class ExtraSkillInfo
{
	public int SkillIndex { get; set; }

	public int SkillLevel { get; set; }

	public ExtraSkillInfo()
	{
	}

	public ExtraSkillInfo(int skillIndex)
	{
		SkillIndex = skillIndex;
		SkillLevel = 0;
	}

	public ExtraSkillInfo(int skillIndex, int skillLevel)
	{
		SkillIndex = skillIndex;
		SkillLevel = skillLevel;
	}

	public bool Equals(ExtraSkillInfo other)
	{
		if (SkillIndex == other.SkillIndex)
		{
			return SkillLevel == other.SkillLevel;
		}
		return false;
	}
}
