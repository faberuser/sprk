using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_ThreatTypeValueComparer : IKeyComparerT<ThreatType>, IKeyComparer, IEqualityComparer<ThreatType>
{
	private Type HandleType => typeof(ThreatType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<ThreatType>.Equals(ThreatType x, ThreatType y)
	{
		return x == y;
	}

	int IEqualityComparer<ThreatType>.GetHashCode(ThreatType obj)
	{
		return (int)obj;
	}
}
