using System;
using System.Collections.Generic;

namespace NShared.NJit.NResolver;

public class StringComparer : IKeyComparerT<string>, IKeyComparer, IEqualityComparer<string>
{
	Type IKeyComparer.GetCompareType()
	{
		return typeof(string);
	}

	bool IEqualityComparer<string>.Equals(string x, string y)
	{
		return x == y;
	}

	int IEqualityComparer<string>.GetHashCode(string obj)
	{
		return JitOption.StringToHashCode(obj);
	}
}
