using NShared.NJit;

namespace NShared;

[JitKey]
public struct EquipUpgradeExpDataKey
{
	public EquipPartType PartType;

	public int Tier;

	public int DetailIndex;

	public int Level;

	public EquipUpgradeExpDataKey(EquipPartType partType, int tier, int detailIndex, int level)
	{
		PartType = partType;
		Tier = tier;
		DetailIndex = detailIndex;
		Level = level;
	}

	public bool Equals(EquipUpgradeExpDataKey other)
	{
		if (PartType == other.PartType && Tier == other.Tier && DetailIndex == other.DetailIndex)
		{
			return Level == other.Level;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(PartType, Tier, DetailIndex, Level);
	}

	public override string ToString()
	{
		return $"PartType={PartType} Tier={Tier} DetailIndex={DetailIndex} Level={Level}";
	}
}
