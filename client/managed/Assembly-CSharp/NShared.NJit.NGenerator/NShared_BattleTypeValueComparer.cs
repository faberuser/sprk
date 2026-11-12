using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_BattleTypeValueComparer : IKeyComparerT<BattleType>, IKeyComparer, IEqualityComparer<BattleType>
{
	private Type HandleType => typeof(BattleType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<BattleType>.Equals(BattleType x, BattleType y)
	{
		return x == y;
	}

	int IEqualityComparer<BattleType>.GetHashCode(BattleType obj)
	{
		return (int)obj;
	}
}
