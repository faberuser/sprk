using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_LoadingData_EGroupTypeValueComparer : IKeyComparerT<LoadingData.EGroupType>, IKeyComparer, IEqualityComparer<LoadingData.EGroupType>
{
	private Type HandleType => typeof(LoadingData.EGroupType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<LoadingData.EGroupType>.Equals(LoadingData.EGroupType x, LoadingData.EGroupType y)
	{
		return x == y;
	}

	int IEqualityComparer<LoadingData.EGroupType>.GetHashCode(LoadingData.EGroupType obj)
	{
		return (int)obj;
	}
}
