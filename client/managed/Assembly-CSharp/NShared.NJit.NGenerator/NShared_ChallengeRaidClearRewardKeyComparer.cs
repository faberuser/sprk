using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_ChallengeRaidClearRewardKeyComparer : IKeyComparerT<ChallengeRaidClearRewardKey>, IKeyComparer, IEqualityComparer<ChallengeRaidClearRewardKey>
{
	private Type HandleType => typeof(ChallengeRaidClearRewardKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<ChallengeRaidClearRewardKey>.Equals(ChallengeRaidClearRewardKey x, ChallengeRaidClearRewardKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<ChallengeRaidClearRewardKey>.GetHashCode(ChallengeRaidClearRewardKey obj)
	{
		return obj.GetHashCode();
	}
}
