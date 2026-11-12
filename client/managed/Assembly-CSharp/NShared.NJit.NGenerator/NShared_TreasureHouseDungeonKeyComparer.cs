using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_TreasureHouseDungeonKeyComparer : IKeyComparerT<TreasureHouseDungeonKey>, IKeyComparer, IEqualityComparer<TreasureHouseDungeonKey>
{
	private Type HandleType => typeof(TreasureHouseDungeonKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<TreasureHouseDungeonKey>.Equals(TreasureHouseDungeonKey x, TreasureHouseDungeonKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<TreasureHouseDungeonKey>.GetHashCode(TreasureHouseDungeonKey obj)
	{
		return obj.GetHashCode();
	}
}
