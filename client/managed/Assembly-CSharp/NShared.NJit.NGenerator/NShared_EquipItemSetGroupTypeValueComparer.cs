using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_EquipItemSetGroupTypeValueComparer : IKeyComparerT<EquipItemSetGroupType>, IKeyComparer, IEqualityComparer<EquipItemSetGroupType>
{
	private Type HandleType => typeof(EquipItemSetGroupType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<EquipItemSetGroupType>.Equals(EquipItemSetGroupType x, EquipItemSetGroupType y)
	{
		return x == y;
	}

	int IEqualityComparer<EquipItemSetGroupType>.GetHashCode(EquipItemSetGroupType obj)
	{
		return (int)obj;
	}
}
