using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_OptionSellCategoryValueComparer : IKeyComparerT<OptionSellCategory>, IKeyComparer, IEqualityComparer<OptionSellCategory>
{
	private Type HandleType => typeof(OptionSellCategory);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<OptionSellCategory>.Equals(OptionSellCategory x, OptionSellCategory y)
	{
		return x == y;
	}

	int IEqualityComparer<OptionSellCategory>.GetHashCode(OptionSellCategory obj)
	{
		return (int)obj;
	}
}
