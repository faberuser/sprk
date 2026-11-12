using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_TowerFloorKeyComparer : IKeyComparerT<TowerFloorKey>, IKeyComparer, IEqualityComparer<TowerFloorKey>
{
	private Type HandleType => typeof(TowerFloorKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<TowerFloorKey>.Equals(TowerFloorKey x, TowerFloorKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<TowerFloorKey>.GetHashCode(TowerFloorKey obj)
	{
		return obj.GetHashCode();
	}
}
