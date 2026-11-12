using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_CreatureStarGradeStatKeyComparer : IKeyComparerT<CreatureStarGradeStatKey>, IKeyComparer, IEqualityComparer<CreatureStarGradeStatKey>
{
	private Type HandleType => typeof(CreatureStarGradeStatKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<CreatureStarGradeStatKey>.Equals(CreatureStarGradeStatKey x, CreatureStarGradeStatKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<CreatureStarGradeStatKey>.GetHashCode(CreatureStarGradeStatKey obj)
	{
		return obj.GetHashCode();
	}
}
