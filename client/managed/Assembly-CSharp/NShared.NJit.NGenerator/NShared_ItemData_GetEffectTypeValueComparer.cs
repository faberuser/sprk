using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_ItemData_GetEffectTypeValueComparer : IKeyComparerT<ItemData.GetEffectType>, IKeyComparer, IEqualityComparer<ItemData.GetEffectType>
{
	private Type HandleType => typeof(ItemData.GetEffectType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<ItemData.GetEffectType>.Equals(ItemData.GetEffectType x, ItemData.GetEffectType y)
	{
		return x == y;
	}

	int IEqualityComparer<ItemData.GetEffectType>.GetHashCode(ItemData.GetEffectType obj)
	{
		return (int)obj;
	}
}
