using System.Collections.Generic;

namespace NShared;

public static class ReadonlyEmpty<T>
{
	private static T[] _empty = new T[0];

	public static IList<T> IList => _empty;

	public static T[] Array => _empty;
}
