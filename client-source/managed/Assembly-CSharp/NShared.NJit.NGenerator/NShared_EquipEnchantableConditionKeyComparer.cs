using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_EquipEnchantableConditionKeyComparer : IKeyComparerT<EquipEnchantableConditionKey>, IKeyComparer, IEqualityComparer<EquipEnchantableConditionKey>
{
	private Type HandleType => typeof(EquipEnchantableConditionKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<EquipEnchantableConditionKey>.Equals(EquipEnchantableConditionKey x, EquipEnchantableConditionKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<EquipEnchantableConditionKey>.GetHashCode(EquipEnchantableConditionKey obj)
	{
		return obj.GetHashCode();
	}
}
