using System.Collections.Generic;

namespace NGame2.NAccount.NStatDataCache;

public class HeroKeyComparer : IEqualityComparer<HeroKey>
{
	public bool Equals(HeroKey x, HeroKey y)
	{
		return HeroKey.IsEquals(x, y);
	}

	public int GetHashCode(HeroKey obj)
	{
		return HeroKey.GetHashCode(obj);
	}
}
