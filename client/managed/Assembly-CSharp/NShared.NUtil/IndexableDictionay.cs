using System;
using System.Collections;
using System.Collections.Generic;
using NVespa;

namespace NShared.NUtil;

public class IndexableDictionay<K, V> : SharedDictionary<K, V>, IEnumerable<KeyValuePair<K, V>>, IEnumerable where K : IComparable<K>
{
	private readonly List<KeyValuePair<K, V>> _list;

	public IndexableDictionay()
	{
		_list = new List<KeyValuePair<K, V>>();
	}

	public IndexableDictionay(int capacity)
		: base(capacity)
	{
		_list = new List<KeyValuePair<K, V>>(capacity);
	}

	public override void Clear()
	{
		base.Clear();
		_list.Clear();
	}

	public override void Add(K key, V value)
	{
		TryAdd(key, value);
	}

	public override bool TryAdd(K key, V value)
	{
		if (!base.TryAdd(key, value))
		{
			return false;
		}
		int sortedListIndex = GetSortedListIndex(key);
		_list.Insert(sortedListIndex, new KeyValuePair<K, V>(key, value));
		return true;
	}

	public override V AddOrUpdate(K key, V value, Func<K, V, V> update)
	{
		if (TryAdd(key, value))
		{
			return value;
		}
		V arg = base[key];
		V val = (base[key] = update(key, arg));
		int sortedListIndex = GetSortedListIndex(key);
		if (sortedListIndex >= 0 && sortedListIndex < _list.Count)
		{
			_list[sortedListIndex] = new KeyValuePair<K, V>(key, val);
		}
		else
		{
			Action<object> logError = VespaDebug.LogError;
			K val3 = key;
			logError("Failed to remove from list : key = " + val3);
		}
		return val;
	}

	public override bool TryRemove(K key, out V value)
	{
		if (!base.TryRemove(key, out value))
		{
			return false;
		}
		int sortedListIndex = GetSortedListIndex(key);
		if (sortedListIndex >= 0 && sortedListIndex < _list.Count)
		{
			_list.RemoveAt(sortedListIndex);
		}
		else
		{
			Action<object> logError = VespaDebug.LogError;
			K val = key;
			logError("Failed to remove from list : key = " + val);
		}
		return true;
	}

	public override bool Remove(K key)
	{
		V value = default(V);
		return TryRemove(key, out value);
	}

	public void ForEach(Action<KeyValuePair<K, V>> action)
	{
		foreach (KeyValuePair<K, V> item in _list)
		{
			action(item);
		}
	}

	public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
	{
		return _list.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public K GetKeyByIndex(int index)
	{
		if (index < 0 || _list.Count <= index)
		{
			return default(K);
		}
		return _list[index].Key;
	}

	public V GetValueByIndex(int index)
	{
		if (index < 0 || _list.Count <= index)
		{
			return default(V);
		}
		return _list[index].Value;
	}

	private int GetSortedListIndex(K key)
	{
		return GetSortedListIndex(key, 0, _list.Count);
	}

	private int GetSortedListIndex(K key, int begin, int end)
	{
		if (begin >= end)
		{
			return begin;
		}
		int num = (begin + end) / 2;
		K key2 = _list[num].Key;
		int num2 = key.CompareTo(key2);
		if (num2 < 0)
		{
			return GetSortedListIndex(key, begin, num);
		}
		if (num2 > 0)
		{
			return GetSortedListIndex(key, num + 1, end);
		}
		return num;
	}

	public void SortByValue(Comparison<V> comparison)
	{
		_list.Sort((KeyValuePair<K, V> lhs, KeyValuePair<K, V> rhs) => comparison(lhs.Value, rhs.Value));
	}
}
