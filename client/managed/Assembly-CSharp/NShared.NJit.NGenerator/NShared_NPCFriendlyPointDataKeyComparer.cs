using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_NPCFriendlyPointDataKeyComparer : IKeyComparerT<NPCFriendlyPointDataKey>, IKeyComparer, IEqualityComparer<NPCFriendlyPointDataKey>
{
	private Type HandleType => typeof(NPCFriendlyPointDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<NPCFriendlyPointDataKey>.Equals(NPCFriendlyPointDataKey x, NPCFriendlyPointDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<NPCFriendlyPointDataKey>.GetHashCode(NPCFriendlyPointDataKey obj)
	{
		return obj.GetHashCode();
	}
}
