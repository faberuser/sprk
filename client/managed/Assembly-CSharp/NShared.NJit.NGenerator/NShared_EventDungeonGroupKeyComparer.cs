using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_EventDungeonGroupKeyComparer : IKeyComparerT<EventDungeonGroupKey>, IKeyComparer, IEqualityComparer<EventDungeonGroupKey>
{
	private Type HandleType => typeof(EventDungeonGroupKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<EventDungeonGroupKey>.Equals(EventDungeonGroupKey x, EventDungeonGroupKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<EventDungeonGroupKey>.GetHashCode(EventDungeonGroupKey obj)
	{
		return obj.GetHashCode();
	}
}
