using System.Collections.Generic;
using NVespa;

public static class Vespa_IVisitor_ExtensionMethod
{
	public static bool TryVisit<T>(this IVisitor This, T t)
	{
		if (!(This is IGenericVisitor<T> genericVisitor))
		{
			return false;
		}
		genericVisitor.Visit(t);
		return true;
	}

	public static void VisitList<T>(this IVisitor This, IList<T> list)
	{
		if (This != null && !__Extension_Method_ICollection__.IsNullOrEmpty(list))
		{
			for (int i = 0; i < list.Count; i++)
			{
				This.TryVisit(list[i]);
			}
		}
	}

	public static void VisitList<T>(this IVisitor This, PoolingList<T> list)
	{
		if (This != null && !list.IsNullOrEmpty())
		{
			for (int i = 0; i < list.Count; i++)
			{
				This.TryVisit(list[i]);
			}
		}
	}
}
