using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_InventoryTypeValueComparer : IKeyComparerT<InventoryType>, IKeyComparer, IEqualityComparer<InventoryType>
{
	private Type HandleType => typeof(InventoryType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<InventoryType>.Equals(InventoryType x, InventoryType y)
	{
		return x == y;
	}

	int IEqualityComparer<InventoryType>.GetHashCode(InventoryType obj)
	{
		return (int)obj;
	}
}
