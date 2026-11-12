using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_OptionSellKeyComparer : IKeyComparerT<OptionSellKey>, IKeyComparer, IEqualityComparer<OptionSellKey>
{
	private Type HandleType => typeof(OptionSellKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<OptionSellKey>.Equals(OptionSellKey x, OptionSellKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<OptionSellKey>.GetHashCode(OptionSellKey obj)
	{
		return obj.GetHashCode();
	}
}
