using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_InspectUserTypeValueComparer : IKeyComparerT<InspectUserType>, IKeyComparer, IEqualityComparer<InspectUserType>
{
	private Type HandleType => typeof(InspectUserType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<InspectUserType>.Equals(InspectUserType x, InspectUserType y)
	{
		return x == y;
	}

	int IEqualityComparer<InspectUserType>.GetHashCode(InspectUserType obj)
	{
		return (int)obj;
	}
}
