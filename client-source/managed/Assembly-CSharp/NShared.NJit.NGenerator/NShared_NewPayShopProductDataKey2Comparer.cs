using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_NewPayShopProductDataKey2Comparer : IKeyComparerT<NewPayShopProductDataKey2>, IKeyComparer, IEqualityComparer<NewPayShopProductDataKey2>
{
	private Type HandleType => typeof(NewPayShopProductDataKey2);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<NewPayShopProductDataKey2>.Equals(NewPayShopProductDataKey2 x, NewPayShopProductDataKey2 y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<NewPayShopProductDataKey2>.GetHashCode(NewPayShopProductDataKey2 obj)
	{
		return obj.GetHashCode();
	}
}
