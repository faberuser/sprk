using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_StateConditionTypeValueComparer : IKeyComparerT<StateConditionType>, IKeyComparer, IEqualityComparer<StateConditionType>
{
	private Type HandleType => typeof(StateConditionType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<StateConditionType>.Equals(StateConditionType x, StateConditionType y)
	{
		return x == y;
	}

	int IEqualityComparer<StateConditionType>.GetHashCode(StateConditionType obj)
	{
		return (int)obj;
	}
}
