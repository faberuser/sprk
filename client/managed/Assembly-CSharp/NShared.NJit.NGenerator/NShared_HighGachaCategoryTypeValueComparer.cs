using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_HighGachaCategoryTypeValueComparer : IKeyComparerT<HighGachaCategoryType>, IKeyComparer, IEqualityComparer<HighGachaCategoryType>
{
	private Type HandleType => typeof(HighGachaCategoryType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<HighGachaCategoryType>.Equals(HighGachaCategoryType x, HighGachaCategoryType y)
	{
		return x == y;
	}

	int IEqualityComparer<HighGachaCategoryType>.GetHashCode(HighGachaCategoryType obj)
	{
		return (int)obj;
	}
}
