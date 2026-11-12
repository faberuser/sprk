using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_StatTypeValueComparer : IKeyComparerT<StatType>, IKeyComparer, IEqualityComparer<StatType>
{
	private Type HandleType => typeof(StatType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<StatType>.Equals(StatType x, StatType y)
	{
		return x == y;
	}

	int IEqualityComparer<StatType>.GetHashCode(StatType obj)
	{
		return (int)obj;
	}
}
