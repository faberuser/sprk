using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct SoulBreakKey
{
	[Key(0)]
	public CreatureTagType TagType;

	[Key(1)]
	public int Grade;

	[Key(2)]
	public int ReinforceLevel;

	public SoulBreakKey(CreatureTagType tagType, int grade, int reinforceLevel)
	{
		TagType = tagType;
		Grade = grade;
		ReinforceLevel = reinforceLevel;
	}

	public bool Equals(SoulBreakKey other)
	{
		if (TagType == other.TagType && Grade == other.Grade)
		{
			return ReinforceLevel == other.ReinforceLevel;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(TagType, Grade, ReinforceLevel);
	}

	public override string ToString()
	{
		return $"TagType={TagType} Grade={Grade} ReinforceLevel={ReinforceLevel}";
	}
}
