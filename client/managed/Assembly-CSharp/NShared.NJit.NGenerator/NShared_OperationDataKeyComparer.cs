using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_OperationDataKeyComparer : IKeyComparerT<OperationDataKey>, IKeyComparer, IEqualityComparer<OperationDataKey>
{
	private Type HandleType => typeof(OperationDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<OperationDataKey>.Equals(OperationDataKey x, OperationDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<OperationDataKey>.GetHashCode(OperationDataKey obj)
	{
		return obj.GetHashCode();
	}
}
