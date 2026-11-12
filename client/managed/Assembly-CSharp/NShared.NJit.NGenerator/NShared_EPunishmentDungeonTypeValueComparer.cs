using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_EPunishmentDungeonTypeValueComparer : IKeyComparerT<EPunishmentDungeonType>, IKeyComparer, IEqualityComparer<EPunishmentDungeonType>
{
	private Type HandleType => typeof(EPunishmentDungeonType);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<EPunishmentDungeonType>.Equals(EPunishmentDungeonType x, EPunishmentDungeonType y)
	{
		return x == y;
	}

	int IEqualityComparer<EPunishmentDungeonType>.GetHashCode(EPunishmentDungeonType obj)
	{
		return (int)obj;
	}
}
