using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_SoulWeaponEtherRateKeyComparer : IKeyComparerT<SoulWeaponEtherRateKey>, IKeyComparer, IEqualityComparer<SoulWeaponEtherRateKey>
{
	private Type HandleType => typeof(SoulWeaponEtherRateKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<SoulWeaponEtherRateKey>.Equals(SoulWeaponEtherRateKey x, SoulWeaponEtherRateKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<SoulWeaponEtherRateKey>.GetHashCode(SoulWeaponEtherRateKey obj)
	{
		return obj.GetHashCode();
	}
}
