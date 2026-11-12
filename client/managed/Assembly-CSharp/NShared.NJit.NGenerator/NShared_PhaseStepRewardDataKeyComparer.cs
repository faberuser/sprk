using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_PhaseStepRewardDataKeyComparer : IKeyComparerT<PhaseStepRewardDataKey>, IKeyComparer, IEqualityComparer<PhaseStepRewardDataKey>
{
	private Type HandleType => typeof(PhaseStepRewardDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<PhaseStepRewardDataKey>.Equals(PhaseStepRewardDataKey x, PhaseStepRewardDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<PhaseStepRewardDataKey>.GetHashCode(PhaseStepRewardDataKey obj)
	{
		return obj.GetHashCode();
	}
}
