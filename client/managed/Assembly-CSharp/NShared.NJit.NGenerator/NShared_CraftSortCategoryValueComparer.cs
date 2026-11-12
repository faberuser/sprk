using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_CraftSortCategoryValueComparer : IKeyComparerT<CraftSortCategory>, IKeyComparer, IEqualityComparer<CraftSortCategory>
{
	private Type HandleType => typeof(CraftSortCategory);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<CraftSortCategory>.Equals(CraftSortCategory x, CraftSortCategory y)
	{
		return x == y;
	}

	int IEqualityComparer<CraftSortCategory>.GetHashCode(CraftSortCategory obj)
	{
		return (int)obj;
	}
}
