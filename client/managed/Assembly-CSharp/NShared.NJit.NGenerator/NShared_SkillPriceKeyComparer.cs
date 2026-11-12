using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_SkillPriceKeyComparer : IKeyComparerT<SkillPriceKey>, IKeyComparer, IEqualityComparer<SkillPriceKey>
{
	private Type HandleType => typeof(SkillPriceKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<SkillPriceKey>.Equals(SkillPriceKey x, SkillPriceKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<SkillPriceKey>.GetHashCode(SkillPriceKey obj)
	{
		return obj.GetHashCode();
	}
}
