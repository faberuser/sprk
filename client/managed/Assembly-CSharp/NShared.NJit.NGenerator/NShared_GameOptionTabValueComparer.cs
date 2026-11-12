using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_GameOptionTabValueComparer : IKeyComparerT<GameOptionTab>, IKeyComparer, IEqualityComparer<GameOptionTab>
{
	private Type HandleType => typeof(GameOptionTab);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<GameOptionTab>.Equals(GameOptionTab x, GameOptionTab y)
	{
		return x == y;
	}

	int IEqualityComparer<GameOptionTab>.GetHashCode(GameOptionTab obj)
	{
		return (int)obj;
	}
}
