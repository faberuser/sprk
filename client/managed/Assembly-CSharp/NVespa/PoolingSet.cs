using System;
using System.Collections.Generic;

namespace NVespa;

public struct PoolingSet<T> : IDisposable
{
	internal HashSet<T> _set;

	private HashSet<T> _safeSet
	{
		get
		{
			if (_set == null)
			{
				_set = CreateSet();
			}
			return _set;
		}
	}

	public int Count => __Extension_Method_ICollection__.COUNT(_set);

	private static HashSet<T> CreateSet()
	{
		return SingletonAuto<ReservePool<HashSet<T>>>.instance.Create();
	}

	private static void ReleaseSet(HashSet<T> set)
	{
		set.Clear();
		ReservePoolUtil.Release(set);
	}

	public int COUNT()
	{
		return __Extension_Method_ICollection__.COUNT(_set);
	}

	public bool IsNullOrEmpty()
	{
		return __Extension_Method_ICollection__.IsNullOrEmpty(_set);
	}

	public HashSet<T> PopHashSet(bool forceCreate = false)
	{
		HashSet<T> set = _set;
		_set = null;
		if (forceCreate && set == null)
		{
			return CreateSet();
		}
		return set;
	}

	public void PushHashSet(HashSet<T> set)
	{
		Clear();
		_set = set;
	}

	public void Clear()
	{
		if (_set != null)
		{
			_set.Clear();
			ReservePoolUtil.Release(_set);
			_set = null;
		}
	}

	public void Add(T item)
	{
		_safeSet.Add(item);
	}

	public bool Contains(T item)
	{
		if (_set == null)
		{
			return false;
		}
		return _set.Contains(item);
	}

	void IDisposable.Dispose()
	{
		Clear();
	}
}
