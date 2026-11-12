using System;
using System.Collections.Generic;

namespace NShared.NJit.NResolver;

public class BooleanValueComparer : IKeyComparerT<bool>, IKeyComparer, IEqualityComparer<bool>
{
	private Type HandleType => typeof(bool);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<bool>.Equals(bool x, bool y)
	{
		return x == y;
	}

	int IEqualityComparer<bool>.GetHashCode(bool obj)
	{
		if (!obj)
		{
			return 0;
		}
		return 1;
	}
}
