using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class UInt32ValueComparer : IKeyComparerT<uint>, IKeyComparer, IEqualityComparer<uint>
{
	private Type HandleType => typeof(uint);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<uint>.Equals(uint x, uint y)
	{
		return x == y;
	}

	int IEqualityComparer<uint>.GetHashCode(uint obj)
	{
		return (int)obj;
	}
}
