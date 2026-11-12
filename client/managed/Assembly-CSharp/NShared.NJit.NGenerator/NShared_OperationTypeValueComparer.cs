using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_OperationTypeValueComparer : IKeyComparerT<OperationType>, IKeyComparer, IEqualityComparer<OperationType>
{
	private Type HandleType => typeof(OperationType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<OperationType>.Equals(OperationType x, OperationType y)
	{
		return x == y;
	}

	int IEqualityComparer<OperationType>.GetHashCode(OperationType obj)
	{
		return (int)obj;
	}
}
