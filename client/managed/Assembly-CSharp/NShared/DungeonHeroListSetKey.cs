using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct DungeonHeroListSetKey
{
	[Key(0)]
	public int GroupIndex;

	[Key(1)]
	public int HeroIndex;

	public DungeonHeroListSetKey(int groupIndex, int heroIndex)
	{
		GroupIndex = groupIndex;
		HeroIndex = heroIndex;
	}

	public bool Equals(DungeonHeroListSetKey other)
	{
		if (GroupIndex == other.GroupIndex)
		{
			return HeroIndex == other.HeroIndex;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(GroupIndex, HeroIndex);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({GroupIndex}, {HeroIndex})";
	}
}
