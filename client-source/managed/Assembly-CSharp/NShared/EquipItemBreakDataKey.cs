using MessagePack;
using NShared.NJit;

namespace NShared;

[MessagePackObject(false)]
[JitKey]
public struct EquipItemBreakDataKey
{
	[Key(0)]
	public int Tier;

	[Key(1)]
	public int DetailIndex;

	[Key(2)]
	public string ItemCode;

	[Key(3)]
	public int Star;

	[Key(4)]
	public int SetIndex;

	public EquipItemBreakDataKey(int tier, int detailIndex, string itemCode, int star, int setIndex)
	{
		Tier = tier;
		DetailIndex = detailIndex;
		ItemCode = itemCode;
		Star = star;
		SetIndex = setIndex;
	}

	public bool Equals(EquipItemBreakDataKey other)
	{
		if (Tier == other.Tier && DetailIndex == other.DetailIndex && ItemCode == other.ItemCode && Star == other.Star)
		{
			return SetIndex == other.SetIndex;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return JitOption.BuildHash(Tier, DetailIndex, ItemCode, Star, SetIndex);
	}

	public override string ToString()
	{
		return $"{GetType().Name}({Tier}, {DetailIndex}, {ItemCode}, {Star}, {SetIndex})";
	}
}
