using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_CreatureStarPriceKeyComparer : IKeyComparerT<CreatureStarPriceKey>, IKeyComparer, IEqualityComparer<CreatureStarPriceKey>
{
	private Type HandleType => typeof(CreatureStarPriceKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<CreatureStarPriceKey>.Equals(CreatureStarPriceKey x, CreatureStarPriceKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<CreatureStarPriceKey>.GetHashCode(CreatureStarPriceKey obj)
	{
		return obj.GetHashCode();
	}
}
