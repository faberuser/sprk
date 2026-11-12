using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_GuildBuildingDataKeyComparer : IKeyComparerT<GuildBuildingDataKey>, IKeyComparer, IEqualityComparer<GuildBuildingDataKey>
{
	private Type HandleType => typeof(GuildBuildingDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<GuildBuildingDataKey>.Equals(GuildBuildingDataKey x, GuildBuildingDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<GuildBuildingDataKey>.GetHashCode(GuildBuildingDataKey obj)
	{
		return obj.GetHashCode();
	}
}
