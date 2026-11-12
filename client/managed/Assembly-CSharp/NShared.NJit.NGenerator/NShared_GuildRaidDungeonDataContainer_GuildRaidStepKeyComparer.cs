using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_GuildRaidDungeonDataContainer_GuildRaidStepKeyComparer : IKeyComparerT<GuildRaidDungeonDataContainer.GuildRaidStepKey>, IKeyComparer, IEqualityComparer<GuildRaidDungeonDataContainer.GuildRaidStepKey>
{
	private Type HandleType => typeof(GuildRaidDungeonDataContainer.GuildRaidStepKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<GuildRaidDungeonDataContainer.GuildRaidStepKey>.Equals(GuildRaidDungeonDataContainer.GuildRaidStepKey x, GuildRaidDungeonDataContainer.GuildRaidStepKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<GuildRaidDungeonDataContainer.GuildRaidStepKey>.GetHashCode(GuildRaidDungeonDataContainer.GuildRaidStepKey obj)
	{
		return obj.GetHashCode();
	}
}
