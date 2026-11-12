using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_ShopItemGroupKeyComparer : IKeyComparerT<ShopItemGroupKey>, IKeyComparer, IEqualityComparer<ShopItemGroupKey>
{
	private Type HandleType => typeof(ShopItemGroupKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<ShopItemGroupKey>.Equals(ShopItemGroupKey x, ShopItemGroupKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<ShopItemGroupKey>.GetHashCode(ShopItemGroupKey obj)
	{
		return obj.GetHashCode();
	}
}
