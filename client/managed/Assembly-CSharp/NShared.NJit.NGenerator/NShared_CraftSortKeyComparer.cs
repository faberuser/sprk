using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_CraftSortKeyComparer : IKeyComparerT<CraftSortKey>, IKeyComparer, IEqualityComparer<CraftSortKey>
{
	private Type HandleType => typeof(CraftSortKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<CraftSortKey>.Equals(CraftSortKey x, CraftSortKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<CraftSortKey>.GetHashCode(CraftSortKey obj)
	{
		return obj.GetHashCode();
	}
}
