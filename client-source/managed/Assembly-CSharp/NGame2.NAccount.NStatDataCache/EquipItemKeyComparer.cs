using System.Collections.Generic;

namespace NGame2.NAccount.NStatDataCache;

public class EquipItemKeyComparer : IEqualityComparer<EquipItemKey>
{
	public bool Equals(EquipItemKey x, EquipItemKey y)
	{
		return EquipItemKey.IsEquals(x, y);
	}

	public int GetHashCode(EquipItemKey obj)
	{
		return EquipItemKey.GetHashCode(obj);
	}
}
