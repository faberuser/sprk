using System;
using System.Collections.Generic;

namespace NVespa;

public struct PoolingList<T> : IDisposable
{
	internal List<T> _list;

	private List<T> _safeList
	{
		get
		{
			if (_list == null)
			{
				_list = CreateList();
			}
			return _list;
		}
	}

	public int Count => __Extension_Method_ICollection__.COUNT(_list);

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

	private static List<T> CreateList()
	{
		return SingletonAuto<ReservePool<List<T>>>.instance.Create();
	}

	private static void ReleaseList(List<T> list)
	{
		ReservePoolUtil.ReleaseList(list);
	}

	public int COUNT()
	{
		return __Extension_Method_ICollection__.COUNT(_list);
	}

	public bool IsNullOrEmpty()
	{
		return __Extension_Method_ICollection__.IsNullOrEmpty(_list);
	}

	public bool IsValidIndex(int index)
	{
		return Vespa_Generic_IList_Extension_Method.IsValidIndex(_list, index);
	}

	public List<T> PopList(bool forceCreate = false)
	{
		List<T> list = _list;
		_list = null;
		if (forceCreate && list == null)
		{
			return CreateList();
		}
		return list;
	}

	public void PushList(List<T> list)
	{
		Clear();
		_list = list;
	}

	public void RemoveAt(int index)
	{
		if (_list != null)
		{
			_list.RemoveAt(index);
		}
	}

	public void Clear()
	{
		if (_list != null)
		{
			ReservePoolUtil.ReleaseList(_list);
			_list = null;
		}
	}

	public void Add(T item)
	{
		_safeList.Add(item);
	}

	public void AddRange(IList<T> addList)
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(addList))
		{
			_safeList.AddRange(addList);
		}
	}

	public void AddRange(PoolingList<T> addList)
	{
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(addList._list))
		{
			_safeList.AddRange(addList._list);
		}
	}

	public bool Contains(T item)
	{
		if (_list == null)
		{
			return false;
		}
		return _list.Contains(item);
	}

	public void Sort(Comparison<T> comparison)
	{
		if (_list != null)
		{
			_list.Sort(comparison);
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

	public List<T> ToEnumerable()
	{
		return _list;
	}

	void IDisposable.Dispose()
	{
		Clear();
	}
}
