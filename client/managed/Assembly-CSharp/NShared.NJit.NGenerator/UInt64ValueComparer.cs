using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class UInt64ValueComparer : IKeyComparerT<ulong>, IKeyComparer, IEqualityComparer<ulong>
{
	private Type HandleType => typeof(ulong);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<ulong>.Equals(ulong x, ulong y)
	{
		return x == y;
	}

	int IEqualityComparer<ulong>.GetHashCode(ulong obj)
	{
		return (int)obj;
	}
}
