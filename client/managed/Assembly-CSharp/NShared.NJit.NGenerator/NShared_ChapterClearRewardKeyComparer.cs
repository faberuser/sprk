using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_ChapterClearRewardKeyComparer : IKeyComparerT<ChapterClearRewardKey>, IKeyComparer, IEqualityComparer<ChapterClearRewardKey>
{
	private Type HandleType => typeof(ChapterClearRewardKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<ChapterClearRewardKey>.Equals(ChapterClearRewardKey x, ChapterClearRewardKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<ChapterClearRewardKey>.GetHashCode(ChapterClearRewardKey obj)
	{
		return obj.GetHashCode();
	}
}
