using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_DungeonHeroListSetKeyComparer : IKeyComparerT<DungeonHeroListSetKey>, IKeyComparer, IEqualityComparer<DungeonHeroListSetKey>
{
	private Type HandleType => typeof(DungeonHeroListSetKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<DungeonHeroListSetKey>.Equals(DungeonHeroListSetKey x, DungeonHeroListSetKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<DungeonHeroListSetKey>.GetHashCode(DungeonHeroListSetKey obj)
	{
		return obj.GetHashCode();
	}
}
