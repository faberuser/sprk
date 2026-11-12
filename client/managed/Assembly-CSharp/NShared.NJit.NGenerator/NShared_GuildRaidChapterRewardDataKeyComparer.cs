using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_GuildRaidChapterRewardDataKeyComparer : IKeyComparerT<GuildRaidChapterRewardDataKey>, IKeyComparer, IEqualityComparer<GuildRaidChapterRewardDataKey>
{
	private Type HandleType => typeof(GuildRaidChapterRewardDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<GuildRaidChapterRewardDataKey>.Equals(GuildRaidChapterRewardDataKey x, GuildRaidChapterRewardDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<GuildRaidChapterRewardDataKey>.GetHashCode(GuildRaidChapterRewardDataKey obj)
	{
		return obj.GetHashCode();
	}
}
