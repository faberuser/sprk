using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_BottomButtonKeyComparer : IKeyComparerT<BottomButtonKey>, IKeyComparer, IEqualityComparer<BottomButtonKey>
{
	private Type HandleType => typeof(BottomButtonKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<BottomButtonKey>.Equals(BottomButtonKey x, BottomButtonKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<BottomButtonKey>.GetHashCode(BottomButtonKey obj)
	{
		return obj.GetHashCode();
	}
}
