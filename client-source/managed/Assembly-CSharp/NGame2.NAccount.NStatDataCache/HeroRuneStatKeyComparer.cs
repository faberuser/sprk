using System.Collections.Generic;

namespace NGame2.NAccount.NStatDataCache;

public class HeroRuneStatKeyComparer : IEqualityComparer<RuneStatKey>
{
	public bool Equals(RuneStatKey x, RuneStatKey y)
	{
		return RuneStatKey.IsEquals(x, y);
	}

	public int GetHashCode(RuneStatKey obj)
	{
		return RuneStatKey.GetHashCode(obj);
	}
}
