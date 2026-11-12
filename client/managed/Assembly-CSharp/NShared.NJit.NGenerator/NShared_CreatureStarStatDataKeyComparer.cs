using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_CreatureStarStatDataKeyComparer : IKeyComparerT<CreatureStarStatDataKey>, IKeyComparer, IEqualityComparer<CreatureStarStatDataKey>
{
	private Type HandleType => typeof(CreatureStarStatDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<CreatureStarStatDataKey>.Equals(CreatureStarStatDataKey x, CreatureStarStatDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<CreatureStarStatDataKey>.GetHashCode(CreatureStarStatDataKey obj)
	{
		return obj.GetHashCode();
	}
}
