using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_ItemTypeValueComparer : IKeyComparerT<ItemType>, IKeyComparer, IEqualityComparer<ItemType>
{
	private Type HandleType => typeof(ItemType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<ItemType>.Equals(ItemType x, ItemType y)
	{
		return x == y;
	}

	int IEqualityComparer<ItemType>.GetHashCode(ItemType obj)
	{
		return (int)obj;
	}
}
