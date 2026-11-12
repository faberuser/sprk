using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_PunishmentRuneDataContainer_PunishmentRuneKeyComparer : IKeyComparerT<PunishmentRuneDataContainer.PunishmentRuneKey>, IKeyComparer, IEqualityComparer<PunishmentRuneDataContainer.PunishmentRuneKey>
{
	private Type HandleType => typeof(PunishmentRuneDataContainer.PunishmentRuneKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<PunishmentRuneDataContainer.PunishmentRuneKey>.Equals(PunishmentRuneDataContainer.PunishmentRuneKey x, PunishmentRuneDataContainer.PunishmentRuneKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<PunishmentRuneDataContainer.PunishmentRuneKey>.GetHashCode(PunishmentRuneDataContainer.PunishmentRuneKey obj)
	{
		return obj.GetHashCode();
	}
}
