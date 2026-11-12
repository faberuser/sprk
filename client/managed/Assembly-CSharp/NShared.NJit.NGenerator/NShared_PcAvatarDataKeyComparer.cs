using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_PcAvatarDataKeyComparer : IKeyComparerT<PcAvatarDataKey>, IKeyComparer, IEqualityComparer<PcAvatarDataKey>
{
	private Type HandleType => typeof(PcAvatarDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<PcAvatarDataKey>.Equals(PcAvatarDataKey x, PcAvatarDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<PcAvatarDataKey>.GetHashCode(PcAvatarDataKey obj)
	{
		return obj.GetHashCode();
	}
}
