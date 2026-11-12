using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_CreatureStarDataKeyComparer : IKeyComparerT<CreatureStarDataKey>, IKeyComparer, IEqualityComparer<CreatureStarDataKey>
{
	private Type HandleType => typeof(CreatureStarDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<CreatureStarDataKey>.Equals(CreatureStarDataKey x, CreatureStarDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<CreatureStarDataKey>.GetHashCode(CreatureStarDataKey obj)
	{
		return obj.GetHashCode();
	}
}
