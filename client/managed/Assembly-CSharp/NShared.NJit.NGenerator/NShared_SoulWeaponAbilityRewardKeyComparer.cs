using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_SoulWeaponAbilityRewardKeyComparer : IKeyComparerT<SoulWeaponAbilityRewardKey>, IKeyComparer, IEqualityComparer<SoulWeaponAbilityRewardKey>
{
	private Type HandleType => typeof(SoulWeaponAbilityRewardKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<SoulWeaponAbilityRewardKey>.Equals(SoulWeaponAbilityRewardKey x, SoulWeaponAbilityRewardKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<SoulWeaponAbilityRewardKey>.GetHashCode(SoulWeaponAbilityRewardKey obj)
	{
		return obj.GetHashCode();
	}
}
