using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_EquipAwakenPriceDataKeyComparer : IKeyComparerT<EquipAwakenPriceDataKey>, IKeyComparer, IEqualityComparer<EquipAwakenPriceDataKey>
{
	private Type HandleType => typeof(EquipAwakenPriceDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<EquipAwakenPriceDataKey>.Equals(EquipAwakenPriceDataKey x, EquipAwakenPriceDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<EquipAwakenPriceDataKey>.GetHashCode(EquipAwakenPriceDataKey obj)
	{
		return obj.GetHashCode();
	}
}
