using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_KingsPassMissionKey2Comparer : IKeyComparerT<KingsPassMissionKey2>, IKeyComparer, IEqualityComparer<KingsPassMissionKey2>
{
	private Type HandleType => typeof(KingsPassMissionKey2);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<KingsPassMissionKey2>.Equals(KingsPassMissionKey2 x, KingsPassMissionKey2 y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<KingsPassMissionKey2>.GetHashCode(KingsPassMissionKey2 obj)
	{
		return obj.GetHashCode();
	}
}
