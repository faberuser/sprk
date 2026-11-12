using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_KingsPassSeasonMissionDataContainer_RewardKey2Comparer : IKeyComparerT<KingsPassSeasonMissionDataContainer.RewardKey2>, IKeyComparer, IEqualityComparer<KingsPassSeasonMissionDataContainer.RewardKey2>
{
	private Type HandleType => typeof(KingsPassSeasonMissionDataContainer.RewardKey2);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<KingsPassSeasonMissionDataContainer.RewardKey2>.Equals(KingsPassSeasonMissionDataContainer.RewardKey2 x, KingsPassSeasonMissionDataContainer.RewardKey2 y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<KingsPassSeasonMissionDataContainer.RewardKey2>.GetHashCode(KingsPassSeasonMissionDataContainer.RewardKey2 obj)
	{
		return obj.GetHashCode();
	}
}
