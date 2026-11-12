using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_PayShopUserGroupValueComparer : IKeyComparerT<PayShopUserGroup>, IKeyComparer, IEqualityComparer<PayShopUserGroup>
{
	private Type HandleType => typeof(PayShopUserGroup);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<PayShopUserGroup>.Equals(PayShopUserGroup x, PayShopUserGroup y)
	{
		return x == y;
	}

	int IEqualityComparer<PayShopUserGroup>.GetHashCode(PayShopUserGroup obj)
	{
		return (int)obj;
	}
}
