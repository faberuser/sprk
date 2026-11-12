using System;
using System.Collections;
using System.Collections.Generic;

namespace NVespa;

public class CircularBuffer<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
{
	private T[] _buffer;

	private int _position;

	public T this[int index]
	{
		get
		{
			if (index < 0 || index >= Count)
			{
				return default(T);
			}
			return GetUnsafe(index);
		}
		set
		{
			if (index >= 0 && index < Count)
			{
				SetUnsafe(index, value);
			}
		}
	}

	public int Capacity { get; private set; }

	public int Count { get; private set; }

	bool ICollection<T>.IsReadOnly => false;

	public CircularBuffer(int capacity)
	{
		if (capacity <= 0)
		{
			Capacity = 1;
		}
		else
		{
			Capacity = capacity;
		}
		_buffer = new T[capacity];
	}

	public T GetUnsafe(int index)
	{
		int num = (_position - Count + index) % Capacity;
		return _buffer[num];
	}

	public void SetUnsafe(int index, T value)
	{
		int num = (_position - Count + index) % Capacity;
		_buffer[num] = value;
	}

	public void ResetAndCopyFrom(CircularBuffer<T> src)
	{
		Clear();
		for (int i = 0; i < src.Count; i++)
		{
			Add(src[i]);
		}
	}

	public void Add(T item)
	{
		_buffer[_position++ % Capacity] = item;
		if (Count < Capacity)
		{
			Count++;
		}
	}

	public void AddFront(T item)
	{
		int num = (_position - Count - 1) % Capacity;
		if (num < 0)
		{
			num += Capacity;
			_position += Capacity;
		}
		_buffer[num] = item;
		if (Count < Capacity)
		{
			Count++;
		}
	}

	public void Clear()
	{
		for (int i = 0; i < Count; i++)
		{
			int num = (_position - Count + i) % Capacity;
			_buffer[num] = default(T);
		}
		_position = 0;
		Count = 0;
	}

	public bool Contains(T item)
	{
		return IndexOf(item) != -1;
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		for (int i = 0; i < Count; i++)
		{
			array[i + arrayIndex] = _buffer[(_position - Count + i) % Capacity];
		}
	}

	public T[] ToArray()
	{
		T[] array = new T[Count];
		CopyTo(array, 0);
		return array;
	}

	public IEnumerator<T> GetEnumerator()
	{
		for (int i = 0; i < Count; i++)
		{
			yield return this[i];
		}
	}

	public int IndexOf(T item)
	{
		for (int i = 0; i < Count; i++)
		{
			T val = _buffer[(_position - Count + i) % Capacity];
			if (item == null && val == null)
			{
				return i;
			}
			if (item != null && item.Equals(val))
			{
				return i;
			}
		}
		return -1;
	}

	public void Insert(int index, T item)
	{
		if (index < 0 || index > Count)
		{
			return;
		}
		if (index == 0)
		{
			AddFront(item);
			return;
		}
		if (index == Count)
		{
			Add(item);
			return;
		}
		int num = System.Math.Min(Count, Capacity - 1) - index;
		int num2 = (_position - Count + index) % Capacity;
		for (int num3 = num2 + num; num3 > num2; num3--)
		{
			int num4 = num3 % Capacity;
			int num5 = (num3 - 1) % Capacity;
			_buffer[num4] = _buffer[num5];
		}
		_buffer[num2] = item;
		if (Count < Capacity)
		{
			Count++;
			_position++;
		}
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
		if (index < 0 || index >= Count)
		{
			return;
		}
		if (index == 0)
		{
			_buffer[(_position - Count) % Capacity] = default(T);
			Count--;
			return;
		}
		for (int i = index; i < Count - 1; i++)
		{
			int num = (_position - Count + i) % Capacity;
			int num2 = (_position - Count + i + 1) % Capacity;
			_buffer[num] = _buffer[num2];
		}
		int num3 = (_position - 1) % Capacity;
		_buffer[num3] = default(T);
		_position--;
		Count--;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
