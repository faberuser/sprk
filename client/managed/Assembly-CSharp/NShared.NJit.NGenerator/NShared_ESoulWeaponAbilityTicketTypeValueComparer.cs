using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_ESoulWeaponAbilityTicketTypeValueComparer : IKeyComparerT<ESoulWeaponAbilityTicketType>, IKeyComparer, IEqualityComparer<ESoulWeaponAbilityTicketType>
{
	private Type HandleType => typeof(ESoulWeaponAbilityTicketType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<ESoulWeaponAbilityTicketType>.Equals(ESoulWeaponAbilityTicketType x, ESoulWeaponAbilityTicketType y)
	{
		return x == y;
	}

	int IEqualityComparer<ESoulWeaponAbilityTicketType>.GetHashCode(ESoulWeaponAbilityTicketType obj)
	{
		return (int)obj;
	}
}
