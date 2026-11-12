using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_GuildAttendanceRewardDataKeyComparer : IKeyComparerT<GuildAttendanceRewardDataKey>, IKeyComparer, IEqualityComparer<GuildAttendanceRewardDataKey>
{
	private Type HandleType => typeof(GuildAttendanceRewardDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<GuildAttendanceRewardDataKey>.Equals(GuildAttendanceRewardDataKey x, GuildAttendanceRewardDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<GuildAttendanceRewardDataKey>.GetHashCode(GuildAttendanceRewardDataKey obj)
	{
		return obj.GetHashCode();
	}
}
