using System.Collections.Generic;

namespace NGame2.NNodeTree;

public class StringKeyDictionary<TValue>
{
	private Dictionary<StringKey, TValue> _dictionary = new Dictionary<StringKey, TValue>(StringKey.Comparer);

	public void Clear()
	{
		_dictionary.Clear();
	}

	public void Add(StringKey key, TValue value)
	{
		_dictionary.Add(key, value);
	}

	public void ADD(StringKey key, TValue value)
	{
		_dictionary[key] = value;
	}

	public bool TryGetValue(StringKey key, out TValue value)
	{
		return _dictionary.TryGetValue(key, out value);
	}

	public bool Remove(StringKey key)
	{
		return _dictionary.Remove(key);
	}

	public Dictionary<StringKey, TValue> CreateDictionary()
	{
		return new Dictionary<StringKey, TValue>(_dictionary);
	}
}
