using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_EquipUpgradeExpDataKeyComparer : IKeyComparerT<EquipUpgradeExpDataKey>, IKeyComparer, IEqualityComparer<EquipUpgradeExpDataKey>
{
	private Type HandleType => typeof(EquipUpgradeExpDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<EquipUpgradeExpDataKey>.Equals(EquipUpgradeExpDataKey x, EquipUpgradeExpDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<EquipUpgradeExpDataKey>.GetHashCode(EquipUpgradeExpDataKey obj)
	{
		return obj.GetHashCode();
	}
}
