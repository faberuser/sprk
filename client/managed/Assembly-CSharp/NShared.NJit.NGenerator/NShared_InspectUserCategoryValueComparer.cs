using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_InspectUserCategoryValueComparer : IKeyComparerT<InspectUserCategory>, IKeyComparer, IEqualityComparer<InspectUserCategory>
{
	private Type HandleType => typeof(InspectUserCategory);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<InspectUserCategory>.Equals(InspectUserCategory x, InspectUserCategory y)
	{
		return x == y;
	}

	int IEqualityComparer<InspectUserCategory>.GetHashCode(InspectUserCategory obj)
	{
		return (int)obj;
	}
}
