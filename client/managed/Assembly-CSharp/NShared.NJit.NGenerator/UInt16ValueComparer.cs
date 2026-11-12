using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class UInt16ValueComparer : IKeyComparerT<ushort>, IKeyComparer, IEqualityComparer<ushort>
{
	private Type HandleType => typeof(ushort);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<ushort>.Equals(ushort x, ushort y)
	{
		return x == y;
	}

	int IEqualityComparer<ushort>.GetHashCode(ushort obj)
	{
		return obj;
	}
}
