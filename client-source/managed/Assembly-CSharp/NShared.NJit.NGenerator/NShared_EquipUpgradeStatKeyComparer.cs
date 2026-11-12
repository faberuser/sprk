using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_EquipUpgradeStatKeyComparer : IKeyComparerT<EquipUpgradeStatKey>, IKeyComparer, IEqualityComparer<EquipUpgradeStatKey>
{
	private Type HandleType => typeof(EquipUpgradeStatKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<EquipUpgradeStatKey>.Equals(EquipUpgradeStatKey x, EquipUpgradeStatKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<EquipUpgradeStatKey>.GetHashCode(EquipUpgradeStatKey obj)
	{
		return obj.GetHashCode();
	}
}
