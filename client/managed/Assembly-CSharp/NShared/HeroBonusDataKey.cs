using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct HeroBonusDataKey
{
	[Key(0)]
	public int HeroIndex;

	[Key(1)]
	public int HeroStar;

	[Key(2)]
	public int HeroTranscended;

	public HeroBonusDataKey(int heroIndex, int heroStar, int heroTranscended)
	{
		HeroIndex = heroIndex;
		HeroStar = heroStar;
		HeroTranscended = heroTranscended;
	}

	public bool Equals(HeroBonusDataKey other)
	{
		if (HeroIndex == other.HeroIndex && HeroStar == other.HeroStar)
		{
			return HeroTranscended == other.HeroTranscended;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(HeroIndex, HeroStar, HeroTranscended);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({HeroIndex}, {HeroStar}, {HeroTranscended})";
	}
}
