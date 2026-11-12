using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct HeroLimitBreakDataKey
{
	[Key(0)]
	public int HeroIndex;

	[Key(1)]
	public int LimitBreakLevel;

	public HeroLimitBreakDataKey(int heroIndex, int limitBreakLevel)
	{
		HeroIndex = heroIndex;
		LimitBreakLevel = limitBreakLevel;
	}

	public bool Equals(HeroLimitBreakDataKey other)
	{
		if (HeroIndex == other.HeroIndex)
		{
			return LimitBreakLevel == other.LimitBreakLevel;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(HeroIndex, LimitBreakLevel);
	}

	public override string ToString()
	{
		return $"HeroIndex={HeroIndex} LimitBreakLevel={LimitBreakLevel}";
	}
}
