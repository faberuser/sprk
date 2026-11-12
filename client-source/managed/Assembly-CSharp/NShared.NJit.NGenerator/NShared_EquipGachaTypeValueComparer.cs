using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_EquipGachaTypeValueComparer : IKeyComparerT<EquipGachaType>, IKeyComparer, IEqualityComparer<EquipGachaType>
{
	private Type HandleType => typeof(EquipGachaType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<EquipGachaType>.Equals(EquipGachaType x, EquipGachaType y)
	{
		return x == y;
	}

	int IEqualityComparer<EquipGachaType>.GetHashCode(EquipGachaType obj)
	{
		return (int)obj;
	}
}
