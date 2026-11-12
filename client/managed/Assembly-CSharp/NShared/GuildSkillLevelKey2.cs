using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct GuildSkillLevelKey2
{
	[Key(0)]
	public int SkillIndex;

	[Key(1)]
	public int Level;

	public GuildSkillLevelKey2(int skillIndex, int level)
	{
		SkillIndex = skillIndex;
		Level = level;
	}

	public bool Equals(GuildSkillLevelKey2 other)
	{
		if (SkillIndex == other.SkillIndex)
		{
			return Level == other.Level;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(SkillIndex, Level);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({SkillIndex}, {Level})";
	}
}
