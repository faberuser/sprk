using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_KarmaRewardExponentialKeyComparer : IKeyComparerT<KarmaRewardExponentialKey>, IKeyComparer, IEqualityComparer<KarmaRewardExponentialKey>
{
	private Type HandleType => typeof(KarmaRewardExponentialKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<KarmaRewardExponentialKey>.Equals(KarmaRewardExponentialKey x, KarmaRewardExponentialKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<KarmaRewardExponentialKey>.GetHashCode(KarmaRewardExponentialKey obj)
	{
		return obj.GetHashCode();
	}
}
