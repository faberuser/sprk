using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_EquipPartTypeValueComparer : IKeyComparerT<EquipPartType>, IKeyComparer, IEqualityComparer<EquipPartType>
{
	private Type HandleType => typeof(EquipPartType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<EquipPartType>.Equals(EquipPartType x, EquipPartType y)
	{
		return x == y;
	}

	int IEqualityComparer<EquipPartType>.GetHashCode(EquipPartType obj)
	{
		return (int)obj;
	}
}
