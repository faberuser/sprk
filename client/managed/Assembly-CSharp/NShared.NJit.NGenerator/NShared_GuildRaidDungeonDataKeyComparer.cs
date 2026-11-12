using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_GuildRaidDungeonDataKeyComparer : IKeyComparerT<GuildRaidDungeonDataKey>, IKeyComparer, IEqualityComparer<GuildRaidDungeonDataKey>
{
	private Type HandleType => typeof(GuildRaidDungeonDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<GuildRaidDungeonDataKey>.Equals(GuildRaidDungeonDataKey x, GuildRaidDungeonDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<GuildRaidDungeonDataKey>.GetHashCode(GuildRaidDungeonDataKey obj)
	{
		return obj.GetHashCode();
	}
}
