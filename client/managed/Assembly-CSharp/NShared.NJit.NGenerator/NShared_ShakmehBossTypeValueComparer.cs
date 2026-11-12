using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_ShakmehBossTypeValueComparer : IKeyComparerT<ShakmehBossType>, IKeyComparer, IEqualityComparer<ShakmehBossType>
{
	private Type HandleType => typeof(ShakmehBossType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<ShakmehBossType>.Equals(ShakmehBossType x, ShakmehBossType y)
	{
		return x == y;
	}

	int IEqualityComparer<ShakmehBossType>.GetHashCode(ShakmehBossType obj)
	{
		return (int)obj;
	}
}
