using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_UnderPrisonDungeonKeyComparer : IKeyComparerT<UnderPrisonDungeonKey>, IKeyComparer, IEqualityComparer<UnderPrisonDungeonKey>
{
	private Type HandleType => typeof(UnderPrisonDungeonKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<UnderPrisonDungeonKey>.Equals(UnderPrisonDungeonKey x, UnderPrisonDungeonKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<UnderPrisonDungeonKey>.GetHashCode(UnderPrisonDungeonKey obj)
	{
		return obj.GetHashCode();
	}
}
