using System;
using System.Collections.Generic;
using NShared.NUtil;
using NVespa;

namespace NShared;

public abstract class BaseSnapshotDictionary<K, V> : ISnapshotObject
{
	protected long _uid;

	public SnapshotRefCounter RefCounter { get; protected set; }

	public ISnapshotObjectPool Pool { get; set; }

	public bool PendingRelease { get; set; }

	public V this[K key]
	{
		get
		{
			return GetDictionary()[key];
		}
		set
		{
			Remove(key);
			Add(key, value);
		}
	}

	public int Count => GetCount();

	public abstract SharedDictionary<K, V> GetDictionary();

	public abstract string GetTypeName();

	protected abstract void ForEach(Action<KeyValuePair<K, V>> action);

	public BaseSnapshotDictionary()
	{
		RefCounter = new SnapshotRefCounter();
		_uid = SnapshotUidAllocator.Allocate();
	}

	public void Add(K key, V value)
	{
		TryAdd(key, value);
	}

	public bool TryAdd(K key, V value)
	{
		if (!GetDictionary().TryAdd(key, value))
		{
			return false;
		}
		SnapshotUtility.IncRefCount(value);
		return true;
	}

	public bool TryGetValue(K key, out V value)
	{
		return GetDictionary().TryGetValue(key, out value);
	}

	public void Clear()
	{
		ForEach(delegate(KeyValuePair<K, V> pair)
		{
			SnapshotUtility.DecRefCount(pair.Value);
		});
		GetDictionary().Clear();
	}

	public int GetCount()
	{
		return GetDictionary().Count;
	}

	public bool ContainsKey(K key)
	{
		return GetDictionary().ContainsKey(key);
	}

	public bool Remove(K key)
	{
		V value;
		return TryRemove(key, out value);
	}

	public bool TryRemove(K key, out V value)
	{
		if (!GetDictionary().TryRemove(key, out value))
		{
			return false;
		}
		SnapshotUtility.DecRefCount(value);
		return true;
	}

	public long GetUid()
	{
		return _uid;
	}

	public void SetUid(long uid)
	{
		_uid = uid;
	}

	object ISnapshotObject.Serialize(SerializedSnapshotContainer container)
	{
		object serialized = container.GetSerialized(this);
		if (serialized != null)
		{
			return serialized;
		}
		int capacity = Count * 2;
		List<object> serializedList = new List<object>(capacity);
		if (container != null)
		{
			container.Add(this, serializedList);
		}
		ForEach(delegate(KeyValuePair<K, V> pair)
		{
			K key = pair.Key;
			V value = pair.Value;
			serializedList.Add(key);
			serializedList.Add(value);
			if (value is ISnapshotObject snapshotObject && container != null)
			{
				snapshotObject.Serialize(container);
			}
		});
		return serializedList;
	}

	void ISnapshotObject.Deserialize(object s)
	{
		if (s is List<object> list)
		{
			Clear();
			for (int i = 0; i < list.Count; i += 2)
			{
				object obj = list[i];
				object obj2 = list[i + 1];
				TryAdd((K)obj, (V)obj2);
			}
		}
	}

	void ISnapshotObject.OnRelease()
	{
		Clear();
	}

	string ISnapshotObject.Dump()
	{
		ISnapshotMemberSerializer keyImpl = SnapshotMemberImplFactory.GetImpl<K>();
		if (keyImpl == null)
		{
			VespaDebug.LogError(GetTypeName() + " Dump : keyimple not found : " + typeof(K).Name);
			return null;
		}
		ISnapshotMemberSerializer valueImpl = SnapshotMemberImplFactory.GetImpl<V>();
		if (valueImpl == null)
		{
			VespaDebug.LogError(GetTypeName() + " Dump : valueimple not found : " + typeof(V).Name);
			return null;
		}
		List<string> dumpedList = new List<string>();
		ForEach(delegate(KeyValuePair<K, V> pair)
		{
			K key = pair.Key;
			V value = pair.Value;
			string item = keyImpl.To(key);
			string item2 = valueImpl.To(value);
			dumpedList.Add(item);
			dumpedList.Add(item2);
		});
		return SnapshotDumper.Dump(this, dumpedList);
	}

	void ISnapshotObject.Parse(IList<string> dumped, DumpedSnapshotContainer container)
	{
		ISnapshotMemberSerializer impl = SnapshotMemberImplFactory.GetImpl<K>();
		if (impl == null)
		{
			VespaDebug.LogError(GetTypeName() + " Parse : keyimple not found : " + typeof(K).Name);
			return;
		}
		ISnapshotMemberSerializer impl2 = SnapshotMemberImplFactory.GetImpl<V>();
		if (impl2 == null)
		{
			VespaDebug.LogError(GetTypeName() + " Parse : valueimple not found : " + typeof(V).Name);
		}
		else if (dumped != null)
		{
			for (int i = 0; i < dumped.Count; i += 2)
			{
				string value = dumped[i];
				string value2 = dumped[i + 1];
				object obj = impl.From(value, container);
				object obj2 = impl2.From(value2, container);
				Add((K)obj, (V)obj2);
			}
		}
	}

	IList<ISnapshotObject> ISnapshotObject.GetChildren()
	{
		if (Count > 0 && (typeof(ISnapshotObject).IsAssignableFrom(typeof(K)) || typeof(ISnapshotObject).IsAssignableFrom(typeof(V))))
		{
			List<ISnapshotObject> list = new List<ISnapshotObject>();
			ForEach(delegate(KeyValuePair<K, V> pair)
			{
				K key = pair.Key;
				if (key != null && key is ISnapshotObject)
				{
					list.Add((ISnapshotObject)(object)key);
				}
				V value = pair.Value;
				if (value != null && value is ISnapshotObject)
				{
					list.Add((ISnapshotObject)(object)value);
				}
			});
			return list;
		}
		return null;
	}
}
