using System;
using System.Collections.Generic;

namespace NShared.NUtil;

public class SharedDictionary<K, V>
{
	private readonly Dictionary<K, V> _dictionary;

	public int Count => _dictionary.Count;

	public V this[K key]
	{
		get
		{
			return _dictionary[key];
		}
		set
		{
			_dictionary[key] = value;
		}
	}

	public SharedDictionary()
	{
		_dictionary = new Dictionary<K, V>();
	}

	public SharedDictionary(int capacity)
	{
		_dictionary = new Dictionary<K, V>(capacity);
	}

	public virtual void Clear()
	{
		_dictionary.Clear();
	}

	public bool ContainsKey(K key)
	{
		return _dictionary.ContainsKey(key);
	}

	public bool TryGetValue(K key, out V value)
	{
		return _dictionary.TryGetValue(key, out value);
	}

	public virtual void Add(K key, V value)
	{
		TryAdd(key, value);
	}

	public virtual bool TryAdd(K key, V value)
	{
		if (_dictionary.ContainsKey(key))
		{
			return false;
		}
		_dictionary.Add(key, value);
		return true;
	}

	public virtual V AddOrUpdate(K key, V value, Func<K, V, V> update)
	{
		if (TryAdd(key, value))
		{
			return value;
		}
		V arg = _dictionary[key];
		V val = update(key, arg);
		_dictionary[key] = val;
		return val;
	}

	public virtual bool TryRemove(K key, out V value)
	{
		value = default(V);
		if (_dictionary.ContainsKey(key))
		{
			value = _dictionary[key];
			_dictionary.Remove(key);
			return true;
		}
		return false;
	}

	public virtual bool Remove(K key)
	{
		V value = default(V);
		return TryRemove(key, out value);
	}

	public void ForEachIrregular(Action<KeyValuePair<K, V>> action)
	{
		foreach (KeyValuePair<K, V> item in _dictionary)
		{
			action(item);
		}
	}
}
