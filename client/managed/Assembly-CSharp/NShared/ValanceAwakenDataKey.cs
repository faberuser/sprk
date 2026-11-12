using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct ValanceAwakenDataKey
{
	[Key(0)]
	public int Tier;

	[Key(1)]
	public int DetailIndex;

	[Key(2)]
	public CreatureTagType TagType;

	[Key(3)]
	public int Star;

	public ValanceAwakenDataKey(int tier, int detailIndex, CreatureTagType tagType, int star)
	{
		Tier = tier;
		DetailIndex = detailIndex;
		TagType = tagType;
		Star = star;
	}

	public bool Equals(ValanceAwakenDataKey other)
	{
		if (Tier == other.Tier && DetailIndex == other.DetailIndex && TagType == other.TagType)
		{
			return Star == other.Star;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(Tier, DetailIndex, TagType, Star);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({Tier}, {DetailIndex}, {TagType}, {Star})";
	}
}
