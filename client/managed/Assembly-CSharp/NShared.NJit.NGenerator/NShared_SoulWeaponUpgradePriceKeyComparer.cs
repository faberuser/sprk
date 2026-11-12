using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_SoulWeaponUpgradePriceKeyComparer : IKeyComparerT<SoulWeaponUpgradePriceKey>, IKeyComparer, IEqualityComparer<SoulWeaponUpgradePriceKey>
{
	private Type HandleType => typeof(SoulWeaponUpgradePriceKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<SoulWeaponUpgradePriceKey>.Equals(SoulWeaponUpgradePriceKey x, SoulWeaponUpgradePriceKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<SoulWeaponUpgradePriceKey>.GetHashCode(SoulWeaponUpgradePriceKey obj)
	{
		return obj.GetHashCode();
	}
}
