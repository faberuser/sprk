using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_InspectUserKeyComparer : IKeyComparerT<InspectUserKey>, IKeyComparer, IEqualityComparer<InspectUserKey>
{
	private Type HandleType => typeof(InspectUserKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<InspectUserKey>.Equals(InspectUserKey x, InspectUserKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<InspectUserKey>.GetHashCode(InspectUserKey obj)
	{
		return obj.GetHashCode();
	}
}
