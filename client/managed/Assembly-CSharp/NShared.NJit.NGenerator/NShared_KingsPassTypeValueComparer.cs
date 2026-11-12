using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_KingsPassTypeValueComparer : IKeyComparerT<KingsPassType>, IKeyComparer, IEqualityComparer<KingsPassType>
{
	private Type HandleType => typeof(KingsPassType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<KingsPassType>.Equals(KingsPassType x, KingsPassType y)
	{
		return x == y;
	}

	int IEqualityComparer<KingsPassType>.GetHashCode(KingsPassType obj)
	{
		return (int)obj;
	}
}
