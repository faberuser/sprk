using System.Collections.Generic;

namespace NGame2.NMessage.NFastHandler;

internal class FastHandlerTupleComparer : IEqualityComparer<Tuple<EValueType, long>>
{
	public static NGame2.NMessage.NFastHandler.FastHandlerTupleComparer Default = new NGame2.NMessage.NFastHandler.FastHandlerTupleComparer();

	public bool Equals(Tuple<EValueType, long> x, Tuple<EValueType, long> y)
	{
		if (x.Item1 == y.Item1)
		{
			return x.Item2 == y.Item2;
		}
		return false;
	}

	public int GetHashCode(Tuple<EValueType, long> obj)
	{
		return (int)obj.Item1 * 37677 + (int)obj.Item2;
	}
}
