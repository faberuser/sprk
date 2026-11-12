using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_ItemActionTypeValueComparer : IKeyComparerT<ItemActionType>, IKeyComparer, IEqualityComparer<ItemActionType>
{
	private Type HandleType => typeof(ItemActionType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<ItemActionType>.Equals(ItemActionType x, ItemActionType y)
	{
		return x == y;
	}

	int IEqualityComparer<ItemActionType>.GetHashCode(ItemActionType obj)
	{
		return (int)obj;
	}
}
