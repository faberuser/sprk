using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_EquipAutoKeyComparer : IKeyComparerT<EquipAutoKey>, IKeyComparer, IEqualityComparer<EquipAutoKey>
{
	private Type HandleType => typeof(EquipAutoKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<EquipAutoKey>.Equals(EquipAutoKey x, EquipAutoKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<EquipAutoKey>.GetHashCode(EquipAutoKey obj)
	{
		return obj.GetHashCode();
	}
}
