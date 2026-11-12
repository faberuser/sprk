using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_WorldBossDailyAchievemenDataKeyComparer : IKeyComparerT<WorldBossDailyAchievemenDataKey>, IKeyComparer, IEqualityComparer<WorldBossDailyAchievemenDataKey>
{
	private Type HandleType => typeof(WorldBossDailyAchievemenDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<WorldBossDailyAchievemenDataKey>.Equals(WorldBossDailyAchievemenDataKey x, WorldBossDailyAchievemenDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<WorldBossDailyAchievemenDataKey>.GetHashCode(WorldBossDailyAchievemenDataKey obj)
	{
		return obj.GetHashCode();
	}
}
