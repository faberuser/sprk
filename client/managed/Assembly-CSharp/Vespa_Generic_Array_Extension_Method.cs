using System.Collections.Generic;
using System.Linq;

public static class Vespa_Generic_Array_Extension_Method
{
	public static T[] AddLast<T>(this T[] This)
	{
		List<T> list = This.ToList();
		list.Add(default(T));
		return list.ToArray();
	}

	public static T[] Insert<T>(this T[] This, int index, T item = default(T))
	{
		List<T> list = This.ToList();
		list.Insert(index, item);
		return list.ToArray();
	}

	public static T[] RemoveLast<T>(this T[] This)
	{
		List<T> list = This.ToList();
		list.RemoveAt(This.Length - 1);
		return list.ToArray();
	}
}
