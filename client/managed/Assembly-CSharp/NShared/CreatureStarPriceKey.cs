using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct CreatureStarPriceKey
{
	[Key(0)]
	public CreatureOpenStatusType OpenStatus;

	[Key(1)]
	public int Star;

	public CreatureStarPriceKey(CreatureOpenStatusType openStatus, int star)
	{
		OpenStatus = openStatus;
		Star = star;
	}

	public bool Equals(CreatureStarPriceKey other)
	{
		if (OpenStatus == other.OpenStatus)
		{
			return Star == other.Star;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(OpenStatus, Star);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({OpenStatus}, {Star})";
	}
}
