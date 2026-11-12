using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_ItemEffectWidgetDataKeyComparer : IKeyComparerT<ItemEffectWidgetDataKey>, IKeyComparer, IEqualityComparer<ItemEffectWidgetDataKey>
{
	private Type HandleType => typeof(ItemEffectWidgetDataKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<ItemEffectWidgetDataKey>.Equals(ItemEffectWidgetDataKey x, ItemEffectWidgetDataKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<ItemEffectWidgetDataKey>.GetHashCode(ItemEffectWidgetDataKey obj)
	{
		return obj.GetHashCode();
	}
}
