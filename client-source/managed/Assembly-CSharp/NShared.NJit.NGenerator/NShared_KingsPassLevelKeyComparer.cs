using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_KingsPassLevelKeyComparer : IKeyComparerT<KingsPassLevelKey>, IKeyComparer, IEqualityComparer<KingsPassLevelKey>
{
	private Type HandleType => typeof(KingsPassLevelKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<KingsPassLevelKey>.Equals(KingsPassLevelKey x, KingsPassLevelKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<KingsPassLevelKey>.GetHashCode(KingsPassLevelKey obj)
	{
		return obj.GetHashCode();
	}
}
