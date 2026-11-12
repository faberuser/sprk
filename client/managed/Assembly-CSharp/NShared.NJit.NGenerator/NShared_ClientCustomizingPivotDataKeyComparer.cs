using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_ClientCustomizingPivotDataKeyComparer : IKeyComparerT<ClientCustomizingPivotDataKey>, IKeyComparer, IEqualityComparer<ClientCustomizingPivotDataKey>
{
	private Type HandleType => typeof(ClientCustomizingPivotDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<ClientCustomizingPivotDataKey>.Equals(ClientCustomizingPivotDataKey x, ClientCustomizingPivotDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<ClientCustomizingPivotDataKey>.GetHashCode(ClientCustomizingPivotDataKey obj)
	{
		return obj.GetHashCode();
	}
}
