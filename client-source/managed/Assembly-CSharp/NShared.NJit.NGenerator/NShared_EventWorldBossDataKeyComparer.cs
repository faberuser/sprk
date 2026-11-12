using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_EventWorldBossDataKeyComparer : IKeyComparerT<EventWorldBossDataKey>, IKeyComparer, IEqualityComparer<EventWorldBossDataKey>
{
	private Type HandleType => typeof(EventWorldBossDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<EventWorldBossDataKey>.Equals(EventWorldBossDataKey x, EventWorldBossDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<EventWorldBossDataKey>.GetHashCode(EventWorldBossDataKey obj)
	{
		return obj.GetHashCode();
	}
}
