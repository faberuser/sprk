using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class CharValueComparer : IKeyComparerT<char>, IKeyComparer, IEqualityComparer<char>
{
	private Type HandleType => typeof(char);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<char>.Equals(char x, char y)
	{
		return x == y;
	}

	int IEqualityComparer<char>.GetHashCode(char obj)
	{
		return obj;
	}
}
