using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct ValanceEnchantOptionCountDataKey
{
	[Key(0)]
	public string ItemCode;

	[Key(1)]
	public int OptionCount;

	public ValanceEnchantOptionCountDataKey(string itemCode, int optionCount)
	{
		ItemCode = itemCode;
		OptionCount = optionCount;
	}

	public bool Equals(ValanceEnchantOptionCountDataKey other)
	{
		if (ItemCode == other.ItemCode)
		{
			return OptionCount == other.OptionCount;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(ItemCode, OptionCount);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({ItemCode}, {OptionCount})";
	}
}
