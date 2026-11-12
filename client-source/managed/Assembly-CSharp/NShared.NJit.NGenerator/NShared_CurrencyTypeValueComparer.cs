using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_CurrencyTypeValueComparer : IKeyComparerT<CurrencyType>, IKeyComparer, IEqualityComparer<CurrencyType>
{
	private Type HandleType => typeof(CurrencyType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<CurrencyType>.Equals(CurrencyType x, CurrencyType y)
	{
		return x == y;
	}

	int IEqualityComparer<CurrencyType>.GetHashCode(CurrencyType obj)
	{
		return (int)obj;
	}
}
