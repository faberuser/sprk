using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct ShopAndIndexKey
{
	[Key(0)]
	public int ShopIndex;

	[Key(1)]
	public int Index;

	[IgnoreMember]
	public bool IsValid
	{
		get
		{
			if (ShopIndex != 0)
			{
				return Index != 0;
			}
			return false;
		}
	}

	public ShopAndIndexKey(int shopIndex, int index)
	{
		ShopIndex = shopIndex;
		Index = index;
	}

	public bool Equals(ShopAndIndexKey other)
	{
		if (ShopIndex == other.ShopIndex)
		{
			return Index == other.Index;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(ShopIndex, Index);
	}

	public override string ToString()
	{
		return $"ShopIndex={ShopIndex} Index={Index}";
	}
}
