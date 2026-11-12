using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_EquipItemSubTypeValueComparer : IKeyComparerT<EquipItemSubType>, IKeyComparer, IEqualityComparer<EquipItemSubType>
{
	private Type HandleType => typeof(EquipItemSubType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<EquipItemSubType>.Equals(EquipItemSubType x, EquipItemSubType y)
	{
		return x == y;
	}

	int IEqualityComparer<EquipItemSubType>.GetHashCode(EquipItemSubType obj)
	{
		return (int)obj;
	}
}
