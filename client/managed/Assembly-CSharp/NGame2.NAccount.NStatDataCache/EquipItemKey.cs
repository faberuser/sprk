using NShared;

namespace NGame2.NAccount.NStatDataCache;

public struct EquipItemKey
{
	public int ItemIndex { get; private set; }

	public int Star { get; private set; }

	public int Level { get; private set; }

	public EquipItemKey(EquipItemInfo info)
	{
		ItemIndex = info.ItemIndex;
		Star = info.Star;
		Level = info.Level;
	}

	public static bool IsEquals(EquipItemKey x, EquipItemKey y)
	{
		if (x.ItemIndex != y.ItemIndex)
		{
			return false;
		}
		if (x.Level != y.Level)
		{
			return false;
		}
		if (x.Star != y.Star)
		{
			return false;
		}
		return true;
	}

	public static int GetHashCode(EquipItemKey obj)
	{
		return ((31 * 17 + obj.ItemIndex) * 17 + obj.Level) * 17 + obj.Star;
	}
}
