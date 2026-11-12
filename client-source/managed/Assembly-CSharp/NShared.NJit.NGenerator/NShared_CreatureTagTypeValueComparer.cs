using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_CreatureTagTypeValueComparer : IKeyComparerT<CreatureTagType>, IKeyComparer, IEqualityComparer<CreatureTagType>
{
	private Type HandleType => typeof(CreatureTagType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<CreatureTagType>.Equals(CreatureTagType x, CreatureTagType y)
	{
		return x == y;
	}

	int IEqualityComparer<CreatureTagType>.GetHashCode(CreatureTagType obj)
	{
		return (int)obj;
	}
}
