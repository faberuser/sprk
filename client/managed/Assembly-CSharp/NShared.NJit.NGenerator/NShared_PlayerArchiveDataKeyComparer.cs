using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_PlayerArchiveDataKeyComparer : IKeyComparerT<PlayerArchiveDataKey>, IKeyComparer, IEqualityComparer<PlayerArchiveDataKey>
{
	private Type HandleType => typeof(PlayerArchiveDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<PlayerArchiveDataKey>.Equals(PlayerArchiveDataKey x, PlayerArchiveDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<PlayerArchiveDataKey>.GetHashCode(PlayerArchiveDataKey obj)
	{
		return obj.GetHashCode();
	}
}
