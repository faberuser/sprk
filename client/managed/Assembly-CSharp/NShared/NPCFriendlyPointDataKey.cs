using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct NPCFriendlyPointDataKey
{
	[Key(0)]
	public int HeroIndex;

	[Key(1)]
	public int Step;

	public NPCFriendlyPointDataKey(int heroIndex, int step)
	{
		HeroIndex = heroIndex;
		Step = step;
	}

	public bool Equals(NPCFriendlyPointDataKey other)
	{
		if (HeroIndex == other.HeroIndex)
		{
			return Step == other.Step;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(HeroIndex, Step);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({HeroIndex}, {Step})";
	}
}
