using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_RewardItemIconTypeDataKeyComparer : IKeyComparerT<RewardItemIconTypeDataKey>, IKeyComparer, IEqualityComparer<RewardItemIconTypeDataKey>
{
	private Type HandleType => typeof(RewardItemIconTypeDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<RewardItemIconTypeDataKey>.Equals(RewardItemIconTypeDataKey x, RewardItemIconTypeDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<RewardItemIconTypeDataKey>.GetHashCode(RewardItemIconTypeDataKey obj)
	{
		return obj.GetHashCode();
	}
}
