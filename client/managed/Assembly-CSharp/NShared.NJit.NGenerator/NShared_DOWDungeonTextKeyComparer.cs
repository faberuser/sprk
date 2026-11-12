using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_DOWDungeonTextKeyComparer : IKeyComparerT<DOWDungeonTextKey>, IKeyComparer, IEqualityComparer<DOWDungeonTextKey>
{
	private Type HandleType => typeof(DOWDungeonTextKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<DOWDungeonTextKey>.Equals(DOWDungeonTextKey x, DOWDungeonTextKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<DOWDungeonTextKey>.GetHashCode(DOWDungeonTextKey obj)
	{
		return obj.GetHashCode();
	}
}
