using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_GachaSelectShopEffectDataKeyComparer : IKeyComparerT<GachaSelectShopEffectDataKey>, IKeyComparer, IEqualityComparer<GachaSelectShopEffectDataKey>
{
	private Type HandleType => typeof(GachaSelectShopEffectDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<GachaSelectShopEffectDataKey>.Equals(GachaSelectShopEffectDataKey x, GachaSelectShopEffectDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<GachaSelectShopEffectDataKey>.GetHashCode(GachaSelectShopEffectDataKey obj)
	{
		return obj.GetHashCode();
	}
}
