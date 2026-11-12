using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_CreatureLevelFactorDataKeyComparer : IKeyComparerT<CreatureLevelFactorDataKey>, IKeyComparer, IEqualityComparer<CreatureLevelFactorDataKey>
{
	private Type HandleType => typeof(CreatureLevelFactorDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<CreatureLevelFactorDataKey>.Equals(CreatureLevelFactorDataKey x, CreatureLevelFactorDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<CreatureLevelFactorDataKey>.GetHashCode(CreatureLevelFactorDataKey obj)
	{
		return obj.GetHashCode();
	}
}
