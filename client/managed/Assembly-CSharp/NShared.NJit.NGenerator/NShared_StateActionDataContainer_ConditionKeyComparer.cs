using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_StateActionDataContainer_ConditionKeyComparer : IKeyComparerT<StateActionDataContainer.ConditionKey>, IKeyComparer, IEqualityComparer<StateActionDataContainer.ConditionKey>
{
	private Type HandleType => typeof(StateActionDataContainer.ConditionKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<StateActionDataContainer.ConditionKey>.Equals(StateActionDataContainer.ConditionKey x, StateActionDataContainer.ConditionKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<StateActionDataContainer.ConditionKey>.GetHashCode(StateActionDataContainer.ConditionKey obj)
	{
		return obj.GetHashCode();
	}
}
