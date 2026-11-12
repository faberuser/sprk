using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct EquipUpgradePriceDataKey
{
	[Key(0)]
	public int Tier;

	[Key(1)]
	public int DetailIndex;

	[Key(2)]
	public int Level;

	public EquipUpgradePriceDataKey(int tier, int detailIndex, int level)
	{
		Tier = tier;
		DetailIndex = detailIndex;
		Level = level;
	}

	public bool Equals(EquipUpgradePriceDataKey other)
	{
		if (Tier == other.Tier && DetailIndex == other.DetailIndex)
		{
			return Level == other.Level;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(Tier, DetailIndex, Level);
	}

	public override string ToString()
	{
		return $"Tier={Tier} DetailIndex={DetailIndex} Level={Level}";
	}
}
