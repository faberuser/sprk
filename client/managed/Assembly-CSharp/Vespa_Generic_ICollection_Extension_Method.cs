using System.Collections.Generic;

public static class Vespa_Generic_ICollection_Extension_Method
{
	public static void CLEAR<T>(this ICollection<T> This)
	{
		This?.Clear();
	}
}
