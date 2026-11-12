using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_PunishmentRaidRewardTypeValueComparer : IKeyComparerT<PunishmentRaidRewardType>, IKeyComparer, IEqualityComparer<PunishmentRaidRewardType>
{
	private Type HandleType => typeof(PunishmentRaidRewardType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<PunishmentRaidRewardType>.Equals(PunishmentRaidRewardType x, PunishmentRaidRewardType y)
	{
		return x == y;
	}

	int IEqualityComparer<PunishmentRaidRewardType>.GetHashCode(PunishmentRaidRewardType obj)
	{
		return (int)obj;
	}
}
