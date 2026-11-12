using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_ValanceEnchantOptionCountDataKeyComparer : IKeyComparerT<ValanceEnchantOptionCountDataKey>, IKeyComparer, IEqualityComparer<ValanceEnchantOptionCountDataKey>
{
	private Type HandleType => typeof(ValanceEnchantOptionCountDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<ValanceEnchantOptionCountDataKey>.Equals(ValanceEnchantOptionCountDataKey x, ValanceEnchantOptionCountDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<ValanceEnchantOptionCountDataKey>.GetHashCode(ValanceEnchantOptionCountDataKey obj)
	{
		return obj.GetHashCode();
	}
}
