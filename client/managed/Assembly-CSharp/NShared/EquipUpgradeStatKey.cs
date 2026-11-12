using NShared.NJit;

namespace NShared;

[JitKey]
public struct EquipUpgradeStatKey
{
	public int Star;

	public int DetailIndex;

	public int Level;

	public EquipUpgradeStatKey(int star, int detailIndex, int level)
	{
		Star = star;
		DetailIndex = detailIndex;
		Level = level;
	}

	public bool Equals(EquipUpgradeStatKey other)
	{
		if (Star == other.Star && DetailIndex == other.DetailIndex)
		{
			return Level == other.Level;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(Star, DetailIndex, Level);
	}

	public override string ToString()
	{
		return $"Star={Star} DetailIndex={DetailIndex} Level={Level}";
	}
}
