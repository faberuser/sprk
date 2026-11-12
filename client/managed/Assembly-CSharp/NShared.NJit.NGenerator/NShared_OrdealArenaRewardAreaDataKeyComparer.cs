using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_OrdealArenaRewardAreaDataKeyComparer : IKeyComparerT<OrdealArenaRewardAreaDataKey>, IKeyComparer, IEqualityComparer<OrdealArenaRewardAreaDataKey>
{
	private Type HandleType => typeof(OrdealArenaRewardAreaDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<OrdealArenaRewardAreaDataKey>.Equals(OrdealArenaRewardAreaDataKey x, OrdealArenaRewardAreaDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<OrdealArenaRewardAreaDataKey>.GetHashCode(OrdealArenaRewardAreaDataKey obj)
	{
		return obj.GetHashCode();
	}
}
