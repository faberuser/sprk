using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_ProbabilityDataKey2Comparer : IKeyComparerT<ProbabilityDataKey2>, IKeyComparer, IEqualityComparer<ProbabilityDataKey2>
{
	private Type HandleType => typeof(ProbabilityDataKey2);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<ProbabilityDataKey2>.Equals(ProbabilityDataKey2 x, ProbabilityDataKey2 y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<ProbabilityDataKey2>.GetHashCode(ProbabilityDataKey2 obj)
	{
		return obj.GetHashCode();
	}
}
