using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_AccessoryPartTypeValueComparer : IKeyComparerT<AccessoryPartType>, IKeyComparer, IEqualityComparer<AccessoryPartType>
{
	private Type HandleType => typeof(AccessoryPartType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<AccessoryPartType>.Equals(AccessoryPartType x, AccessoryPartType y)
	{
		return x == y;
	}

	int IEqualityComparer<AccessoryPartType>.GetHashCode(AccessoryPartType obj)
	{
		return (int)obj;
	}
}
