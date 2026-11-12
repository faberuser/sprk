using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class Int64ValueComparer : IKeyComparerT<long>, IKeyComparer, IEqualityComparer<long>
{
	private Type HandleType => typeof(long);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<long>.Equals(long x, long y)
	{
		return x == y;
	}

	int IEqualityComparer<long>.GetHashCode(long obj)
	{
		return (int)obj;
	}
}
