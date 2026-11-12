using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_DispatchStateValueComparer : IKeyComparerT<DispatchState>, IKeyComparer, IEqualityComparer<DispatchState>
{
	private Type HandleType => typeof(DispatchState);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<DispatchState>.Equals(DispatchState x, DispatchState y)
	{
		return x == y;
	}

	int IEqualityComparer<DispatchState>.GetHashCode(DispatchState obj)
	{
		return (int)obj;
	}
}
