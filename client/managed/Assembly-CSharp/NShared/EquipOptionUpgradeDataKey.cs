using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct EquipOptionUpgradeDataKey
{
	[Key(0)]
	public int Tier;

	[Key(1)]
	public int DetailIndex;

	public EquipOptionUpgradeDataKey(int tier, int detailIndex)
	{
		Tier = tier;
		DetailIndex = detailIndex;
	}

	public bool Equals(EquipOptionUpgradeDataKey other)
	{
		if (Tier == other.Tier)
		{
			return DetailIndex == other.DetailIndex;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(Tier, DetailIndex);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({Tier}, {DetailIndex})";
	}
}
