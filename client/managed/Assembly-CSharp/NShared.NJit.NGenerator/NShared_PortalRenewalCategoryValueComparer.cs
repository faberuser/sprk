using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_PortalRenewalCategoryValueComparer : IKeyComparerT<PortalRenewalCategory>, IKeyComparer, IEqualityComparer<PortalRenewalCategory>
{
	private Type HandleType => typeof(PortalRenewalCategory);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<PortalRenewalCategory>.Equals(PortalRenewalCategory x, PortalRenewalCategory y)
	{
		return x == y;
	}

	int IEqualityComparer<PortalRenewalCategory>.GetHashCode(PortalRenewalCategory obj)
	{
		return (int)obj;
	}
}
