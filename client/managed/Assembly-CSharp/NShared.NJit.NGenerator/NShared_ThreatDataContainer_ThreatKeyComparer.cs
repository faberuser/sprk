using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_ThreatDataContainer_ThreatKeyComparer : IKeyComparerT<ThreatDataContainer.ThreatKey>, IKeyComparer, IEqualityComparer<ThreatDataContainer.ThreatKey>
{
	private Type HandleType => typeof(ThreatDataContainer.ThreatKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<ThreatDataContainer.ThreatKey>.Equals(ThreatDataContainer.ThreatKey x, ThreatDataContainer.ThreatKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<ThreatDataContainer.ThreatKey>.GetHashCode(ThreatDataContainer.ThreatKey obj)
	{
		return obj.GetHashCode();
	}
}
