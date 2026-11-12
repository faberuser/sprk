using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_SoulBreakKeyComparer : IKeyComparerT<SoulBreakKey>, IKeyComparer, IEqualityComparer<SoulBreakKey>
{
	private Type HandleType => typeof(SoulBreakKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<SoulBreakKey>.Equals(SoulBreakKey x, SoulBreakKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<SoulBreakKey>.GetHashCode(SoulBreakKey obj)
	{
		return obj.GetHashCode();
	}
}
