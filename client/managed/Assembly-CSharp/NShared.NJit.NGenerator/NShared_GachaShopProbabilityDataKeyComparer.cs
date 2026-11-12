using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_GachaShopProbabilityDataKeyComparer : IKeyComparerT<GachaShopProbabilityDataKey>, IKeyComparer, IEqualityComparer<GachaShopProbabilityDataKey>
{
	private Type HandleType => typeof(GachaShopProbabilityDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<GachaShopProbabilityDataKey>.Equals(GachaShopProbabilityDataKey x, GachaShopProbabilityDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<GachaShopProbabilityDataKey>.GetHashCode(GachaShopProbabilityDataKey obj)
	{
		return obj.GetHashCode();
	}
}
