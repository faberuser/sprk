using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_PunishmentRaidRewardDataContainer_RewardAndIndexKeyComparer : IKeyComparerT<PunishmentRaidRewardDataContainer.RewardAndIndexKey>, IKeyComparer, IEqualityComparer<PunishmentRaidRewardDataContainer.RewardAndIndexKey>
{
	private Type HandleType => typeof(PunishmentRaidRewardDataContainer.RewardAndIndexKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<PunishmentRaidRewardDataContainer.RewardAndIndexKey>.Equals(PunishmentRaidRewardDataContainer.RewardAndIndexKey x, PunishmentRaidRewardDataContainer.RewardAndIndexKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<PunishmentRaidRewardDataContainer.RewardAndIndexKey>.GetHashCode(PunishmentRaidRewardDataContainer.RewardAndIndexKey obj)
	{
		return obj.GetHashCode();
	}
}
