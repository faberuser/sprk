using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct SoulWeaponEtherRateKey
{
	[Key(0)]
	public int ReinforceLevel;

	[Key(1)]
	public string ItemCode;

	public SoulWeaponEtherRateKey(int reinforceLevel, string itemCode)
	{
		ReinforceLevel = reinforceLevel;
		ItemCode = itemCode;
	}

	public bool Equals(SoulWeaponEtherRateKey other)
	{
		if (ReinforceLevel == other.ReinforceLevel)
		{
			return ItemCode == other.ItemCode;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(ReinforceLevel, ItemCode);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({ReinforceLevel}, {ItemCode})";
	}
}
