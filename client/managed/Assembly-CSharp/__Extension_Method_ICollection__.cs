using System;
using System.Collections.Generic;

public static class __Extension_Method_ICollection__
{
	public static bool IsNullOrEmpty<T>(this ICollection<T> This)
	{
		if (This == null)
		{
			return true;
		}
		return This.Count == 0;
	}

	public static int COUNT<T>(this ICollection<T> This)
	{
		return This?.Count ?? 0;
	}

	public static void ADDRANGE<T>(this HashSet<T> This, IEnumerable<T> values)
	{
		if (values == null)
		{
			return;
		}
		foreach (T value in values)
		{
			This.Add(value);
		}
	}

	public static void ForEach_to_End<T>(this IEnumerable<T> This, Action<T> action)
	{
		if (This == null || action == null)
		{
			return;
		}
		foreach (T Thi in This)
		{
			action(Thi);
		}
	}

	public static void TryDequeue<T>(this Queue<T> This, out T value)
	{
		lock (This)
		{
			if (This.Count > 0)
			{
				value = This.Dequeue();
			}
			else
			{
				value = default(T);
			}
		}
	}
}
