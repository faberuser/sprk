using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_MatchBanPickStateValueComparer : IKeyComparerT<MatchBanPickState>, IKeyComparer, IEqualityComparer<MatchBanPickState>
{
	private Type HandleType => typeof(MatchBanPickState);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<MatchBanPickState>.Equals(MatchBanPickState x, MatchBanPickState y)
	{
		return x == y;
	}

	int IEqualityComparer<MatchBanPickState>.GetHashCode(MatchBanPickState obj)
	{
		return (int)obj;
	}
}
