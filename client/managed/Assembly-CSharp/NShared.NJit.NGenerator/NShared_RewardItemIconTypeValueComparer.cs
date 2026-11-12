using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_RewardItemIconTypeValueComparer : IKeyComparerT<RewardItemIconType>, IKeyComparer, IEqualityComparer<RewardItemIconType>
{
	private Type HandleType => typeof(RewardItemIconType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<RewardItemIconType>.Equals(RewardItemIconType x, RewardItemIconType y)
	{
		return x == y;
	}

	int IEqualityComparer<RewardItemIconType>.GetHashCode(RewardItemIconType obj)
	{
		return (int)obj;
	}
}
