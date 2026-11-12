using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_AccessoryPartPivotTypeValueComparer : IKeyComparerT<AccessoryPartPivotType>, IKeyComparer, IEqualityComparer<AccessoryPartPivotType>
{
	private Type HandleType => typeof(AccessoryPartPivotType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<AccessoryPartPivotType>.Equals(AccessoryPartPivotType x, AccessoryPartPivotType y)
	{
		return x == y;
	}

	int IEqualityComparer<AccessoryPartPivotType>.GetHashCode(AccessoryPartPivotType obj)
	{
		return (int)obj;
	}
}
