using System;
using System.Collections.Generic;
using NVespa;

namespace NShared;

public class ReserveList<T> where T : class, new()
{
	public const int DefaultReserveCount = 32;

	private T[] _array;

	public int Count { get; private set; }

	public T this[int index]
	{
		get
		{
			return _array[index];
		}
		set
		{
			_array[index] = value;
		}
	}

	public ReserveList()
		: this(32)
	{
	}

	public ReserveList(T t)
		: this()
	{
		Add(t);
	}

	public ReserveList(IEnumerable<T> collection)
		: this()
	{
		foreach (T item in collection)
		{
			Add(item);
		}
	}

	public ReserveList(int reserveCount)
	{
		_array = new T[System.Math.Max(1, reserveCount)];
	}

	public void ReserveEmpty(int count)
	{
		if (Count + count > _array.Length)
		{
			int num;
			for (num = _array.Length * 2; num < Count + count; num *= 2)
			{
			}
			T[] array = _array;
			_array = new T[num];
			for (int i = 0; i < Count; i++)
			{
				_array[i] = array[i];
			}
		}
	}

	public void ForEach(Action<T> action)
	{
		if (action != null)
		{
			for (int i = 0; i < Count; i++)
			{
				action(_array[i]);
			}
		}
	}

	public T[] ToArray()
	{
		T[] array = new T[Count];
		for (int i = 0; i < Count; i++)
		{
			array[i] = _array[i];
		}
		return array;
	}

	public List<T> ToList()
	{
		List<T> list = new List<T>();
		for (int i = 0; i < Count; i++)
		{
			list.Add(_array[i]);
		}
		return list;
	}

	public void Add(T item)
	{
		ReserveEmpty(1);
		_array[Count] = item;
		Count++;
	}

	public void AddRange(ReserveList<T> list)
	{
		ReserveEmpty(list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			_array[Count + i] = list[i];
		}
		Count += list.Count;
	}

	public void AddRange(IList<T> list)
	{
		ReserveEmpty(list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			_array[Count + i] = list[i];
		}
		Count += list.Count;
	}

	public bool Contains(T item)
	{
		return IndexOf(item) != -1;
	}

	public int IndexOf(T item)
	{
		for (int i = 0; i < Count; i++)
		{
			if (_array[i] == item)
			{
				return i;
			}
		}
		return -1;
	}

	public bool Remove(T item)
	{
		int num = IndexOf(item);
		if (num == -1)
		{
			return false;
		}
		RemoveAt(num);
		return true;
	}

	public void RemoveAt(int index)
	{
		RemoveRange(index, 1);
	}

	public void RemoveRange(int index, int removeCount)
	{
		for (int i = index; i < Count - removeCount; i++)
		{
			_array[i] = _array[i + removeCount];
		}
		for (int j = Count - removeCount; j < Count; j++)
		{
			_array[j] = null;
		}
		Count -= removeCount;
	}

	public int Remove(Predicate<T> match)
	{
		int num = 0;
		int i;
		for (i = 0; i < Count; i++)
		{
			if (match(_array[i]))
			{
				i++;
				num++;
				break;
			}
		}
		for (; i < Count; i++)
		{
			if (match(_array[i]))
			{
				num++;
			}
			else
			{
				_array[i - num] = _array[i];
			}
		}
		Count -= num;
		return num;
	}

	public virtual void Sort(Comparison<T> comparison)
	{
		if (Count == 0)
		{
			return;
		}
		int num = 0;
		int num2 = Count - 1;
		bool flag = true;
		int num3 = 0;
		while (flag)
		{
			num3++;
			if (num3 > System.Math.Max(1000, Count * Count))
			{
				VespaDebug.LogError("Too Many Loop");
				break;
			}
			flag = false;
			for (int i = num; i < num2; i++)
			{
				if (comparison(_array[i], _array[i + 1]) > 0)
				{
					T val = _array[i];
					_array[i] = _array[i + 1];
					_array[i + 1] = val;
					flag = true;
				}
				else if (!flag)
				{
					num = ((i != 0) ? (i - 1) : 0);
				}
			}
		}
	}
}
