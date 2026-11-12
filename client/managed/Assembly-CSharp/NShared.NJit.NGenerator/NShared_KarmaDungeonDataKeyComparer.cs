using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_KarmaDungeonDataKeyComparer : IKeyComparerT<KarmaDungeonDataKey>, IKeyComparer, IEqualityComparer<KarmaDungeonDataKey>
{
	private Type HandleType => typeof(KarmaDungeonDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<KarmaDungeonDataKey>.Equals(KarmaDungeonDataKey x, KarmaDungeonDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<KarmaDungeonDataKey>.GetHashCode(KarmaDungeonDataKey obj)
	{
		return obj.GetHashCode();
	}
}
