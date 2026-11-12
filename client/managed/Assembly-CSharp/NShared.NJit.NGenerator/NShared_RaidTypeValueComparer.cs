using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_RaidTypeValueComparer : IKeyComparerT<RaidType>, IKeyComparer, IEqualityComparer<RaidType>
{
	private Type HandleType => typeof(RaidType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<RaidType>.Equals(RaidType x, RaidType y)
	{
		return x == y;
	}

	int IEqualityComparer<RaidType>.GetHashCode(RaidType obj)
	{
		return (int)obj;
	}
}
