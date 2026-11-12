using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_PayShopDiscountGroupKeyComparer : IKeyComparerT<PayShopDiscountGroupKey>, IKeyComparer, IEqualityComparer<PayShopDiscountGroupKey>
{
	private Type HandleType => typeof(PayShopDiscountGroupKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<PayShopDiscountGroupKey>.Equals(PayShopDiscountGroupKey x, PayShopDiscountGroupKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<PayShopDiscountGroupKey>.GetHashCode(PayShopDiscountGroupKey obj)
	{
		return obj.GetHashCode();
	}
}
