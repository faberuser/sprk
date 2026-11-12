using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_ClientCreatureKeyComparer : IKeyComparerT<ClientCreatureKey>, IKeyComparer, IEqualityComparer<ClientCreatureKey>
{
	private Type HandleType => typeof(ClientCreatureKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<ClientCreatureKey>.Equals(ClientCreatureKey x, ClientCreatureKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<ClientCreatureKey>.GetHashCode(ClientCreatureKey obj)
	{
		return obj.GetHashCode();
	}
}
