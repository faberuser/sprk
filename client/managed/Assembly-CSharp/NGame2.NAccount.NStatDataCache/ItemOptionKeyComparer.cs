using System.Collections.Generic;

namespace NGame2.NAccount.NStatDataCache;

public class ItemOptionKeyComparer : IEqualityComparer<ItemOptionKey>
{
	public bool Equals(ItemOptionKey x, ItemOptionKey y)
	{
		return ItemOptionKey.IsEquals(x, y);
	}

	public int GetHashCode(ItemOptionKey obj)
	{
		return ItemOptionKey.GetHashCode(obj);
	}
}
