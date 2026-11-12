using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_StepUpGachaRewardDataContainer_GachaRoundKeyComparer : IKeyComparerT<StepUpGachaRewardDataContainer.GachaRoundKey>, IKeyComparer, IEqualityComparer<StepUpGachaRewardDataContainer.GachaRoundKey>
{
	private Type HandleType => typeof(StepUpGachaRewardDataContainer.GachaRoundKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<StepUpGachaRewardDataContainer.GachaRoundKey>.Equals(StepUpGachaRewardDataContainer.GachaRoundKey x, StepUpGachaRewardDataContainer.GachaRoundKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<StepUpGachaRewardDataContainer.GachaRoundKey>.GetHashCode(StepUpGachaRewardDataContainer.GachaRoundKey obj)
	{
		return obj.GetHashCode();
	}
}
