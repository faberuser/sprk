using System.Collections.Generic;

namespace NVespa;

public struct CollectionProxy<T, U> where T : ICollection<U>, new()
{
	private T _collection;

	public T Value
	{
		get
		{
			if (_collection == null)
			{
				_collection = new T();
			}
			return _collection;
		}
	}

	public int Count
	{
		get
		{
			if (_collection == null)
			{
				return 0;
			}
			return _collection.Count;
		}
	}

	public void Clear()
	{
		if (_collection != null)
		{
			_collection.Clear();
		}
	}
}
