using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_TutorialKeyComparer : IKeyComparerT<TutorialKey>, IKeyComparer, IEqualityComparer<TutorialKey>
{
	private Type HandleType => typeof(TutorialKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<TutorialKey>.Equals(TutorialKey x, TutorialKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<TutorialKey>.GetHashCode(TutorialKey obj)
	{
		return obj.GetHashCode();
	}
}
