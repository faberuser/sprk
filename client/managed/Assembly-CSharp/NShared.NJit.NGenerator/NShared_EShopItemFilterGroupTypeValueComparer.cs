using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_EShopItemFilterGroupTypeValueComparer : IKeyComparerT<EShopItemFilterGroupType>, IKeyComparer, IEqualityComparer<EShopItemFilterGroupType>
{
	private Type HandleType => typeof(EShopItemFilterGroupType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<EShopItemFilterGroupType>.Equals(EShopItemFilterGroupType x, EShopItemFilterGroupType y)
	{
		return x == y;
	}

	int IEqualityComparer<EShopItemFilterGroupType>.GetHashCode(EShopItemFilterGroupType obj)
	{
		return (int)obj;
	}
}
