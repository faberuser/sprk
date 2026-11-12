using System;
using System.Collections.Generic;

namespace NShared;

public struct TemporaryList<T> : IDisposable
{
	private List<T> _list;

	private List<T> _safeList
	{
		get
		{
			if (_list == null)
			{
				_list = new List<T>();
			}
			return _list;
		}
	}

	public int Count
	{
		get
		{
			if (_list != null)
			{
				return _list.Count;
			}
			return 0;
		}
	}

	public T this[int i]
	{
		get
		{
			return _safeList[i];
		}
		set
		{
			_safeList[i] = value;
		}
	}

	public void Add(T item)
	{
		_safeList.Add(item);
	}

	public void AddRange(IList<T> addList)
	{
		if (addList != null && addList.Count != 0)
		{
			_safeList.AddRange(addList);
		}
	}

	public void Clear()
	{
		if (_list != null)
		{
			_list.Clear();
		}
	}

	public T[] ToArray()
	{
		if (_list != null)
		{
			return _list.ToArray();
		}
		return null;
	}

	public bool Contains(T item)
	{
		if (_list != null)
		{
			return _list.Contains(item);
		}
		return false;
	}

	void IDisposable.Dispose()
	{
		Clear();
	}
}
