using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_KarmaRewardKeyComparer : IKeyComparerT<KarmaRewardKey>, IKeyComparer, IEqualityComparer<KarmaRewardKey>
{
	private Type HandleType => typeof(KarmaRewardKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<KarmaRewardKey>.Equals(KarmaRewardKey x, KarmaRewardKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<KarmaRewardKey>.GetHashCode(KarmaRewardKey obj)
	{
		return obj.GetHashCode();
	}
}
