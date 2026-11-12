using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_KarmaGaugeExponentialKeyComparer : IKeyComparerT<KarmaGaugeExponentialKey>, IKeyComparer, IEqualityComparer<KarmaGaugeExponentialKey>
{
	private Type HandleType => typeof(KarmaGaugeExponentialKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<KarmaGaugeExponentialKey>.Equals(KarmaGaugeExponentialKey x, KarmaGaugeExponentialKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<KarmaGaugeExponentialKey>.GetHashCode(KarmaGaugeExponentialKey obj)
	{
		return obj.GetHashCode();
	}
}
