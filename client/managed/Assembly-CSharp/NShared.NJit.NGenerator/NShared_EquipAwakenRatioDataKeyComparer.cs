using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_EquipAwakenRatioDataKeyComparer : IKeyComparerT<EquipAwakenRatioDataKey>, IKeyComparer, IEqualityComparer<EquipAwakenRatioDataKey>
{
	private Type HandleType => typeof(EquipAwakenRatioDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<EquipAwakenRatioDataKey>.Equals(EquipAwakenRatioDataKey x, EquipAwakenRatioDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<EquipAwakenRatioDataKey>.GetHashCode(EquipAwakenRatioDataKey obj)
	{
		return obj.GetHashCode();
	}
}
