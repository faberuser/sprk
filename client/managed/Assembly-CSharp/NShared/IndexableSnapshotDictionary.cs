using System;
using System.Collections;
using System.Collections.Generic;
using NShared.NUtil;

namespace NShared;

public class IndexableSnapshotDictionary<K, V> : BaseSnapshotDictionary<K, V>, IEnumerable<KeyValuePair<K, V>>, IEnumerable where K : IComparable<K>
{
	private readonly IndexableDictionay<K, V> _dictionary;

	static IndexableSnapshotDictionary()
	{
		SnapshotFactory.Register<IndexableSnapshotDictionary<K, V>>(GetTypeName(typeof(K), typeof(V)));
	}

	public IndexableSnapshotDictionary()
	{
		_dictionary = new IndexableDictionay<K, V>();
	}

	public override SharedDictionary<K, V> GetDictionary()
	{
		return _dictionary;
	}

	public override string GetTypeName()
	{
		return GetTypeName(typeof(K), typeof(V));
	}

	public static string GetTypeName(Type keyType, Type valueType)
	{
		return "IndexableSnapshotDictionary_" + keyType.Name + "_" + valueType.Name;
	}

	public K GetKeyByIndex(int index)
	{
		return _dictionary.GetKeyByIndex(index);
	}

	public V GetValueByIndex(int index)
	{
		return _dictionary.GetValueByIndex(index);
	}

	public V GetOrDefault(K key, V defaultValue = default(V))
	{
		if (_dictionary.TryGetValue(key, out var value))
		{
			return value;
		}
		return defaultValue;
	}

	public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
	{
		return _dictionary.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	protected override void ForEach(Action<KeyValuePair<K, V>> action)
	{
		_dictionary.ForEach(action);
	}

	public void SortByValue(Comparison<V> comparison)
	{
		_dictionary.SortByValue(comparison);
	}
}
