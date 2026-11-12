using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_MatchRewardDataKey2Comparer : IKeyComparerT<MatchRewardDataKey2>, IKeyComparer, IEqualityComparer<MatchRewardDataKey2>
{
	private Type HandleType => typeof(MatchRewardDataKey2);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<MatchRewardDataKey2>.Equals(MatchRewardDataKey2 x, MatchRewardDataKey2 y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<MatchRewardDataKey2>.GetHashCode(MatchRewardDataKey2 obj)
	{
		return obj.GetHashCode();
	}
}
