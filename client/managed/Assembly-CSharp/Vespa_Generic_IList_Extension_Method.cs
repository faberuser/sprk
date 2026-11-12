using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NVespa;
using UnityEngine;

public static class Vespa_Generic_IList_Extension_Method
{
	public static T RandomGet<T>(this IList<T> This)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(This))
		{
			return default(T);
		}
		int index = UnityEngine.Random.Range(0, This.Count);
		return This[index];
	}

	public static T RandomPop<T>(this IList<T> This)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(This))
		{
			return default(T);
		}
		int index = UnityEngine.Random.Range(0, This.Count);
		T result = This[index];
		This.RemoveAt(index);
		return result;
	}

	public static int RemoveNull<T>(this IList<T> This) where T : class
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(This))
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < This.Count; i++)
		{
			if (This[i] == null)
			{
				This.RemoveAt(i);
				num++;
				i--;
			}
		}
		return num;
	}

	public static T GetLast<T>(this IList<T> This)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(This))
		{
			return default(T);
		}
		return This[This.Count - 1];
	}

	public static T GetValid<T>(this IList<T> This, int index) where T : class
	{
		if (!IsValidIndex(This, index))
		{
			return null;
		}
		return This[index];
	}

	public static T GetValidValue<T>(this IList<T> This, int index, T defaultT = default(T)) where T : struct
	{
		if (!IsValidIndex(This, index))
		{
			return defaultT;
		}
		return This[index];
	}

	public static bool IsValidIndex<T>(this IList<T> This, int index)
	{
		if (This == null)
		{
			return false;
		}
		if (index < 0)
		{
			return false;
		}
		if (index >= This.Count)
		{
			return false;
		}
		return true;
	}

	public static bool IsValidIndex<T>(this IList<T> This, int index0, int index1)
	{
		if (IsValidIndex(This, index0))
		{
			return IsValidIndex(This, index1);
		}
		return false;
	}

	public static bool IsValidIndex<T>(this IList<T> This, int index0, int index1, int index2)
	{
		if (IsValidIndex(This, index0) && IsValidIndex(This, index1))
		{
			return IsValidIndex(This, index2);
		}
		return false;
	}

	public static void Swap<T>(this IList<T> This, int indexA, int indexB)
	{
		if (This != null && indexA != indexB)
		{
			T value = This[indexA];
			This[indexA] = This[indexB];
			This[indexB] = value;
		}
	}

	public static bool GetNeighbor<T>(this IList<T> This, out T o_minT, out T o_maxT, Func<T, int> decisionFunc)
	{
		if (This.Count == 0)
		{
			o_minT = default(T);
			o_maxT = default(T);
			return false;
		}
		int num = 0;
		int num2 = This.Count - 1;
		int num3 = decisionFunc(This[num]);
		if (num3 > 0)
		{
			o_minT = default(T);
			o_maxT = default(T);
			return false;
		}
		if (num3 == 0)
		{
			o_minT = This[num];
			o_maxT = This[num];
			return true;
		}
		int num4 = decisionFunc(This[num2]);
		if (num4 < 0)
		{
			o_minT = default(T);
			o_maxT = default(T);
			return false;
		}
		if (num4 == 0)
		{
			o_minT = This[num2];
			o_maxT = This[num2];
			return true;
		}
		while (true)
		{
			if (num2 - num == 0)
			{
				VespaDebug.LogError("Invalid Calculation");
				o_minT = This[num];
				o_maxT = This[num];
				return true;
			}
			if (num2 - num == 1)
			{
				break;
			}
			int num5 = (num + num2) / 2;
			int num6 = decisionFunc(This[num5]);
			if (num6 == 0)
			{
				o_minT = This[num5];
				o_maxT = This[num5];
				return true;
			}
			if (num6 < 0)
			{
				num = num5;
			}
			else
			{
				num2 = num5;
			}
		}
		o_minT = This[num];
		o_maxT = This[num2];
		return true;
	}

	public static IEnumerable<T> SKIP<T>(this IEnumerable<T> source, int count)
	{
		if (source is IList<T>)
		{
			IList<T> list = (IList<T>)source;
			for (int i = count; i < list.Count; i++)
			{
				yield return list[i];
			}
			yield break;
		}
		if (source is IList)
		{
			IList list2 = (IList)source;
			for (int i = count; i < list2.Count; i++)
			{
				yield return (T)list2[i];
			}
			yield break;
		}
		using IEnumerator<T> e = source.GetEnumerator();
		while (count > 0 && e.MoveNext())
		{
			count--;
		}
		if (count <= 0)
		{
			while (e.MoveNext())
			{
				yield return e.Current;
			}
		}
	}

	public static IEnumerable<T> TAKE<T>(this IEnumerable<T> source, int count)
	{
		int _count;
		if (source is IList<T>)
		{
			IList<T> list = (IList<T>)source;
			_count = count;
			if (list.Count < count)
			{
				_count = list.Count;
			}
			for (int i = 0; i < _count; i++)
			{
				yield return list[i];
			}
			yield break;
		}
		if (source is IList)
		{
			IList list2 = (IList)source;
			_count = count;
			if (list2.Count < count)
			{
				_count = list2.Count;
			}
			for (int i = 0; i < _count; i++)
			{
				yield return (T)list2[i];
			}
			yield break;
		}
		_count = 0;
		IEnumerator<T> it = source.GetEnumerator();
		while (it.MoveNext())
		{
			_count++;
			if (count <= _count)
			{
				yield return it.Current;
			}
		}
	}

	public static string ToReadLine<T>(this IEnumerable<T> source, string separator = ",")
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (source is IList<T>)
		{
			IList<T> list = (IList<T>)source;
			for (int i = 0; i < list.Count; i++)
			{
				stringBuilder.Append(list[i].ToString()).Append((i == list.Count - 1) ? string.Empty : separator);
			}
		}
		else if (source is IList)
		{
			IList list2 = (IList)source;
			for (int j = 0; j < list2.Count; j++)
			{
				stringBuilder.Append(((T)list2[j]/*cast due to .constrained prefix*/).ToString()).Append((j == list2.Count - 1) ? string.Empty : separator);
			}
		}
		else
		{
			using IEnumerator<T> enumerator = source.GetEnumerator();
			bool flag = enumerator.MoveNext();
			while (flag)
			{
				stringBuilder.Append(enumerator.Current.ToString());
				flag = enumerator.MoveNext();
				stringBuilder.Append(flag ? separator : string.Empty);
			}
		}
		return stringBuilder.ToString();
	}
}
