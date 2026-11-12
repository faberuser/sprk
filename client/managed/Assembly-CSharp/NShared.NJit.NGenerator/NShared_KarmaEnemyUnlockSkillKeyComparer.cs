using System;
using System.Collections.Generic;
using NShared.NJit.NResolver;

namespace NShared.NJit.NGenerator;

public class NShared_KarmaEnemyUnlockSkillKeyComparer : IKeyComparerT<KarmaEnemyUnlockSkillKey>, IKeyComparer, IEqualityComparer<KarmaEnemyUnlockSkillKey>
{
	private Type HandleType => typeof(KarmaEnemyUnlockSkillKey);

	Type IKeyComparer.GetCompareType()
	{
		return HandleType;
	}

	bool IEqualityComparer<KarmaEnemyUnlockSkillKey>.Equals(KarmaEnemyUnlockSkillKey x, KarmaEnemyUnlockSkillKey y)
	{
		return x.Equals(y);
	}

	int IEqualityComparer<KarmaEnemyUnlockSkillKey>.GetHashCode(KarmaEnemyUnlockSkillKey obj)
	{
		return obj.GetHashCode();
	}
}
