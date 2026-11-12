using System.Collections.Generic;

namespace NGame2.NAccount.NStatDataCache;

public class CachedHeroKeyComparer : IEqualityComparer<CachedHeroKey>
{
	public bool Equals(CachedHeroKey x, CachedHeroKey y)
	{
		return CachedHeroKey.IsEquals(x, y);
	}

	public int GetHashCode(CachedHeroKey obj)
	{
		return CachedHeroKey.GetHashCode(obj);
	}
}
