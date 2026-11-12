using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_DbLogDataKeyComparer : IKeyComparerT<DbLogDataKey>, IKeyComparer, IEqualityComparer<DbLogDataKey>
{
	private Type HandleType => typeof(DbLogDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<DbLogDataKey>.Equals(DbLogDataKey x, DbLogDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<DbLogDataKey>.GetHashCode(DbLogDataKey obj)
	{
		return obj.GetHashCode();
	}
}
