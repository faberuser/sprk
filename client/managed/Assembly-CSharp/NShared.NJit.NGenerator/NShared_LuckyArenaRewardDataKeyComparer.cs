using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_LuckyArenaRewardDataKeyComparer : IKeyComparerT<LuckyArenaRewardDataKey>, IKeyComparer, IEqualityComparer<LuckyArenaRewardDataKey>
{
	private Type HandleType => typeof(LuckyArenaRewardDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<LuckyArenaRewardDataKey>.Equals(LuckyArenaRewardDataKey x, LuckyArenaRewardDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<LuckyArenaRewardDataKey>.GetHashCode(LuckyArenaRewardDataKey obj)
	{
		return obj.GetHashCode();
	}
}
