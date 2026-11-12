using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct ValanceEnchantDataKey
{
	[Key(0)]
	public string ItemCode;

	[Key(1)]
	public string PartType;

	[Key(2)]
	public string SubType;

	public ValanceEnchantDataKey(string itemCode, string partType, string subType)
	{
		ItemCode = itemCode;
		PartType = partType;
		SubType = subType;
	}

	public bool Equals(ValanceEnchantDataKey other)
	{
		if (ItemCode == other.ItemCode && PartType == other.PartType)
		{
			return SubType == other.SubType;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(ItemCode, PartType, SubType);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({ItemCode}, {PartType}, {SubType})";
	}
}
