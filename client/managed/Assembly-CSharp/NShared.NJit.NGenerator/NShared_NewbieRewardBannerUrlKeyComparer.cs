using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_NewbieRewardBannerUrlKeyComparer : IKeyComparerT<NewbieRewardBannerUrlKey>, IKeyComparer, IEqualityComparer<NewbieRewardBannerUrlKey>
{
	private Type HandleType => typeof(NewbieRewardBannerUrlKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<NewbieRewardBannerUrlKey>.Equals(NewbieRewardBannerUrlKey x, NewbieRewardBannerUrlKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<NewbieRewardBannerUrlKey>.GetHashCode(NewbieRewardBannerUrlKey obj)
	{
		return obj.GetHashCode();
	}
}
