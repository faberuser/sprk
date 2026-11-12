using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_HeroLimitBreakDataKeyComparer : IKeyComparerT<HeroLimitBreakDataKey>, IKeyComparer, IEqualityComparer<HeroLimitBreakDataKey>
{
	private Type HandleType => typeof(HeroLimitBreakDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<HeroLimitBreakDataKey>.Equals(HeroLimitBreakDataKey x, HeroLimitBreakDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<HeroLimitBreakDataKey>.GetHashCode(HeroLimitBreakDataKey obj)
	{
		return obj.GetHashCode();
	}
}
