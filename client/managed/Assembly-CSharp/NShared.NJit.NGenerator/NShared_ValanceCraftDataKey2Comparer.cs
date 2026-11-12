using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_ValanceCraftDataKey2Comparer : IKeyComparerT<ValanceCraftDataKey2>, IKeyComparer, IEqualityComparer<ValanceCraftDataKey2>
{
	private Type HandleType => typeof(ValanceCraftDataKey2);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<ValanceCraftDataKey2>.Equals(ValanceCraftDataKey2 x, ValanceCraftDataKey2 y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<ValanceCraftDataKey2>.GetHashCode(ValanceCraftDataKey2 obj)
	{
		return obj.GetHashCode();
	}
}
