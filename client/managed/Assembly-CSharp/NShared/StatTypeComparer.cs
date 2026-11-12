using System.Collections.Generic;

namespace NShared;

public class StatTypeComparer : IEqualityComparer<StatType>
{
	public static StatTypeComparer Default = new StatTypeComparer();

	public bool Equals(StatType x, StatType y)
	{
		return x == y;
	}

	public int GetHashCode(StatType obj)
	{
		return (int)obj;
	}
}
