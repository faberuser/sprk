using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_NewbieRewardTypeValueComparer : IKeyComparerT<NewbieRewardType>, IKeyComparer, IEqualityComparer<NewbieRewardType>
{
	private Type HandleType => typeof(NewbieRewardType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<NewbieRewardType>.Equals(NewbieRewardType x, NewbieRewardType y)
	{
		return x == y;
	}

	int IEqualityComparer<NewbieRewardType>.GetHashCode(NewbieRewardType obj)
	{
		return (int)obj;
	}
}
