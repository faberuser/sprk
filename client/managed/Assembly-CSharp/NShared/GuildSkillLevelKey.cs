namespace NShared;

public class GuildSkillLevelKey : Tuple<int, int>
{
	public GuildSkillLevelKey()
		: base(0, 0)
	{
	}

	public GuildSkillLevelKey(int skillIndex, int level)
		: base(skillIndex, level)
	{
	}
}
