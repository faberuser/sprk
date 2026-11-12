using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_PetAwakenDataKeyComparer : IKeyComparerT<PetAwakenDataKey>, IKeyComparer, IEqualityComparer<PetAwakenDataKey>
{
	private Type HandleType => typeof(PetAwakenDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<PetAwakenDataKey>.Equals(PetAwakenDataKey x, PetAwakenDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<PetAwakenDataKey>.GetHashCode(PetAwakenDataKey obj)
	{
		return obj.GetHashCode();
	}
}
