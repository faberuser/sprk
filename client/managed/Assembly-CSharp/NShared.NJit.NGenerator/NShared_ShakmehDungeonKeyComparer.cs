using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_ShakmehDungeonKeyComparer : IKeyComparerT<ShakmehDungeonKey>, IKeyComparer, IEqualityComparer<ShakmehDungeonKey>
{
	private Type HandleType => typeof(ShakmehDungeonKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<ShakmehDungeonKey>.Equals(ShakmehDungeonKey x, ShakmehDungeonKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<ShakmehDungeonKey>.GetHashCode(ShakmehDungeonKey obj)
	{
		return obj.GetHashCode();
	}
}
