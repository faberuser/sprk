using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_AttendanceRewardUrlKeyComparer : IKeyComparerT<AttendanceRewardUrlKey>, IKeyComparer, IEqualityComparer<AttendanceRewardUrlKey>
{
	private Type HandleType => typeof(AttendanceRewardUrlKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<AttendanceRewardUrlKey>.Equals(AttendanceRewardUrlKey x, AttendanceRewardUrlKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<AttendanceRewardUrlKey>.GetHashCode(AttendanceRewardUrlKey obj)
	{
		return obj.GetHashCode();
	}
}
