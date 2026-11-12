using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_AchievementKey2Comparer : IKeyComparerT<AchievementKey2>, IKeyComparer, IEqualityComparer<AchievementKey2>
{
	private Type HandleType => typeof(AchievementKey2);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<AchievementKey2>.Equals(AchievementKey2 x, AchievementKey2 y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<AchievementKey2>.GetHashCode(AchievementKey2 obj)
	{
		return obj.GetHashCode();
	}
}
