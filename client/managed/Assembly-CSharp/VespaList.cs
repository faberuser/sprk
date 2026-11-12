using System;
using System.Collections;
using System.Collections.Generic;
using NVespa;
using NVespa.NInternalUsage;
using UnityEngine;

public class VespaList<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable, ICollection
{
	public delegate int CompareFunc(T left, T right);

	private GenericCircularCache<T>.Buffer _buffer;

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot => null;

	bool ICollection<T>.IsReadOnly => false;

	public int Count
	{
		get
		{
			if (_buffer != null)
			{
				return _buffer.Circular.Count;
			}
			return 0;
		}
	}

	private bool _isFull
	{
		get
		{
			if (_buffer != null)
			{
				return _buffer.Circular.Capacity == _buffer.Circular.Count;
			}
			return true;
		}
	}

	public T this[int i]
	{
		get
		{
			return _buffer.Circular[i];
		}
		set
		{
			_buffer.Circular[i] = value;
		}
	}

	public VespaList()
	{
	}

	public VespaList(int minReserveSize)
	{
		if (minReserveSize > 0)
		{
			_buffer = GenericCircularCache<T>.CreateBuffer(minReserveSize);
		}
	}

	~VespaList()
	{
		if (_buffer != null)
		{
			ClearAndRelease();
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		if (_buffer != null)
		{
			int i = 0;
			while (i < Count)
			{
				yield return _buffer.Circular[i];
				int num = i + 1;
				i = num;
			}
		}
	}

	public IEnumerator<T> GetEnumerator()
	{
		if (_buffer != null)
		{
			int i = 0;
			while (i < Count)
			{
				yield return _buffer.Circular[i];
				int num = i + 1;
				i = num;
			}
		}
	}

	private void AllocateMore()
	{
		if (_buffer == null)
		{
			_buffer = GenericCircularCache<T>.CreateBuffer(0);
			return;
		}
		GenericCircularCache<T>.Buffer buffer = GenericCircularCache<T>.CreateBuffer(_buffer.Circular.Capacity * 2);
		buffer.Circular.ResetAndCopyFrom(_buffer.Circular);
		GenericCircularCache<T>.ReleaseBuffer(_buffer);
		_buffer = buffer;
	}

	public void Clear()
	{
		if (Count != 0)
		{
			_buffer.Circular.Clear();
		}
	}

	public void ClearAndRelease()
	{
		Clear();
		if (_buffer != null)
		{
			GenericCircularCache<T>.ReleaseBuffer(_buffer);
			_buffer = null;
		}
	}

	public void Add(T item)
	{
		if (_isFull)
		{
			AllocateMore();
		}
		_buffer.Circular.Add(item);
	}

	public void AddList(IList<T> itemList)
	{
		for (int i = 0; i < itemList.Count; i++)
		{
			Add(itemList[i]);
		}
	}

	public void Insert(int pos, T item)
	{
		if (_isFull)
		{
			AllocateMore();
		}
		if (pos > -1 && pos < _buffer.Circular.Count)
		{
			_buffer.Circular.Insert(pos, item);
		}
		else
		{
			Add(item);
		}
	}

	public bool Contains(T item)
	{
		if (Count == 0)
		{
			return false;
		}
		return _buffer.Circular.Contains(item);
	}

	void ICollection.CopyTo(Array array, int arrayIndex)
	{
		if (Count != 0)
		{
			Array.Copy(_buffer.Circular.ToArray(), 0, array, arrayIndex, Count);
		}
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		if (Count != 0)
		{
			_buffer.Circular.CopyTo(array, arrayIndex);
		}
	}

	public int IndexOf(T item)
	{
		if (_buffer == null)
		{
			return -1;
		}
		return _buffer.Circular.IndexOf(item);
	}

	public bool Remove(T item)
	{
		if (_buffer == null)
		{
			return false;
		}
		return _buffer.Circular.Remove(item);
	}

	public void RemoveAt(int indexAt)
	{
		if (_buffer != null)
		{
			_buffer.Circular.RemoveAt(indexAt);
		}
	}

	public T[] ToArray()
	{
		if (Count == 0)
		{
			return null;
		}
		return _buffer.Circular.ToArray();
	}

	public void Sort(Comparison<T> comparison)
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
			if (num3 > Mathf.Max(1000, Count * Count))
			{
				VespaDebug.LogError("Too Many Loop");
				break;
			}
			flag = false;
			for (int i = num; i < num2; i++)
			{
				if (comparison(_buffer.Circular[i], _buffer.Circular[i + 1]) > 0)
				{
					T value = _buffer.Circular[i];
					_buffer.Circular[i] = _buffer.Circular[i + 1];
					_buffer.Circular[i + 1] = value;
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
