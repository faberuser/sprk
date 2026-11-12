using System.Collections.Generic;

public static class Vespa_Generic_ISortedList_Extension_Method
{
	public static Tuple<int, int> FindNear<TKey, TValue>(this SortedList<TKey, TValue> i_this, TKey i_key)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(i_this))
		{
			return Tuple.Create(-1, -1);
		}
		Tuple<int, int> tuple = Tuple.Create(0, i_this.Count);
		int num;
		while (true)
		{
			if (tuple.Item2 - tuple.Item1 <= 1)
			{
				return tuple;
			}
			num = (tuple.Item1 + tuple.Item2) / 2;
			int num2 = i_this.Comparer.Compare(i_this.Keys[num], i_key);
			if (num2 == 0)
			{
				break;
			}
			if (num2 > 0)
			{
				tuple.Item1 = num;
			}
			else
			{
				tuple.Item2 = num;
			}
		}
		tuple.Item1 = num;
		tuple.Item2 = num;
		return tuple;
	}
}
