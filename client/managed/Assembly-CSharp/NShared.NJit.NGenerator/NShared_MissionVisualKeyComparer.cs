using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_MissionVisualKeyComparer : IKeyComparerT<MissionVisualKey>, IKeyComparer, IEqualityComparer<MissionVisualKey>
{
	private Type HandleType => typeof(MissionVisualKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<MissionVisualKey>.Equals(MissionVisualKey x, MissionVisualKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<MissionVisualKey>.GetHashCode(MissionVisualKey obj)
	{
		return obj.GetHashCode();
	}
}
