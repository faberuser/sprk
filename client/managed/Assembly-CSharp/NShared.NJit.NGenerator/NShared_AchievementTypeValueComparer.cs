using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_AchievementTypeValueComparer : IKeyComparerT<AchievementType>, IKeyComparer, IEqualityComparer<AchievementType>
{
	private Type HandleType => typeof(AchievementType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<AchievementType>.Equals(AchievementType x, AchievementType y)
	{
		return x == y;
	}

	int IEqualityComparer<AchievementType>.GetHashCode(AchievementType obj)
	{
		return (int)obj;
	}
}
