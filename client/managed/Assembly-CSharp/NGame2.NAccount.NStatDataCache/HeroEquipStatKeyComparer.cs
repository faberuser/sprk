using System.Collections.Generic;

namespace NGame2.NAccount.NStatDataCache;

public class HeroEquipStatKeyComparer : IEqualityComparer<HeroEquipStatKey>
{
	public bool Equals(HeroEquipStatKey x, HeroEquipStatKey y)
	{
		return HeroEquipStatKey.IsEquals(x, y);
	}

	public int GetHashCode(HeroEquipStatKey obj)
	{
		return HeroEquipStatKey.GetHashCode(obj);
	}
}
