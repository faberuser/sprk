using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_StaminaTypeValueComparer : IKeyComparerT<StaminaType>, IKeyComparer, IEqualityComparer<StaminaType>
{
	private Type HandleType => typeof(StaminaType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<StaminaType>.Equals(StaminaType x, StaminaType y)
	{
		return x == y;
	}

	int IEqualityComparer<StaminaType>.GetHashCode(StaminaType obj)
	{
		return (int)obj;
	}
}
