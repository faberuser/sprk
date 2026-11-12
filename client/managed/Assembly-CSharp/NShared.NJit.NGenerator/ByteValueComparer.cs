using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class ByteValueComparer : IKeyComparerT<byte>, IKeyComparer, IEqualityComparer<byte>
{
	private Type HandleType => typeof(byte);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<byte>.Equals(byte x, byte y)
	{
		return x == y;
	}

	int IEqualityComparer<byte>.GetHashCode(byte obj)
	{
		return obj;
	}
}
