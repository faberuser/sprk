using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_ValanceAwakenDataKeyComparer : IKeyComparerT<ValanceAwakenDataKey>, IKeyComparer, IEqualityComparer<ValanceAwakenDataKey>
{
	private Type HandleType => typeof(ValanceAwakenDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<ValanceAwakenDataKey>.Equals(ValanceAwakenDataKey x, ValanceAwakenDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<ValanceAwakenDataKey>.GetHashCode(ValanceAwakenDataKey obj)
	{
		return obj.GetHashCode();
	}
}
