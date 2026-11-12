using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_ShopAndIndexKeyComparer : IKeyComparerT<ShopAndIndexKey>, IKeyComparer, IEqualityComparer<ShopAndIndexKey>
{
	private Type HandleType => typeof(ShopAndIndexKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<ShopAndIndexKey>.Equals(ShopAndIndexKey x, ShopAndIndexKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<ShopAndIndexKey>.GetHashCode(ShopAndIndexKey obj)
	{
		return obj.GetHashCode();
	}
}
