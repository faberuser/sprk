using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct SoulWeaponUpgradePriceKey
{
	[Key(0)]
	public int ItemIndex;

	[Key(1)]
	public int Grade;

	public SoulWeaponUpgradePriceKey(int itemIndex, int grade)
	{
		ItemIndex = itemIndex;
		Grade = grade;
	}

	public bool Equals(SoulWeaponUpgradePriceKey other)
	{
		if (ItemIndex == other.ItemIndex)
		{
			return Grade == other.Grade;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(ItemIndex, Grade);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({ItemIndex}, {Grade})";
	}
}
