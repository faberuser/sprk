using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class Int32ValueComparer : IKeyComparerT<int>, IKeyComparer, IEqualityComparer<int>
{
	private Type HandleType => typeof(int);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<int>.Equals(int x, int y)
	{
		return x == y;
	}

	int IEqualityComparer<int>.GetHashCode(int obj)
	{
		return obj;
	}
}
