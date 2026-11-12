using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_AttendanceRewardInfoKeyComparer : IKeyComparerT<AttendanceRewardInfoKey>, IKeyComparer, IEqualityComparer<AttendanceRewardInfoKey>
{
	private Type HandleType => typeof(AttendanceRewardInfoKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<AttendanceRewardInfoKey>.Equals(AttendanceRewardInfoKey x, AttendanceRewardInfoKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<AttendanceRewardInfoKey>.GetHashCode(AttendanceRewardInfoKey obj)
	{
		return obj.GetHashCode();
	}
}
