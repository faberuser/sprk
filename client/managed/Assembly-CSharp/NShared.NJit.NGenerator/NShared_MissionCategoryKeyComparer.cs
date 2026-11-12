using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_MissionCategoryKeyComparer : IKeyComparerT<MissionCategoryKey>, IKeyComparer, IEqualityComparer<MissionCategoryKey>
{
	private Type HandleType => typeof(MissionCategoryKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<MissionCategoryKey>.Equals(MissionCategoryKey x, MissionCategoryKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<MissionCategoryKey>.GetHashCode(MissionCategoryKey obj)
	{
		return obj.GetHashCode();
	}
}
