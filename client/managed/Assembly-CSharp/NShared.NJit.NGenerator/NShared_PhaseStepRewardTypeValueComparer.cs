using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_PhaseStepRewardTypeValueComparer : IKeyComparerT<PhaseStepRewardType>, IKeyComparer, IEqualityComparer<PhaseStepRewardType>
{
	private Type HandleType => typeof(PhaseStepRewardType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<PhaseStepRewardType>.Equals(PhaseStepRewardType x, PhaseStepRewardType y)
	{
		return x == y;
	}

	int IEqualityComparer<PhaseStepRewardType>.GetHashCode(PhaseStepRewardType obj)
	{
		return (int)obj;
	}
}
