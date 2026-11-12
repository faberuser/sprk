using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_LuckyMatchRewardTypeValueComparer : IKeyComparerT<LuckyMatchRewardType>, IKeyComparer, IEqualityComparer<LuckyMatchRewardType>
{
	private Type HandleType => typeof(LuckyMatchRewardType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<LuckyMatchRewardType>.Equals(LuckyMatchRewardType x, LuckyMatchRewardType y)
	{
		return x == y;
	}

	int IEqualityComparer<LuckyMatchRewardType>.GetHashCode(LuckyMatchRewardType obj)
	{
		return (int)obj;
	}
}
