using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_AttendanceUrlDataKeyComparer : IKeyComparerT<AttendanceUrlDataKey>, IKeyComparer, IEqualityComparer<AttendanceUrlDataKey>
{
	private Type HandleType => typeof(AttendanceUrlDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<AttendanceUrlDataKey>.Equals(AttendanceUrlDataKey x, AttendanceUrlDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<AttendanceUrlDataKey>.GetHashCode(AttendanceUrlDataKey obj)
	{
		return obj.GetHashCode();
	}
}
