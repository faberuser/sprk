using System.Collections.Generic;

namespace NVespa;

public class ThreadSafeDic<TKey, TValue>
{
	internal Dictionary<TKey, TValue> _dic;

	public object LockObj => _dic;

	public int Count
	{
		get
		{
			lock (_dic)
			{
				return _dic.Count;
			}
		}
	}

	public bool IsReadOnly => false;

	public ThreadSafeDic()
	{
		_dic = new Dictionary<TKey, TValue>();
	}

	public ThreadSafeDic(Dictionary<TKey, TValue> dic)
	{
		if (dic == null)
		{
			_dic = new Dictionary<TKey, TValue>();
		}
		else
		{
			_dic = dic;
		}
	}

	public bool Add(TKey key, TValue value)
	{
		lock (_dic)
		{
			if (_dic.ContainsKey(key))
			{
				return false;
			}
			_dic.Add(key, value);
			return true;
		}
	}

	public void ADD(TKey key, TValue value)
	{
		lock (_dic)
		{
			if (!_dic.ContainsKey(key))
			{
				_dic.Add(key, value);
			}
			else
			{
				_dic[key] = value;
			}
		}
	}

	public bool Remove(TKey key)
	{
		lock (_dic)
		{
			return _dic.Remove(key);
		}
	}

	public void Clear()
	{
		lock (_dic)
		{
			_dic.Clear();
		}
	}

	public bool ContainsKey(TKey key)
	{
		lock (_dic)
		{
			return _dic.ContainsKey(key);
		}
	}

	public bool ContainsValue(TValue value)
	{
		lock (_dic)
		{
			return _dic.ContainsValue(value);
		}
	}

	public bool TryGetValue(TKey key, out TValue o_value)
	{
		lock (_dic)
		{
			return _dic.TryGetValue(key, out o_value);
		}
	}

	public void CopyValueTo(List<TValue> list)
	{
		if (list == null)
		{
			return;
		}
		lock (_dic)
		{
			list.AddRange(_dic.Values);
		}
	}
}
