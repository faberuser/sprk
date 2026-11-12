using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_DungeonHeroSetKeyComparer : IKeyComparerT<DungeonHeroSetKey>, IKeyComparer, IEqualityComparer<DungeonHeroSetKey>
{
	private Type HandleType => typeof(DungeonHeroSetKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<DungeonHeroSetKey>.Equals(DungeonHeroSetKey x, DungeonHeroSetKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<DungeonHeroSetKey>.GetHashCode(DungeonHeroSetKey obj)
	{
		return obj.GetHashCode();
	}
}
