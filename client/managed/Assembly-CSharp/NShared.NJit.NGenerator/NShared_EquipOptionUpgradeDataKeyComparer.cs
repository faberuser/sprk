using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_EquipOptionUpgradeDataKeyComparer : IKeyComparerT<EquipOptionUpgradeDataKey>, IKeyComparer, IEqualityComparer<EquipOptionUpgradeDataKey>
{
	private Type HandleType => typeof(EquipOptionUpgradeDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<EquipOptionUpgradeDataKey>.Equals(EquipOptionUpgradeDataKey x, EquipOptionUpgradeDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<EquipOptionUpgradeDataKey>.GetHashCode(EquipOptionUpgradeDataKey obj)
	{
		return obj.GetHashCode();
	}
}
