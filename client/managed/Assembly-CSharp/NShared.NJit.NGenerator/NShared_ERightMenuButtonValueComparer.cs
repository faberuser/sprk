using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_ERightMenuButtonValueComparer : IKeyComparerT<ERightMenuButton>, IKeyComparer, IEqualityComparer<ERightMenuButton>
{
	private Type HandleType => typeof(ERightMenuButton);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<ERightMenuButton>.Equals(ERightMenuButton x, ERightMenuButton y)
	{
		return x == y;
	}

	int IEqualityComparer<ERightMenuButton>.GetHashCode(ERightMenuButton obj)
	{
		return (int)obj;
	}
}
