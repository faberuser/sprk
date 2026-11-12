using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_HeroBonusDataKeyComparer : IKeyComparerT<HeroBonusDataKey>, IKeyComparer, IEqualityComparer<HeroBonusDataKey>
{
	private Type HandleType => typeof(HeroBonusDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<HeroBonusDataKey>.Equals(HeroBonusDataKey x, HeroBonusDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<HeroBonusDataKey>.GetHashCode(HeroBonusDataKey obj)
	{
		return obj.GetHashCode();
	}
}
