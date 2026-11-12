using System.Collections.Generic;
using NShared;

public static class __Extension_Method_NShared_ReadonlyEmpty__
{
	public static IList<T> GetEmptyIfNull<T>(this IList<T> This)
	{
		return This ?? ReadonlyEmpty<T>.IList;
	}

	public static T[] GetEmptyIfNull<T>(this T[] This)
	{
		return This ?? ReadonlyEmpty<T>.Array;
	}
}
