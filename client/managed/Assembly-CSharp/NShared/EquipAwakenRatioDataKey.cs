using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct EquipAwakenRatioDataKey
{
	[Key(0)]
	public int Star;

	[Key(1)]
	public int AwakenPoint;

	public EquipAwakenRatioDataKey(int star, int awakenPoint)
	{
		Star = star;
		AwakenPoint = awakenPoint;
	}

	public bool Equals(EquipAwakenRatioDataKey other)
	{
		if (Star == other.Star)
		{
			return AwakenPoint == other.AwakenPoint;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(Star, AwakenPoint);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({Star}, {AwakenPoint})";
	}
}
