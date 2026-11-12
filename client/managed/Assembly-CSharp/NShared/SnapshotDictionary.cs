using System;
using System.Collections.Generic;
using NShared.NUtil;

namespace NShared;

public class SnapshotDictionary<K, V> : BaseSnapshotDictionary<K, V>
{
	private readonly SharedDictionary<K, V> _dictionary;

	static SnapshotDictionary()
	{
		SnapshotFactory.Register<SnapshotDictionary<K, V>>(GetTypeName(typeof(K), typeof(V)));
	}

	public SnapshotDictionary()
	{
		_dictionary = new SharedDictionary<K, V>();
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
		return "SnapshotDictionary_" + keyType.Name + "_" + valueType.Name;
	}

	protected override void ForEach(Action<KeyValuePair<K, V>> action)
	{
		_dictionary.ForEachIrregular(action);
	}

	public void ForEachIrregular(Action<KeyValuePair<K, V>> action)
	{
		_dictionary.ForEachIrregular(action);
	}
}
