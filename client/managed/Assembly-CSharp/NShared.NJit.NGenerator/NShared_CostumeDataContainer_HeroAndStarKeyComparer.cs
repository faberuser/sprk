using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_CostumeDataContainer_HeroAndStarKeyComparer : IKeyComparerT<CostumeDataContainer.HeroAndStarKey>, IKeyComparer, IEqualityComparer<CostumeDataContainer.HeroAndStarKey>
{
	private Type HandleType => typeof(CostumeDataContainer.HeroAndStarKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<CostumeDataContainer.HeroAndStarKey>.Equals(CostumeDataContainer.HeroAndStarKey x, CostumeDataContainer.HeroAndStarKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<CostumeDataContainer.HeroAndStarKey>.GetHashCode(CostumeDataContainer.HeroAndStarKey obj)
	{
		return obj.GetHashCode();
	}
}
