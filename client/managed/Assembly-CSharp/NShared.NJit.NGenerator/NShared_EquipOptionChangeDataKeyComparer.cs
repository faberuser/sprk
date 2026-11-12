using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_EquipOptionChangeDataKeyComparer : IKeyComparerT<EquipOptionChangeDataKey>, IKeyComparer, IEqualityComparer<EquipOptionChangeDataKey>
{
	private Type HandleType => typeof(EquipOptionChangeDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<EquipOptionChangeDataKey>.Equals(EquipOptionChangeDataKey x, EquipOptionChangeDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<EquipOptionChangeDataKey>.GetHashCode(EquipOptionChangeDataKey obj)
	{
		return obj.GetHashCode();
	}
}
