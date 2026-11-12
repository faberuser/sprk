using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_EventDungeonKeyComparer : IKeyComparerT<EventDungeonKey>, IKeyComparer, IEqualityComparer<EventDungeonKey>
{
	private Type HandleType => typeof(EventDungeonKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<EventDungeonKey>.Equals(EventDungeonKey x, EventDungeonKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<EventDungeonKey>.GetHashCode(EventDungeonKey obj)
	{
		return obj.GetHashCode();
	}
}
