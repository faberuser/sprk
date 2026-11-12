using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class SByteValueComparer : IKeyComparerT<sbyte>, IKeyComparer, IEqualityComparer<sbyte>
{
	private Type HandleType => typeof(sbyte);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<sbyte>.Equals(sbyte x, sbyte y)
	{
		return x == y;
	}

	int IEqualityComparer<sbyte>.GetHashCode(sbyte obj)
	{
		return obj;
	}
}
