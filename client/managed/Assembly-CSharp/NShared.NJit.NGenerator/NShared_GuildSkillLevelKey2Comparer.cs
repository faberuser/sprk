using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_GuildSkillLevelKey2Comparer : IKeyComparerT<GuildSkillLevelKey2>, IKeyComparer, IEqualityComparer<GuildSkillLevelKey2>
{
	private Type HandleType => typeof(GuildSkillLevelKey2);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<GuildSkillLevelKey2>.Equals(GuildSkillLevelKey2 x, GuildSkillLevelKey2 y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<GuildSkillLevelKey2>.GetHashCode(GuildSkillLevelKey2 obj)
	{
		return obj.GetHashCode();
	}
}
