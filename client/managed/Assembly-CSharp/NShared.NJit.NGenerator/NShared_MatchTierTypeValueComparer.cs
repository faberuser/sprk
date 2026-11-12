using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_MatchTierTypeValueComparer : IKeyComparerT<MatchTierType>, IKeyComparer, IEqualityComparer<MatchTierType>
{
	private Type HandleType => typeof(MatchTierType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<MatchTierType>.Equals(MatchTierType x, MatchTierType y)
	{
		return x == y;
	}

	int IEqualityComparer<MatchTierType>.GetHashCode(MatchTierType obj)
	{
		return (int)obj;
	}
}
