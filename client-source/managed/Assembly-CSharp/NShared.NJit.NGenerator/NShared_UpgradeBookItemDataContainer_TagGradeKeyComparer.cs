using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_UpgradeBookItemDataContainer_TagGradeKeyComparer : IKeyComparerT<UpgradeBookItemDataContainer.TagGradeKey>, IKeyComparer, IEqualityComparer<UpgradeBookItemDataContainer.TagGradeKey>
{
	private Type HandleType => typeof(UpgradeBookItemDataContainer.TagGradeKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<UpgradeBookItemDataContainer.TagGradeKey>.Equals(UpgradeBookItemDataContainer.TagGradeKey x, UpgradeBookItemDataContainer.TagGradeKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<UpgradeBookItemDataContainer.TagGradeKey>.GetHashCode(UpgradeBookItemDataContainer.TagGradeKey obj)
	{
		return obj.GetHashCode();
	}
}
