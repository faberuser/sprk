using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class Int16ValueComparer : IKeyComparerT<short>, IKeyComparer, IEqualityComparer<short>
{
	private Type HandleType => typeof(short);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<short>.Equals(short x, short y)
	{
		return x == y;
	}

	int IEqualityComparer<short>.GetHashCode(short obj)
	{
		return obj;
	}
}
