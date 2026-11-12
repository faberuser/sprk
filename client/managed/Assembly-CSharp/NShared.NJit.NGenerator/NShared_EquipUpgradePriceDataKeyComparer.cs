using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_EquipUpgradePriceDataKeyComparer : IKeyComparerT<EquipUpgradePriceDataKey>, IKeyComparer, IEqualityComparer<EquipUpgradePriceDataKey>
{
	private Type HandleType => typeof(EquipUpgradePriceDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<EquipUpgradePriceDataKey>.Equals(EquipUpgradePriceDataKey x, EquipUpgradePriceDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<EquipUpgradePriceDataKey>.GetHashCode(EquipUpgradePriceDataKey obj)
	{
		return obj.GetHashCode();
	}
}
