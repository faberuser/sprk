using System;
using System.Collections.Generic;
using NVespa.NInternalUsage;
using UnityEngine;

namespace NVespa;

public class RenderingList<T> where T : struct
{
	private int _cacheBaseCount;

	private GenericArrayCache<T>.Buffer _buffer;

	public int size => Count;

	public T[] buffer => _buffer.Array;

	public int Count { get; private set; }

	public T this[int i]
	{
		get
		{
			return _buffer.Array[i];
		}
		set
		{
			_buffer.Array[i] = value;
		}
	}

	public RenderingList(int cacheBaseCount = 32)
	{
		_cacheBaseCount = Mathf.Max(2, cacheBaseCount);
		_buffer = SingletonAuto<GenericArrayCache<T>>.instance.GetEmptyBuffer();
	}

	~RenderingList()
	{
		if (_buffer != null)
		{
			SingletonAuto<GenericArrayCache<T>>.instance.ReleaseBuffer(_buffer);
			_buffer = null;
		}
	}

	private void AllocateMore(int requestSize)
	{
		GenericArrayCache<T>.Buffer buffer = SingletonAuto<GenericArrayCache<T>>.instance.CreateBuffer(Mathf.Max(requestSize, _buffer.Size * 2), _cacheBaseCount);
		if (Count > 0)
		{
			Array.Copy(_buffer.Array, buffer.Array, Count);
		}
		SingletonAuto<GenericArrayCache<T>>.instance.ReleaseBuffer(_buffer);
		_buffer = buffer;
	}

	public void Clear()
	{
		Count = 0;
	}

	public void ClearAndReleaseBuffer()
	{
		Count = 0;
		SingletonAuto<GenericArrayCache<T>>.instance.ReleaseBuffer(_buffer);
		_buffer = SingletonAuto<GenericArrayCache<T>>.instance.GetEmptyBuffer();
	}

	public void ReserveCount(int count)
	{
		int num = Count + count;
		if (num > _buffer.Size)
		{
			AllocateMore(num);
		}
	}

	public void AddUnsafe(T item)
	{
		_buffer.Array[Count++] = item;
	}

	public void Add(T item)
	{
		if (Count >= _buffer.Size)
		{
			AllocateMore(Count + 1);
		}
		_buffer.Array[Count++] = item;
	}

	public void AddEmpty(int addCount)
	{
		if (Count + addCount > _buffer.Size)
		{
			AllocateMore(Count + addCount);
		}
		Count += addCount;
	}

	public void AddCount(T item, int addCount)
	{
		if (Count + addCount > _buffer.Size)
		{
			AllocateMore(Count + addCount);
		}
		for (int i = 0; i < addCount; i++)
		{
			_buffer.Array[Count + i] = item;
		}
		Count += addCount;
	}

	public void AddList(RenderingList<T> srcList)
	{
		AddArray(srcList._buffer.Array, srcList.Count);
	}

	public void AddList(RenderingList<T> srcList, int count)
	{
		AddArray(srcList._buffer.Array, count);
	}

	public void AddArray(T[] array, int arrayCount)
	{
		if (Count + arrayCount > _buffer.Size)
		{
			AllocateMore(Count + arrayCount);
		}
		Array.Copy(array, 0, _buffer.Array, Count, arrayCount);
		Count += arrayCount;
	}

	public void AddArray(T[] array, int arrayOffset, int arrayCount)
	{
		if (Count + arrayCount > _buffer.Size)
		{
			AllocateMore(Count + arrayCount);
		}
		Array.Copy(array, arrayOffset, _buffer.Array, Count, arrayCount);
		Count += arrayCount;
	}

	public bool Remove(T item)
	{
		EqualityComparer<T> equalityComparer = EqualityComparer<T>.Default;
		for (int i = 0; i < Count; i++)
		{
			if (equalityComparer.Equals(_buffer.Array[i], item))
			{
				int count = Count - 1;
				Count = count;
				_buffer.Array[i] = default(T);
				for (int j = i; j < Count; j++)
				{
					_buffer.Array[j] = _buffer.Array[j + 1];
				}
				_buffer.Array[Count] = default(T);
				return true;
			}
		}
		return false;
	}

	public void RemoveAt(int indexAt)
	{
		if (indexAt >= 0 && indexAt < Count)
		{
			int count = Count - 1;
			Count = count;
			_buffer.Array[indexAt] = default(T);
			for (int i = indexAt; i < Count; i++)
			{
				_buffer.Array[i] = _buffer.Array[i + 1];
			}
			_buffer.Array[Count] = default(T);
		}
	}

	public void RemoveAt(int indexAt, int removeCount)
	{
		if (indexAt >= 0 && indexAt + removeCount <= Count)
		{
			Count -= removeCount;
			for (int i = indexAt; i < Count; i++)
			{
				_buffer.Array[i] = _buffer.Array[i + removeCount];
			}
			for (int j = Count; j < Count + removeCount; j++)
			{
				_buffer.Array[j] = default(T);
			}
		}
	}

	public T[] ToArray()
	{
		if (Count == 0)
		{
			return null;
		}
		T[] array = new T[Count];
		for (int i = 0; i < Count; i++)
		{
			array[i] = this[i];
		}
		return array;
	}
}
