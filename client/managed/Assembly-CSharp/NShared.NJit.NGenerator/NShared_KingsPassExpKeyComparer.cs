using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_KingsPassExpKeyComparer : IKeyComparerT<KingsPassExpKey>, IKeyComparer, IEqualityComparer<KingsPassExpKey>
{
	private Type HandleType => typeof(KingsPassExpKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<KingsPassExpKey>.Equals(KingsPassExpKey x, KingsPassExpKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<KingsPassExpKey>.GetHashCode(KingsPassExpKey obj)
	{
		return obj.GetHashCode();
	}
}
