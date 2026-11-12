using System.Collections.Generic;

namespace NGame2.N3D;

public static class ListHelper
{
	private static bool IsValidIndex<T>(this List<T> list, int index)
	{
		if (index < 0)
		{
			return false;
		}
		if (list.Count <= index)
		{
			return false;
		}
		return true;
	}
}
