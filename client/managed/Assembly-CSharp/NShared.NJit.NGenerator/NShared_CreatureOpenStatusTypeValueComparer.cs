using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_CreatureOpenStatusTypeValueComparer : IKeyComparerT<CreatureOpenStatusType>, IKeyComparer, IEqualityComparer<CreatureOpenStatusType>
{
	private Type HandleType => typeof(CreatureOpenStatusType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<CreatureOpenStatusType>.Equals(CreatureOpenStatusType x, CreatureOpenStatusType y)
	{
		return x == y;
	}

	int IEqualityComparer<CreatureOpenStatusType>.GetHashCode(CreatureOpenStatusType obj)
	{
		return (int)obj;
	}
}
