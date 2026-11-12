using System.Collections.Generic;

namespace NGame2.NAccount.NStatDataCache;

public class SoulWeaponKeyComparer : IEqualityComparer<SoulWeaponKey>
{
	public bool Equals(SoulWeaponKey x, SoulWeaponKey y)
	{
		return SoulWeaponKey.IsEquals(x, y);
	}

	public int GetHashCode(SoulWeaponKey obj)
	{
		return SoulWeaponKey.GetHashCode(obj);
	}
}
