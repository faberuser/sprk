using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_EquipGachaClientDataKeyComparer : IKeyComparerT<EquipGachaClientDataKey>, IKeyComparer, IEqualityComparer<EquipGachaClientDataKey>
{
	private Type HandleType => typeof(EquipGachaClientDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<EquipGachaClientDataKey>.Equals(EquipGachaClientDataKey x, EquipGachaClientDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<EquipGachaClientDataKey>.GetHashCode(EquipGachaClientDataKey obj)
	{
		return obj.GetHashCode();
	}
}
