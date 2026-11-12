using System.Collections.Generic;
using NVespa;

public static class Extension_Method_Vespa_PoolingList
{
	public static T GetValid<T>(this PoolingList<T> This, int index) where T : class
	{
		return Vespa_Generic_IList_Extension_Method.GetValid(This._list, index);
	}

	public static void ADDRANGE<T>(this List<T> This, PoolingList<T> poolingList)
	{
		if (This != null)
		{
			List<T> list = poolingList._list;
			if (!__Extension_Method_ICollection__.IsNullOrEmpty(list))
			{
				This.AddRange(list);
			}
		}
	}
}
