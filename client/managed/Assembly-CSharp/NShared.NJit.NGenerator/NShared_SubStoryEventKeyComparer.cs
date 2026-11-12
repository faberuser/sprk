using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_SubStoryEventKeyComparer : IKeyComparerT<SubStoryEventKey>, IKeyComparer, IEqualityComparer<SubStoryEventKey>
{
	private Type HandleType => typeof(SubStoryEventKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<SubStoryEventKey>.Equals(SubStoryEventKey x, SubStoryEventKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<SubStoryEventKey>.GetHashCode(SubStoryEventKey obj)
	{
		return obj.GetHashCode();
	}
}
