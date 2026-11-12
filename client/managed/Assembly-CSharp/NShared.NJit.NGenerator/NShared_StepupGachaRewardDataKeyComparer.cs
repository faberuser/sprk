using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_StepupGachaRewardDataKeyComparer : IKeyComparerT<StepupGachaRewardDataKey>, IKeyComparer, IEqualityComparer<StepupGachaRewardDataKey>
{
	private Type HandleType => typeof(StepupGachaRewardDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<StepupGachaRewardDataKey>.Equals(StepupGachaRewardDataKey x, StepupGachaRewardDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<StepupGachaRewardDataKey>.GetHashCode(StepupGachaRewardDataKey obj)
	{
		return obj.GetHashCode();
	}
}
