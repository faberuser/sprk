using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_EValanceFilterGroupTypeValueComparer : IKeyComparerT<EValanceFilterGroupType>, IKeyComparer, IEqualityComparer<EValanceFilterGroupType>
{
	private Type HandleType => typeof(EValanceFilterGroupType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<EValanceFilterGroupType>.Equals(EValanceFilterGroupType x, EValanceFilterGroupType y)
	{
		return x == y;
	}

	int IEqualityComparer<EValanceFilterGroupType>.GetHashCode(EValanceFilterGroupType obj)
	{
		return (int)obj;
	}
}
