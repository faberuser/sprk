using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_MainStoryKeyComparer : IKeyComparerT<MainStoryKey>, IKeyComparer, IEqualityComparer<MainStoryKey>
{
	private Type HandleType => typeof(MainStoryKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<MainStoryKey>.Equals(MainStoryKey x, MainStoryKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<MainStoryKey>.GetHashCode(MainStoryKey obj)
	{
		return obj.GetHashCode();
	}
}
