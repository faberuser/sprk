using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_PunishmentGroupDataKeyComparer : IKeyComparerT<PunishmentGroupDataKey>, IKeyComparer, IEqualityComparer<PunishmentGroupDataKey>
{
	private Type HandleType => typeof(PunishmentGroupDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<PunishmentGroupDataKey>.Equals(PunishmentGroupDataKey x, PunishmentGroupDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<PunishmentGroupDataKey>.GetHashCode(PunishmentGroupDataKey obj)
	{
		return obj.GetHashCode();
	}
}
