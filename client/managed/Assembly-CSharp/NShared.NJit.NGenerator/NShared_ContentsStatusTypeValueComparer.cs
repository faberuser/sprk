using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_ContentsStatusTypeValueComparer : IKeyComparerT<ContentsStatusType>, IKeyComparer, IEqualityComparer<ContentsStatusType>
{
	private Type HandleType => typeof(ContentsStatusType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<ContentsStatusType>.Equals(ContentsStatusType x, ContentsStatusType y)
	{
		return x == y;
	}

	int IEqualityComparer<ContentsStatusType>.GetHashCode(ContentsStatusType obj)
	{
		return (int)obj;
	}
}
