using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_GuildRaidBonusDungeonDataKeyComparer : IKeyComparerT<GuildRaidBonusDungeonDataKey>, IKeyComparer, IEqualityComparer<GuildRaidBonusDungeonDataKey>
{
	private Type HandleType => typeof(GuildRaidBonusDungeonDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<GuildRaidBonusDungeonDataKey>.Equals(GuildRaidBonusDungeonDataKey x, GuildRaidBonusDungeonDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<GuildRaidBonusDungeonDataKey>.GetHashCode(GuildRaidBonusDungeonDataKey obj)
	{
		return obj.GetHashCode();
	}
}
