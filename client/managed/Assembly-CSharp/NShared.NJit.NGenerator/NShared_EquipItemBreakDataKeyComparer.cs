using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_EquipItemBreakDataKeyComparer : IKeyComparerT<EquipItemBreakDataKey>, IKeyComparer, IEqualityComparer<EquipItemBreakDataKey>
{
	private Type HandleType => typeof(EquipItemBreakDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<EquipItemBreakDataKey>.Equals(EquipItemBreakDataKey x, EquipItemBreakDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<EquipItemBreakDataKey>.GetHashCode(EquipItemBreakDataKey obj)
	{
		return obj.GetHashCode();
	}
}
