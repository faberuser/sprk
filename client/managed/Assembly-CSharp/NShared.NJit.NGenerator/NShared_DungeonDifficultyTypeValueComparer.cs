using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_DungeonDifficultyTypeValueComparer : IKeyComparerT<DungeonDifficultyType>, IKeyComparer, IEqualityComparer<DungeonDifficultyType>
{
	private Type HandleType => typeof(DungeonDifficultyType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<DungeonDifficultyType>.Equals(DungeonDifficultyType x, DungeonDifficultyType y)
	{
		return x == y;
	}

	int IEqualityComparer<DungeonDifficultyType>.GetHashCode(DungeonDifficultyType obj)
	{
		return (int)obj;
	}
}
