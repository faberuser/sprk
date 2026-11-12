using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_NPCFriendlyActionKey2Comparer : IKeyComparerT<NPCFriendlyActionKey2>, IKeyComparer, IEqualityComparer<NPCFriendlyActionKey2>
{
	private Type HandleType => typeof(NPCFriendlyActionKey2);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<NPCFriendlyActionKey2>.Equals(NPCFriendlyActionKey2 x, NPCFriendlyActionKey2 y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<NPCFriendlyActionKey2>.GetHashCode(NPCFriendlyActionKey2 obj)
	{
		return obj.GetHashCode();
	}
}
