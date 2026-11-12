using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct ShopItemGroupKey
{
	[Key(0)]
	public int ShopIndex;

	[Key(1)]
	public int GroupIndex;

	public ShopItemGroupKey(int shopIndex, int groupIndex)
	{
		ShopIndex = shopIndex;
		GroupIndex = groupIndex;
	}

	public bool Equals(ShopItemGroupKey other)
	{
		if (ShopIndex == other.ShopIndex)
		{
			return GroupIndex == other.GroupIndex;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(ShopIndex, GroupIndex);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({ShopIndex}, {GroupIndex})";
	}
}
