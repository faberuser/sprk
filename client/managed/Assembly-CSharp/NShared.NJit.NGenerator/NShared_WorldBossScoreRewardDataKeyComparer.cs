using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_WorldBossScoreRewardDataKeyComparer : IKeyComparerT<WorldBossScoreRewardDataKey>, IKeyComparer, IEqualityComparer<WorldBossScoreRewardDataKey>
{
	private Type HandleType => typeof(WorldBossScoreRewardDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<WorldBossScoreRewardDataKey>.Equals(WorldBossScoreRewardDataKey x, WorldBossScoreRewardDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<WorldBossScoreRewardDataKey>.GetHashCode(WorldBossScoreRewardDataKey obj)
	{
		return obj.GetHashCode();
	}
}
