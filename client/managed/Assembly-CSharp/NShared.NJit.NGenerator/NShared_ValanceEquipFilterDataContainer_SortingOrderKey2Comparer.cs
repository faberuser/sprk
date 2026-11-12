using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_ValanceEquipFilterDataContainer_SortingOrderKey2Comparer : IKeyComparerT<ValanceEquipFilterDataContainer.SortingOrderKey2>, IKeyComparer, IEqualityComparer<ValanceEquipFilterDataContainer.SortingOrderKey2>
{
	private Type HandleType => typeof(ValanceEquipFilterDataContainer.SortingOrderKey2);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<ValanceEquipFilterDataContainer.SortingOrderKey2>.Equals(ValanceEquipFilterDataContainer.SortingOrderKey2 x, ValanceEquipFilterDataContainer.SortingOrderKey2 y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<ValanceEquipFilterDataContainer.SortingOrderKey2>.GetHashCode(ValanceEquipFilterDataContainer.SortingOrderKey2 obj)
	{
		return obj.GetHashCode();
	}
}
