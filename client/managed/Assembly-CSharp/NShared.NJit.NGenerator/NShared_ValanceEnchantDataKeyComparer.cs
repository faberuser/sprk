using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_ValanceEnchantDataKeyComparer : IKeyComparerT<ValanceEnchantDataKey>, IKeyComparer, IEqualityComparer<ValanceEnchantDataKey>
{
	private Type HandleType => typeof(ValanceEnchantDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<ValanceEnchantDataKey>.Equals(ValanceEnchantDataKey x, ValanceEnchantDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<ValanceEnchantDataKey>.GetHashCode(ValanceEnchantDataKey obj)
	{
		return obj.GetHashCode();
	}
}
