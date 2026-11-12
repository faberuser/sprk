using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_SubQuestDataKeyComparer : IKeyComparerT<SubQuestDataKey>, IKeyComparer, IEqualityComparer<SubQuestDataKey>
{
	private Type HandleType => typeof(SubQuestDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<SubQuestDataKey>.Equals(SubQuestDataKey x, SubQuestDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<SubQuestDataKey>.GetHashCode(SubQuestDataKey obj)
	{
		return obj.GetHashCode();
	}
}
