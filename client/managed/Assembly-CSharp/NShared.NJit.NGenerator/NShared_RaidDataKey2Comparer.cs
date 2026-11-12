using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_RaidDataKey2Comparer : IKeyComparerT<RaidDataKey2>, IKeyComparer, IEqualityComparer<RaidDataKey2>
{
	private Type HandleType => typeof(RaidDataKey2);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<RaidDataKey2>.Equals(RaidDataKey2 x, RaidDataKey2 y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<RaidDataKey2>.GetHashCode(RaidDataKey2 obj)
	{
		return obj.GetHashCode();
	}
}
